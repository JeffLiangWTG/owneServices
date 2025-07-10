using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class EntryInstructionAuthorizationProvider : IAuthorisation
	{
		public EntryInstructionAuthorizationProvider(CusAuthorizationUsage authorizationUsage)
		{
			this.authorizationUsage = Argument.NotNull(authorizationUsage, nameof(authorizationUsage));
		}

		readonly CusAuthorizationUsage authorizationUsage;

		public string Type => authorizationUsage.AGC_Code;

		public string Reference => authorizationUsage.AGC_Number;

		public string HolderOfTheAuthorisation => CachedValueHelper.GetValue(ref holderOfTheAuthorisation, () =>
			authorizationUsage.Factory.Load<OrgHeader>(authorizationUsage.AGC_OH_Owner)?.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, GlbCompany.CurrentCompany.Country) ?? ZString.Empty);
		CachedValue<string> holderOfTheAuthorisation;
	}
}
