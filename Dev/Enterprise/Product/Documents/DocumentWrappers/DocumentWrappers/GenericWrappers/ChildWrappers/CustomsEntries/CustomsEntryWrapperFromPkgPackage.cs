using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class CustomsEntryWrapperFromPkgPackage : CustomsEntryWrapperFromCusEntryNumber
	{
		public CustomsEntryWrapperFromPkgPackage(CusEntryNumber referenceEntryNumber, BusinessObjectFactory factory)
			: base(referenceEntryNumber, factory)
		{
			ReferenceEntryNumber = referenceEntryNumber ?? Factory.GetNull<CusEntryNumber>();
		}

		readonly CusEntryNumber ReferenceEntryNumber;

		protected override CodeAndDescriptionWrapper GetEntryType()
		{
			return new CodeAndDescriptionWrapper(ReferenceEntryNumber.CE_EntryType, WarehouseDataRegistry.Instance.PackageAdditionalReferenceType.Value.GetDescriptionFromCode(ReferenceEntryNumber.CE_EntryType), Factory);
		}
	}
}
