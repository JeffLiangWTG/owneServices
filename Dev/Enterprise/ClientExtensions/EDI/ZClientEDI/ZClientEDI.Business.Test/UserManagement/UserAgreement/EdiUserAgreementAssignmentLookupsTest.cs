using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	[TestedType(typeof(EdiUserAgreementAssignmentLookups))]
	public class EdiUserAgreementAssignmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParentTypes()
		{
			var lookups = new EdiUserAgreementAssignmentLookups(null);
			AssertEquals(true, lookups.ParentTypes.ContainsCode(LicenceEnterpriseSchema.Constants.Prefix));
			AssertEquals(EdiUserAgreementAssignmentLookups.EnterpriseLicenceParentDescription, lookups.ParentTypes.GetDescriptionFromCode(LicenceEnterpriseSchema.Constants.Prefix));
			AssertEquals(true, lookups.ParentTypes.ContainsCode(LicenceDatabaseSchema.Constants.Prefix));
			AssertEquals(EdiUserAgreementAssignmentLookups.DatabaseParentDescription, lookups.ParentTypes.GetDescriptionFromCode(LicenceDatabaseSchema.Constants.Prefix));
		}

		public void TestLicenceDatabaseParents()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db3 = Factory.NewWithValidTestData<LicenceDatabase>();
			parent.Databases.Add(db1);
			parent.Databases.Add(db2);
			var otherEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			otherEnterprise.Databases.Add(db3);

			AssertEquals("Precondition", parent.PK, db1.LD_LE);
			AssertEquals("Precondition", parent.PK, db2.LD_LE);
			AssertEquals("Precondition", otherEnterprise.PK, db3.LD_LE);
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.EnterpriseParent = parent;
			var lookups = new EdiUserAgreementAssignmentLookups(assignment1);
			AssertEquals(2, lookups.LicenceDatabaseParents.Count);
			AssertEquals(true, lookups.LicenceDatabaseParents.Contains(db1));
			AssertEquals(true, lookups.LicenceDatabaseParents.Contains(db2));
			AssertEquals(false, lookups.LicenceDatabaseParents.Contains(db3));

			assignment1.EnterpriseParent = otherEnterprise;
			AssertEquals(1, lookups.LicenceDatabaseParents.Count);
			AssertEquals(false, lookups.LicenceDatabaseParents.Contains(db1));
			AssertEquals(false, lookups.LicenceDatabaseParents.Contains(db2));
			AssertEquals(true, lookups.LicenceDatabaseParents.Contains(db3));

			assignment1.EnterpriseParent = null;
			AssertEquals(3, lookups.LicenceDatabaseParents.Count);
			AssertEquals(true, lookups.LicenceDatabaseParents.Contains(db1));
			AssertEquals(true, lookups.LicenceDatabaseParents.Contains(db2));
			AssertEquals(true, lookups.LicenceDatabaseParents.Contains(db3));
		}

		public void TestLicenceDatabaseParents_ShouldOnlyIncludeEnterpriseProducts()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db3 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db4 = Factory.NewWithValidTestData<LicenceDatabase>();
			parent.Databases.Add(db1);
			parent.Databases.Add(db2);
			parent.Databases.Add(db3);
			parent.Databases.Add(db4);

			AssertEquals("Precondition", parent.PK, db1.LD_LE);
			AssertEquals("Precondition", parent.PK, db2.LD_LE);
			AssertEquals("Precondition", parent.PK, db3.LD_LE);
			AssertEquals("Precondition", parent.PK, db4.LD_LE);

			db1.LD_Product = ProductTypes.Codes.CargoWiseOne;
			db2.LD_Product = ProductTypes.Codes.Enterprise;
			db3.LD_Product = ProductTypes.Codes.CargoWiseNext;
			db4.LD_Product = ProductTypes.Codes.CargoSphere;
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EnterpriseParent = parent;
			var lookups = new EdiUserAgreementAssignmentLookups(assignment1);
			AssertEquals("CWN agreements should only allow particular products", 3, lookups.LicenceDatabaseParents.Count);
			AssertEquals("CWN agreements should only allow particular products", true, lookups.LicenceDatabaseParents.Contains(db1));
			AssertEquals("CWN agreements should only allow particular products", true, lookups.LicenceDatabaseParents.Contains(db2));
			AssertEquals("CWN agreements should only allow particular products", true, lookups.LicenceDatabaseParents.Contains(db3));
			AssertEquals("CWN agreements should only allow particular products", false, lookups.LicenceDatabaseParents.Contains(db4));

			assignment1.EAE_AgreementType = "ZZZ";
			AssertEquals("Other agreements allow any product", 4, lookups.LicenceDatabaseParents.Count);
			AssertEquals("Other agreements allow any product", true, lookups.LicenceDatabaseParents.Contains(db1));
			AssertEquals("Other agreements allow any product", true, lookups.LicenceDatabaseParents.Contains(db2));
			AssertEquals("Other agreements allow any product", true, lookups.LicenceDatabaseParents.Contains(db3));
			AssertEquals("Other agreements allow any product", true, lookups.LicenceDatabaseParents.Contains(db4));
		}
	}
}
