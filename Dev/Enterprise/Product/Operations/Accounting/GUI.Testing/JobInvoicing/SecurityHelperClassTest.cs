using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class SecurityHelperClassTest : TestCaseWithFactory
	{
		public void TestJobInvoicingEditSecurity()
		{
			var shipment = Factory.NewWithValidTestData<DummyShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_INCO = "FOB";
			shipment.JS_UniqueConsignRef = "S00010001";
			shipment.JS_HouseBill = "UVWXYZ";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_ActualChargeable = 100M;

			var shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();

			var pluginToFreight = new InvoicingPluginToFreight(shipment);

			const string loginUserName1 = "login1";
			const string loginUserCode1 = "NL1";
			const string loginUserName2 = "login2";
			const string loginUserCode2 = "NL2";
			const string passwordRight = "right";
			const string passwordWrong = "wrong";

			var dialogResult = DialogResult.OK;

			string loginUserName = loginUserName1;
			string loginPassword = passwordWrong;

			int pass = 0;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs
			(
				delegate(object form)
				{
					var loginForm = (LoginForm)form;

					if (pass == 0)
					{
						loginForm.DoLoginForTest(loginUserName, loginPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = dialogResult;

						pass++;
					}
					else
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				}
			);

			SecurityTestObject.CreateTestUser(true, shipment.InvoicingSupporter.EditSecurityCheckpoint.Code, loginUserCode1, loginUserName1, passwordRight);

			try
			{
				loginPassword = passwordWrong;

				shipmentJob.HasChanges = true;

				ZFormModaliser.LastFormShownDialogForTest = null;

				((DummyShipment.DummyShipmentInvoicingSupporter)shipment.InvoicingSupporter).fEditSecurityLockCore = false;

				ContinueWithSave isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("Should NOT Prompt Login Form", null, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.Yes", ContinueWithSave.Yes, isContinue);

				((DummyShipment.DummyShipmentInvoicingSupporter)shipment.InvoicingSupporter).fEditSecurityLockCore = true;
				dialogResult = DialogResult.Cancel;

				isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.No", ContinueWithSave.No, isContinue);

				dialogResult = DialogResult.OK;

				pass = 0;
				isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.No", ContinueWithSave.No, isContinue);

				loginPassword = passwordRight;

				pass = 0;
				isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.Yes", ContinueWithSave.Yes, isContinue);

				SecurityTestObject.CreateTestUser(false, shipment.InvoicingSupporter.EditSecurityCheckpoint.Code, loginUserCode2, loginUserName2, passwordRight);
				loginUserName = loginUserName2;

				pass = 0;
				isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.No", ContinueWithSave.No, isContinue);

				loginPassword = passwordWrong;

				pass = 0;
				isContinue = pluginToFreight.ShowPreSaveDialogsCore();
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("ShowPreSaveDialogsCore should return ContinueWithSave.No", ContinueWithSave.No, isContinue);
			}
			finally
			{
				pluginToFreight.Dispose();
			}
		}

		public class DummyShipment : ForwardingShipment
		{
			public DummyShipment(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
			{
				return new DummyShipmentInvoicingSupporter(this);
			}

			public class DummyShipmentInvoicingSupporter : ForwardingShipmentInvoicingSupporter
			{
				public DummyShipmentInvoicingSupporter(DummyShipment parent)
					: base(parent)
				{
				}

				protected override SecurityCheckpoint GetEditSecurityCheckpointCore()
				{
					return Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.AgencyBillOfLadingJobInvoicing, SecurityCore.AllowInvAmendmentsDays);
				}

				public bool fEditSecurityLockCore;
				protected override bool EditSecurityLockCore
				{
					get
					{
						return fEditSecurityLockCore;
					}
				}
			}
		}
	}
}
