using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	public interface ICodeDescriptionPairListWithExtraBool : ICodeDescriptionPairList
	{
		new ICodeDescription this[int index] { get; }
		string GetCodeFromDescription(string description);
		CodeDescriptionPairList GetCodeDescriptionPairList();
		CodeDescriptionBoolWithExtraBool FindElementByCode(string code);
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolWithExtraBoolCollection : CodeDescriptionBoolCollection, ICodeDescriptionPairListWithExtraBool, IXmlSerializable
	{
		public CodeDescriptionBoolWithExtraBoolCollection()
			: base()
		{
		}
		public CodeDescriptionBoolWithExtraBoolCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionBoolWithExtraBoolCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
			SetSystemDefined(list);
		}

		public CodeDescriptionBoolWithExtraBoolCollection(ReadOnlyCodeDescriptionPairList list, bool defaultBoolForNewChild, int codeMaxLength)
			: base(list, defaultBoolForNewChild, codeMaxLength)
		{
			SetSystemDefined(list);
		}

		public CodeDescriptionBoolWithExtraBoolCollection(ReadOnlyCodeDescriptionPairList list, bool defaultBoolForNewChild)
			: base(list, defaultBoolForNewChild)
		{
			SetSystemDefined(list);
		}
		public CodeDescriptionBoolWithExtraBoolCollection(ReadOnlyCodeDescriptionPairList list, int codeMaxLength)
			: base(list, codeMaxLength)
		{
			SetSystemDefined(list);
		}

		void SetSystemDefined(ReadOnlyCodeDescriptionPairList list)
		{
			if (list != null && list.Count > 0)
			{
				foreach (var item in this.ToArray<CodeDescriptionBoolWithExtraBool>().Where(x => list.ContainsCode(x.Code)))
				{
					item.SystemDefined = true;
				}
			}
		}

		public new CodeDescriptionBoolWithExtraBool this[int index]
		{
			get { return (CodeDescriptionBoolWithExtraBool)base[index]; }
		}

		public new CodeDescriptionBoolWithExtraBool AddNew()
		{
			return (CodeDescriptionBoolWithExtraBool)base.AddNew();
		}

		protected override string GetCodeFromDescriptionCore(string description)
		{
			var element = this.ToArray<CodeDescriptionBoolWithExtraBool>().FirstOrDefault(x => x.Description == description);
			if (element != null)
			{
				return element.Code;
			}
			else
			{
				return string.Empty;
			}
		}

		public CodeDescriptionBoolWithExtraBool FindElementByCode(string code)
		{
			return this.ToArray<CodeDescriptionBoolWithExtraBool>().FirstOrDefault(x => x.Code == code);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionBoolWithExtraBool();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CodeDescriptionBoolWithExtraBoolCollection();
		}

		#region ICodeDescriptionPairListWithExtraBool Members

		ICodeDescription ICodeDescriptionPairListWithExtraBool.this[int index]
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
			ReadXmlCore(reader);
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			WriteXmlCore(writer);
		}

		protected virtual void ReadXmlCore(XmlReader reader)
		{
			reader.Read();
			while (reader.IsStartElement("CodeDescriptionBoolWithExtraBool"))
			{
				CodeDescriptionBoolWithExtraBool element = (CodeDescriptionBoolWithExtraBool)ElementSerialiser.Deserialize(reader);
				if (!element.SystemDefined)
				{
					Add(element);
				}
				else
				{
					foreach (CodeDescriptionBoolWithExtraBool el in this)
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

		protected virtual void WriteXmlCore(XmlWriter writer)
		{
			foreach (CodeDescriptionBoolWithExtraBool element in this)
			{
				ElementSerialiser.Serialize(writer, element);
			}
		}

		protected ZXmlSerializer ElementSerialiser
		{
			get { return elementSerialiser ?? (elementSerialiser = ZXmlSerializer.New(TypeOfElements)); }
		}
		ZXmlSerializer elementSerialiser;

		#endregion
	}
}
