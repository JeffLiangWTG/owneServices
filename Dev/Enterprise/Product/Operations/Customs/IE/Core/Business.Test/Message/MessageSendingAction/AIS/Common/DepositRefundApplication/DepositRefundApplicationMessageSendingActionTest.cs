using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;
using ZPropertyInfoExtensions = Enterprise.Customs.Business.ZPropertyInfoExtensions;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DepositRefundApplicationMessageSendingAction))]
	sealed class DepositRefundApplicationMessageSendingActionTest : CusEntryHeaderMessageSendingActionTest<DepositRefundApplicationMessageSendingAction>
	{
		public void TestMovementReferenceNumber_DefaultValue()
		{
			entryHeader.MovementReferenceNumberSetter("MRN001");
			AssertEquals("MRN001", testSendingAction.MovementReferenceNumber);
		}

		public void TestMovementReferenceNumber_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.MovementReferenceNumberInfo);
			AssertEquals("Import MRN", resData.Caption);
		}

		public void TestMovementReferenceNumber_Readonly()
		{
			Assert(testSendingAction.MovementReferenceNumberInfo.ReadOnly);
		}

		public void TestExportMovementReferenceNumber_DefaultValue()
		{
			AssertEquals("IE23AB987", testSendingAction.ExportMovementReferenceNumber);
		}

		public void TestExportMovementReferenceNumber_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.ExportMovementReferenceNumberInfo);
			AssertEquals("Export MRN", resData.Caption);
		}

		public void TestExportDate_DefaultValue()
		{
			AssertEquals(ZDateTime.BrettsBirthday, testSendingAction.ExportDate);
		}

		public void TestExportDate_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.ExportDateInfo);
			AssertEquals("Export Date", resData.Caption);
		}

		public void TestCustomsDuty_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.CustomsDutyInfo);
			AssertEquals("Customs Duty", resData.Caption);
		}

		public void TestCheckCustomsDuty_DecimalAttributes()
		{
			AssertEquals(16, testSendingAction.CustomsDutyInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			AssertEquals(2, testSendingAction.CustomsDutyInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestVat_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.VatInfo);
			AssertEquals("VAT", resData.Caption);
		}

		public void TestCheckVat_DecimalAttributes()
		{
			AssertEquals(16, testSendingAction.VatInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			AssertEquals(2, testSendingAction.VatInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestOtherDuties_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.OtherDutiesInfo);
			AssertEquals("Other Duties", resData.Caption);
		}

		public void TestCheckOtherDuties_DecimalAttributes()
		{
			AssertEquals(16, testSendingAction.OtherDutiesInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			AssertEquals(2, testSendingAction.OtherDutiesInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestImportedGoodsDischarged_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.ImportedGoodsDischargedInfo);
			AssertEquals("Imported Goods Discharged", resData.Caption);
		}

		public void TestOutstandingBalance_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.OutstandingBalanceInfo);
			AssertEquals("Outstanding Balance", resData.Caption);
			AssertEquals("Outstanding balance on imported goods.", resData.FullDescription);
		}

		public void TestCheckOutstandingBalance_DecimalAttributes()
		{
			AssertEquals(16, testSendingAction.OutstandingBalanceInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			AssertEquals(2, testSendingAction.OutstandingBalanceInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestAmountOfDepositRefund_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.AmountOfDepositRefundInfo);
			AssertEquals("Amount of Deposit Refund", resData.Caption);
		}

		public void TestCheckAmountOfDepositRefund_DecimalAttributes()
		{
			AssertEquals(16, testSendingAction.AmountOfDepositRefundInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			AssertEquals(2, testSendingAction.AmountOfDepositRefundInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestPayerEori_DefaultValue()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TEST001";
			organisation.OH_FullName = "Test Organisation";
			var address = organisation.Addresses.AddNew();
			address.OA_Address1 = "Street 1";
			address.OA_City = "City";
			address.OA_PostCode = "123456";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			var orgCusCode = organisation.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			orgCusCode.OK_CustomsRegNo = "1234567890";
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;

			entryHeader.Declaration.JE_OH_DutyPayer = organisation.PK;

			testSendingAction = new DepositRefundApplicationMessageSendingAction(entryHeader);

			AssertEquals("1234567890", testSendingAction.PayerEori);
		}

		public void TestPayerEori_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.PayerEoriInfo);
			AssertEquals("Payer EORI", resData.Caption);
			AssertEquals("Payer EORI for Refund.", resData.FullDescription);
		}

		public void TestCheckPayerEori_MaxLength()
		{
			AssertEquals(17, testSendingAction.PayerEoriInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestPeriodForDischarge_DefaultValue()
		{
			AssertEquals(78, testSendingAction.PeriodForDischarge);
		}

		public void TestPeriodForDischarge_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.PeriodForDischargeInfo);
			AssertEquals("Period for Discharge", resData.Caption);
		}

		public void TestCheckPeriodForDischarge_MaxLength()
		{
			AssertEquals(2, testSendingAction.PeriodForDischargeInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestRateOfYield_DefaultValue()
		{
			AssertEquals("Some text", testSendingAction.RateOfYield);
		}

		public void TestRateOfYield_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(testSendingAction.RateOfYieldInfo);
			AssertEquals("Rate of Yield", resData.Caption);
		}

		public void TestCheckRateOfYield_MaxLength()
		{
			AssertEquals(512, testSendingAction.RateOfYieldInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		protected override Type ExpectedLookupsType => typeof(DepositRefundApplicationMessageSendingActionLookups);

		protected override Type ExpectedSenderType => typeof(DepositRefundApplicationMessageSender);

		protected override Type ExpectedValidationType => typeof(DepositRefundApplicationMessageSendingActionValidation);

		protected override BusinessObject GetNewBusinessObject() => testSendingAction;

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			entryInstruction.ZG_PeriodForDischarge = 78;
			entryInstruction.PeriodForDischargeDetails = "Some text";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var previousDocument = entryInstruction.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "MRN";
			previousDocument.CSI_ReferenceNumber = "IE23AB987";
			var previousDocument2 = entryInstruction.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "MRN";
			previousDocument2.CSI_ReferenceNumber = "IE23CD123";
			testSendingAction = new DepositRefundApplicationMessageSendingAction(entryHeader);
		}

		CusEntryHeader entryHeader;
	}
}
