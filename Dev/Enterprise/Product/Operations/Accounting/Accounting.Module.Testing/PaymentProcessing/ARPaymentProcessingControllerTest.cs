using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARPaymentProcessingController))]
	public class ARPaymentProcessingControllerTest : PaymentProcessingControllerTest
	{
		public void TestLedgerSpecificBehaviour()
		{
			ARPaymentProcessingController controller = new ARPaymentProcessingController();
			AssertEquals("ControllerID", ControllerIDs.ARPaymentProcessing, controller.ID);
			AssertEquals("ModuleID", ModuleIDs.ARPaymentProcessing, controller.ModuleID);
			AssertEquals("TypeOfTopLevelBusinessObject", typeof(ARPaymentApprovalWithAuthorisation),
				controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARPaymentProcessing;
		}

		protected override PaymentProcessingController GetNewController()
		{
			return new ARPaymentProcessingController();
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.CompanyData.OB_IsCreditor = true;
			header.OH_Code = "ABC";
			Factory.Save();

			ARPaymentApprovalWithAuthorisation approval = Factory.New<ARPaymentApprovalWithAuthorisation>();
			approval.AV_OH = header.PK;
			Factory.Save();

			return approval;
		}
	}
}
