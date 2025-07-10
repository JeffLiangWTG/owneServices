using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(LicenceUsageRequestForm))]
	internal sealed class LicenceUsageRequestFormBasherTest : ZFormBasherTest
	{
		public void TestSend()
		{
			var database = DatabaseForTest();
			string confirmationMessage = "A request for Licence Usage has been sent to the client's batch processor.\n\nTheir system will respond with a Report and ediProd will be automatically updated in a few minutes.";

			LicenceUsageRequest request = new LicenceUsageRequest(null);
			using (LicenceUsageRequestFormForTest form = new LicenceUsageRequestFormForTest(request))
			{
				form.sendButton_Click_Exposed();
				AssertEquals("A request for a Licence Usage could not be sent to the client:\r\n\r\nObject reference not set to an instance of an object.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}

			request = new LicenceUsageRequest(database);
			using (LicenceUsageRequestFormForTest form = new LicenceUsageRequestFormForTest(request))
			{
				form.sendButton_Click_Exposed();
				AssertEquals(confirmationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNotSupported()
		{
			var database = DatabaseForTest();
			database.LD_PublicEmailAddressForUpdate = "";
			LicenceUsageRequest request = new LicenceUsageRequest(database);
			using (LicenceUsageRequestFormForTest form = new LicenceUsageRequestFormForTest(request))
			{
				form.Show();
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
			return new LicenceUsageRequestFormForTest(new LicenceUsageRequest(null));
		}

		#endregion

		class LicenceUsageRequestFormForTest : LicenceUsageRequestForm
		{
			public LicenceUsageRequestFormForTest(LicenceUsageRequest businessEntity)
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
				AssertEquals("Not supported label text should match validation error.", LicenceUsageRequest.InvalidVersionValidationMessage, notSupportedLabel.Text);
			}
		}
	}
}
