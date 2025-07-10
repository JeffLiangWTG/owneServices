using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]

	public class DropDownCodeDescriptionBoolCollection : CodeDescriptionBoolCollection
	{
		public DropDownCodeDescriptionBoolCollection()
			: this(0, null, null)
		{
		}

		public DropDownCodeDescriptionBoolCollection(int codeMaxLength, ReadOnlyCodeDescriptionPairList codeLookup, ReadOnlyCodeDescriptionPairList descriptionLookup)
			: base(null, true, codeMaxLength)
		{
			this.CodeLookup = codeLookup;
			this.DescriptionLookup = descriptionLookup;
		}

		public new DropDownCodeDescriptionBool Add(ZString code, MultilingualString description)
		{
			return Add(code, description, true);
		}

		public new DropDownCodeDescriptionBool Add(ZString code, MultilingualString description, bool booleanValue)
		{
			DropDownCodeDescriptionBool result = AddNew();
			result.CodeMaxLength = CodeMaxLength;
			result.Code = code;
			result.Description = description;
			result.Bool = booleanValue;
			return result;
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new DropDownCodeDescriptionBoolCollection(this.CodeMaxLength, this.CodeLookup, this.DescriptionLookup);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((DropDownCodeDescriptionBool)child).Bool = true;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var dropDownCodeDescriptionBool = (DropDownCodeDescriptionBool)bizOAdded;
			dropDownCodeDescriptionBool.Parent = this;
		}
		public new DropDownCodeDescriptionBool this[int index]
		{
			get { return (DropDownCodeDescriptionBool)Elements[index]; }
		}

		public new DropDownCodeDescriptionBool AddNew()
		{
			return (DropDownCodeDescriptionBool)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DropDownCodeDescriptionBool(this);
		}

		public ReadOnlyCodeDescriptionPairList CodeLookup { get; set; }
		public ReadOnlyCodeDescriptionPairList DescriptionLookup { get; set; }
	}
}
