using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class DeclarantProvider : OrganizationProvider, IDeclarant03
	{
		public static DeclarantProvider New(OrgAddress orgAddress) => orgAddress == null ? null : new DeclarantProvider(orgAddress, GetEori(orgAddress));

		DeclarantProvider(OrgAddress orgAddress, string id) : base(orgAddress, id)
		{
		}

		public IContactDetails ContactDetails => null;
	}
}
