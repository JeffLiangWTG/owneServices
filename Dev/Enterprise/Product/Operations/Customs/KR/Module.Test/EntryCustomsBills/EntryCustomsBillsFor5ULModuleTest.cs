using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryCustomsBillsFor5ULModule))]
	sealed class EntryCustomsBillsFor5ULModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ZFilterModule CreateModuleForFetchHintsTest() => new EntryCustomsBillsFor5ULModule();

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.KR.EntryCustomsBillsFor5UL;

		protected override void SetupDataForFetchHintsTest()
		{
			for (int idx = 1; idx < 20; idx++)
			{
				CreateKREntryCustomsBillsViewForFetchHintTest(Factory, idx);
			}
			Factory.Save();
		}
		void CreateKREntryCustomsBillsViewForFetchHintTest(BusinessObjectFactory factory, int idx)
		{
			var orgHeaderPayer = factory.New<OrgHeader>();
			orgHeaderPayer.OH_Category = "BUS";
			orgHeaderPayer.OH_Code = "RK Payer" + idx.ToString();
			orgHeaderPayer.OH_FullName = "RK Payer Test" + idx.ToString();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryReleaseDate = ZDateTime.Today;

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "123452212345" + idx.ToString("00");
			entryNum.CE_IssueDate = ZDateTime.Today;

			var statement = factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = orgHeaderPayer.PK;
			statement.B2_StatementType = "D";
			statement.B2_PaymentStatus = "PYC";
			statement.B2_StatementNumber = "12090013912345678" + idx.ToString("00");
			statement.B2_PrintDate = ZDateTime.Today;
			statement.B2_ProcessDate = ZDateTime.Today;
			statement.B2_DueDate = ZDateTime.Today;
			statement.B2_PaymentAuthorizationDate = ZDateTime.Today;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryType = "IMP";
			statementLine.B3_EntryNum = entryNum.CE_EntryNum;
			statementLine.B3_CustomsFeesTotal = 1234500m + idx;

			SetChargeAmount("DTY", 1000m + idx);
			SetChargeAmount("VAT", 2000m + idx);
			SetChargeAmount("LQT", 3000m + idx);
			SetChargeAmount("AGT", 4000m + idx);
			SetChargeAmount("SCT", 5000m + idx);
			SetChargeAmount("TRT", 6000m + idx);
			SetChargeAmount("EDT", 7000m + idx);
			SetChargeAmount("PLT", 8000m + idx);
			SetChargeAmount("PMT", 9000m + idx);
			SetChargeAmount("VFV", 10000m + idx);

			void SetChargeAmount(ZString chargeType, ZDecimal chargeAmount)
			{
				var charge = statementLine.Charges.AddNew();
				charge.B4_ChargeType = chargeType;
				charge.B4_ChargeAmount = chargeAmount;
			}
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var factory = collection.Factory;
			CreateKREntryCustomsBillsViewForFetchHintTest(factory, 0);
			factory.Save();
		}
	}
}
