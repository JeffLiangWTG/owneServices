using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class TaxIDMacroData
	{
		public TaxIDMacroData(ZString orgTaxRegistrationPrefix, ZString orgTaxRegistrationCode, string extraOrgTaxRegistrationPrefix = "", string extraOrgTaxRegistrationCode = "")
		{
			Argument.NotNullOrEmpty(orgTaxRegistrationPrefix, "OrgTaxRegistrationPrefix");
			Argument.NotNullOrEmpty(orgTaxRegistrationCode, "OrgTaxRegistrationCode");
			Argument.NotNull(extraOrgTaxRegistrationPrefix, "ExtraOrgTaxRegistrationPrefix");
			Argument.NotNull(extraOrgTaxRegistrationCode, "ExtraOrgTaxRegistrationCode");

			this.OrgTaxRegistrationPrefix = orgTaxRegistrationPrefix;
			this.OrgTaxRegistrationCode = orgTaxRegistrationCode;
			this.ExtraOrgTaxRegistrationPrefix = extraOrgTaxRegistrationPrefix;
			this.ExtraOrgTaxRegistrationCode = extraOrgTaxRegistrationCode;
		}

		public ZString OrgTaxRegistrationPrefix { get; }

		public ZString OrgTaxRegistrationCode { get; }

		public ZString ExtraOrgTaxRegistrationPrefix { get; }

		public ZString ExtraOrgTaxRegistrationCode { get; }
	}
}
