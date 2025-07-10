using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	[TestedType(typeof(EdiUserAgreementAssignmentCollection))]
	public class EdiUserAgreementAssignmentCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiUserAgreementAssignmentCollection>
	{
		public void TestAttachToParentAfterAdding()
		{
			var collection = GetCollectionToTest();

			var assignment1 = collection.AddNew();
			AssertEquals(Parent.PK, assignment1.EAE_ParentID);
			AssertEquals(Parent.TablePrefix, assignment1.EAE_ParentTableCode);
			AssertEquals(Parent, assignment1.Parent);
			Factory.Save();

			var assignmentFromAnotherSession = (new BusinessObjectFactory() { RefreshEnabled = false }).Load<EdiUserAgreementAssignment>(assignment1.PK);
			AssertEquals(Parent.PK, assignmentFromAnotherSession.EAE_ParentID);
			AssertEquals(Parent.TablePrefix, assignmentFromAnotherSession.EAE_ParentTableCode);
		}

		public void TestEnterpriseParent_ShouldIncludeDatabaseAssignments()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			Parent.Databases.Add(db1);
			Parent.Databases.Add(db2);

			AssertEquals("Precondition", Parent.PK, db1.LD_LE);
			AssertEquals("Precondition", Parent.PK, db2.LD_LE);
			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = db1;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = db2;
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals(2, collection.Count);
			var collectionAssignment1 = collection.FirstOrDefault(x => x.EAE_ParentID == db1.PK);
			AssertEquals(assignment1, collectionAssignment1);
			AssertEquals(LicenceDatabaseSchema.Constants.Prefix, collectionAssignment1.EAE_ParentTableCode);
			var collectionAssignment2 = collection.FirstOrDefault(x => x.EAE_ParentID == db2.PK);
			AssertEquals(assignment2, collectionAssignment2);
			AssertEquals(LicenceDatabaseSchema.Constants.Prefix, collectionAssignment2.EAE_ParentTableCode);
		}

		public void TestNewlyAddedAssignmentsShouldDefaultParentWhenParentTableCodeSetToLE()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			Parent.Databases.Add(db1);
			Parent.Databases.Add(db2);

			AssertEquals("Precondition", Parent.PK, db1.LD_LE);
			AssertEquals("Precondition", Parent.PK, db2.LD_LE);
			Factory.Save();

			var collection = GetCollectionToTest();
			var assignment1 = collection.AddNew();

			AssertEquals("Precondition: Should be set on add to collection", LicenceEnterpriseSchema.Constants.Prefix, assignment1.EAE_ParentTableCode);
			AssertEquals("Precondition: Should be set on add to collection", Parent.PK, assignment1.EAE_ParentID);

			assignment1.Parent = db1;
			AssertEquals("Should be set to db", LicenceDatabaseSchema.Constants.Prefix, assignment1.EAE_ParentTableCode);
			AssertEquals("Should be set to db", db1.PK, assignment1.EAE_ParentID);

			assignment1.EAE_ParentTableCode = LicenceEnterpriseSchema.Constants.Prefix;
			AssertEquals("Should default back to collection parent", Parent.PK, assignment1.EAE_ParentID);
		}

		public void TestAddToCollectionShouldIncludeUnsavedAssignments()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			Parent.Databases.Add(db1);
			Parent.Databases.Add(db2);

			AssertEquals("Precondition", Parent.PK, db1.LD_LE);
			AssertEquals("Precondition", Parent.PK, db2.LD_LE);
			Factory.Save();

			var collection = GetCollectionToTest();
			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = db1;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = db2;

			AssertEquals("Should include unsaved assignments", 2, collection.Count);
			AssertEquals("Should include unsaved assignment", true, collection.Contains(assignment1));
			AssertEquals("Should include unsaved assignment", true, collection.Contains(assignment2));
		}

		public void TestCollectionShouldIncludeAssignmentsWithNoParent()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			Parent.Databases.Add(db1);
			Parent.Databases.Add(db2);

			AssertEquals("Precondition", Parent.PK, db1.LD_LE);
			AssertEquals("Precondition", Parent.PK, db2.LD_LE);
			Factory.Save();

			var collection = GetCollectionToTest();
			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.EAE_ParentTableCode = LicenceDatabaseSchema.Constants.Prefix;
			assignment1.EAE_ParentID = ZGuid.Empty;

			AssertEquals("Should include assignments with no parent", 1, collection.Count);
			AssertEquals("Should include assignment with no parent", true, collection.Contains(assignment1));
		}

		public void TestCollectionShouldIncludeAssignmentsWithMissingParent()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			Parent.Databases.Add(db1);
			Parent.Databases.Add(db2);

			AssertEquals("Precondition", Parent.PK, db1.LD_LE);
			AssertEquals("Precondition", Parent.PK, db2.LD_LE);
			Factory.Save();

			var collection = GetCollectionToTest();
			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.EAE_ParentTableCode = LicenceDatabaseSchema.Constants.Prefix;
			assignment1.EAE_ParentID = ZGuid.Missing;

			AssertEquals("Should include assignments with missing parent since this case can happen for unsaved row", 1, collection.Count);
			AssertEquals("Should include assignment with missing parent since this case can happen for unsaved row", true, collection.Contains(assignment1));
		}

		public void TestLoadedAssignmentsShouldDefaultParentWhenParentTableCodeSetToLE()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			Parent.Databases.Add(db1);
			Parent.Databases.Add(db2);

			AssertEquals("Precondition", Parent.PK, db1.LD_LE);
			AssertEquals("Precondition", Parent.PK, db2.LD_LE);
			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = db1;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = db2;
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals(2, collection.Count);
			var collectionAssignment1 = collection.FirstOrDefault(x => x.EAE_ParentID == db1.PK);
			AssertEquals("Precondition", assignment1, collectionAssignment1);
			AssertEquals("Precondition", LicenceDatabaseSchema.Constants.Prefix, collectionAssignment1.EAE_ParentTableCode);

			collectionAssignment1.EAE_ParentTableCode = LicenceEnterpriseSchema.Constants.Prefix;
			AssertEquals("Should default the parent back to the LicenceEnterprise", Parent.PK, collectionAssignment1.EAE_ParentID);

			collectionAssignment1.EAE_ParentTableCode = LicenceDatabaseSchema.Constants.Prefix;
			collectionAssignment1.EAE_ParentID = db1.PK;
			AssertEquals("Should allow the change", db1.PK, collectionAssignment1.EAE_ParentID);

			collectionAssignment1.Parent = db2;
			AssertEquals("Should allow the change", db2.PK, collectionAssignment1.EAE_ParentID);
			AssertEquals("Should allow the change", LicenceDatabaseSchema.Constants.Prefix, collectionAssignment1.EAE_ParentTableCode);
		}

		public void TestNewlyAddedAssignmentsShouldSetEnterpriseParent()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			Parent.Databases.Add(db1);
			Parent.Databases.Add(db2);

			AssertEquals("Precondition", Parent.PK, db1.LD_LE);
			AssertEquals("Precondition", Parent.PK, db2.LD_LE);
			Factory.Save();

			var collection = GetCollectionToTest();
			var assignment1 = collection.AddNew();

			AssertEquals("Should be set on add to collection", Parent, assignment1.EnterpriseParent);
		}

		public void TestLoadedAssignmentsShouldSetEnterpriseParent()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			Parent.Databases.Add(db1);
			Parent.Databases.Add(db2);

			AssertEquals("Precondition", Parent.PK, db1.LD_LE);
			AssertEquals("Precondition", Parent.PK, db2.LD_LE);
			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = db1;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = db2;
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals(2, collection.Count);
			var collectionAssignment1 = collection.FirstOrDefault(x => x.EAE_ParentID == db1.PK);
			AssertEquals("Should be set on load into collection", Parent, collectionAssignment1.EnterpriseParent);
		}

		protected override EdiUserAgreementAssignmentCollection GetCollectionToTest()
		{
			return new EdiUserAgreementAssignmentCollection(Parent);
		}

		LicenceEnterprise Parent;

		protected override void SetUp()
		{
			base.SetUp();
			Parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			Factory.Save();
		}
	}
}
