using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZAddressControlTestWithFactory : TestCaseWithFactory
	{
		public void TestCreateNewOrganizationWithEmptyCode()
		{
			// otherwise organizations code will be overriden when UNLOCO is set
			Env.Registry.CanUserEditOrganisationCode = true;

			var bo = Factory.New<DummyWithZAddress>();
			var dummyWithAddress = bo.Dummies.AddNew();
			dummyWithAddress.Z0_Guid_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler((orgHeader) => { return orgHeader == null ? ZGuid.Empty : orgHeader.MainAddress.PK; });

			Assert("address should not set here", dummyWithAddress.Z0_Guid.IsEmpty);

			using (var testForm = new ZAddressControlTest.TestZAddressControlForm(bo))
			{
				testForm.Show();

				testForm.AddressControl.ShowEditOrViewForm();

				var organizationsForm = (ZForm)ZFormModaliser.LastFormShownForTest;

				var org = (IOrgHeader)organizationsForm.BusinessEntity;

				org.OH_Code = "TESTORG";
				org.OH_FullName = "TEST ORG";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.OH_IsSalesLead = true;

				org.MainAddress.OA_Address1 = "TEST ADDR";
				org.MainAddress.OA_State = "NSW";
				org.MainAddress.OA_City = "SYDNEY";
				org.MainAddress.OA_PostCode = "1234";

				AssertEquals(ContinueWithSave.Yes, organizationsForm.FireSaveButton());
				organizationsForm.Close();

				AssertEquals("TESTORG", ((IFindBox)testForm.AddressControl.OrganisationFindBox).Code);
				AssertEquals("The org PK should be set after form close", org.PK, dummyWithAddress.Z0_Guid_ZAddress.OrgPK);
				AssertEquals("The address should be set by default", org.MainAddress.PK, dummyWithAddress.Z0_Guid);
			}
		}

		public void TestCreateNewOrganizationWithCode()
		{
			// otherwise organizations code will be overriden when UNLOCO is set
			Env.Registry.CanUserEditOrganisationCode = true;

			var bo = Factory.New<DummyWithZAddress>();
			var dummyWithAddress = bo.Dummies.AddNew();
			dummyWithAddress.Z0_Guid_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler((orgHeader) => { return orgHeader == null ? ZGuid.Empty : orgHeader.MainAddress.PK; });

			Assert("address should not set here", dummyWithAddress.Z0_Guid.IsEmpty);

			using (var testForm = new ZAddressControlTest.TestZAddressControlForm(bo))
			{
				testForm.Show();

				((IFindBox)testForm.AddressControl.OrganisationFindBox).Code = "TESTORG";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, create new organization
				testForm.AddressControl.ShowEditOrViewForm();

				var organizationsForm = (ZForm)ZFormModaliser.LastFormShownForTest;

				var org = (IOrgHeader)organizationsForm.BusinessEntity;

				org.OH_FullName = "TEST ORG";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.OH_IsSalesLead = true;

				org.MainAddress.OA_Address1 = "TEST ADDR";
				org.MainAddress.OA_State = "NSW";
				org.MainAddress.OA_City = "SYDNEY";
				org.MainAddress.OA_PostCode = "1234";

				AssertEquals(ContinueWithSave.Yes, organizationsForm.FireSaveButton());
				organizationsForm.Close();

				AssertEquals("TESTORG", ((IFindBox)testForm.AddressControl.OrganisationFindBox).Code);
				AssertEquals("The org PK should be set after form close", org.PK, dummyWithAddress.Z0_Guid_ZAddress.OrgPK);
				AssertEquals("The address should be set by default", org.MainAddress.PK, dummyWithAddress.Z0_Guid);
			}
		}

		public void TestAddressTextIsUnaccessibleWhenOrgIsNotSelected()
		{
			Env.Registry.CanUserEditOrganisationCode = true;

			SetOrganisationForTesting();

			var bo = Factory.New<DummyWithZAddress>();
			var dummyWithAddress = bo.Dummies.AddNew();
			dummyWithAddress.Z0_Guid_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler((orgHeader) => { return orgHeader == null ? ZGuid.Empty : orgHeader.MainAddress.PK; });

			Assert("address should not set here", dummyWithAddress.Z0_Guid.IsEmpty);
			using (var testForm = new ZAddressControlTest.TestZAddressControlForm(bo))
			{
				testForm.Show();
				Application.DoEvents();
				Assert(testForm.AddressControl.AddressDropEdit.ReadOnly);
				testForm.AddressControl.OrganisationFindBox.Focus();
				((IFindBox)testForm.AddressControl.OrganisationFindBox).Code = "TESTORG";
				Assert(testForm.AddressControl.AddressDropEdit.ReadOnly);
				testForm.AddressControl.AddressDropEdit.Focus();
				Assert(!testForm.AddressControl.AddressDropEdit.ReadOnly);
			}
		}

		public void TestIsOrgVisible_OnFirstShown()
		{
			var canEditOrgCodeOldValue = Env.Registry.CanUserEditOrganisationCode;

			try
			{
				Env.Registry.CanUserEditOrganisationCode = true;

				var bo = Factory.New<DummyWithZAddress>();

				using (var testForm = new ZAddressControlTest.TestZAddressControlForm(bo))
				{
					testForm.Show();
					Application.DoEvents();

					var dummyWithAddress = bo.Dummies.AddNew();
					dummyWithAddress.Z0_Guid_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler((orgHeader) => { return orgHeader == null ? ZGuid.Empty : orgHeader.MainAddress.PK; });

					AssertEquals("Pre-condition: IsOrgVisible", false, ((ZAddress)testForm.AddressControl.AddressDropEdit.Addy).IsOrgVisible);

					testForm.AddressControl.OrganisationFindBox.Focus();
					((IFindBox)testForm.AddressControl.OrganisationFindBox).Code = "ZZZ"; //Enter invalid org

					testForm.AddressControl.AddressDropEdit.Focus();

					AssertEquals("IsOrgVisible", true, ((ZAddress)testForm.AddressControl.AddressDropEdit.Addy).IsOrgVisible); //should be visible so that org will be validated
					AssertEquals("Validation message", "* THE SELECTED ORGANIZATION IS NOT VALID", ((ZAddress)testForm.AddressControl.AddressDropEdit.Addy).Address);
				}
			}
			finally
			{
				Env.Registry.CanUserEditOrganisationCode = canEditOrgCodeOldValue;
			}
		}

		IOrgHeader SetOrganisationForTesting()
		{
			IOrgHeader org = Factory.New<IOrgHeader>();

			org.OH_Code = "TESTORG";
			org.OH_FullName = "TEST ORG";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_IsSalesLead = true;

			org.MainAddress.OA_Address1 = "TEST ADDR";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_City = "SYDNEY";
			org.MainAddress.OA_PostCode = "1234";

			Factory.Save();

			return org;
		}

		public void TestOpenOrganizationByAddressGuid()
		{
			// otherwise organizations code will be overriden when UNLOCO is set
			Env.Registry.CanUserEditOrganisationCode = true;

			IOrgHeader org = SetOrganisationForTesting();

			var bo = Factory.New<DummyWithZAddress>();
			var dummyWithAddress = bo.Dummies.AddNew();

			dummyWithAddress.Z0_Guid = org.MainAddress.PK;

			using (ZAddressControlTest.TestZAddressControlForm testForm = new ZAddressControlTest.TestZAddressControlForm(bo))
			{
				testForm.Show();

				AssertEquals("TESTORG", ((IFindBox)testForm.AddressControl.OrganisationFindBox).Code);
				AssertEquals(dummyWithAddress.Z0_Guid, org.MainAddress.PK);

				testForm.AddressControl.ShowEditOrViewForm();

				var organizationsForm = (ZForm)ZFormModaliser.LastFormShownForTest;
				AssertEquals(org.PK, ((IOrgHeader)organizationsForm.BusinessEntity).PK);
			}
		}

		protected override void SetUp()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			base.SetUp();
		}
	}
}
