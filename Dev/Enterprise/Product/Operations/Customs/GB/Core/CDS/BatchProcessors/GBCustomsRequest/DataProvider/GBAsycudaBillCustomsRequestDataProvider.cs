using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CDS
{
	class GBAsycudaBillCustomsRequestDataProvider : GBCustomsRequestDataProviderBase
	{
		readonly AsycudaBill bill;
		readonly ZString eori;
		readonly ZString badgeCode;

		public GBAsycudaBillCustomsRequestDataProvider(AsycudaBill asycudaBill)
		{
			this.bill = Argument.NotNull(asycudaBill, nameof(asycudaBill));
			this.eori = bill.Header?.Branch?.OrgProxy?.GetEORI() ?? ZString.Empty;
			this.badgeCode = bill.Header?.AMA_CustomsProfile ?? ZString.Empty;
		}

		protected override ZString JobNumberCore => bill.ABL_BillNumber;

		protected override ZString GatewayCore => bill.Header?.CSP ?? ZString.Empty;

		protected override ZString GetCredentialsKey() => GBExtensions.GetCredentialsKey(eori, badgeCode);

		CredentialsSettingCollection credentials => GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(bill.Header.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		protected override CredentialsSetting GetCredentialsSetting() => credentials.FindByBadgeCode(badgeCode);

		public ZString GetBadgeCode() => badgeCode;
	}
}
