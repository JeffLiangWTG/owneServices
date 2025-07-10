using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocJobComInvoiceLine))]
	sealed class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
	{
		#region New

		public void TestNewAndOverriddingNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			DocJobComInvoiceLine docInvoiceLine = DocJobComInvoiceLine.New(invoiceLine, Factory);
			AssertNotNull("New should create a new doc wrapper", docInvoiceLine);

			MockDocJobComInvoiceLineOverride.RegisterThisSubTypeOverride();
			DocJobComInvoiceLine overriddenDocInvoiceLine = DocJobComInvoiceLine.New(invoiceLine, Factory);
			Assert("Overriding New should return the overridden doc wrapper", overriddenDocInvoiceLine is MockDocJobComInvoiceLineOverride);
		}

		class MockDocJobComInvoiceLineOverride : DocJobComInvoiceLine
		{
			#region Constructors and Type Overriding

			protected MockDocJobComInvoiceLineOverride(JobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap)
				: base(invoiceLine, factoryToWrap)
			{
			}

			public new static DocJobComInvoiceLine New(JobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap)
			{
				return (invoiceLine == null) ? null : new MockDocJobComInvoiceLineOverride(invoiceLine, factoryToWrap);
			}

			public static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = new NewDelegate(New);
			}

			#endregion
		}

		#endregion

		#region Override Fields

		public void TestTariffInformation()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = InvoiceLine.Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			InvoiceLine.JI_CL = entryLine.PK;
			InvoiceLine.JI_Tariff = "12345678 63";
			entryLine.InvoiceLineAddInfo.ZA_TreatmentCode_Hidden = "CCC";
			entryLine.InvoiceLineAddInfo.ZA_InstrumentCode_Hidden = "654321";
			entryLine.InvoiceLineAddInfo.ZA_InstrumentType_Hidden = "TC";
			AssertEquals("TariffInformation", "1234.56.78 63 / CCC / TC 654321", InvoiceLineWrapper.TariffInformation);
			AssertEquals("TariffNumber", "12345678", InvoiceLineWrapper.TariffNumber);
		}

		#endregion

		#region ZString Fields

		public void TestWarehouseCCP()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("", InvoiceLineWrapper.WarehouseCCP);
			InvoiceLine.Declaration.WarehouseDocAddress.E2_OA_Address = OrgHeader.New(Factory).MainAddress.PK;
			InvoiceLine.Declaration.WarehouseAddress.LocalControlledPremisesID = "123z";
			AssertEquals("123Z", InvoiceLineWrapper.WarehouseCCP.ToUpper());
		}

		public void TestWarehouseAddress()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			InvoiceLine.Declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("", InvoiceLineWrapper.WarehouseNameAndAddress);
			InvoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			InvoiceLine.AddInfo.WarehouseAddress.OA_Address1 = "99 Rue Rd";
			InvoiceLine.AddInfo.WarehouseAddress.Header.OH_FullName = "Super Warehouse";
			AssertEquals("Super Warehouse (99 Rue Rd)", InvoiceLineWrapper.WarehouseNameAndAddress);
		}

		public void TestParentLineCode()
		{
			AssertEquals("ParentLineCode", InvoiceLine.JI_ParentLineCode, InvoiceLineWrapper.ParentLineCode);
		}

		public void TestAUState()
		{
			InvoiceLine.JI_AUState = "AU";
			AssertEquals("AUState", InvoiceLine.JI_AUState, InvoiceLineWrapper.AUState);
		}

		public void TestTempImportNum()
		{
			InvoiceLine.JI_TempImportNum = "TempImport";
			AssertEquals("TempImportNum", InvoiceLine.JI_TempImportNum, InvoiceLineWrapper.TempImportNum);
		}

		public void TestPermitNumbers()
		{
			InvoiceLine.AddInfo.ZA_PermitNumbers_Hidden = "";
			AssertEquals("PermitNumber1", ZString.Empty, InvoiceLineWrapper.PermitNumber1);
			AssertEquals("PermitNumber2", ZString.Empty, InvoiceLineWrapper.PermitNumber2);
			AssertEquals("PermitNumber3", ZString.Empty, InvoiceLineWrapper.PermitNumber3);
			InvoiceLine.AddInfo.ZA_PermitNumbers_Hidden = "P1";
			AssertEquals("PermitNumber1", "P1", InvoiceLineWrapper.PermitNumber1);
			AssertEquals("PermitNumber2", ZString.Empty, InvoiceLineWrapper.PermitNumber2);
			AssertEquals("PermitNumber3", ZString.Empty, InvoiceLineWrapper.PermitNumber3);
			InvoiceLine.AddInfo.ZA_PermitNumbers_Hidden = "P1,P2";
			AssertEquals("PermitNumber1", "P1", InvoiceLineWrapper.PermitNumber1);
			AssertEquals("PermitNumber2", "P2", InvoiceLineWrapper.PermitNumber2);
			AssertEquals("PermitNumber3", ZString.Empty, InvoiceLineWrapper.PermitNumber3);
			InvoiceLine.AddInfo.ZA_PermitNumbers_Hidden = "P1,P2,P3";
			AssertEquals("PermitNumber1", "P1", InvoiceLineWrapper.PermitNumber1);
			AssertEquals("PermitNumber2", "P2", InvoiceLineWrapper.PermitNumber2);
			AssertEquals("PermitNumber3", "P3", InvoiceLineWrapper.PermitNumber3);
		}

		public void TestInstrumentType()
		{
			AssertEquals("InstrumentType", InvoiceLine.InstrumentType, InvoiceLineWrapper.InstrumentType);
		}

		public void TestInstrumentCode()
		{
			AssertEquals("InstrumentCode", InvoiceLine.InstrumentCode, InvoiceLineWrapper.InstrumentCode);
		}

		public void TestLinePrefix()
		{
			InvoiceLine.JI_LinePrefix = "L";
			AssertEquals("LinePrefix", InvoiceLine.JI_LinePrefix, InvoiceLineWrapper.LinePrefix);
		}

		public override void TestOriginCode()
		{
			base.TestOriginCode();

			InvoiceLine.InvoiceHeader.AddInfo.ZA_ORG = "ZA";
			InvoiceLine.JI_CountryOfOrigin = "";
			AssertEquals("Aggregated Origin", "ZA", InvoiceLineWrapper.OriginCode);
		}

		#endregion

		#region Wrapper Fields

		public void TestLocalCurr()
		{
			AssertNotNull("LocalCurr", InvoiceLineWrapper.LocalCurr);
		}

		public void TestEntryLineWrapper()
		{
			InvoiceLine.JI_CL = Factory.New(typeof(CusEntryLine)).PK;
			AssertNotNull("EntryLine", InvoiceLineWrapper.EntryLine);
			AssertEquals("EntryLine is of type DocCusEntryLine", typeof(DocCusEntryLine), InvoiceLineWrapper.EntryLine.GetType());
		}

		public void TestOrder()
		{
			InvoiceLine.JI_OrderNumber = "1234";
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "1234";

			AssertNotNull(InvoiceLine.Order);
			AssertNotNull(InvoiceLineWrapper.Order);
		}

		#endregion

		#region ZDecimal Fields

		public void TestDutyAmountIncludingAllTheOtherDuties()
		{
			CusEntryLine entryLine = InvoiceLine.CusEntryLine;
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			InvoiceLine.JI_CL = entryLine.PK;
			InvoiceLine.JI_LinePrice = 1000m;

			DocJobComInvoiceLine docInvoiceLine = DocJobComInvoiceLine.New(InvoiceLine, Factory);
			AssertEquals(0m, docInvoiceLine.DutyAmountIncludingAllTheOtherDuties);

			CusEntryLineFee duty = entryLine.Fees.AddNew();
			duty.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
			duty.CF_ChargeAmount = 1000m;

			CusEntryLineFee flatDuty = entryLine.Fees.AddNew();
			flatDuty.CF_ChargeType = CusEntryChargeTypeList.Codes.FlatDutyPortion;
			flatDuty.CF_ChargeAmount = 900m;

			CusEntryLineFee dumpingDuty = entryLine.Fees.AddNew();
			dumpingDuty.CF_ChargeType = CusEntryChargeTypeList.Codes.DumpingDuty;
			dumpingDuty.CF_ChargeAmount = 800m;

			CusEntryLineFee interimAntiDumpingDuty = entryLine.Fees.AddNew();
			interimAntiDumpingDuty.CF_ChargeType = CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty;
			interimAntiDumpingDuty.CF_ChargeAmount = 700m;

			CusEntryLineFee interimCountervailingDuty = entryLine.Fees.AddNew();
			interimCountervailingDuty.CF_ChargeType = CusEntryChargeTypeList.Codes.InterimCountervailingDuty;
			interimCountervailingDuty.CF_ChargeAmount = 600m;

			AssertEquals(3100m, docInvoiceLine.DutyAmountIncludingAllTheOtherDuties);
		}

		public void TestDutyAmountForInvoiceReport()
		{
			DocJobComInvoiceLine docInvoiceLine = DocJobComInvoiceLine.New(InvoiceLine, Factory);
			AssertEquals(0m, docInvoiceLine.DutyAmountForInvoiceReport);

			CusEntryLine entryLine = InvoiceLine.CusEntryLine;
			CusEntryLineFee duty = entryLine.Fees.AddNew();
			duty.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
			duty.CF_ChargeAmount = 1000m;
			AssertEquals(1000m, entryLine.DutyAmount);
			CusEntryLineFee flatDutyPortion = entryLine.Fees.AddNew();
			flatDutyPortion.CF_ChargeType = CusEntryChargeTypeList.Codes.FlatDutyPortion;
			flatDutyPortion.CF_ChargeAmount = 200m;
			AssertEquals(200m, entryLine.FlatDutyPortion);

			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			InvoiceLine.JI_CL = entryLine.PK;
			InvoiceLine.JI_LinePrice = 1000m;

			AssertEquals(1200m, docInvoiceLine.DutyAmountForInvoiceReport);
		}

		protected override void TestVOTICore()
		{
			DocJobComInvoiceLine docInvoiceLine = DocJobComInvoiceLine.New(InvoiceLine, Factory);
			AssertEquals(0m, docInvoiceLine.VOTI);

			CusEntryLine entryLine = InvoiceLine.CusEntryLine;
			CusEntryLineFee duty = entryLine.Fees.AddNew();
			duty.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
			duty.CF_ChargeAmount = 1000m;
			entryLine.CL_CustomsValue = 500m;

			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			InvoiceLine.JI_CL = entryLine.PK;
			InvoiceLine.JI_LinePrice = 500m;

			AssertEquals(1500m, docInvoiceLine.VOTI);
		}

		public void TestWeightedCostByLocation()
		{
			AssertEquals("WeightedCostByLocation", InvoiceLine.JI_WeightedCostByLocation, InvoiceLineWrapper.WeightedCostByLocation);
		}

		public void TestInterimAntiDumpingDuty()
		{
			AssertEquals("InterimAntiDumpingDuty", InvoiceLine.JI_Calc_InterimAntiDumpingDuty, InvoiceLineWrapper.InterimAntiDumpingDuty);
		}

		public void TestDutyPercent()
		{
			AssertEquals("DutyPercent should be 0", 0M, InvoiceLineWrapper.DutyPercent);

			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_DutyPercent = 12.32M;
			InvoiceLine.JI_CL = entryLine.PK;
			AssertEquals("DutyPercent should be 12.32", 12.32M, InvoiceLineWrapper.DutyPercent);
		}

		#endregion

		#region ZBool Fields

		public void TestDrawback()
		{
			InvoiceLine.JI_Drawback = ZBool.False;
			Assert("!Drawback", !InvoiceLineWrapper.Drawback);

			InvoiceLine.JI_Drawback = ZBool.True;
			Assert("Drawback", InvoiceLineWrapper.Drawback);
		}

		public void TestTexco()
		{
			InvoiceLine.JI_Texco = ZBool.False;
			Assert("!Texco", !InvoiceLineWrapper.Texco);

			InvoiceLine.JI_Texco = ZBool.True;
			Assert("Texco", InvoiceLineWrapper.Texco);
		}

		public void TestMotorVehiclePlan()
		{
			InvoiceLine.JI_MotorVehiclePlan = ZBool.False;
			Assert("!MotorVehiclePlan", !InvoiceLineWrapper.MotorVehiclePlan);

			InvoiceLine.JI_MotorVehiclePlan = ZBool.True;
			Assert("MotorVehiclePlan", InvoiceLineWrapper.MotorVehiclePlan);
		}

		public void TestIsPackToBondForLine()
		{
			InvoiceLine.JI_IsPackToBondForLine = ZBool.False;
			Assert("!IsPackToBondForLine", !InvoiceLineWrapper.IsPackToBondForLine);

			InvoiceLine.JI_IsPackToBondForLine = ZBool.True;
			Assert("IsPackToBondForLine", InvoiceLineWrapper.IsPackToBondForLine);
		}

		#endregion

		public void TestDrawbackFields()
		{
			JobDeclaration declaration = InvoiceLine.Declaration;
			declaration.JE_MessageType = declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			InvoiceLine.AddInfo.ZA_EDN_Hidden = "LEDN";
			AssertEquals("DrawbackEDN", "LEDN", InvoiceLineWrapper.DrawbackEDN);
			InvoiceLine.AddInfo.ZA_DDN_Hidden = "IMPDEC";
			AssertEquals("DrawbackImportDec", "IMPDEC", InvoiceLineWrapper.DrawbackImportDec);
			InvoiceLine.AddInfo.ZA_DDL_Hidden = 123;
			AssertEquals("DrawbackImportDecLine", " / 123", InvoiceLineWrapper.DrawbackImportDecLine);
			AssertEquals("DrawbackImportDecLine", "123", InvoiceLineWrapper.DrawbackImportDecLineAlone);
			InvoiceLine.AddInfo.ZA_DDL_Hidden = 0;
			AssertEquals("DrawbackImportDecLine", "", InvoiceLineWrapper.DrawbackImportDecLine);
			AssertEquals("IsDrawbackDeclarationMethodA", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodA);
			AssertEquals("IsDrawbackDeclarationMethodB", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodB);
			AssertEquals("IsDrawbackDeclarationMethodC", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodC);
			InvoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.ActualShipment;
			AssertEquals("DrawbackMethod", JobDeclaration.DrawbackAssessmentMethods.ActualShipment, InvoiceLineWrapper.DrawbackMethod);
			AssertEquals("IsDrawbackDeclarationMethodA", "Y", InvoiceLineWrapper.IsDrawbackDeclarationMethodA);
			AssertEquals("IsDrawbackDeclarationMethodB", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodB);
			AssertEquals("IsDrawbackDeclarationMethodC", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodC);
			InvoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			AssertEquals("DrawbackMethod", JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment, InvoiceLineWrapper.DrawbackMethod);
			AssertEquals("IsDrawbackDeclarationMethodA", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodA);
			AssertEquals("IsDrawbackDeclarationMethodB", "Y", InvoiceLineWrapper.IsDrawbackDeclarationMethodB);
			AssertEquals("IsDrawbackDeclarationMethodC", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodC);
			InvoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.Imputation;
			AssertEquals("DrawbackMethod", JobDeclaration.DrawbackAssessmentMethods.Imputation, InvoiceLineWrapper.DrawbackMethod);
			AssertEquals("IsDrawbackDeclarationMethodA", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodA);
			AssertEquals("IsDrawbackDeclarationMethodB", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodB);
			AssertEquals("IsDrawbackDeclarationMethodC", "Y", InvoiceLineWrapper.IsDrawbackDeclarationMethodC);
			InvoiceLine.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.OtherMethod;
			AssertEquals("DrawbackMethod", JobDeclaration.DrawbackAssessmentMethods.OtherMethod, InvoiceLineWrapper.DrawbackMethod);
			AssertEquals("IsDrawbackDeclarationMethodOther", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodA);
			AssertEquals("IsDrawbackDeclarationMethodOther", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodB);
			AssertEquals("IsDrawbackDeclarationMethodOther", "", InvoiceLineWrapper.IsDrawbackDeclarationMethodC);
			AssertEquals("IsDrawbackLineAmberReasonC", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonC);
			AssertEquals("IsDrawbackLineAmberReasonD", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonD);
			AssertEquals("IsDrawbackLineAmberReasonT", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonT);
			InvoiceLine.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Calculation;
			AssertEquals("IsDrawbackLineAmberReasonC", "Y", InvoiceLineWrapper.IsDrawbackLineAmberReasonC);
			AssertEquals("IsDrawbackLineAmberReasonD", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonD);
			AssertEquals("IsDrawbackLineAmberReasonT", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonT);
			InvoiceLine.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Declaration;
			AssertEquals("IsDrawbackLineAmberReasonC", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonC);
			AssertEquals("IsDrawbackLineAmberReasonD", "Y", InvoiceLineWrapper.IsDrawbackLineAmberReasonD);
			AssertEquals("IsDrawbackLineAmberReasonT", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonT);
			InvoiceLine.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Time;
			AssertEquals("IsDrawbackLineAmberReasonC", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonC);
			AssertEquals("IsDrawbackLineAmberReasonD", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonD);
			AssertEquals("IsDrawbackLineAmberReasonT", "Y", InvoiceLineWrapper.IsDrawbackLineAmberReasonT);
			InvoiceLine.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.LegacyMigration;
			AssertEquals("IsDrawbackLineAmberReasonC", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonC);
			AssertEquals("IsDrawbackLineAmberReasonD", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonD);
			AssertEquals("IsDrawbackLineAmberReasonT", "", InvoiceLineWrapper.IsDrawbackLineAmberReasonT);
			InvoiceLine.AddInfo.ZA_DCV_Hidden = 124m;
			AssertEquals("DrawbackCustomsValue", 124m, InvoiceLineWrapper.DrawbackCustomsValue);
			InvoiceLine.AddInfo.ZA_DTR_Hidden = 125m;
			AssertEquals("DrawbackDutyRate", 125m, InvoiceLineWrapper.DrawbackDutyRate);
			InvoiceLine.AddInfo.ZA_DDT_Hidden = 126m;
			AssertEquals("DrawbackClaimAmount", 126m, InvoiceLineWrapper.DrawbackClaimAmount);
			InvoiceLine.AddInfo.ZA_DDT_Hidden = 0m;
			InvoiceLine.AddInfo.ZA_DCV_Hidden = 0m;
			AssertEquals("DrawbackExceptionComment", "Cannot calculate claim amount", InvoiceLineWrapper.DrawbackExceptionComment);
			InvoiceLine.AddInfo.ZA_DCV_Hidden = 1m;
			AssertEquals("DrawbackExceptionComment", "Duty free", InvoiceLineWrapper.DrawbackExceptionComment);
			InvoiceLine.AddInfo.ZA_DDT_Hidden = 1m;
			AssertEquals("DrawbackExceptionComment", "", InvoiceLineWrapper.DrawbackExceptionComment);
			InvoiceLine.IsDrawbackLineValueOverriden = true;
			AssertEquals("DrawbackExceptionComment", "Claim amount manual override", InvoiceLineWrapper.DrawbackExceptionComment);
			InvoiceLine.IsDrawbackLineValueOverriden = false;
			var mockInvoiceLine = Factory.NewMoq<JobComInvoiceLine>();
			JobComInvoiceLine invoiceLine2 = mockInvoiceLine.Object;
			DocJobComInvoiceLine invoiceLineWrapper2 = CreateInvoiceLineWrapper(invoiceLine2);
			invoiceLine2.AddInfo.ZA_DDT_Hidden = 1m;
			invoiceLine2.AddInfo.ZA_DCV_Hidden = 1m;
			mockInvoiceLine.Setup(m => m.IsDrawbackClaimAmountUnusual).Returns(ZBool.True);
			mockInvoiceLine.Setup(m => m.AverageCustomsValue).Returns(123.45m);
			invoiceLine2.JI_CustomsQuantity = 2;
			AssertEquals("DrawbackExceptionComment", "Unusual claim amount, average customs value (times this quantity), over the preceding 18 months, is 246.90", invoiceLineWrapper2.DrawbackExceptionComment);
			mockInvoiceLine.Setup(m => m.IsDrawbackClaimAmountUnusual).Returns(ZBool.False);
			mockInvoiceLine.Setup(m => m.DrawbackCalculationMethodComment).Returns("XYZ");
			AssertEquals("DrawbackExceptionComment", "XYZ", invoiceLineWrapper2.DrawbackExceptionComment);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}

		protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLineInternal)
		{
			return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
		}

		#endregion
	}
}
