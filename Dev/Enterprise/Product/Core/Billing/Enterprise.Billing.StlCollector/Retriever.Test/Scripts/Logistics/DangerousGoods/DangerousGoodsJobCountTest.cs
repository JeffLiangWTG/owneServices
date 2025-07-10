using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(DangerousGoodsJobCount))]
	sealed class DangerousGoodsJobCountTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @companyID UNIQUEIDENTIFIER = newid();

				DECLARE @jobPackLinePK01 UNIQUEIDENTIFIER = newid();
				DECLARE @jobContainerPK01 UNIQUEIDENTIFIER = newid();
				DECLARE @JPAFRBillsPK01 UNIQUEIDENTIFIER = newid();
				DECLARE @WhsUNDGLimitPK01 UNIQUEIDENTIFIER = newid();

				DECLARE @dataItemPK01 UNIQUEIDENTIFIER = newid();
				DECLARE @dataItemPK02 UNIQUEIDENTIFIER = newid();

				INSERT INTO dbo.GlbCompany
					(GC_PK, GC_Code, GC_Name)
				VALUES
					(@companyID, 'TG1', 'TG company');

				INSERT INTO dbo.GlbBranch
					(GB_PK, GB_Code, GB_GC)
				VALUES
					(newid(), 'TD1', @companyID);

				INSERT INTO dbo.UNDGSubstance 
					(DG_PK, DG_UNNO, DG_Variant, DG_Standard, DG_Class, DG_Code, DG_IsActive, DG_IsSystem, DG_Variation, DG_SubLabel1, DG_SubLabel2, DG_PSN, DG_PG, DG_EMS, DG_MP, DG_LQMaxAmt, DG_LQMaxAmtUQ, DG_TechName, DG_TreatAs, DG_DglPhrase, DG_PackIns, DG_PackProv, DG_IBCIns, DG_IBCProv, DG_IMOTankIns, DG_UNTankIns, DG_TankProv, DG_Markers, DG_Pointers, DG_EXVector, DG_CargoMaxAmt, DG_CargoMaxAmtUQ, DG_CargoPackAmtType, DG_CargoPackIns, DG_CodedStow, DG_Country, DG_EmergencyResponseGuide, DG_ExceptedQuantityCode, DG_ExpLim, DG_FlashPoint, DG_Hazards, DG_Identifier, DG_LQ2OrPaxMaxAmt, DG_LQ2OrPaxMaxAmtType, DG_LQ2OrPaxMaxAmtUQ, DG_LQMaxAmtType, DG_LQSpecProvIndex, DG_Mode, DG_SpecialHandlingCodes, DG_State, DG_StowCat, DG_UlineEMS, DG_UniqueRecordId, DG_UsrUSDOTShippingName, DG_IsNotOtherwiseSpecified, DG_PaxPackIns)
				VALUES
					(newid(), '0001', 'a', 'IMO', '1.1', '0001a',1,0, '0', '0', '0', '0', '0', '0', '0',0, '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0',0, '0', '0', 'NAP', '0', '0', '0', '0', '0', '0', '0',0, '0', '0', 'NAP', '0', 'NAP', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0'),
					(newid(), '0002', 'a', 'IMO', '7.1', '0002a',1,0, '0', '0', '0', '0', '0', '0', '0',0, '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0',0, '0', '0', 'NAP', '0', '0', '0', '0', '0', '0', '0',0, '0', '0', 'NAP', '0', 'NAP', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0'),
					(newid(), '0003', 'a', 'IMO', '1.1', '0003a',1,0, '0', '0', '0', '0', '0', '0', '0',0, '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0',0, '0', '0', 'NAP', '0', '0', '0', '0', '0', '0', '0',0, '0', '0', 'NAP', '0', 'NAP', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0'),
					(newid(), '0003', 'b', 'IMO', '2.1', '0003b',1,0, '0', '0', '0', '0', '0', '0', '0',0, '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0',0, '0', '0', 'NAP', '0', '0', '0', '0', '0', '0', '0',0, '0', '0', 'NAP', '0', 'NAP', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');

				INSERT INTO dbo.UNDGSubstancePivot
					(DP_PK, DP_UNNO, DP_Variant, DP_Standard, DP_ParentId, DP_ParentTableCode)
				VALUES
					(newid(), '0001', 'a', 'IMO', @dataItemPK01, 'DI'),
					(newid(), '0002', 'a', 'IMO', @dataItemPK02, 'DI'),
					(newid(), '0003', 'a', 'IMO', @JPAFRBillsPK01, 'JPB'),
					(newid(), '0003', 'b', 'IMO', @WhsUNDGLimitPK01, 'WWD');	
				
				INSERT INTO dbo.UNDGDataItem
					(DI_PK, DI_ParentID, DI_ParentTableCode)
				VALUES
					(@dataItemPK01, @jobPackLinePK01, 'JL'),
					(@dataItemPK02, @jobContainerPK01, 'JC');

				INSERT INTO dbo.StmALog
					(SL_PK, SL_Table, SL_Parent, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_GB_NKBranch)
				VALUES
					(newid(), 'JobPackLines', @jobPackLinePK01, 'DGC', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobContainer', @jobContainerPK01, 'DGC', '2014-10-01', '2014-10-01', 'US2', 'TD1'),
					(newid(), 'JPAFRBills', @JPAFRBillsPK01, 'DGC', '2014-10-01', '2014-10-01', 'US3', 'TD1'),
					(newid(), 'WhsUNDGLimit', @WhsUNDGLimitPK01, 'DGC', '2014-10-01', '2014-10-01', 'US4', 'TD1'),
					(newid(), 'ContainerLoadListLine', newid(), 'ADD', '2014-10-01', '2014-10-01', 'US5', 'TD1'),
					(newid(), 'JobDocumentData', newid(), 'ISN', '2014-10-01', '2014-10-01', 'US1', 'TD1'),
					(newid(), 'JobShipment', newid(), 'ADD', '2014-10-01', '2014-10-01', 'GB0', 'N');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "WhsUNDGLimit");
			AssertEquals("[T1] CompanyCode", "TG1", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "TD1", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 10, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US4", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertAdditionalRefs(transaction1, "2.1", "IMO", "0003", "b");

			var transaction2 = FindRowByRef1(transactions, "JPAFRBills");
			AssertEquals("[T2] CompanyCode", "TG1", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "TD1", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 10, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertAdditionalRefs(transaction2, "1.1", "IMO", "0003", "a");

			var transaction3 = FindRowByRef1(transactions, "JobContainer");
			AssertEquals("[T3] CompanyCode", "TG1", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "TD1", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 10, 1), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US2", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertAdditionalRefs(transaction3, "7.1", "IMO", "0002", "a");

			var transaction4 = FindRowByRef1(transactions, "JobPackLines");
			AssertEquals("[T4] CompanyCode", "TG1", transaction4.GetCompanyCode());
			AssertEquals("[T4] BranchCode", "TD1", transaction4.GetBranchCode());
			AssertEquals("[T4] TransactionDateUtc", new DateTime(2014, 10, 1), transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] UserCode", "US1", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
			AssertAdditionalRefs(transaction4, "1.1", "IMO", "0001", "a");
		}

		void AssertAdditionalRefs(IStlTransaction transaction, string expectedClass, string expectedStandard, string expectedUnno, string expectedVariant)
		{
			var expectedValues = new Dictionary<string, string>()
			{
				{ "DG Class" , expectedClass },
				{ "DP Standard", expectedStandard },
				{ "DP UNNO", expectedUnno },
				{ "DP Variant", expectedVariant }
			};
			
			var actualValues = transaction.AdditionalRefs
				.Trim('{', '}')
				.Split(',')
				.Select(part => part.Split(':'))
				.ToDictionary(split => split[0].Trim('"'), split => split[1].Trim('"'));

			foreach (var kv in expectedValues)
			{
				AssertEquals($"Additional Ref's value for key '{kv.Key}' does not match", kv.Value, actualValues[kv.Key]);
			}
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 10);
			}
		}
	}
}
