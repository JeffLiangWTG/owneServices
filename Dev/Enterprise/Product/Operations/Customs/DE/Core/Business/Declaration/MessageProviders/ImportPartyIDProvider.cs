using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ImportPartyIDProvider : IPartyID
	{
		public static ImportPartyIDProvider NewOrNull(OrgAddress orgAddress) => orgAddress == null ? null : new ImportPartyIDProvider(orgAddress);

		ImportPartyIDProvider(OrgAddress orgAddress)
		{
			this.orgAddress = orgAddress;
		}
		readonly OrgAddress orgAddress;

		public string EoriNumber => CachedValueHelper.GetValue(ref eoriNumberCached, () => orgAddress.Header.GetEUEoriDetails(errorOnMultiple: true));
		CachedValue<string> eoriNumberCached;

		public string EoriBranchSuffix => CachedValueHelper.GetValue(ref eoriBranchSuffixCached, () =>
		{
			string result = null;
			if (!EoriNumber.IsEmpty())
			{
				result = orgAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix);
				if (result.IsEmpty())
				{
					result = "0000";
				}
			}
			return result;
		});
		CachedValue<string> eoriBranchSuffixCached;

		public string TCUNumber => null;
	}
}
