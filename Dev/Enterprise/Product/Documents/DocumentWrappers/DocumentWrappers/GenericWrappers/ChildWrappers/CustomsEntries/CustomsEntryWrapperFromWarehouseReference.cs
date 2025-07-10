using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class CustomsEntryWrapperFromWarehouseReference : CustomsEntryWrapper
	{
		public CustomsEntryWrapperFromWarehouseReference(WhsDocketReference referenceEntryNumber, BusinessObjectFactory factory)
			: base(referenceEntryNumber, factory)
		{
			ReferenceEntryNumber = referenceEntryNumber ?? Factory.GetNull<WhsDocketReference>();
		}

		readonly WhsDocketReference ReferenceEntryNumber;

		protected override CodeAndDescriptionWrapper GetEntryType()
		{
			return new CodeAndDescriptionWrapper(ReferenceEntryNumber.WX_RefType, WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetDescriptionFromCode(ReferenceEntryNumber.WX_RefType), Factory);
		}

		protected override ZString GetEntryNumber()
		{
			return ReferenceEntryNumber.WX_Reference;
		}
		protected override ZString GetEntryCategory()
		{
			return "";
		}

		protected override ZString GetInformation()
		{
			return "";
		}

		protected override ZDateTime GetIssueDate()
		{
			return ZDateTime.Empty;
		}

		protected override ZString GetCountry()
		{
			return "";
		}
	}
}
