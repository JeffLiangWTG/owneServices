using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business.Testing
{
	internal sealed class StaffContactValueObjectHelperTest : TestCaseWithFactory
	{
		public void TestStaffToContact_Scenario1()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "John Smith";
			staff.GS_FriendlyName = "John";
			staff.GS_NameSuffix = "Sr.";
			staff.GS_HomePhone = "123";
			staff.GS_MobilePhone = "234";
			staff.GS_WorkPhone = "456";
			staff.GS_EmailAddress = "staff@cargowise.com\u00a0";
			staff.GS_NameTitle = "Mr.";
			staff.GS_Title = "Operation Manager";
			staff.GS_WorkExtension = "225";
			staff.GS_FaxNum = "555";
			staff.GS_Birthdate = ZDate.BrettsBirthday;
			staff.GS_WorkingLanguage = "EN";

			Xsd.OrgContact contact = StaffContactValueObjectHelper.StaffToContact(staff);
			AssertEquals("John Smith Sr.", contact.Name);
			AssertEquals("234", contact.Mobile);
			AssertEquals("456", contact.Phone);
			AssertEquals("225", contact.PhoneExtension);
			AssertEquals("staff@cargowise.com", contact.EmailAddress);
			AssertEquals("Operation Manager", contact.JobTitle);
			AssertEquals("EN", contact.Language);

			AssertEquals("", contact.Salutation);
			AssertEquals("", contact.HomePhone);
			AssertEquals("", contact.Fax);
			AssertEquals(ZDate.Empty, contact.Birthday);
		}

		public void TestStaffToContact_Scenario2()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "John Smith";
			staff.GS_NameSuffix = "Sr.";
			staff.GS_HomePhone = "** VIEW DENIED **";
			staff.GS_MobilePhone = "** VIEW DENIED **";
			staff.GS_WorkPhone = "** VIEW DENIED **";
			staff.GS_NameTitle = "Mr.";
			staff.GS_Title = "Operation Manager";
			staff.GS_WorkExtension = "225";
			staff.GS_FaxNum = "555";
			staff.GS_Birthdate = ZDate.BrettsBirthday;
			staff.GS_WorkingLanguage = "EN";

			Xsd.OrgContact contact = StaffContactValueObjectHelper.StaffToContact(staff);
			AssertEquals("John Smith Sr.", contact.Name);
			AssertEquals("", contact.Mobile);
			AssertEquals("", contact.Phone);
			AssertEquals("", contact.PhoneExtension);
			AssertEquals("Operation Manager", contact.JobTitle);
			AssertEquals("EN", contact.Language);

			AssertEquals("", contact.Salutation);
			AssertEquals("", contact.HomePhone);
			AssertEquals("", contact.Fax);
			AssertEquals(ZDateTime.Empty, contact.Birthday);
		}

		public void TestStaffToContact_Validation()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "John Smith";
			staff.GS_FriendlyName = "John";
			staff.GS_NameSuffix = "Sr.";
			staff.GS_HomePhone = "123abc";
			staff.GS_MobilePhone = "234%^";
			staff.GS_WorkPhone = "456H*";
			staff.GS_EmailAddress = "staff@cargowise.com\u00a0";
			staff.GS_NameTitle = "Mr.";
			staff.GS_Title = "Operation Manager";
			staff.GS_WorkExtension = "225";
			staff.GS_FaxNum = "555";
			staff.GS_Birthdate = ZDate.BrettsBirthday;
			staff.GS_WorkingLanguage = "EN";

			Xsd.OrgContact contact = StaffContactValueObjectHelper.StaffToContact(staff);
			AssertEquals("John Smith Sr.", contact.Name);
			AssertEquals("", contact.Mobile);
			AssertEquals("", contact.Phone);
			AssertEquals("", contact.PhoneExtension);
			AssertEquals("staff@cargowise.com", contact.EmailAddress);
			AssertEquals("Operation Manager", contact.JobTitle);
			AssertEquals("EN", contact.Language);

			AssertEquals("", contact.Salutation);
			AssertEquals("", contact.HomePhone);
			AssertEquals("", contact.Fax);
			AssertEquals(ZDateTime.Empty, contact.Birthday);
		}

		public void TestGetStaffNameForXsd()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "John Smith";
			staff.GS_FriendlyName = "John";
			staff.GS_NameSuffix = "Sr.";
			AssertEquals("John Smith Sr.", StaffContactValueObjectHelper.GetStaffNameForXsd(staff));

			staff.GS_NameSuffix = "Sr.\u00A0";
			AssertEquals("John Smith Sr.", StaffContactValueObjectHelper.GetStaffNameForXsd(staff));

			staff.GS_FullName = "abc \u069A";
			staff.GS_LoginName = "Logname";
			AssertEquals("Logname", StaffContactValueObjectHelper.GetStaffNameForXsd(staff));

			staff.GS_FriendlyName = "abc \u069A";
			staff.GS_FullName = "John Smith";
			AssertEquals("John Smith Sr.", StaffContactValueObjectHelper.GetStaffNameForXsd(staff));

			staff.GS_FullName = "abc \u069A";
			AssertEquals("Logname", StaffContactValueObjectHelper.GetStaffNameForXsd(staff));
		}

		public void TestCurrentStaffToSecuredQueryString()
		{
			string actual = new StaffContactValueObjectHelper().CurrentStaffAndRegistrationToSecuredQueryString().ToString();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			Xsd.OrgContact contact = StaffContactValueObjectHelper.StaffToContact(GlbStaff.CurrentUser);
			string xmlData = ValueObjectEncoder.Serialize(contact);
			SecureQueryString queryString = new SecureQueryString
			{
				{ StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber, registrationKey.DatabaseNumber.ToString() },
				{ StaffContactValueObjectHelper.QueryStringKeys.ContactData, xmlData },
				{ StaffContactValueObjectHelper.QueryStringKeys.LicenceCode, Env.CurrentCompany.GetLicenceCode() },
				{ StaffContactValueObjectHelper.QueryStringKeys.Password, registrationKey.Password },
				{ StaffContactValueObjectHelper.QueryStringKeys.StaffCode, GlbStaff.CurrentUser.GS_Code },
				{ StaffContactValueObjectHelper.QueryStringKeys.HomeBranchCode, ZString.Empty }
			};
			string expected = queryString.ToString();

			AssertEquals(expected, actual);
		}
	}
}
