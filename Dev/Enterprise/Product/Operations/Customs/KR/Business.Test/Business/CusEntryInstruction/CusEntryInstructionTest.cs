using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(Factory.New<CusEntryInstruction>(), "KRCusEntryInstruction");
		}

		void SetUpTariffData()
		{
			#region Tariff
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMapType("KRPRG", "OUT", "KR Preference Code and its group code", false);

			helper.CreateCusMap("KRPRG", "A", "기", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMap("KRPRG", "A1", "기", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMap("KRPRG", "B", "잠", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusMap("KRPRG", "C", "가", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();
			#endregion
		}

		public void TestSupportsClone()
		{
			Assert(Factory.New<CusEntryInstruction>().SupportsClone());
		}

		public void TestCloneEntryInstruction()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertNoExceptionThrown(() => instruction.Clone());
		}

		public void TestTariffRateClassificationShortName()
		{
			SetUpTariffData();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			entry.CH_CEI_Instruction = instruction.PK;

			instruction.CEI_AgreedDutyRatePreferenceCode = "A";
			AssertEquals("기", instruction.TariffRateClassificationShortName);

			instruction.CEI_AgreedDutyRatePreferenceCode = "B";
			AssertEquals("잠", instruction.TariffRateClassificationShortName);
		}

		public void TestAddInfoIsOverflowWhenMaxLengthData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			entry.CH_CEI_Instruction = instruction.PK;

			instruction.CEI_AgreedDutyRate = 111111111111.11m;
			instruction.CEI_AgreedDutyRatePreferenceCode = "AAAAAA";
			instruction.CEI_BondedFactoryArrivalDate = DateTime.Today;
			instruction.CEI_BondedFactoryUseCode = "A";
			instruction.CEI_TaxPenaltyCause = "01";
			instruction.CEI_DutyPenaltyCause = "02";
			instruction.CEI_FTARelationArticleCode = "A";
			instruction.CEI_PackQty = 1111111111;
			instruction.CEI_ApplyDutyPenaltyReduction = "Y";
			instruction.CEI_RefundCauseCode = "AA";
			instruction.CEI_RefundReasonCode = "AA";
			instruction.CEI_RefundType = "A";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newInstruction = newFactory.Load<CusEntryInstruction>(instruction.PK);

			AssertEquals(true, newInstruction.CEI_AddInfo.Length <= CusEntryInstruction.Schema.CEI_AddInfoMaxLength);
			AssertEquals(111111111111.11m, newInstruction.CEI_AgreedDutyRate);
			AssertEquals("AAAAAA", newInstruction.CEI_AgreedDutyRatePreferenceCode);
			AssertEquals(DateTime.Today, newInstruction.CEI_BondedFactoryArrivalDate);
			AssertEquals("A", newInstruction.CEI_BondedFactoryUseCode);
			AssertEquals("01", newInstruction.CEI_TaxPenaltyCause);
			AssertEquals("02", newInstruction.CEI_DutyPenaltyCause);
			AssertEquals("A", newInstruction.CEI_FTARelationArticleCode);
			AssertEquals(1111111111, newInstruction.CEI_PackQty);
			AssertEquals("Y", newInstruction.CEI_ApplyDutyPenaltyReduction);
			AssertEquals("AA", newInstruction.CEI_RefundCauseCode);
			AssertEquals("AA", newInstruction.CEI_RefundReasonCode);
			AssertEquals("A", newInstruction.CEI_RefundType);
		}

		public void TestInstructionsWhenChangingMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;

			AssertNotNull(declaration.CustomsEntryInstructions);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "TT";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			AssertEquals("TT", invoiceLine.EntryInstruction.CEI_Style);

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;

			AssertNotNull(declaration.CustomsEntryInstructions);
			AssertEquals(0, declaration.CustomsEntryInstructions.Count);
			AssertNull(invoiceLine.EntryInstruction);

			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;

			AssertNotNull(declaration.CustomsEntryInstructions);
			AssertEquals(0, declaration.CustomsEntryInstructions.Count);
			AssertNull(invoiceLine.EntryInstruction);
		}

		public void TestSessionalData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();

			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.O;
			AssertNull(amendmentSessionalData.PenaltyExemptionSessionalData);

			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			AssertNotNull(amendmentSessionalData.PenaltyExemptionSessionalData);

			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.B;
			AssertNotNull(amendmentSessionalData.PenaltyExemptionSessionalData);

			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.C;
			AssertNull(amendmentSessionalData.PenaltyExemptionSessionalData);

			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.X;
			AssertNull(amendmentSessionalData.PenaltyExemptionSessionalData);
		}
	}
}
