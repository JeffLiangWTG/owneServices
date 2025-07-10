using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APPaymentProcessingController))]
	public class APPaymentProcessingControllerTest : PaymentProcessingControllerTest
	{
		public void TestLedgerSpecificBehaviour()
		{
			APPaymentProcessingController controller = new APPaymentProcessingController();
			AssertEquals("ControllerID", ControllerIDs.APPaymentProcessing, controller.ID);
			AssertEquals("ModuleID", ModuleIDs.APPaymentProcessing, controller.ModuleID);
			AssertEquals("TypeOfTopLevelBusinessObject", typeof(APPaymentApprovalWithAuthorisation),
				controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APPaymentProcessing;
		}

		protected override PaymentProcessingController GetNewController()
		{
			return new APPaymentProcessingController();
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.CompanyData.OB_IsCreditor = true;
			header.OH_Code = "ABC";
			Factory.Save();

			APPaymentApprovalWithAuthorisation approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval.AV_OH = header.PK;
			Factory.Save();

			return approval;
		}
	}
}
