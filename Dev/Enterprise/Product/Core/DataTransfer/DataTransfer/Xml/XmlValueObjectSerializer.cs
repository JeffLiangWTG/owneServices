using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DataTransfer.Xml
{
	public class XmlValueObjectSerializer : IXmlValueObjectSerializer
	{
		public XmlValueObjectSerializer(Type valueObjectType)
		{
			if (!typeof(IValueObject).IsAssignableFrom(valueObjectType))
			{
				throw new ArgumentException("Only " + typeof(IValueObject).FullName + " objects supported");
			}

			ValueObjectType = valueObjectType;

			var valueTypeRootAttribute = (XmlRootAttribute[])ValueObjectType.GetCustomAttributes(typeof(XmlRootAttribute), false);

			if (valueTypeRootAttribute.Length > 0 && !string.IsNullOrEmpty(valueTypeRootAttribute[0].ElementName))
			{
				RootElementName = valueTypeRootAttribute[0].ElementName;
			}
			else
			{
				RootElementName = valueObjectType.Name;
			}

			if (valueTypeRootAttribute.Length > 0 && valueTypeRootAttribute[0].Namespace != null)
			{
				DefaultNamespace = valueTypeRootAttribute[0].Namespace;
			}
			else
			{
				DefaultNamespace = XmlSchemaDefinitions.EdiXmlNamespace;
			}
		}

		public XmlValueObjectSerializer(Type valueObjectType, bool importInSingleFactory) : this(valueObjectType)
		{
			this.importInSingleFactory = importInSingleFactory;
		}

		public bool CanDeserialize(XmlReader reader)
		{
			return Serializer.CanDeserialize(reader) || reader.IsStartElement(RootElementName, DefaultNamespace) || reader.IsStartElement(RootElementName, "");
		}

		public IValueObject DeserialiseFromXmlElement(XmlElement element)
		{
			return (IValueObject)Deserialize(new XmlNodeReader(element));
		}

		bool importInSingleFactory { get; set; }

		#region Serialize / Deserialize

		public void Serialize(Stream stream, IValueObject value)
		{
			var writer = new StreamWriter(stream, Encoding.UTF8);
			Serialize(writer, value);
			writer.Flush();
		}

		public void Serialize(TextWriter writer, IValueObject value)
		{
			var xmlWriter = new XmlTextWriter(writer);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize(xmlWriter, value);
			xmlWriter.Flush();
		}

		public void Serialize(XmlWriter writer, IValueObject value)
		{
			Serialize(writer, value, null);
		}

		public void Serialize(XmlWriter writer, IValueObject value, XmlSerializerNamespaces namespaces)
		{
			try
			{
				Serializer.Serialize(writer, value, namespaces);
			}
			catch (Exception ex)
			{
				throw new XmlException(ex.Message, ex);
			}
		}

		public object Deserialize(XmlReader reader)
		{
			try
			{
				var rootNamespaceRenamingReader = new RootNamespaceRenamingXmlReader(reader, DefaultNamespace);
				return Serializer.Deserialize(rootNamespaceRenamingReader);
			}
			catch (InvalidOperationException ex)
			{
				string message = Res.GetString("8750BA94-23DD-4183-A560-5F57708EF11A", "Cannot parse XML on element '{0}'. Error Message:", reader.Name);
				throw new XmlException(message + System.Environment.NewLine + ex.Message + (ex.InnerException != null ? "\r\n\t" + ex.InnerException.Message : "" + reader.Name), ex);
			}
		}

		#endregion

		#region ReadInterchangeOrCollectionFromXml

		public void ReadInterchangeOrCollectionFromXml(XmlReader reader, IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
		{
			reader = new RootNamespaceRenamingXmlReader(reader, DefaultNamespace);
			try
			{
				reader.MoveToContent();
			}
			catch (XmlException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}

			var foundCollectionStartElement = reader.LocalName == dataAdapter.RootCollectionElementName;
			var foundRootElement = reader.Name == dataAdapter.RootElementName;
			var foundInterchangeStartElement = CanDeserializeInterchange(reader);
			var hasExceptionBeenCaughtAndNotified = false;

			var interchange = XmlInterchange.Empty;

			if (foundInterchangeStartElement)
			{
				try
				{
					interchange = XmlInterchange.ReadInterchangeOnly(reader, notifications) ?? interchange;
					if (interchange.IsSpecified)
					{
						interchange.Payload.Data = collection;
						interchange.Payload.DataAdapter = dataAdapter;
					}

					reader.Read();
					reader.MoveToContent();
					foundCollectionStartElement = true;
				}
				catch (XmlException e)
				{
					notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
					hasExceptionBeenCaughtAndNotified = true;
				}
			}

			ImportContext = GetNewImportContext(factoryProvider ?? new SingleBusinessObjectFactoryProvider(collection.Factory), interchange, notifications);
			try
			{
				((IDbConnected)ImportContext.FactoryProvider.Current).Connection.EnableAppTransactionCountLog();

				if (foundCollectionStartElement)
				{
					try
					{
						ReadCollectionFromXml(reader, dataAdapter, collection, ImportContext);
					}
					catch (XmlException e)
					{
						notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
					}
				}
				else if (foundRootElement)
				{
					ReadRootElementFromXml(reader, dataAdapter, collection, ImportContext);
				}
				else if (!hasExceptionBeenCaughtAndNotified)
				{
					notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, Res.GetString("d9b74ff5-400b-4fee-b785-5ec13c116cea", "Document is malformed. (Could not find root collection element '{0}'. Please check that each opening tag has a corresponding closing tag of the same spelling and case.)", dataAdapter.RootCollectionElementName)));
				}
			}
			finally
			{
				((IDbConnected)ImportContext.FactoryProvider.Current).Connection.DisableAppTransactionCountLog();
			}
		}

		bool CanDeserializeInterchange(XmlReader reader)
		{
			bool result;
			try
			{
				result = InterchangeXmlSerialiser.CanDeserialize(reader);
			}
			catch (XmlException)
			{
				result = false;
			}
			return result;
		}

		protected virtual void ReadCollectionFromXml(XmlReader reader, IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObjectImportContext context)
		{
			reader.ReadStartElement(dataAdapter.RootCollectionElementName);
			do
			{
				try
				{
					reader.MoveToContent();

					var value = CreateValueObject(reader, dataAdapter, context);
					CreateOrUpdateFromValueObject(dataAdapter, collection, value, context);
				}
				catch (XmlException ex)
				{
					context.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, ex.Message));
				}
			}
			while (CanDeserialize(reader) || CanDeserialize(new RootNamespaceRenamingXmlReader(reader, string.Empty)));

			context.CurrentObjectXMLUTF8 = null;
		}

		protected IValueObject CreateValueObject(XmlReader reader, IValueObjectDataAdapter dataAdapter, IValueObjectImportContext context)
		{
			// tempFileStream is used several times

			using (var tempFileStream = TempFile.CreateWithDeleteOnClose(FileOptions.SequentialScan))
			{
				var settings = new XmlWriterSettings()
				{
					ConformanceLevel = ConformanceLevel.Fragment,
				};

				using (var writer = XmlWriter.Create(tempFileStream, settings))
				{
					writer.WriteNode(reader, false);
				}

				tempFileStream.Position = 0;
				var elementReader = new XmlTextReader(tempFileStream);
				return (IValueObject)Deserialize(elementReader);
			}
		}

		protected virtual void ReadRootElementFromXml(XmlReader reader, IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObjectImportContext context)
		{
			try
			{
				if (reader.IsStartElement(dataAdapter.RootElementName))
				{
					var xml = string.Format("<{0}><{1}>{2}</{1}></{0}>", dataAdapter.RootCollectionElementName, dataAdapter.RootElementName, reader.ReadInnerXml());
					var elementReader = new XmlTextReader(new StringReader(xml));
					ReadCollectionFromXml(elementReader, dataAdapter, collection, context);
				}
			}
			catch (XmlException ex)
			{
				context.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, ex.Message));
			}
		}
		protected virtual BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			var bizObj = dataAdapter.CreateOrUpdateFromValueObject(valueObject, context);
			if (bizObj != null && !bizObj.IsDeleted && !collection.Contains(bizObj.PK) && !importInSingleFactory)
			{
				collection.Add(bizObj);
			}
			return bizObj;
		}

		protected virtual IValueObjectImportContext GetNewImportContext(BusinessObjectFactoryProvider factoryProvider, XmlInterchange interchange, INotifications notifications)
		{
			return new ValueObjectImportContext(factoryProvider, interchange, notifications);
		}

		#endregion

		#region WriteToXml / WriteCollectionToXml

		public void WriteToXml(TextWriter writer, IValueObjectDataAdapter dataAdapter, BusinessObject bizObj, IValueObjectExportContext context)
		{
			var xmlWriter = new XmlTextWriter(writer);
			xmlWriter.Formatting = Formatting.Indented;
			WriteToXml(xmlWriter, dataAdapter, bizObj, context);
			xmlWriter.Flush();
		}

		public void WriteToXml(XmlWriter writer, IValueObjectDataAdapter dataAdapter, BusinessObject bizObj, IValueObjectExportContext context)
		{
			var constructedValueObject = (IValueObject)Activator.CreateInstance(dataAdapter.ValueObjectType);
			WriteToXml(writer, dataAdapter, bizObj, constructedValueObject, context);
		}

		public void WriteToXml(XmlWriter writer, IValueObjectDataAdapter dataAdapter, BusinessObject bizObj, IValueObject constructedValueObject, IValueObjectExportContext context)
		{
			dataAdapter.ExportToValueObject(bizObj, constructedValueObject, context);
			try
			{
				Serialize(
					new RootElementRenamingXmlWriter(writer, dataAdapter.RootElementName),
					constructedValueObject,
					new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName("", DefaultNamespace) }));
			}
			catch (XmlException ex)
			{
				context.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, ex.Message));
			}
		}

		public void WriteToXml(XmlWriter writer, IValueObjectDataAdapter dataAdapter, IValueObject constructedValueObject, INotifications notifications)
		{
			try
			{
				Serialize(
					new RootElementRenamingXmlWriter(writer, dataAdapter.RootElementName),
					constructedValueObject,
					new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName("", DefaultNamespace) }));
			}
			catch (XmlException ex)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, ex.Message));
			}
		}

		public void WriteCollectionToXml(TextWriter writer, IValueObjectDataAdapter dataAdapter, BusinessObject[] bizObjs, IValueObjectExportContext context)
		{
			var xmlWriter = new XmlTextWriter(writer);
			xmlWriter.Formatting = Formatting.Indented;
			xmlWriter.WriteStartDocument();
			WriteCollectionToXml(xmlWriter, dataAdapter, bizObjs, context);
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
		}

		public void WriteCollectionToXml(XmlWriter writer, IValueObjectDataAdapter dataAdapter, IEnumerable bizObjs, IValueObjectExportContext context)
		{
			IProgressSupporter progressSupporter = dataAdapter as IProgressSupporter;

			writer.WriteStartElement(dataAdapter.RootCollectionElementName, DefaultNamespace);
			foreach (BusinessObject bizObj in bizObjs)
			{
				WriteToXml(writer, dataAdapter, bizObj, context);

				if (progressSupporter != null)
				{
					progressSupporter.OnProgress();
				}
			}
			writer.WriteEndElement();
		}

		#endregion

		#region ImportXmlData / ExportXmlData

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This string will be appended to a resource string")]
		public virtual void ImportXmlData(Stream reader, IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
		{
			if (RootElementName != dataAdapter.RootElementName)
			{
				throw new ArgumentException(string.Format("Expected the root element name '{0}' to be the same as the data adapter's root element name '{1}'", RootElementName, dataAdapter.RootElementName));
			}

			MoveToNextNonWhitespaceChar(reader);
			var xmlReader = new EscapingReader(reader, XmlNodeType.Document, null);
			try
			{
				ReadInterchangeOrCollectionFromXml(xmlReader, dataAdapter, collection, factoryProvider, notifications);
			}
			catch (ArgumentException ex)
			{
				string problemXml = string.Empty;
				try
				{
					char[] buffer = new char[200];
					reader.Position = reader.Position > 100 ? reader.Position - 100 : 0;
					if (new StreamReader(reader).Read(buffer, 0, 200) == 200)
					{
						problemXml = string.Format("\r\nXML fragment containing exception: {0}", new string(buffer));
					}
				}
				catch (Exception ex1) when (!ex1.IsCriticalException())
				{
				}

				throw new ArgumentException(Res.GetString("585d707c-d488-40ea-93ce-2dbc77ef9bea", "Unexpected error occurred importing XML.\r\nFilename: {0}\r\nRoot Element Name: {1}\r\nInner Exception message: {2}{3}", dataAdapter.FileName, RootElementName, ex.Message, problemXml), ex);
			}
		}

		class EscapingReader : XmlTextReader
		{
			public EscapingReader(Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
				: base(xmlFragment, fragType, context)
			{ }

			public override string Value
			{
				get
				{
					string value = base.Value;
					if (value.Contains("\f"))
					{
						value = value.Replace("\f", "\r\n");
					}
					return value;
				}
			}
		}

		public void MoveToNextNonWhitespaceChar(Stream stream)
		{
			while (stream.CanRead)
			{
				char readChar;
				unchecked
				{
					readChar = (char)stream.ReadByte();
				}
				if (!char.IsWhiteSpace(readChar) && readChar != '\r')
				{
					break;
				}
			}
			if (stream.Position >= 1)
			{
				stream.Position--;
			}
		}

		public virtual void ExportXmlData(Stream stream, IValueObjectDataAdapter dataAdapter, IList businessObjectsToExport, IValueObjectExportContext context)
		{
			ExportXmlData(stream, dataAdapter, businessObjectsToExport, context, "", "", "");
		}

		public virtual void ExportXmlData(Stream stream, IValueObjectDataAdapter dataAdapter, IList businessObjectsToExport, IValueObjectExportContext context, string senderId, string receiverId, string purpose)
		{
			if (RootElementName != dataAdapter.RootElementName)
			{
				throw new ArgumentException(string.Format("Expected the root element name '{0}' to be the same as the data adapter's root element name '{1}'", RootElementName, dataAdapter.RootElementName));
			}

			var interchange = (XmlInterchange)dataAdapter.ToXmlInterchange(businessObjectsToExport, context);
			interchange.InterchangeInfo.Source.SenderCode = senderId;
			interchange.InterchangeInfo.Target.ReceiverCode = receiverId;
			interchange.InterchangeInfo.Source.Purpose = purpose;
			context.ExportPurpose = purpose;

			var serialiser = new XmlValueObjectSerializer(typeof(XmlInterchange));
			serialiser.Serialize(stream, interchange);
		}

		#endregion

		#region SerialiseToXmlElement

		public XmlElement SerialiseToXmlElement(IValueObject @object)
		{
			using (var stream = new MemoryStream())
			{
				Serialize(stream, @object);

				stream.Flush();
				stream.Position = 0;
				var reader = new XmlTextReader(stream);

				var document = new XmlDocument();
				document.Load(reader);
				var result = (XmlElement)document.ChildNodes[1];

				return result;
			}
		}

		#endregion

		#region Implementation

		public IValueObjectImportContext ImportContext;
		readonly Type ValueObjectType;
		readonly string RootElementName;
		readonly string DefaultNamespace;

		ZXmlSerializer Serializer
		{
			get { return fSerializer ?? (fSerializer = ZXmlSerializer.New(ValueObjectType)); }
		}
		ZXmlSerializer fSerializer;

		XmlValueObjectSerializer InterchangeXmlSerialiser
		{
			get
			{
				if (fInterchangeXmlSerialiser == null)
				{
					fInterchangeXmlSerialiser = new XmlValueObjectSerializer(typeof(XmlInterchange));
				}
				return fInterchangeXmlSerialiser;
			}
		}
		XmlValueObjectSerializer fInterchangeXmlSerialiser;

		#endregion
	}
}
