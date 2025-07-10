using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(UpdateOldUserDefinedTPCReferenceType))]
	public class UpdateOldUserDefinedTPCReferenceTypeTest : RegistryDataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow(UpdateOldUserDefinedTPCReferenceType.RegistryItemName, "BIN", Encoding.Unicode.GetBytes(originalXml1));
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var client = new OrgHeader("CLIENT").AppendInsertAndReturnObject(sql);

			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var tpcReference1 = new WhsDocketReference(order1, "TPC", "tpc01").AppendInsertAndReturnObject(sql);
			var otherReference2 = new WhsDocketReference(order1, "001", "other02").AppendInsertAndReturnObject(sql);

			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O2").AppendInsertAndReturnObject(sql);
			var tpcReference2 = new WhsDocketReference(order2, "TPC", "tpc02").AppendInsertAndReturnObject(sql);

			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O3").AppendInsertAndReturnObject(sql);
			var otherReference1 = new WhsDocketReference(order3, "DUM", "other01").AppendInsertAndReturnObject(sql);

			var order4 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O4").AppendInsertAndReturnObject(sql);
			var otherReference3 = new WhsDocketReference(order4, "002", "other03").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			var referenceTypeBytes = Helper.GetStmDataValue(UpdateOldUserDefinedTPCReferenceType.RegistryItemName);
			AssertNotNull("Should have a not NULL binary value", referenceTypeBytes);
			var actualXml1 = Encoding.Unicode.GetString(referenceTypeBytes);
			AssertEquals("Should have updated registry item", expectedXml1, actualXml1);

			var previousTPCReference1 = WhsDocketReference.ShallowLoadFromDB(TestConnection, dr => dr.WX_Reference == "tpc01").Single();
			previousTPCReference1.BuildAssertion(TestConnection)
				.ExpectEquals("TPC reference 1 should have been changed to the new Type", dr => dr.WX_RefType, "003")
				.VerifyAll();

			var previousTPCReference2 = WhsDocketReference.ShallowLoadFromDB(TestConnection, dr => dr.WX_Reference == "tpc02").Single();
			previousTPCReference2.BuildAssertion(TestConnection)
				.ExpectEquals("TPC reference 2 should have been changed to the new Type", dr => dr.WX_RefType, "003")
				.VerifyAll();

			var previousOtherReference1 = WhsDocketReference.ShallowLoadFromDB(TestConnection, dr => dr.WX_Reference == "other01").Single();
			previousOtherReference1.BuildAssertion(TestConnection)
				.ExpectEquals("Other reference 1 should NOT be changed", dr => dr.WX_RefType, "DUM")
				.VerifyAll();

			var previousOtherReference2 = WhsDocketReference.ShallowLoadFromDB(TestConnection, dr => dr.WX_Reference == "other02").Single();
			previousOtherReference2.BuildAssertion(TestConnection)
				.ExpectEquals("Other reference 2 should NOT be changed", dr => dr.WX_RefType, "001")
				.VerifyAll();

			var previousOtherReference3 = WhsDocketReference.ShallowLoadFromDB(TestConnection, dr => dr.WX_Reference == "other03").Single();
			previousOtherReference3.BuildAssertion(TestConnection)
				.ExpectEquals("Other reference 3 should NOT be changed", dr => dr.WX_RefType, "002")
				.VerifyAll();
		}

		public void TestUpdateOldUserDefinedTPCReferenceType_NoTPCTypeExists()
		{
			Helper.InsertStmDataRow(UpdateOldUserDefinedTPCReferenceType.RegistryItemName, "BIN", Encoding.Unicode.GetBytes(originalXml2));
			GetNewTestTransformationInstance().Run();
			var referenceTypeBytes = Helper.GetStmDataValue(UpdateOldUserDefinedTPCReferenceType.RegistryItemName);
			AssertNotNull("Should have a not NULL binary value", referenceTypeBytes);
			AssertEquals("Should Not update registry item", originalXml2, Encoding.Unicode.GetString(referenceTypeBytes));
		}

		public void TestUpdateOldUserDefinedTPCReferenceType_NoExtraDefinedReference()
		{
			var referenceTypeBytesPrevious = Helper.GetStmDataValue(UpdateOldUserDefinedTPCReferenceType.RegistryItemName);
			AssertNull("Should Not have a reord", referenceTypeBytesPrevious);
			GetNewTestTransformationInstance().Run();
			var referenceTypeBytes = Helper.GetStmDataValue(UpdateOldUserDefinedTPCReferenceType.RegistryItemName);
			AssertNull("Should Not have a reord", referenceTypeBytes);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateOldUserDefinedTPCReferenceType();
		}

		const string originalXml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><SystemDefinableCodeDescriptionBoolCollection DefaultCode=\"OTH\"><SystemDefinableCodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>DUM</Code><Description>Dummy Code Description</Description></SystemDefinableCodeDescriptionBool><SystemDefinableCodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>TPC</Code><Description>Old TPC Type Description</Description></SystemDefinableCodeDescriptionBool><SystemDefinableCodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>001</Code><Description>Number1 Type Description</Description></SystemDefinableCodeDescriptionBool><SystemDefinableCodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>002</Code><Description>Number1 Type Description</Description></SystemDefinableCodeDescriptionBool></SystemDefinableCodeDescriptionBoolCollection>";

		const string expectedXml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><SystemDefinableCodeDescriptionBoolCollection DefaultCode=\"OTH\"><SystemDefinableCodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>DUM</Code><Description>Dummy Code Description</Description></SystemDefinableCodeDescriptionBool><SystemDefinableCodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>003</Code><Description>Old TPC Type Description</Description></SystemDefinableCodeDescriptionBool><SystemDefinableCodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>001</Code><Description>Number1 Type Description</Description></SystemDefinableCodeDescriptionBool><SystemDefinableCodeDescriptionBool><CodeMaxLength>3</CodeMaxLength><Code>002</Code><Description>Number1 Type Description</Description></SystemDefinableCodeDescriptionBool></SystemDefinableCodeDescriptionBoolCollection>";

		const string originalXml2 = expectedXml1;
	}
}
