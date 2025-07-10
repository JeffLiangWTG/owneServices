using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIOrganisationControllerOverride))]
	public class EDIOrganisationControllerOverrideTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Organisation;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			EDIOrgHeader bO = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();
			return bO;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var orgHeader = base.GetBusinessObjectWithoutValidationErrors() as EDIOrgHeader;
			orgHeader.MainAddress.OA_Address1 = "Test Address 1";
			orgHeader.MainAddress.OA_Phone = "(02) 1234 5678";
			orgHeader.MainAddress.OA_PostCode = "123456";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			orgHeader.OH_FullName = "Test Consignor";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			return orgHeader;
		}
	}
}
