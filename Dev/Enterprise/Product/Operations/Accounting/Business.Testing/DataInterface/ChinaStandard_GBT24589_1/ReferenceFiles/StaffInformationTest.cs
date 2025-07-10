using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(StaffInformation))]
	public class StaffInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T108", StaffInformation.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StaffInformation();
		}
	}

	[TestedType(typeof(StaffInformationCollection))]
	public class StaffInformationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StaffInformationCollection>
	{
		[TestDate(2017, 09, 28)]
		public void TestDefaultElements()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Tt1";
			staff.GS_FullName = "staff name";
			staff.GS_Birthdate = new ZDate(1970, 2, 13);
			staff.GS_EmploymentDate = new ZDateTime("2008-04-18");
			staff.GS_DepartureDate = new ZDateTime("2018-02-18");
			staff.GS_IsController = true;
			staff.RunPreSaveValidation();
			GenRegCertAccredMaintList glbCertificates = staff.Certificates.AddNew();
			glbCertificates.XZ_RefNumber = "z1234";
			glbCertificates.XZ_Type = "NID";
			GlbStaff staffNonLogin = Factory.NewWithValidTestData<GlbStaff>();
			staffNonLogin.GS_Code = "Tt2";
			GlbSecurity security = Factory.NewWithValidTestData<GlbSecurity>();
			security.GU_SecurityRight = "Login";
			security.GU_GS = staffNonLogin.PK;
			security.GU_SecurityItemIsAllowed = false;
			Factory.Save();
			StaffInformationCollection collection = new StaffInformationCollection(Factory);
			StaffInformation staffInformation = collection.Cast<StaffInformation>().FirstOrDefault(var => var.StaffCode == "Tt1");
			AssertEquals(staffInformation.BirthDate, "19700213");
			AssertEquals(staffInformation.EmploymentDate, "20080418");
			AssertEquals(staffInformation.LeaveDate, "20180218");
			AssertEquals(staffInformation.IDType, "身份证");
			StaffInformation staffInfo = collection.Cast<StaffInformation>().FirstOrDefault(var => var.StaffCode == "Tt2");
			AssertNull(staffInfo);
		}

		public void TestGetDepartmentCode()
		{
			GlbDepartment glbDept = Factory.NewWithValidTestData<GlbDepartment>();
			glbDept.GE_Code = "TSD";
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Tt1";
			staff.GS_IsController = true;
			staff.GS_GE_HomeDepartment = glbDept.PK;
			Factory.Save();
			StaffInformationCollection collection = new StaffInformationCollection(Factory);
			StaffInformation staffInformation = collection.Cast<StaffInformation>().FirstOrDefault(var => var.StaffCode == "Tt1");
			AssertEquals(staffInformation.DepartmentCode, "TSD");
		}

		public void TestGetIDTypeAndNumber()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "Tt1";
			staff1.GS_IsController = true;
			GenRegCertAccredMaintList glbCertificates1 = staff1.Certificates.AddNew();
			glbCertificates1.XZ_RefNumber = "123456654321";
			glbCertificates1.XZ_Type = CertificateTypePairList.Codes.NI1;
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Tt2";
			staff.GS_IsController = true;
			GenRegCertAccredMaintList glbCertificates = staff.Certificates.AddNew();
			glbCertificates.XZ_RefNumber = "0123456";
			glbCertificates.XZ_Type = CertificateTypePairList.Codes.PA1;
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "Tt3";
			staff3.GS_IsController = true;
			Factory.Save();
			StaffInformationCollection collection = new StaffInformationCollection(Factory);
			StaffInformation staffInformation = collection.Cast<StaffInformation>().FirstOrDefault(var => var.StaffCode == "Tt1");
			AssertEquals(staffInformation.IDType, "身份证");
			AssertEquals(staffInformation.IDNumber, "123456654321");
			staffInformation = collection.Cast<StaffInformation>().FirstOrDefault(var => var.StaffCode == "Tt2");
			AssertEquals(staffInformation.IDType, "护照");
			AssertEquals(staffInformation.IDNumber, "0123456");
			staffInformation = collection.Cast<StaffInformation>().FirstOrDefault(var => var.StaffCode == "Tt3");
			AssertEquals(staffInformation.IDType, ZString.Empty);
			AssertEquals(staffInformation.IDNumber, ZString.Empty);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StaffInformation();
		}

		protected override StaffInformationCollection GetCollectionToTest()
		{
			return new StaffInformationCollection(Factory);
		}
	}
}
