using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	class JCDTableAndPartitionCreatorTest : TestCaseWithFactory
	{
		public void TestTablesAndPartitionsAreCreated()
		{
			//Creating Object for the first time
			AssertNoExceptionThrown(() => JCDTableAndPartitionCreator.Create(TestConnection, 0, (s) => { }));

			CreateTablesAndPartitionsAndAssertResult();
		}

		public void TestDeleteMainReportTableAndPartition()
		{
			CreateTablesAndPartitionsAndAssertResult();

			JCDTableAndPartitionCreator.DeleteMainReportTableAndPartition(TestConnection, (s) => { });

			#region RptDtUnprocessedAccTransactionLines

			//Assert RptDtUnprocessedAccTransactionLines table is not removed
			var tableExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.objects WHERE name='RptDtUnprocessedAccTransactionLines'") > 0;
			AssertEquals("RptDtUnprocessedAccTransactionLines should Exist", true, tableExists);

			#endregion

			#region RptDtUnprocessedReversedAL

			//Assert RptDtUnprocessedReversedAL table is not removed
			tableExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.objects WHERE name='RptDtUnprocessedReversedAL'") > 0;
			AssertEquals("RptDtUnprocessedReversedAL should Exist", true, tableExists);

			#endregion

			#region Partition

			//Assert Partition Scheme is removed
			var psExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.partition_schemes ps where NAME = 'PS_AccountingPeriodCompany'") > 0;
			AssertEquals("PS_AccountingPeriodCompany scheme should Exist", false, psExists);

			#endregion

			#region RptDtJobCostingData

			//Assert RptDtJobCostingData table is removed
			tableExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.objects WHERE name='RptDtJobCostingData'") > 0;
			AssertEquals("RptDtJobCostingData should Exist", false, tableExists);

			#endregion
		}

		public void TestDeleteTempTableAndIndex()
		{
			CreateTablesAndPartitionsAndAssertResult();

			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });

			#region RptDtUnprocessedAccTransactionLines

			//Assert RptDtUnprocessedAccTransactionLines table is removed
			var tableExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.objects WHERE name='RptDtUnprocessedAccTransactionLines'") > 0;
			AssertEquals("RptDtUnprocessedAccTransactionLines should Exist", false, tableExists);

			#endregion

			#region RptDtUnprocessedReversedAL

			//Assert RptDtUnprocessedReversedAL table is removed
			tableExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.objects WHERE name='RptDtUnprocessedReversedAL'") > 0;
			AssertEquals("RptDtUnprocessedReversedAL should Exist", false, tableExists);

			#endregion

			#region Partition

			//Assert Partition Scheme is not removed
			var psExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.partition_schemes ps where NAME = 'PS_AccountingPeriodCompany'") > 0;
			AssertEquals("PS_AccountingPeriodCompany scheme should Exist", true, psExists);

			#endregion

			#region RptDtJobCostingData

			//Assert RptDtJobCostingData table is not removed
			tableExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.objects WHERE name='RptDtJobCostingData'") > 0;
			AssertEquals("RptDtJobCostingData should Exist", true, tableExists);

			#endregion
		}

		public void TestColumnDataTypesAreInSync()
		{
			//JobCostingData table pulls data from dbo.AccTransactionLines, AccTransactionHeader and JobHeader Tables.
			//If any of the following column gets changed, please update the corresponding columns in JobCostingData table 
			//otherwise a Trigger on AccTransactionLines table and Job Costing Data Queue service task will throw error.

			AssertEquals("JCD_PK", SqlDbType.UniqueIdentifier, AccTransactionLinesSchema.PK.SqlDbType);
			AssertEquals("JCD_AH", SqlDbType.UniqueIdentifier, AccTransactionLinesSchema.AL_AH.SqlDbType);
			AssertEquals("JCD_JH", SqlDbType.UniqueIdentifier, AccTransactionLinesSchema.AL_JH.SqlDbType);
			AssertEquals("JCD_AC", SqlDbType.UniqueIdentifier, AccTransactionLinesSchema.AL_AC.SqlDbType);
			AssertEquals("JCD_AG", SqlDbType.UniqueIdentifier, AccTransactionLinesSchema.AL_AG.SqlDbType);
			AssertEquals("JCD_OH", SqlDbType.UniqueIdentifier, AccTransactionLinesSchema.AL_OH.SqlDbType);
			AssertEquals("JCD_GE", SqlDbType.UniqueIdentifier, AccTransactionLinesSchema.AL_GE.SqlDbType);
			AssertEquals("JCD_GB", SqlDbType.UniqueIdentifier, AccTransactionLinesSchema.AL_GB.SqlDbType);
			AssertEquals("JCD_GC", SqlDbType.UniqueIdentifier, AccTransactionLinesSchema.AL_GC.SqlDbType);

			AssertEquals("JCD_LineType", SqlDbType.VarChar, AccTransactionLinesSchema.AL_LineType.SqlDbType);
			AssertEquals("JCD_LineType : Length", 3, AccTransactionLinesSchema.AL_LineType.MaxLength);

			AssertEquals("JCD_Desc", SqlDbType.NVarChar, AccTransactionLinesSchema.AL_Desc.SqlDbType);
			AssertEquals("JCD_Desc : Length", 1024, AccTransactionLinesSchema.AL_Desc.MaxLength);

			AssertEquals("JCD_PostDate", SqlDbType.SmallDateTime, AccTransactionLinesSchema.AL_PostDate.SqlDbType);

			AssertEquals("JCD_RevRecognitionType", SqlDbType.VarChar, AccTransactionLinesSchema.AL_RevRecognitionType.SqlDbType);
			AssertEquals("JCD_Desc : Length", 3, AccTransactionLinesSchema.AL_RevRecognitionType.MaxLength);

			AssertEquals("JCD_LineAmount", SqlDbType.Money, AccTransactionLinesSchema.AL_LineAmount.SqlDbType);
			AssertEquals("JCD_LineAmount : Precision", (byte)19, AccTransactionLinesSchema.AL_LineAmount.Precision);
			AssertEquals("JCD_LineAmount : Scale", (byte)4, AccTransactionLinesSchema.AL_LineAmount.Scale);

			AssertEquals("JCD_GSTVAT", SqlDbType.Money, AccTransactionLinesSchema.AL_GSTVAT.SqlDbType);
			AssertEquals("JCD_GSTVAT : Precision", (byte)19, AccTransactionLinesSchema.AL_GSTVAT.Precision);
			AssertEquals("JCD_GSTVAT : Scale", (byte)4, AccTransactionLinesSchema.AL_GSTVAT.Scale);

			AssertEquals("JCD_RX_NKLocalCurrency", SqlDbType.VarChar, GlbCompanySchema.GC_RX_NKLocalCurrency.SqlDbType);
			AssertEquals("JCD_RX_NKLocalCurrency : Length", 3, GlbCompanySchema.GC_RX_NKLocalCurrency.MaxLength);

			AssertEquals("JCD_OSAmount", SqlDbType.Money, AccTransactionLinesSchema.AL_OSAmount.SqlDbType);
			AssertEquals("JCD_OSAmount : Precision", (byte)19, AccTransactionLinesSchema.AL_OSAmount.Precision);
			AssertEquals("JCD_OSAmount : Scale", (byte)4, AccTransactionLinesSchema.AL_OSAmount.Scale);

			AssertEquals("JCD_RX_NKCurrency", SqlDbType.VarChar, AccTransactionLinesSchema.AL_RX_NKTransactionCurrency.SqlDbType);
			AssertEquals("JCD_RX_NKCurrency : Length", 3, AccTransactionLinesSchema.AL_RX_NKTransactionCurrency.MaxLength);

			AssertEquals("JCD_TransactionNum", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_TransactionNum.SqlDbType);
			AssertEquals("JCD_TransactionNum : Length", 38, AccTransactionHeaderSchema.AH_TransactionNum.MaxLength);

			AssertEquals("JCD_TransactionType", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_TransactionType.SqlDbType);
			AssertEquals("JCD_TransactionType : Length", 3, AccTransactionHeaderSchema.AH_TransactionType.MaxLength);

			AssertEquals("JCD_Ledger", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_Ledger.SqlDbType);
			AssertEquals("JCD_Ledger : Length", 2, AccTransactionHeaderSchema.AH_Ledger.MaxLength);

			AssertEquals("JCD_ParentID", SqlDbType.UniqueIdentifier, JobHeaderSchema.JH_ParentID.SqlDbType);

			AssertEquals("JCD_ParentTableCode", SqlDbType.VarChar, JobHeaderSchema.JH_ParentTableCode.SqlDbType);
			AssertEquals("JCD_ParentTableCode : Length", 3, JobHeaderSchema.JH_ParentTableCode.MaxLength);
		}

		void CreateTablesAndPartitionsAndAssertResult()
		{
			JCDTableAndPartitionCreator.Create(TestConnection, 0, (s) => { });

			#region RptDtUnprocessedAccTransactionLines

			//Assert RptDtUnprocessedAccTransactionLines table is created
			var tableExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.objects WHERE name='RptDtUnprocessedAccTransactionLines'") > 0;
			AssertEquals("RptDtUnprocessedAccTransactionLines should Exist", true, tableExists);

			//Assert Index is created on RptDtUnprocessedAccTransactionLines table
			var indexExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.indexes WHERE name='NR_RC__Clustered_UL_RowNumber' AND object_id = OBJECT_ID('RptDtUnprocessedAccTransactionLines')") > 0;
			AssertEquals("NR_RC__Clustered_UL_RowNumber should Exist", true, indexExists);

			#endregion

			#region RptDtUnprocessedReversedAL

			//Assert RptDtUnprocessedReversedAL table is created
			tableExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.objects WHERE name='RptDtUnprocessedReversedAL'") > 0;
			AssertEquals("RptDtUnprocessedReversedAL should Exist", true, tableExists);

			//Assert Index is created on RptDtUnprocessedReversedAL table
			indexExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.indexes WHERE name='FK_UC__Clustered_URL_ALPK' AND object_id = OBJECT_ID('RptDtUnprocessedReversedAL')") > 0;
			AssertEquals("FK_UC__Clustered_URL_ALPK should Exist", true, indexExists);

			#endregion

			#region Partition

			//Assert Partition Scheme is created
			var psExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.partition_schemes ps where NAME = 'PS_AccountingPeriodCompany'") > 0;
			AssertEquals("PS_AccountingPeriodCompany scheme should Exist", true, psExists);

			#endregion

			#region RptDtJobCostingData

			//Assert RptDtJobCostingData table is created
			tableExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.objects WHERE name='RptDtJobCostingData'") > 0;
			AssertEquals("RptDtJobCostingData should Exist", true, tableExists);

			//Assert Check Constraints are Created and Trusted
			var dt = DataUtils.GetDataTableFromQuery(Helper.Connection, @"SELECT	[name] as ConstraintName, 
																							is_disabled, 
																							is_not_trusted 
																					FROM	sys.check_constraints 
																					WHERE	[name] in ('Constraint_JCD_Ledger', 'Constraint_JCD_LineType', 'Constraint_JCD_ParentTableCode', 'Constraint_JCD_PeriodCompanyKey', 'Constraint_JCD_PostPeriod', 'Constraint_JCD_TransactionType')  
																					ORDER BY ConstraintName");

			AssertNotNull("Constraint Should Exist", dt);

			var expectedConstraintsInfo = new List<string>() {
					"Constraint_JCD_Ledger-False-False",
					"Constraint_JCD_LineType-False-False",
					"Constraint_JCD_ParentTableCode-False-False",
					"Constraint_JCD_PostPeriod-False-False",
					"Constraint_JCD_TransactionType-False-False"
				};
			var constraintInfo = dt.Select().Select(r => string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", Convert.ToString(r["ConstraintName"]), Convert.ToString(r["is_disabled"]), Convert.ToString(r["is_not_trusted"])));
			AssertContainsExactElementsInAnyOrder("All Constraints Should be Enabled and Trusted", expectedConstraintsInfo, constraintInfo);

			//Assert Index is created on RptDtJobCostingData table
			indexExists = Helper.Connection.ExecuteScalar<int>("SELECT ISNULL(COUNT(*), -1) FROM sys.indexes WHERE name='CI_JCD_PostDate' AND object_id = OBJECT_ID('RptDtJobCostingData')") > 0;
			AssertEquals("CI_JCD_PostDate should Exist", true, indexExists);

			#endregion
		}

		protected override void SetUp()
		{
			JCDTableAndPartitionCreator.DeleteTempTableAndIndex(TestConnection, (s) => { });

			JCDTableAndPartitionCreator.DeleteMainReportTableAndPartition(TestConnection, (s) => { });
		}

		JobCostingReportDataTestHelper Helper
		{
			get { return helper ?? (helper = new JobCostingReportDataTestHelper(ObjectCreator)); }
		}
		JobCostingReportDataTestHelper helper;

		TestObjectCreator ObjectCreator
		{
			get { return objectCreator ?? (objectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator objectCreator;
	}
}
