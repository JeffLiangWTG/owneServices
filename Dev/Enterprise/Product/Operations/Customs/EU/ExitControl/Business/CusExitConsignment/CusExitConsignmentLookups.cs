using CargoWise.Types;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentLookups : ExitControlBase.Business.CusExitConsignmentLookups
	{
		public CusExitConsignmentLookups(AutoCusExitConsignment parent)
			: base(parent)
		{
		}

		public OrganisationsFindBoxCollection OrganizationsFindBoxList => Header.Lookups.OrganizationsFindBoxList;

		public CodeDescriptionPairList StatusList => RefCusCodeListTypes.GetCachedList(Factory,
				Header.CountryCode,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus,
				ZDateTime.Today);

		new CusExitConsignment Parent => (CusExitConsignment)base.Parent;

		CusExitHeader Header => Parent.Header;
	}
}
