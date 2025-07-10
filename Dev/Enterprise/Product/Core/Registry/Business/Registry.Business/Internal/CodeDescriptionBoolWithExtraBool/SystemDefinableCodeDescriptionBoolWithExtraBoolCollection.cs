using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public interface ICodeDescriptionPairListWithDefaultCodeAndExtraBool : ICodeDescriptionPairList
	{
		string GetCodeFromDescription(string description);
		ZString DefaultCode { get; }
		CodeDescriptionPairList GetCodeDescriptionPairList();
		SystemDefinableCodeDescriptionBoolWithExtraBool FindElementByCode(string code);
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class SystemDefinableCodeDescriptionBoolWithExtraBoolCollection : CodeDescriptionBoolWithExtraBoolCollection, ICodeDescriptionPairListWithDefaultCodeAndExtraBool, IXmlSerializable
	{
		public SystemDefinableCodeDescriptionBoolWithExtraBoolCollection()
			: this(true)
		{
		}

		internal SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(ZBool allowNew)
			: base()
		{
			AllowNewCore = AllowRemoveCore = allowNew;
		}

		public SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(ZInt codeMaxLength)
			: this(codeMaxLength, null, false, true)
		{
		}

		public SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(ZInt codeMaxLength, ReadOnlyCodeDescriptionPairList list, ZBool canEditDefaultList)
			: this(codeMaxLength, list, canEditDefaultList, true)
		{
		}

		public SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(ZInt codeMaxLength, ReadOnlyCodeDescriptionPairList list, ZBool canEditDefaultList, ZBool allowNew)
			: base(list, codeMaxLength)
		{
			foreach (SystemDefinableCodeDescriptionBoolWithExtraBool element in this)
			{
				element.SystemDefined = !canEditDefaultList;
			}

			AllowNewCore = AllowRemoveCore = allowNew;
		}

		public new SystemDefinableCodeDescriptionBoolWithExtraBool this[int i]
		{
			get { return (SystemDefinableCodeDescriptionBoolWithExtraBool)base[i]; }
		}

		public new SystemDefinableCodeDescriptionBoolWithExtraBool AddNew()
		{
			return (SystemDefinableCodeDescriptionBoolWithExtraBool)base.AddNew();
		}

		protected override bool AllowNewCore { get; }
		protected override bool AllowRemoveCore { get; }

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			if (!DefaultCode.IsEmpty && DefaultElement == null)
			{
				SystemDefinableCodeDescriptionBoolWithExtraBool newElement = businessObject as SystemDefinableCodeDescriptionBoolWithExtraBool;
				if (newElement != null && newElement.Code == DefaultCode)
				{
					DefaultElement = newElement;
				}
			}
		}

		public ZString DefaultCode
		{
			get { return defaultCode; }
		}

		public new SystemDefinableCodeDescriptionBoolWithExtraBool FindElementByCode(string code)
		{
			return (SystemDefinableCodeDescriptionBoolWithExtraBool)base.FindByCode(code);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SystemDefinableCodeDescriptionBoolWithExtraBool();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			SystemDefinableCodeDescriptionBoolWithExtraBoolCollection result = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(AllowNewCore);
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
					reader.Read();
					reader.Read();
					((IXmlSerializable)this).ReadXml(reader);
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

		internal SystemDefinableCodeDescriptionBoolWithExtraBool DefaultElement
		{
			get
			{
				if (!defaultElementCalculated)
				{
					defaultElementCalculated = true;
					if (!DefaultCode.IsEmpty)
					{
						foreach (SystemDefinableCodeDescriptionBoolWithExtraBool element in this)
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
		SystemDefinableCodeDescriptionBoolWithExtraBool defaultElement;

		#region IXmlSerializable Members

		protected override void ReadXmlCore(XmlReader reader)
		{
			SetDefaultCode(reader.GetAttribute("DefaultCode"), true);
			reader.Read();
			while (reader.IsStartElement("SystemDefinableCodeDescriptionBoolWithExtraBool"))
			{
				SystemDefinableCodeDescriptionBoolWithExtraBool element = (SystemDefinableCodeDescriptionBoolWithExtraBool)ElementSerialiser.Deserialize(reader);
				if (!element.SystemDefined)
				{
					Add(element);
				}
				else
				{
					foreach (SystemDefinableCodeDescriptionBoolWithExtraBool el in this)
					{
						if (el.Code == element.Code)
						{
							el.Bool2 = element.Bool2;
							break;
						}
					}
				}
			}
		}

		protected override void WriteXmlCore(XmlWriter writer)
		{
			writer.WriteAttributeString("DefaultCode", DefaultCode);
			base.WriteXmlCore(writer);
		}

		#endregion
	}
}
