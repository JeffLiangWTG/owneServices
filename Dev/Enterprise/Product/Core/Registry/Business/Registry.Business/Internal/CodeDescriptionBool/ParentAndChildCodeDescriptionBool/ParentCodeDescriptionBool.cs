using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ParentCodeDescriptionBool : CodeDescriptionBool
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string ChildList = "ChildList";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ParentCodeDescriptionBool result = new ParentCodeDescriptionBool();
			result.fChildList = (CodeDescriptionBoolCollection)ChildList.Clone(fallbackLevel, factory);
			return result;
		}

		#region Child List

		public void SetChildList(CodeDescriptionBoolCollection newChildList)
		{
			UnRegisterEditableChildObject(fChildList);
			fChildList = newChildList;
			RegisterEditableChildObject(fChildList);
		}

		public CodeDescriptionBoolCollection ChildList
		{
			get
			{
				if (fChildList == null)
				{
					fChildList = new CodeDescriptionBoolCollection();
					RegisterEditableChildObject(fChildList);
				}

				return fChildList;
			}
		}

		ZXmlSerializer CodeDescriptionBoolCollectionSerialiser
		{
			get
			{
				if (fCodeDescriptionBoolCollectionSerialiser == null)
				{
					fCodeDescriptionBoolCollectionSerialiser = ZXmlSerializer.New(typeof(ChildListCodeDescriptionBoolCollection));
				}
				return fCodeDescriptionBoolCollectionSerialiser;
			}
		}

		internal CodeDescriptionBoolCollection fChildList;
		ZXmlSerializer fCodeDescriptionBoolCollectionSerialiser;

		#endregion

		#region XML Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			ChildListCodeDescriptionBoolCollection list = new ChildListCodeDescriptionBoolCollection();
			list.AddRange(ChildList);
			CodeDescriptionBoolCollectionSerialiser.Serialize(writer, list);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			fChildList = (CodeDescriptionBoolCollection)CodeDescriptionBoolCollectionSerialiser.Deserialize(reader);
		}

		#endregion
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("ChildList")]
	[TestExcludeBusinessObjectsAllHaveTestCases]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ChildListCodeDescriptionBoolCollection : CodeDescriptionBoolCollection
	{
		public ChildListCodeDescriptionBoolCollection()
		{
		}

		public ChildListCodeDescriptionBoolCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public ChildListCodeDescriptionBoolCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
		}

		public ChildListCodeDescriptionBoolCollection(ReadOnlyCodeDescriptionPairList list, bool defaultBoolForNewChild)
			: base(list, defaultBoolForNewChild)
		{
		}

		public ChildListCodeDescriptionBoolCollection(ReadOnlyCodeDescriptionPairList list, int codeMaxLength)
			: base(list, codeMaxLength)
		{
		}
	}
}
