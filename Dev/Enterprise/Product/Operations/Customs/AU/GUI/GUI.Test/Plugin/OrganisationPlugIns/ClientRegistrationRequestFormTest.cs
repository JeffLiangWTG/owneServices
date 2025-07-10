using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(ClientRegistrationRequestForm))]
	sealed class ClientRegistrationRequestFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = new ClientRegistrationRequestForm(new OrgHeaderWrapper(Factory.New<OrgHeader>())))
			{
				AssertEquals("Customs Client Registration Request", form.FormCaption);
			}
		}

		public void TestValidation()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = Factory.New<OrgAddress>();
			address.Address1 = "Address1";
			address.Address2 = "Address2";
			address.City = "Sydney";
			address.Postcode = "1111";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			orgCusCode.OK_CustomsRegNo = "12345";
			orgCusCode.OK_OA_PremisesAddress = address.PK;
			var wrapper = new OrgHeaderWrapper(orgHeader);
			var dataProvider = wrapper.CLREGInfoProvider;
			var roll = Factory.New<Roll>();
			roll.ZA_Roll = CMRClientRolls.Codes.SeaCargoReporter;
			dataProvider.Rolls.Add(roll);
			dataProvider.ZA_ABN = "123456";
			dataProvider.ZA_Bsn1 = "Address1";
			dataProvider.ZA_Bsn2 = "Address2";
			dataProvider.ZA_BsnCity = "Sydney";
			dataProvider.ZA_BsnPostCode = "1111";
			using (var form = new ClientRegistrationRequestForm(wrapper))
			{
				form.OKBoundButton_Click(null, new EventArgs());
				AssertContains("Should show CCID exists notifications", ClientRegistrationRequestForm.CCIDExists, UnitTestUserNotification.Instance.LastMessage.Text);
				dataProvider.ZA_Bsn1 = "NewAddress";

				form.OKBoundButton_Click(null, new EventArgs());
				AssertContains("Should contains Contact Information notifications", "At least one of the following must be supplied: Contact Business Address details, Contact Phone, Contact Fax, Contact AH, Contact Mobile or Contact Email.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestControlsVisibility()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			wrapper.CLREGInfoProvider.ZA_ABN = "123456";
			using (var form = new ClientRegistrationRequestForm(wrapper))
			{
				AssertEquals("Individual Data - Title should not be visible for ABN", false, form.TitleTextBox.Visible);
				AssertEquals("Individual Data - First Name should not be visible for ABN", false, form.FirstNameTextBox.Visible);
				AssertEquals("Individual Data - Second Name should not be visible for ABN", false, form.SecondNameTextBox.Visible);
				AssertEquals("Individual Data - Family Name should not be visible for ABN", false, form.FamilyNameTextBox.Visible);
				AssertEquals("Individual Data - Gender should not be visible for ABN", false, form.GenderDropEdit.Visible);
				AssertEquals("Individual Data - DOB should not be visible for ABN", false, form.DOBDateEdit.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = new OrgHeaderWrapper(org);
			Factory.Save();
			return new ClientRegistrationRequestForm(wrapper);
		}
	}
}
