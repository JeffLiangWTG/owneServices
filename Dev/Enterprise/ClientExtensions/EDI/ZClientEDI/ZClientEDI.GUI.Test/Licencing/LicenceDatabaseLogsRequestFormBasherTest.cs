using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(LicenceDatabaseLogsRequestForm))]
	internal sealed class LicenceDatabaseLogsRequestFormBasherTest : ZFormBasherTest
	{
		public void TestSend()
		{
			var dummyIncident = Factory.NewWithValidTestData<SupportIncident>();
			dummyIncident.IM_IncidentNumber = "CS01234567";
			Factory.Save();

			var database = DatabaseForTest();

			string confirmationMessage = "A request for the service task logs has been sent to the client's batch processor.\r\nTheir system will respond with a Report and ediProd will be automatically updated in a few minutes.";

			LicenceDatabaseLogsRequest request = new LicenceDatabaseLogsRequest(null);
			request.IncidentNumber = dummyIncident.IM_IncidentNumber;
			request.ServiceTaskCode = "123";
			using (LicenceDatabaseLogsRequestFormForTest form = new LicenceDatabaseLogsRequestFormForTest(request))
			{
				form.sendButton_Click_Exposed();
				AssertEquals("A request for service task logs could not be sent to the client:\r\n\r\nObject reference not set to an instance of an object.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}

			request = new LicenceDatabaseLogsRequest(database);
			request.IncidentNumber = dummyIncident.IM_IncidentNumber;
			request.ServiceTaskCode = "123";
			using (LicenceDatabaseLogsRequestFormForTest form = new LicenceDatabaseLogsRequestFormForTest(request))
			{
				form.sendButton_Click_Exposed();
				AssertEquals(confirmationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNotSupported()
		{
			var database = DatabaseForTest();
			database.LD_PublicEmailAddressForUpdate = "";
			LicenceDatabaseLogsRequest request = new LicenceDatabaseLogsRequest(database);
			using (LicenceDatabaseLogsRequestFormForTest form = new LicenceDatabaseLogsRequestFormForTest(request))
			{
				form.CheckNotSupported();
			}
		}

		LicenceDatabase DatabaseForTest()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_Code = "ABCXYZ";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			return testHeader.LicCompany.LicDatabases.AddNew();
		}

		EDIOrgHeader HeaderForTest
		{
			get
			{
				if (fHeaderForTest == null)
				{
					fHeaderForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
					fHeaderForTest.OH_RL_NKClosestPort = "AUBNE";
					fHeaderForTest.OH_FullName = "My Organisation";
					fHeaderForTest.OH_Code = "TGBLOG";

					OrgAddress newAddress = fHeaderForTest.Addresses.AddNew();
					newAddress.OA_Address1 = "666 Test Address";
				}

				return fHeaderForTest;
			}
		}
		EDIOrgHeader fHeaderForTest;

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new LicenceDatabaseLogsRequestFormForTest(new LicenceDatabaseLogsRequest(null));
		}

		#endregion

		class LicenceDatabaseLogsRequestFormForTest : LicenceDatabaseLogsRequestForm
		{
			public LicenceDatabaseLogsRequestFormForTest(LicenceDatabaseLogsRequest businessEntity)
				: base(businessEntity)
			{
			}

			public void sendButton_Click_Exposed()
			{
				base.sendButton_Click(this, EventArgs.Empty);
			}

			public void CheckNotSupported()
			{
				Assertion.Assert(!sendButton.Enabled);
				Assertion.Assert(dateToField.ReadOnly);
				Assertion.Assert(dateFromField.ReadOnly);
				Assertion.Assert(IncidentNumberFindBox.ReadOnly);
				Assertion.Assert(serviceTaskCodeField.ReadOnly);
				AssertEquals("Not supported label text should match validation error.",LicenceDatabaseLogsRequest.EmailValidationMessage, notSupportedLabel.Text);
			}
		}
	}
}
