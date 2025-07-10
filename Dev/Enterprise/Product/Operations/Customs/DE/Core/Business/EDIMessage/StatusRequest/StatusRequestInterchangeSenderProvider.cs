using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class StatusRequestInterchangeSenderProvider : IPartyID
	{
		public string EoriNumber => CachedValueHelper.GetValue(ref eoriNumberCached, () => DECustomsDataRegistry.Instance.ATLASEORINumber.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		CachedValue<string> eoriNumberCached;

		public string EoriBranchSuffix => CachedValueHelper.GetValue(ref eoriBranchSuffixCached, () => DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
		CachedValue<string> eoriBranchSuffixCached;

		public string TCUNumber => null;
	}
}
