using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

#pragma warning disable SA1312        // Variable names should begin with lower-case letter
#pragma warning disable SA1313        // Parameter names should begin with lower-case letter

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(RemoveCA_AccountingAgeFromGenAddOnColumn))]
	public sealed class RemoveCA_AccountingAgeFromGenAddOnColumnTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RemoveCA_AccountingAgeFromGenAddOnColumn();

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.GenAddOnColumn");
			XA_PK1 = CreateGenAddOnColumn("CA_AccountingAge", "1", "JE");
			XA_PK2 = CreateGenAddOnColumn("CA_AccountingAge", "9", "XX");
			XA_PK3 = CreateGenAddOnColumn("CA_EstimatedPaymentDueDate", "5", "JE");
			XA_PK4 = CreateGenAddOnColumn("CA_AccountingAge", "8", "JE");
			XA_PK5 = CreateGenAddOnColumn("US_AccountingAge", "1", "JE");
		}

		protected override void AssertTransformationResults()
		{
			var dt = new DataTable("GenAddOnColumn");
			using (var reader = Db.Connection.Command("SELECT * FROM dbo.GenAddOnColumn").ExecuteReader())
			{
				dt.Load(reader);
			}
			AssertEquals(3, dt.Rows.Count);
			AssertEquals(0, dt.Select($"XA_PK = '{XA_PK1}'").Length);
			AssertEquals(1, dt.Select($"XA_PK = '{XA_PK2}'").Length);
			AssertEquals(1, dt.Select($"XA_PK = '{XA_PK3}'").Length);
			AssertEquals(0, dt.Select($"XA_PK = '{XA_PK4}'").Length);
			AssertEquals(1, dt.Select($"XA_PK = '{XA_PK5}'").Length);
		}

		Guid CreateGenAddOnColumn(string XA_Name, string XA_Data, string XA_ParentTableCode)
			=> data.CreateGenAddOnColumn(XA_Name, XA_Data, XA_ParentTableCode, Guid.NewGuid());

		protected override void SetUp()
		{
			base.SetUp();
			data = new TransformationTestDataCreator();
			var GC_PK = data.CreateCompany(Guid.NewGuid(), "CP1", "CA", "CAD");
			data.CreateBranch("BR1", "001", GC_PK);
		}

		TransformationTestDataCreator data;
		Guid XA_PK1, XA_PK2, XA_PK3, XA_PK4, XA_PK5;
	}
}
