using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Testing
{
	public class ClientOrgLicencePlugInTest : TestCaseWithFactory
	{
		public void TestPlugInName()
		{
			using (ClientOrgLicencePlugInForTest plugIn = new ClientOrgLicencePlugInForTest(SupportIncident))
			{
				AssertEquals("PlugIn name", "License", plugIn.Name);
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			using (ClientOrgLicencePlugInForTest plugIn = new ClientOrgLicencePlugInForTest(SupportIncident))
			{
				AssertNull("Pre-condition: Client Org is NULL", SupportIncident.Client);
				Assert("PlugIn GUI and business entity should NOT be created: NO client organisation spesified", !plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated);

				EDIOrgHeader testClientOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
				SupportIncident.IM_OH_Client = testClientOrg.PK;
				Assert("PlugIn GUI and business entity should be created: client organisation spesified", plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated);

				SupportIncident.IM_OH_Client = ZGuid.Empty;
				Assert("PlugIn GUI and business entity should NOT be created: NO client organisation spesified", !plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated);
			}
		}

		public void TestPlugInNotDisplayedMessage()
		{
			using (ClientOrgLicencePlugInForTest plugIn = new ClientOrgLicencePlugInForTest(SupportIncident))
			{
				AssertNull("Pre-condition: Client Org is NULL", SupportIncident.Client);
				AssertEquals("PlugInNotDisplayedMessage", "You need to specify the Client Organization", plugIn.PlugInNotDisplayedMessage);

				EDIOrgHeader testClientOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
				SupportIncident.IM_OH_Client = testClientOrg.PK;
				AssertNotContains("PlugInNotDisplayedMessage", "You need to specify the Client Organization", plugIn.PlugInNotDisplayedMessage);
				Assert("PlugIn GUI and business entity should NOT be created: NO client organisation spesified", plugIn.ShouldPlugInGUIAndBusinessEntityBeCreated);
			}
		}

		public void TestRegisterPlugInBusinessEntityAsEditable()
		{
			using (ClientOrgLicencePlugInForTest plugIn = new ClientOrgLicencePlugInForTest(SupportIncident, true))
			{
				AssertEquals("Should not register business entity as editable", false, plugIn.RegisterPlugInBusinessEntityAsEditable);
			}
		}

		public void TestGetBusinessEntityForPlugIn()
		{
			EDIOrgHeader testClientOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_OH = testClientOrg.PK;
			SupportIncident.IM_OH_Client = testClientOrg.PK;

			AssertNotNull(testClientOrg.LicCompany);

			using (ClientOrgLicencePlugInForTest plugIn = new ClientOrgLicencePlugInForTest(SupportIncident))
			{
				AssertEquals(typeof(EDIOrgHeader), plugIn.BusinessEntity.GetType());
				Assert(!((EDIOrgHeader)plugIn.BusinessEntity).LicCompany.ReadOnly);
				Assert(!((EDIOrgHeader)plugIn.BusinessEntity).IsValidationSuspended);

				plugIn.Setup_Exposed();
				Assert(SupportIncident.IsRegisteredEditableChildObject(testClientOrg));
			}

			SupportIncident.UnRegisterEditableChildObject(testClientOrg);
			using (ClientOrgLicencePlugInForTest plugIn = new ClientOrgLicencePlugInForTest(SupportIncident, true))
			{
				AssertEquals(typeof(EDIOrgHeader), plugIn.BusinessEntity.GetType());
				Assert(((EDIOrgHeader)plugIn.BusinessEntity).LicCompany.ReadOnly);
				Assert(((EDIOrgHeader)plugIn.BusinessEntity).IsValidationSuspended);

				plugIn.Setup_Exposed();
				Assert(!SupportIncident.IsRegisteredEditableChildObject(testClientOrg));
			}
		}

		public void TestGetNewUserControl()
		{
			using (ClientOrgLicencePlugInForTest plugIn = new ClientOrgLicencePlugInForTest(SupportIncident))
			{
				using (LicenceKeyBuilderControl userControl = plugIn.UserControl as LicenceKeyBuilderControl)
				{
					AssertNotNull("GetNewUserControl", userControl);
					AssertEquals("GetNewUserControl assigns ContextBusinessEntity", SupportIncident, userControl.ContextBusinessEntity);
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SupportIncident = Factory.New<SupportIncident>();
		}

		class ClientOrgLicencePlugInForTest : ClientOrgLicencePlugIn
		{
			public ClientOrgLicencePlugInForTest(SupportIncident hostBusinessEntity)
				: base(hostBusinessEntity)
			{
			}

			public ClientOrgLicencePlugInForTest(SupportIncident hostBusinessEntity, bool isPluginReadonly)
				: base(hostBusinessEntity, isPluginReadonly)
			{
			}

			public new bool ShouldPlugInGUIAndBusinessEntityBeCreated
			{
				get { return base.ShouldPlugInGUIAndBusinessEntityBeCreated(); }
			}

			public new bool RegisterPlugInBusinessEntityAsEditable
			{
				get { return base.RegisterPlugInBusinessEntityAsEditable; }
			}

			public void Setup_Exposed()
			{
				base.Setup();
			}
		}

		SupportIncident SupportIncident;

		#endregion
	}
}