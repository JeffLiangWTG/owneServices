using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.Xml
{
	public class XmlValidator
	{
		public XmlValidator(XmlSchema xmlSchema)
		{
			if (xmlSchema == null)
			{
				throw new ArgumentNullException(nameof(xmlSchema));
			}
			Schema = xmlSchema;
		}

		public void Validate(string xml, INotifications notifications, bool useSchemaNamespaceAsDefault = true)
		{
			try
			{
				XmlReader reader = XmlReader.Create(NewXmlTextReader(xml, XmlNodeType.Document, NewParserContext(useSchemaNamespaceAsDefault)), ReaderSettings);
				Validate(reader, notifications);
			}
			catch (ArgumentNullException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
			catch (InvalidOperationException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
			catch (XmlException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
			catch (NullReferenceException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
			catch (ArgumentException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
		}

		public void Validate(Stream xmlStream, INotifications notifications)
		{
			Validate(xmlStream, notifications, true);
		}

		public void Validate(Stream xmlStream, INotifications notifications, bool useSchemaNamespaceAsDefault)
		{
			try
			{
				XmlReader reader = XmlReader.Create(NewXmlTextReader(xmlStream, XmlNodeType.Element, NewParserContext(useSchemaNamespaceAsDefault)), ReaderSettings);
				Validate(reader, notifications);
			}
			catch (ArgumentNullException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
			catch (InvalidOperationException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
			catch (XmlException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
			catch (NullReferenceException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
			catch (ArgumentException e)
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
		}

		#region NewXmlTextReader

		XmlTextReader NewXmlTextReader(Stream stream, XmlNodeType nodeType, XmlParserContext context)
		{
			XmlTextReader result;
			if (SystemDataRegistry.Instance.XmlSchemaValidationStrict.Value)
			{
				result = new XmlTextReader(stream, nodeType, context);
			}
			else
			{
				result = new XmlTextReaderFilter(Schema, stream, nodeType, context);
			}
			return result;
		}

		XmlTextReader NewXmlTextReader(string xml, XmlNodeType nodeType, XmlParserContext context)
		{
			XmlTextReader result;
			if (SystemDataRegistry.Instance.XmlSchemaValidationStrict.Value)
			{
				result = new XmlTextReader(xml, nodeType, context);
			}
			else
			{
				result = new XmlTextReaderFilter(Schema, xml, nodeType, context);
			}
			return result;
		}

		#endregion

		#region Implementation

		readonly XmlSchema Schema;

		XmlParserContext NewParserContext(bool useSchemaNamespaceAsDefault)
		{
			XmlNameTable nameTable = new XmlDocument().NameTable;
			XmlNamespaceManager namespaceManager = new XmlNamespaceManager(nameTable);
			if (useSchemaNamespaceAsDefault)
			{
				namespaceManager.AddNamespace("", Schema.TargetNamespace ?? string.Empty);
			}
			XmlParserContext context = new XmlParserContext(nameTable, namespaceManager, "", XmlSpace.None);
			return context;
		}

		public void Validate(XmlReader reader, INotifications notifications)
		{
			fNotifyForOnValidationError = notifications;
			try
			{
				while (reader.Read())
				{
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
			}
			finally
			{
				fNotifyForOnValidationError = null;
			}
		}

		XmlReaderSettings ReaderSettings
		{
			get
			{
				if (readerSettings == null)
				{
					readerSettings = new XmlReaderSettings();
					readerSettings.Schemas.Add(Schema);
					readerSettings.ConformanceLevel = ConformanceLevel.Auto;
					readerSettings.ValidationType = ValidationType.Schema;
					readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
					readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
					readerSettings.ValidationEventHandler += new ValidationEventHandler(OnValidationError);
				}
				return readerSettings;
			}
		}
		XmlReaderSettings readerSettings;

		INotifications fNotifyForOnValidationError;
		void OnValidationError(object sender, ValidationEventArgs e)
		{
			fNotifyForOnValidationError.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
		}

		#endregion
	}
}
