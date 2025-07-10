using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.ZA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA.ModelViews.ZACusEntryInstruction))]
	class ZACusEntryInstructionTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ZACusEntryInstruction",
				"CusEntryInstruction",
				new []
				{
					new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_BankCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_CargoCarrierOverride", VarChar, 35),
					new TestDbViewHelper.DbColumn("CEI_CreditTerms", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_CustomsOfficeOverride", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_EntityType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_ExchangeRateDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CEI_HAWBDateOverride", DateTime, -1),
					new TestDbViewHelper.DbColumn("CEI_HAWBOverride", VarChar, 35),
					new TestDbViewHelper.DbColumn("CEI_IsUCROverridden", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_MRNToBeReplaced", VarChar, 35),
					new TestDbViewHelper.DbColumn("CEI_PortOfExit", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_PreviousMRN", VarChar, 35),
					new TestDbViewHelper.DbColumn("CEI_ProvisionalPaymentAmount", Decimal, -1, 12, 0),
					new TestDbViewHelper.DbColumn("CEI_ProvisionalPaymentSuretyAmount", Decimal, -1, 12, 0),
					new TestDbViewHelper.DbColumn("CEI_ProvisionalPaymentType", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_RebateUserOverride", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CEI_RefType", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_RemoverEDI", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_RX_NKTransactionValueCurrency", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_Scope", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_SubContractorEDI", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_TransactionValue", Decimal, -1, 12, 0),
					new TestDbViewHelper.DbColumn("CEI_UCROrderNumber", VarChar, 24),
					new TestDbViewHelper.DbColumn("CEI_UCROverride", VarChar, 35),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("ZACusEntryInstruction doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "ZACusEntryInstruction_Idx"));
		}
	}
}
