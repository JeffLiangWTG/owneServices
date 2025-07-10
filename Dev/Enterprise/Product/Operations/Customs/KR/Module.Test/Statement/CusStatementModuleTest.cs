using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(CusStatementModule))]
	sealed class CusStatementModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ZFilterModule CreateModuleForFetchHintsTest() => new CusStatementModule();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.KR.CustomsStatement;
		protected override void SetupDataForFetchHintsTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			for (int idx = 0; idx < 20; idx++)
			{
				CreateStatementForFetchHintTest(idx);
			}
			Factory.Save();
		}

		void CreateStatementForFetchHintTest(int idx)
		{
			var organisations = Factory.GetCachedValue("OrganisationsForStatementModuleTest", delegate
			{
				return Factory.Load<OrgHeader>(new ZQuery() { MaximumRows = 20 });
			});

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "01270" + idx.ToString("000000");
			statement.B2_AccountNo = "030511900081" + idx.ToString("000");
			statement.B2_DueDate = ZDateTime.Today.AddMinutes(idx);
			statement.B2_ProcessDate = ZDateTime.Today.AddMinutes(idx + 1);
			statement.B2_StatementAmount = 10m * idx;
			statement.B2_PaymentStatus = "PYI";
			statement.B2_StatementType = "R";
			statement.B2_PaymentType = "VEP";
			statement.B2_ProcessPort = "010";
			statement.B2_IsMonthlyStatement = true;
			statement.B2_PaymentParty = "OWN";
			statement.B2_OH_Importer = organisations[idx].PK;
			statement.B2_ImporterCustomsID = "123456" + idx.ToString("0000000");
			statement.B2_Status = StatementHeaderStatusList.Codes.Z;
			var month = idx % 12 + 1;
			statement.B2_PeriodStartDate = new ZDate(2021, month, 1);
			statement.B2_PeriodEndDate = new ZDate(2021, month, DateTime.DaysInMonth(2021, month));
			statement.B2_PaymentAuthorizationDate = new ZDate(2021, month, 1);
			var line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "1233420017772M";
			line.B3_CustomsFeesTotal = 19283m;
			line.B3_SequenceNumber = 1;
			var charge = line.Charges.AddNew();
			charge.B4_ChargeType = "VFV";
			charge.B4_ChargeAmount = idx * 1000m;
		}
	}
}
