using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class CustomsEntryWrapperForConsignmentAdditionalReference : CustomsEntryWrapper
	{
		public CustomsEntryWrapperForConsignmentAdditionalReference(CusEntryNumber referenceEntryNumber, BusinessObjectFactory factory)
			: base(referenceEntryNumber, factory)
		{
			ReferenceEntryNumber = referenceEntryNumber ?? Factory.GetNull<CusEntryNumber>();
		}

		protected readonly CusEntryNumber ReferenceEntryNumber;

		protected override CodeAndDescriptionWrapper GetEntryType()
		{
			return new CodeAndDescriptionWrapper(ReferenceEntryNumber.AdditionalReferenceNumberTypeDescription, new CodeDescriptionPairList(), Factory);
		}

		protected override ZString GetEntryNumber()
		{
			return ReferenceEntryNumber.CE_EntryNum;
		}
		protected override ZString GetEntryCategory()
		{
			return ReferenceEntryNumber.CE_Category;
		}

		protected override ZString GetInformation()
		{
			return ReferenceEntryNumber.CE_EntryLineReference;
		}

		protected override ZDateTime GetIssueDate()
		{
			return ReferenceEntryNumber.CE_IssueDate;
		}

		protected override ZString GetCountry()
		{
			return ReferenceEntryNumber.CE_RN_NKCountryCode;
		}
	}
}
