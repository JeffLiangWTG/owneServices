using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

#pragma warning disable SA1312        // Variable names should begin with lower-case letter
#pragma warning disable SA1313        // Parameter names should begin with lower-case letter

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(RemoveDeclarationException060))]
	public sealed class RemoveDeclarationException060Test : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RemoveDeclarationException060(batchSize: 1);

		protected override void PrepareTestData()
		{
			Guid CH_PK, CL_PK;

			/* Create IMP declaration with exception code 060 and no discrepancy in duty/tax between cw1 and customs */
			JE_PK1 = CreateJobDeclaration("IMP", "WTG=Y*DeclarationException=060*ServiceOption=IID");
			CH_PK = CreateCusEntryHeader(JE_PK1);
			CL_PK = CreateCusEntryLine(CH_PK);
			CreateCusEntryLineFee(100.5m, "GSD", CL_PK, "CW1");
			CreateCusEntryLineFee(100.5m, "GST", CL_PK, "CUS");
			CreateCusEntryLineFee(200.5m, "SIM", CL_PK, "CW1");
			CreateCusEntryLineFee(200.5m, "CVD", CL_PK, "CUS");
			CreateCusEntryLineFee(300.5m, "DTY", CL_PK, "CW1");
			CreateCusEntryLineFee(300.5m, "CUD", CL_PK, "CUS");
			CreateCusEntryLineFee(601.5m, "TOT", CL_PK, "CUS");
			CreateGenAddOnColumn("CA_DeclarationException", "060", JE_PK1);

			/* Create IMP declaration with exception code 060 and discrepancy in duty/tax between cw1 and customs */
			JE_PK2 = CreateJobDeclaration("IMP", "WTG=Y*DeclarationException=060*ServiceOption=IID");
			CH_PK = CreateCusEntryHeader(JE_PK2);
			CL_PK = CreateCusEntryLine(CH_PK);
			CreateCusEntryLineFee(100.0m, "GSD", CL_PK, "CW1");
			CreateCusEntryLineFee(100.5m, "GST", CL_PK, "CUS");
			CreateCusEntryLineFee(200.5m, "SIM", CL_PK, "CW1");
			CreateCusEntryLineFee(200.5m, "CVD", CL_PK, "CUS");
			CreateCusEntryLineFee(300.5m, "DTY", CL_PK, "CW1");
			CreateCusEntryLineFee(300.5m, "CUD", CL_PK, "CUS");
			CreateCusEntryLineFee(601.0m, "TOT", CL_PK, "CUS");
			CreateGenAddOnColumn("CA_DeclarationException", "060", JE_PK2);

			/* Create LVS declaration with exception code 060 and no discrepancy in duty/tax between cw1 and customs */
			JE_PK3 = CreateJobDeclaration("LVS", "WTG=Y*DeclarationException=060*ServiceOption=IID");
			CH_PK = CreateCusEntryHeader(JE_PK3);
			CL_PK = CreateCusEntryLine(CH_PK);
			CreateCusEntryLineFee(100.5m, "GSD", CL_PK, "CW1");
			CreateCusEntryLineFee(100.5m, "GST", CL_PK, "CUS");
			CreateCusEntryLineFee(200.5m, "SIM", CL_PK, "CW1");
			CreateCusEntryLineFee(200.5m, "CVD", CL_PK, "CUS");
			CreateCusEntryLineFee(300.5m, "DTY", CL_PK, "CW1");
			CreateCusEntryLineFee(300.5m, "CUD", CL_PK, "CUS");
			CreateCusEntryLineFee(601.5m, "TOT", CL_PK, "CUS");
			CreateGenAddOnColumn("CA_DeclarationException", "060", JE_PK3);

			/* Create LVS declaration with exception code 060 and discrepancy in duty/tax between cw1 and customs */
			JE_PK4 = CreateJobDeclaration("LVS", "WTG=Y*DeclarationException=060*ServiceOption=IID");
			CH_PK = CreateCusEntryHeader(JE_PK4);
			CL_PK = CreateCusEntryLine(CH_PK);
			CreateCusEntryLineFee(100.0m, "GSD", CL_PK, "CW1");
			CreateCusEntryLineFee(100.5m, "GST", CL_PK, "CUS");
			CreateCusEntryLineFee(200.5m, "SIM", CL_PK, "CW1");
			CreateCusEntryLineFee(200.5m, "CVD", CL_PK, "CUS");
			CreateCusEntryLineFee(300.5m, "DTY", CL_PK, "CW1");
			CreateCusEntryLineFee(300.5m, "CUD", CL_PK, "CUS");
			CreateCusEntryLineFee(601.0m, "TOT", CL_PK, "CUS");
			CreateGenAddOnColumn("CA_DeclarationException", "060", JE_PK4);

			/* Create IMP declaration with more than one entry header and exception code 060 and no discrepancy in duty/tax between cw1 and customs */
			JE_PK5 = CreateJobDeclaration("IMP", "WTG=Y*DeclarationException=060*ServiceOption=IID");
			CH_PK = CreateCusEntryHeader(JE_PK5, "REL");
			CL_PK = CreateCusEntryLine(CH_PK);
			CreateCusEntryLineFee(100.5m, "GSD", CL_PK, "CW1");
			CH_PK = CreateCusEntryHeader(JE_PK5);
			CL_PK = CreateCusEntryLine(CH_PK);
			CreateCusEntryLineFee(100.5m, "GSD", CL_PK, "CW1");
			CreateCusEntryLineFee(100.5m, "GST", CL_PK, "CUS");
			CreateCusEntryLineFee(200.5m, "SIM", CL_PK, "CW1");
			CreateCusEntryLineFee(200.5m, "CVD", CL_PK, "CUS");
			CreateCusEntryLineFee(300.5m, "DTY", CL_PK, "CW1");
			CreateCusEntryLineFee(300.5m, "CUD", CL_PK, "CUS");
			CreateCusEntryLineFee(601.5m, "TOT", CL_PK, "CUS");
			CreateGenAddOnColumn("CA_DeclarationException", "060", JE_PK5);
		}

		protected override void AssertTransformationResults()
		{
			var dt = new DataTable("JobDeclaration");
			dt.Load(Db.Connection.Command("SELECT JE_PK, JE_AddInfo, JE_ClusterKey, JE_SystemCreateTimeUtc FROM dbo.JobDeclaration").ExecuteReader());
			AssertEquals(1, dt.Select($"JE_PK = '{JE_PK1}' AND JE_AddInfo = 'WTG=Y*ServiceOption=IID'").Length);
			AssertEquals(1, dt.Select($"JE_PK = '{JE_PK2}' AND JE_AddInfo = 'WTG=Y*DeclarationException=060*ServiceOption=IID'").Length);
			AssertEquals(1, dt.Select($"JE_PK = '{JE_PK3}' AND JE_AddInfo = 'WTG=Y*ServiceOption=IID'").Length);
			AssertEquals(1, dt.Select($"JE_PK = '{JE_PK4}' AND JE_AddInfo = 'WTG=Y*DeclarationException=060*ServiceOption=IID'").Length);
			AssertEquals(1, dt.Select($"JE_PK = '{JE_PK5}' AND JE_AddInfo = 'WTG=Y*ServiceOption=IID'").Length);

			dt = new DataTable("GenAddOnColumn");
			dt.Load(Db.Connection.Command("SELECT XA_ParentID, XA_Name FROM dbo.GenAddOnColumn").ExecuteReader());
			AssertEquals(0, dt.Select($"XA_ParentID = '{JE_PK1}' AND XA_Name = 'CA_DeclarationException'").Length);
			AssertEquals(1, dt.Select($"XA_ParentID = '{JE_PK2}' AND XA_Name = 'CA_DeclarationException'").Length);
			AssertEquals(0, dt.Select($"XA_ParentID = '{JE_PK3}' AND XA_Name = 'CA_DeclarationException'").Length);
			AssertEquals(1, dt.Select($"XA_ParentID = '{JE_PK4}' AND XA_Name = 'CA_DeclarationException'").Length);
			AssertEquals(0, dt.Select($"XA_ParentID = '{JE_PK5}' AND XA_Name = 'CA_DeclarationException'").Length);
		}

		public void TestOnlinePostUpgradeVerbosity()
		{
			PrepareTestData();
			var expected = new List<string>
			{
				"Processing batch with cluster key ranging from 0 to 2..."
				, "\t2 records processed."
				, "Processing batch with cluster key ranging from 2 to 4..."
				, "\t2 records processed."
				, "Processing batch with cluster key ranging from 4 to 5..."
				, "\t2 records processed."
				, "\tCompleted: Remove declaration exception code 060 unless there is a discrepancy in duty/tax"
			};
			var actual = new List<string>();
			var transformation = new RemoveDeclarationException060(batchSize: 2);
			((IOnlineTransformation)transformation).Run(actual.Add, CancellationToken.None);
			AssertContainsExactElementsInExactOrder(expected, actual);
		}

		Guid CreateJobDeclaration(string JE_MessageType, string JE_AddInfo)
		{
			var JE_PK = Guid.NewGuid();
			clusterKey++;
			data.CreateDeclaration(JE_PK, $"B{clusterKey:00000000}", clusterKey, GB_PK, GC_PK, addInfo: JE_AddInfo, dataModel: "CA", messageType: JE_MessageType);
			return JE_PK;
		}

		Guid CreateCusEntryHeader(Guid JE_PK, string CH_MessageType = "CAD")
			=> data.CreateCusEntryHeader("CA", JE_PK, clusterKey, CH_MessageType);

		Guid CreateCusEntryLine(Guid CL_CH)
			=> data.CreateCusEntryLine(CL_CH, clusterKey, "CA");

		Guid CreateCusEntryLineFee(decimal CF_ChargeAmount, string CF_ChargeType, Guid CF_CL, string CF_Source)
			=> data.CreateCusEntryLineFee(CF_ChargeAmount, CF_ChargeType, CF_CL, clusterKey, CF_Source);

		Guid CreateGenAddOnColumn(string XA_Name, string XA_Data, Guid JE_PK)
			=> data.CreateGenAddOnColumn(XA_Name, XA_Data, "JE", JE_PK);

		protected override void SetUp()
		{
			base.SetUp();
			data = new TransformationTestDataCreator();
			GC_PK = data.CreateCompany(Guid.NewGuid(), "CP1", "CA", "CAD");
			GB_PK = data.CreateBranch("BR1", "001", GC_PK);
		}

		TransformationTestDataCreator data;
		Guid GC_PK, GB_PK, JE_PK1, JE_PK2, JE_PK3, JE_PK4, JE_PK5;
		int clusterKey;
	}
}
