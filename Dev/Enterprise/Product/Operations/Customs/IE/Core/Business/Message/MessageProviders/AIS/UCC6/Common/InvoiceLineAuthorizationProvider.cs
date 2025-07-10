using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class InvoiceLineAuthorizationProvider : IAuthorisation
	{
		public InvoiceLineAuthorizationProvider(CusAuthorizationUsage authorizationUsage)
		{
			this.authorizationUsage = Argument.NotNull(authorizationUsage, nameof(authorizationUsage));
		}

		readonly CusAuthorizationUsage authorizationUsage;

		public string Type => CachedValueHelper.GetValue(ref authorisationType, GetAuthorisationType);
		CachedValue<string> authorisationType;

		public string Reference => authorizationUsage.AGC_Number;

		public string HolderOfTheAuthorisation => CachedValueHelper.GetValue(ref holderOfTheAuthorisation, () =>
			authorizationUsage.Factory.Load<OrgHeader>(authorizationUsage.AGC_OH_Owner)?.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, GlbCompany.CurrentCompany.Country) ?? ZString.Empty);
		CachedValue<string> holderOfTheAuthorisation;

		string GetAuthorisationType()
		{
			var customsValue = authorizationUsage.CustomsCode;
			if (customsValue.IsEmpty)
			{
				customsValue = authorizationUsage.AGC_Code;
			}
			return customsValue;
		}
	}
}
