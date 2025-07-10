using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NctsHeaderConsigneeWrapper : PartyWrapper
	{
		public new static NctsHeaderConsigneeWrapper New(JobDocAddress docAddress)
		{
			var orgAddress = docAddress?.Address;
			return orgAddress == null ? null : new NctsHeaderConsigneeWrapper(orgAddress);
		}

		protected NctsHeaderConsigneeWrapper(OrgAddress address)
		: base(address)
		{
		}

		protected override ZString IdCore => ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(orgHeader.MainAddress.OA_RN_NKCountryCode) ? base.IdCore : null;
	}
}
