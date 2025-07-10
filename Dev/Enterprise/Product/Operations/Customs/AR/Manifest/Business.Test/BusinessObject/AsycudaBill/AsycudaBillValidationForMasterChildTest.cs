using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class AsycudaBillValidationForMasterChildTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_BillIssueDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			header.AMA_MasterBillIssueDate = ZDate.Empty;
			AssertHasMessageErrorContaining(header.AMA_MasterBillIssueDateInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_MasterBillIssueDate = ZDate.Today;
			AssertNoMessageErrors(header.AMA_MasterBillIssueDateInfo);
		}
	}
}
