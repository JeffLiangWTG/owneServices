using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public interface IParentCodeDescriptionBoolList : ICodeDescriptionBoolList
	{
		ICodeDescriptionBoolList GetChildList(string code);
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ParentCodeDescriptionBoolCollection : CodeDescriptionBoolCollection, IParentCodeDescriptionBoolList
	{
		public ParentCodeDescriptionBoolCollection()
		{
		}

		public ParentCodeDescriptionBoolCollection(ReadOnlyCodeDescriptionPairList list) : base(list)
		{
		}

		public ParentCodeDescriptionBoolCollection(ReadOnlyCodeDescriptionPairList list, bool defaultBoolForNewChild) : base(list, defaultBoolForNewChild)
		{
		}

		public new ParentCodeDescriptionBool this[int i]
		{
			get { return (ParentCodeDescriptionBool)base[i]; }
		}

		public new ParentCodeDescriptionBool AddNew()
		{
			return (ParentCodeDescriptionBool)base.AddNew();
		}

		public ICodeDescriptionBoolList GetChildList(string code)
		{
			CodeDescriptionBoolCollection result;
			ParentCodeDescriptionBool parent = (ParentCodeDescriptionBool)FindByCode(code);

			if (parent != null && parent.ChildList.Count > 0)
			{
				result = parent.ChildList;
			}
			else
			{
				result = new CodeDescriptionBoolCollection();
				CodeDescriptionBool element = result.AddNew();
				element.Code = "UDF";
				element.Description = ResString.GetMultilingualString("894c8f5b-4a53-496e-b2bd-9c5a1f1f70c0", "Undefined");
			}

			return result;
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new ParentCodeDescriptionBoolCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ParentCodeDescriptionBool();
		}
	}
}
