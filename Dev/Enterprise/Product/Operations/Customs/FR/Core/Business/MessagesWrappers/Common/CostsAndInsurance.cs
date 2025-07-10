using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class CostsAndInsuranceWrapper : ICostsAndInsurance
	{
		public CostsAndInsuranceWrapper(CusEntryHeader entryHeader, ZString costsChargeCode, ZString insuranceChargeCode, ZString flags)
		{
			this.EntryHeader = Argument.NotNull(entryHeader, "CustEntryHeader cannot be null");
			this.Declaration = Argument.NotNull(this.EntryHeader.Declaration, "JobDeclaration cannot be null");
			this.CostsChargeCode = costsChargeCode;
			this.InsuranceChargeCode = insuranceChargeCode;
			this.FlagsForCosts = flags;
			this.FlagsForInsurance = flags;
		}

		public CostsAndInsuranceWrapper(CusEntryHeader entryHeader, ZString costsChargeCode, ZString insuranceChargeCode, ZString flagsForCosts, ZString flagsForInsurance)
		{
			this.EntryHeader = Argument.NotNull(entryHeader, "CustEntryHeader cannot be null");
			this.Declaration = Argument.NotNull(this.EntryHeader.Declaration, "JobDeclaration cannot be null");
			this.CostsChargeCode = costsChargeCode;
			this.InsuranceChargeCode = insuranceChargeCode;
			this.FlagsForCosts = flagsForCosts;
			this.FlagsForInsurance = flagsForInsurance;
		}

		public IAmountAndCurrency Costs => AmountAndCurrencyWrapper.New(EntryHeader, new ZString[] { CostsChargeCode }, FlagsForCosts);

		public IAmountAndCurrency Insurance => AmountAndCurrencyWrapper.New(EntryHeader, new ZString[] { InsuranceChargeCode }, FlagsForInsurance);

		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;
		protected readonly ZString CostsChargeCode;
		protected readonly ZString InsuranceChargeCode;
		protected readonly ZString FlagsForCosts;
		protected readonly ZString FlagsForInsurance;
	}
}
