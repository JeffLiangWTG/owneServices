using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitControlHeaderLookups : AutoCusExitControlHeaderLookups
	{
		public CusExitControlHeaderLookups(AutoCusExitControlHeader parent) : base(parent)
		{
		}

		public virtual CustomsOfficeCodeCollection CustomsOffices => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, new ZString[]
		{
			EuOfficeCodesTypes.Codes.OfficeOfExit,
			EuOfficeCodesTypes.Codes.CompetentAuthorityOfExport,
			EuOfficeCodesTypes.Codes.OfficeOfExitInland,
			EuOfficeCodesTypes.Codes.OfficeOfLodgementExit,
			EuOfficeCodesTypes.Codes.OfficeOfExport
		});

		public OrganisationsFindBoxCollection OrganizationsFindBoxList => new OrganisationsFindBoxCollection(Factory);
	}
}
