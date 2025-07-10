using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	public interface ICodeDescriptionPairListWithDefaultCode : ICodeDescriptionPairList
	{
		new ICodeDescription this[int index] { get; }
		string GetCodeFromDescription(string description);
		ZString DefaultCode { get; }
		CodeDescriptionPairList GetCodeDescriptionPairList();
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class SystemDefinableCodeDescriptionBoolCollection : CodeDescriptionBoolCollection, ICodeDescriptionPairListWithDefaultCode, IXmlSerializable
	{
		public SystemDefinableCodeDescriptionBoolCollection()
			: base()
		{
		}

		public SystemDefinableCodeDescriptionBoolCollection(ZInt codeMaxLength)
			: this(codeMaxLength, null, false)
		{
		}

		public SystemDefinableCodeDescriptionBoolCollection(ZInt codeMaxLength, ReadOnlyCodeDescriptionPairList list, ZBool canEditDefaultList)
			: base(list, codeMaxLength)
		{
			foreach (SystemDefinableCodeDescriptionBool element in this)
			{
				element.SystemDefined = !canEditDefaultList;

				var listElement = list[list.IndexOfCode(element.Code)];
				if (listElement is CodeDescriptionBoolDefaultReadonly)
				{
					element.DefaultColumnReadOnly = ((CodeDescriptionBoolDefaultReadonly)listElement).DefaultColumnReadOnly;
				}
			}
		}

		public new SystemDefinableCodeDescriptionBool this[int i]
		{
			get { return (SystemDefinableCodeDescriptionBool)base[i]; }
		}

		public new SystemDefinableCodeDescriptionBool AddNew()
		{
			return (SystemDefinableCodeDescriptionBool)base.AddNew();
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			if (!DefaultCode.IsEmpty && DefaultElement == null)
			{
				SystemDefinableCodeDescriptionBool newElement = businessObject as SystemDefinableCodeDescriptionBool;
				if (newElement != null && newElement.Code == DefaultCode)
				{
					DefaultElement = newElement;
				}
			}
		}

		protected override string GetCodeFromDescriptionCore(string description)
		{
			foreach (SystemDefinableCodeDescriptionBool element in this)
			{
				if (element.Description == description)
				{
					return element.Code;
				}
			}
			return string.Empty;
		}

		public ZString DefaultCode
		{
			get { return defaultCode; }
		}

		protected override CodeDescriptionPairList GetCodeDescriptionPairListCore()
		{
			var result = base.GetCodeDescriptionPairListCore();
			result.DefaultCode = DefaultCode;

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SystemDefinableCodeDescriptionBool();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			SystemDefinableCodeDescriptionBoolCollection result = new SystemDefinableCodeDescriptionBoolCollection();
			result.SetDefaultCode(DefaultCode, true);
			return result;
		}

		internal void Populate(byte[] serialisedUserDefinedList)
		{
			if (serialisedUserDefinedList.Length > 0)
			{
				using (MemoryStream stream = new MemoryStream(serialisedUserDefinedList))
				using (XmlTextReader reader = new XmlTextReader(stream))
				{
					while (reader.Read())
					{
						if (reader.Name == "SystemDefinableCodeDescriptionBoolCollection")
						{
							break;
						}
					}

					ReadXml(reader);
				}
			}
		}

		public void SetDefaultCode(string value, bool resetDefaultElement)
		{
			if (DefaultCode != value)
			{
				defaultCode = value;
				if (resetDefaultElement)
				{
					defaultElement = null;
					defaultElementCalculated = false;
				}
			}
		}

		internal SystemDefinableCodeDescriptionBool DefaultElement
		{
			get
			{
				if (!defaultElementCalculated)
				{
					defaultElementCalculated = true;
					if (!DefaultCode.IsEmpty)
					{
						foreach (SystemDefinableCodeDescriptionBool element in this)
						{
							if (element.Code == DefaultCode)
							{
								defaultElement = element;
								break;
							}
						}
					}
				}
				return defaultElement;
			}
			set
			{
				defaultElementCalculated = true;
				defaultCode = value.Code;
				defaultElement = value;
			}
		}

		ZString defaultCode;
		bool defaultElementCalculated;
		SystemDefinableCodeDescriptionBool defaultElement;

		#region ICodeDescriptionPairListWithDefaultCode Members

		ICodeDescription ICodeDescriptionPairListWithDefaultCode.this[int index]
		{
			get { return this[index]; }
		}

		#endregion

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			ReadXml(reader);
		}

		void ReadXml(XmlReader reader)
		{
			SetDefaultCode(reader.GetAttribute("DefaultCode"), true);
			reader.Read();
			while (reader.IsStartElement("SystemDefinableCodeDescriptionBool"))
			{
				SystemDefinableCodeDescriptionBool element = (SystemDefinableCodeDescriptionBool)ElementSerialiser.Deserialize(reader);
				Add(element);
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString("DefaultCode", DefaultCode);
			foreach (SystemDefinableCodeDescriptionBool element in this)
			{
				if (!element.SystemDefined)
				{
					ElementSerialiser.Serialize(writer, element);
				}
			}
		}

		ZXmlSerializer ElementSerialiser
		{
			get
			{
				if (elementSerialiser == null)
				{
					elementSerialiser = ZXmlSerializer.New(typeof(SystemDefinableCodeDescriptionBool));
				}
				return elementSerialiser;
			}
		}

		ZXmlSerializer elementSerialiser;

		#endregion
	}
}
