using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(ContactAddressSource))]
	public class ContactAddressSourceTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				ContactAddressSource.New(JobDocAddress, Factory)
			};
		}

		public void TestGoingPostal()
		{
			AssertEquals("Contact\nCOMPANY\n11\n21\n31 41 51", ContactWrapper.PostalAddress);

			JobDocAddress.E2_Contact = "";
			AssertEquals("COMPANY\n11\n21\n31 41 51", ContactWrapper.PostalAddress);
		}

		public void TestPostalAddressInEnglish()
		{
			var staffDE = Factory.New<GlbStaff>();
			staffDE.GS_Code = "ABC";
			staffDE.GS_LoginName = "Dieter";
			staffDE.GS_FullName = "Dieter";
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				JobDocAddress.E2_CompanyName = "My Company";
				JobDocAddress.E2_Address1 = "12A TOWER BUILDING";
				JobDocAddress.E2_Address2 = "HIGH TOWER";
				JobDocAddress.E2_City = "Star City";
				JobDocAddress.E2_Postcode = "1111";
				JobDocAddress.E2_RN_NKCountryCode = "DE";
				AssertEquals("PostalAddress", "Contact\nMY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\n1111 STAR CITY\nDEUTSCHLAND", ContactWrapper.PostalAddress);
				AssertEquals("PostalAddressInEnglish", "Contact\nMY COMPANY\n12A TOWER BUILDING\nHIGH TOWER\n1111 STAR CITY\nGERMANY", ContactWrapper.PostalAddressInEnglish);
			}
		}

		public void TestCode()
		{
			AssertEquals("", ContactWrapper.Code);
		}

		public void TestAttachmentType()
		{
			AssertEquals("", ContactWrapper.AttachmentType);
		}

		public void TestBirthday()
		{
			AssertEquals(ZDateTime.Empty, ContactWrapper.Birthday);
		}

		public void TestContactName()
		{
			JobDocAddress.E2_Contact = "MICHAEL SMITH JNR.";
			AssertEquals("MICHAEL SMITH JNR.", ContactWrapper.ContactName);
		}

		public void TestEmail()
		{
			JobDocAddress.E2_Email = "nicolem@syd.pacnet.com.au";
			AssertEquals("nicolem@syd.pacnet.com.au", ContactWrapper.Email);
		}

		public void TestFax()
		{
			JobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			JobDocAddress.E2_Fax = "039 338 0716";
			AssertEquals("+61 3 9338 0716", ContactWrapper.Fax);
		}

		public void TestHomePhone()
		{
			AssertEquals("", ContactWrapper.HomePhone);
		}

		public void TestLanguage()
		{
			AssertEquals("", ContactWrapper.Language);
		}

		public void TestMobile()
		{
			AssertEquals("", ContactWrapper.Mobile);
		}

		public void TestNotifyMode()
		{
			AssertEquals("", ContactWrapper.NotifyMode);
		}

		public void TestOrgAddress()
		{
			AssertNull(ContactWrapper.OrgAddress);
		}

		public void TestOrganisation()
		{
			AssertEquals("Company", ContactWrapper.Organisation.Name);
		}

		public void TestAddressOverride()
		{
			AssertEquals("Company", ContactWrapper.AddressOverride.Name);
		}

		public void TestOtherPhone()
		{
			AssertEquals("", ContactWrapper.OtherPhone);
		}

		public void TestPager()
		{
			AssertEquals("", ContactWrapper.Pager);
		}

		public void TestPassword()
		{
			AssertEquals("", ContactWrapper.Password);
		}

		public void TestPersonalInfo()
		{
			AssertEquals("", ContactWrapper.PersonalInfo);
		}

		public void TestPhone()
		{
			JobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			JobDocAddress.E2_Phone = "03 9289 2829";
			AssertEquals("+61 3 9289 2829", ContactWrapper.Phone);
		}

		public void TestTitle()
		{
			AssertEquals("", ContactWrapper.Title);
		}

		#region Implementation

		protected override void SetUp()
		{
			JobDocAddress = CreateNewJobDocAddressWithTestData;
			ContactWrapper = ContactAddressSource.New(JobDocAddress, Factory);
			AssertNotNull("Wrapper created not null", ContactWrapper);

			base.SetUp();
		}

		JobDocAddress CreateNewJobDocAddressWithTestData
		{
			get
			{
				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				JobDocAddress result = JobDocAddress.New(shipment);

				result.E2_AddressOverride = true;
				result.E2_Contact = "Contact";
				result.E2_CompanyName = "Company";
				result.E2_Address1 = "11";
				result.E2_Address2 = "21";
				result.E2_City = "31";
				result.E2_State = "41";
				result.E2_Postcode = "51";
				result.E2_RN_NKCountryCode = "XY";
				result.E2_Phone = "61";
				result.E2_Fax = "71";
				result.E2_Email = "81";

				return result;
			}
		}

		ContactAddressSource ContactWrapper;
		JobDocAddress JobDocAddress;

		#endregion
	}
}
