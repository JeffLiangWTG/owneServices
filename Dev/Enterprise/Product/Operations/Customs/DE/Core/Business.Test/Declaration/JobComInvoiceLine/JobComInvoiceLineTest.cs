using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
	{
		public void TestUpdateJI_CustomsSecondQuantityIfNeeded()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();

			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			product.OP_PartNum = "Koushuiwa001";
			product.OP_StockKeepingUnit = "NAR";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_SecondQty = 10;

			declaration.JE_OH_Importer = importer.PK;
			invoiceLine.JI_InvoiceUQ = "NAR";
			invoiceLine.JI_InvoiceQuantity = 100;
			invoiceLine.JI_PartNo = "Koushuiwa001";

			CombineAssertions(() =>
			{
				AssertEquals("Successfully change the JI_CustomsSecondQuantity when product changed", 1000m, invoiceLine.JI_CustomsSecondQuantity);

				invoiceLine.JI_InvoiceQuantity = 200;
				invoiceLine.OnSaving();
				AssertEquals("Successfully change the JI_CustomsSecondQuantity when save invoiceLine", 2000m, invoiceLine.JI_CustomsSecondQuantity);

				invoiceLine.JI_InvoiceUQ = "TES";
				invoiceLine.JI_InvoiceQuantity = 300;
				invoiceLine.OnSaving();
				AssertEquals("JI_InvoiceUQ != OP_StockKeepingUnit of Product, update failed", 2000m, invoiceLine.JI_CustomsSecondQuantity);
			});
		}

		public void TestCustomsUnitDefaultingStrategy()
		{
			AssertType<TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>>(typeof(BaseJobComInvoiceLine).GetProperty("CustomsUnitDefaultingStrategy", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(invoiceLine));
		}

		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be DE", Core.Constants.CountryCodes.Germany, invoiceLine.CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				CombineAssertions(() =>
				{
					AssertEquals(Core.Constants.CountryCodes.Germany, partDetails.CustomsCountryCode);
					AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
				});
			}
		}

		public void TestNeedsCustomsQuantity()
		{
			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			invoiceLineMock.Setup(m => m.CustomsUQ).Returns("XXX");
			var newInvoiceLine = invoiceLineMock.Object;
			newInvoiceLine.JI_JZ = invoiceHeader.PK;

			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Isn't Export and JE_TransportMode isn't FIX", true, newInvoiceLine.NeedsCustomsQuantity);

				declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				AssertEquals("Isn't Export and JE_TransportMode is FIX", true, newInvoiceLine.NeedsCustomsQuantity);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Is Export and JE_TransportMode is FIX", false, newInvoiceLine.NeedsCustomsQuantity);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Is Export and JE_TransportMode isn't FIX", true, newInvoiceLine.NeedsCustomsQuantity);
			});
		}

		public void TestCalculateNetPriceIfNeededOnSaving()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 25000m;
			var dis1 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount);

			invoiceLine.JI_NetPrice = ZDecimal.Zero;
			dis1.J7_Amount = 100m;
			invoiceLine.OnSaving();
			AssertEquals("High Value", 24900m, invoiceLine.JI_NetPrice);
		}

		public void TestCalculateNetPriceIfNeededOnSaving_IfLinePriceIsNotEntered()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 0m;
			var dis1 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount);
			dis1.J7_Amount = 100m;
			invoiceLine.JI_NetPrice = ZDecimal.Zero;
			invoiceLine.OnSaving();
			AssertEquals("Line Price Is Not Entered", ZDecimal.Zero, invoiceLine.JI_NetPrice);
		}

		public void TestCalculateNetPriceIfNeededOnSaving_IfNetPriceIsEntered()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 25000m;
			var dis1 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount);
			dis1.J7_Amount = 100m;
			invoiceLine.JI_NetPrice = 5555m;
			invoiceLine.OnSaving();
			AssertEquals("Net Price Is Entered", 5555m, invoiceLine.JI_NetPrice);
		}

		public void TestCalculateNetPriceIfNeededOnSaving_IfLowValue()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.False;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 25000m;
			var dis1 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount);
			dis1.J7_Amount = 100m;
			invoiceLine.JI_NetPrice = ZDecimal.Zero;
			invoiceLine.OnSaving();
			AssertEquals("Low Value", ZDecimal.Zero, invoiceLine.JI_NetPrice);
		}

		public void TestCalculateNetPriceIfNeededOnSaving_IfInvoiceCurrencyIsNotEntered()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.False;
			invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			invoiceLine.JI_LinePrice = 25000m;
			var dis1 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount);
			dis1.J7_Amount = 100m;
			invoiceLine.JI_NetPrice = ZDecimal.Zero;
			invoiceLine.OnSaving();
			AssertEquals("Invoice Currency Is Not Entered", ZDecimal.Zero, invoiceLine.JI_NetPrice);
		}

		public void TestCalculateNetPriceIfNeededOnSaving_IfNoDiscountCharge()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 25000m;
			invoiceLine.JI_NetPrice = ZDecimal.Zero;

			invoiceLine.OnSaving();
			AssertEquals("No Discount Charges", 25000m, invoiceLine.JI_NetPrice);
		}

		public void TestCalculateNetPriceIfNeededOnSaving_IfMultipleDiscountCharges()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 25000m;
			invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 100m);
			invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 200m);
			invoiceLine.JI_NetPrice = ZDecimal.Zero;

			invoiceLine.OnSaving();
			AssertEquals("Multiple Discount Charges", 0m, invoiceLine.JI_NetPrice);
		}

		public void TestCalculateNetPriceIfNeededOnSaving_DiscountChargeAmountIsZero()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 25000m;
			var dis1 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount);

			invoiceLine.JI_NetPrice = ZDecimal.Zero;
			dis1.J7_Amount = 0m;
			invoiceLine.OnSaving();
			AssertEquals("J7_Amount is 0", 25000m, invoiceLine.JI_NetPrice);
		}

		public void TestCalculateNetPriceIfNeededManualOperation_IfNetPriceIsEntered()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceLine.JI_LinePrice = 25000m;
			var dis1 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount);

			invoiceLine.JI_NetPrice = 5555m;
			dis1.J7_Amount = 100m;
			invoiceLine.CalculateNetPriceIfNeeded(true);
			AssertEquals("Net Price Is Entered", 24900m, invoiceLine.JI_NetPrice);
		}

		public void TestJI_NetPrice_AddDiscountCharge()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
			invoiceHeader.JZ_InvoiceCurrExRate = 10m;
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2021, 1, 5);
			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_NetPrice = 100m;

			var disCharge = invoiceLine.Charges.Cast<InvoiceLineCharge>().Single(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.Discount);
			CombineAssertions(() =>
			{
				AssertEquals("J7_Amount", 100m, disCharge.J7_Amount);
				AssertEquals("J7_RX_NKCurrency", "USD", disCharge.J7_RX_NKCurrency);
				AssertEquals("J7_IsDutiable", false, disCharge.J7_IsDutiable);
				AssertEquals("J7_IsStatisticalValueApplicable", false, disCharge.J7_IsStatisticalValueApplicable);
				AssertEquals("J7_IsGSTApplicable", false, disCharge.J7_IsGSTApplicable);
				AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", false, disCharge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("J7_IsIncludedInITOT", false, disCharge.J7_IsIncludedInITOT);
				AssertEquals("IsJ7_ExchangeRateIATA ", false, disCharge.IsJ7_ExchangeRateIATA);
				AssertEquals("IsJ7_ExchangeRateUserEnterable ", true, disCharge.IsJ7_ExchangeRateUserEnterable);
				AssertEquals("J7_ExchangeRate ", 10m, disCharge.J7_ExchangeRate);
				AssertEquals("J7_ExchangeRateDate ", new ZDate(2021, 1, 5), disCharge.J7_ExchangeRateDate);
				AssertEquals("JI_NetPrice ", 100m, invoiceLine.JI_NetPrice);
			});
		}

		public void TestJI_NetPrice_UpdateDiscountCharge()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_LinePrice = 300m;
			var disCharge = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount);

			invoiceLine.JI_NetPrice = 200m;
			CombineAssertions(() =>
			{
				AssertEquals("J7_Amount", 100m, disCharge.J7_Amount);
				AssertEquals("JI_NetPrice ", 200m, invoiceLine.JI_NetPrice);
			});
		}

		public void TestJI_NetPrice_NotUpdateMultipleDiscountCharges()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_LinePrice = 300m;
			var disCharge = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 50m);
			var disCharge2 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 100m);

			invoiceLine.JI_NetPrice = 200m;
			CombineAssertions(() =>
			{
				AssertEquals("disCharge.J7_Amount", 50m, disCharge.J7_Amount);
				AssertEquals("disCharge2.J7_Amount", 100m, disCharge2.J7_Amount);
				AssertEquals("InvoiceLine.JI_NetPrice ", 200m, invoiceLine.JI_NetPrice);
			});
		}

		public void TestJI_NetPrice_DeleteDiscountCharges()
		{
			declaration.ZG_IsHighValueOvrd = ZBool.True;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_LinePrice = 300m;
			var disCharge = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 100m);

			CombineAssertions(() =>
			{
				AssertEquals("JI_NetPrice is 200", 200m, invoiceLine.JI_NetPrice);

				var disCharge2 = invoiceLine.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 100m);
				AssertEquals("JI_NetPrice is still 200", 200m, invoiceLine.JI_NetPrice);

				invoiceLine.JI_NetPrice = 300m;
				AssertEquals("disCharge is deleted", true, disCharge.IsDeleted);
				AssertEquals("disCharge2 is deleted", true, disCharge2.IsDeleted);
			});
		}

		public void TestPreviousProcedures()
		{
			CombineAssertions(() =>
			{
				var previousProcedures = invoiceLine.PreviousProcedures;
				AssertEquals("IsRegisteredEditableChildObject", true, invoiceLine.IsRegisteredEditableChildObject(previousProcedures));
				AssertSame("Cached", previousProcedures, invoiceLine.PreviousProcedures);
			});
		}

		public void TestPreviousProcedureMaster()
		{
			var previousProcedureMaster = invoiceLine.PreviousProcedureMaster;
			AssertSame("Cached", previousProcedureMaster, invoiceLine.PreviousProcedureMaster);
		}

		public void TestGetCusSupportingInfoTypes_InwardProcessingProduct()
		{
			var supporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)invoiceLine;
			AssertEquals(typeof(InwardProcessingProduct), supporter.GetCusSupportingInfoTypes()[InwardProcessingProduct.CusSupportingInfoType]);
		}

		public void TestInwardProcessingProducts()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			var invoiceHeader = instruction.JobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			AssertEquals(true, invoiceLine.IsRegisteredEditableChildObject(invoiceLine.InwardProcessingProducts));
		}

		public void TestZG_IdentificationMeansType_ReadOnly()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			var invoiceHeader = instruction.JobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				AssertEquals("CEI_SimplifiedGrantAuthorization = 'J' -> Read Write", false, invoiceLine.ZG_IdentificationMeansTypeInfo.ReadOnly);

				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
				AssertEquals("CEI_SimplifiedGrantAuthorization = 'N' -> Read Only", true, invoiceLine.ZG_IdentificationMeansTypeInfo.ReadOnly);

				instruction.CEI_SimplifiedGrantAuthorization = ZString.Empty;
				AssertEquals("CEI_SimplifiedGrantAuthorization empty -> Read Only", true, invoiceLine.ZG_IdentificationMeansTypeInfo.ReadOnly);
			});
		}

		public void TestJI_ExtraInfoForClassification_ReadOnly()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			var invoiceHeader = instruction.JobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Read Only", true, invoiceLine.JI_ExtraInfoForClassificationInfo.ReadOnly);
				invoiceLine.ZG_IdentificationMeansType = IdentificationMeansTypeList.Codes.D;
				AssertEquals("Read Write", false, invoiceLine.JI_ExtraInfoForClassificationInfo.ReadOnly);
			});
		}

		public void TestClearInwardProcessingDetailsIfNeeded()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			var invoiceHeader = instruction.JobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.ZG_EconomicConditions = "01";
			invoiceLine.ZG_IdentificationMeansType = IdentificationMeansTypeList.Codes.D;
			invoiceLine.JI_ExtraInfoForClassification = "EXTRA INFORMATION";
			invoiceLine.InwardProcessingProducts.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EGN;
			CombineAssertions(() =>
			{
				AssertEquals("Economic Conditions", ZString.Empty, invoiceLine.ZG_EconomicConditions);
				AssertEquals("Identification Means Type", ZString.Empty, invoiceLine.ZG_IdentificationMeansType);
				AssertEquals("Extra Info For Classification", ZString.Empty, invoiceLine.JI_ExtraInfoForClassification);
				AssertEquals("Inward Processing Products", 0, invoiceLine.InwardProcessingProducts.Count);
			});
		}

		public void TestZG_CountryOfSupply_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.ZG_CountryOfSupplyInfo, multipleResourceKey: null, "Preferential Country/Region", "Pref. Ctry./Rgn.", "Preferential Ctry./Rgn.");
		}

		public void TestZG_EconomicConditions_Caption()
		{
			AssertEquals("Economic Condition", invoiceLine.ZG_EconomicConditionsInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestZG_EconomicConditions_ReadOnly()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			var invoiceHeader = instruction.JobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				AssertEquals("CEI_SimplifiedGrantAuthorization = 'J' -> Read Write", false, invoiceLine.ZG_EconomicConditionsInfo.ReadOnly);

				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
				AssertEquals("CEI_SimplifiedGrantAuthorization = 'N' -> Read Only", true, invoiceLine.ZG_EconomicConditionsInfo.ReadOnly);

				instruction.CEI_SimplifiedGrantAuthorization = ZString.Empty;
				AssertEquals("CEI_SimplifiedGrantAuthorization empty -> Read Only", true, invoiceLine.ZG_EconomicConditionsInfo.ReadOnly);
			});
		}

		public void TestMaxNumberOfAdditionalProcedureCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Import", 99, invoiceLine.MaxNumberOfAdditionalProcedureCode);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export", 0, invoiceLine.MaxNumberOfAdditionalProcedureCode);
			});
		}

		public void TestAdditionalInfoDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, invoiceLine.AdditionalInfoDescription);

				var additional = invoiceLine.AdditionalInfos.AddNew();
				additional.CSI_Description = "test1";
				AssertEquals("test1", invoiceLine.AdditionalInfoDescription);

				invoiceLine.AdditionalInfoDescription = "test2";
				AssertEquals("test2", additional.CSI_Description);

				invoiceLine.AdditionalInfoDescription = ZString.Empty;
				AssertEquals(0, invoiceLine.AdditionalInfos.Count);

				AssertEquals(100, invoiceLine.AdditionalInfoDescriptionInfo.MaxLength);
			});
		}

		public void TestAdditionalInfos()
		{
			AssertEquals(typeof(AdditionalInfoCollection), invoiceLine.AdditionalInfos.GetType());
		}

		public void TestSupportingInfoTypes()
		{
			CombineAssertions(() =>
			{
				var types = (InvoiceLine as Integration.Customs.ICusSupportingInfoTypeSupporter).GetCusSupportingInfoTypes();
				AssertEquals(Common.EU.CusSupportingInfoTypeList.Descriptions.AdditionalInfo, typeof(AdditionalInfo), types[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals(Common.EU.CusSupportingInfoTypeList.Descriptions.PreviousDocument, typeof(PreviousDocument), types[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals(Common.EU.CusSupportingInfoTypeList.Descriptions.SupportingDocument, typeof(SupportingDocument), types[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
				AssertEquals(InwardProcessingProduct.CusSupportingInfoType, typeof(InwardProcessingProduct), types[InwardProcessingProduct.CusSupportingInfoType]);
			});
		}

		public void TestConcession()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "E01";
				AssertEquals(ZString.Empty, invoiceLine.Concession);

				invoiceLine.JI_Procedure = "1111E01";
				AssertEquals("E01", invoiceLine.Concession);
			});
		}

		public void TestJI_Procedure()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_CessionFlag = "01";
				invoiceLine.JI_Procedure = "01";
				AssertEquals("JI_Procedure isn't empty, JI_CessionFlag is '01'", "01", invoiceLine.JI_CessionFlag);
				invoiceLine.JI_Procedure = ZString.Empty;
				AssertEquals("JI_Procedure is empty, JI_CessionFlag is empty", ZString.Empty, invoiceLine.JI_CessionFlag);

				invoiceLine.JI_NetPrice = 1m;
				invoiceLine.JI_Procedure = "01";
				AssertEquals("JI_Procedure is '01'", 1m, invoiceLine.JI_NetPrice);
				invoiceLine.JI_Procedure = "E01";
				AssertEquals("JI_Procedure is 'E01'", 1m, invoiceLine.JI_NetPrice);
				invoiceLine.JI_Procedure = "1111E01";
				AssertEquals("JI_Procedure is '1111E01'", ZDecimal.Zero, invoiceLine.JI_NetPrice);
			});
		}

		public void TestIsProcedureInE01OrE02()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "E01";
				Assert(!invoiceLine.IsProcedureInE01OrE02);

				invoiceLine.JI_Procedure = "01";
				Assert(!invoiceLine.IsProcedureInE01OrE02);

				invoiceLine.JI_Procedure = "1111E01";
				Assert(invoiceLine.IsProcedureInE01OrE02);
			});
		}

		public void TestProcedureNeedSpecialRateCharge()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "E01";
				Assert(!invoiceLine.ProcedureNeedSpecialRateCharge);

				invoiceLine.JI_Procedure = "40718E6";
				Assert(invoiceLine.ProcedureNeedSpecialRateCharge);

				invoiceLine.JI_Procedure = "40718E8";
				Assert(invoiceLine.ProcedureNeedSpecialRateCharge);

				invoiceLine.JI_Procedure = "40718E9";
				Assert(invoiceLine.ProcedureNeedSpecialRateCharge);
			});
		}

		public void TestJI_CessionFlagReadOnly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "40", "00", "0C9", "", "IMP");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.TAXFLAG, "01");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.TAXFLAG, "02");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				invoiceLine.JI_FormattedProcedure = "40000C9";
				AssertEquals("Test1: CessionFlagList is not empty (01, 02)", false, invoiceLine.JI_CessionFlag_ReadOnly);

				invoiceLine.JI_FormattedProcedure = "40000C8";
				AssertEquals("Test2: CessionFlagList is empty", true, invoiceLine.JI_CessionFlag_ReadOnly);
			});
		}

		public void TestSupplementaryInformation()
		{
			AssertEquals(100, invoiceLine.SupplementaryInformationInfo.MaxLength);
		}

		public void TestJI_TariffDefaultingCustomsSecondaryUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Export);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, Core.Constants.Weight.Kilograms);
			var uom = helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, Core.Constants.Weight.Milligrams);
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.ClassificationUOMType, Core.Constants.Weight.Pounds);

			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
				AssertEquals(ZString.Empty, invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals(ZString.Empty, invoiceLine.JI_CustomsThirdUnitQty);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals(Core.Constants.Weight.Milligrams, invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals(ZString.Empty, invoiceLine.JI_CustomsThirdUnitQty);

				invoiceLine.JI_Tariff = ZString.Empty;
				uom.ZZ8_UOM = "abcd";
				AssertLessThanOrEqualTo("If this test fails, update the string above, to make it longer than the max length", uom.ZZ8_UOM.Length, JobComInvoiceLine.Schema.JI_CustomsSecondUnitQtyMaxLength);
				AssertNoExceptionThrown("Should not throw an error that says the max length of JI_CustomsSecondUnitQty was exceeded", () => invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode);
				AssertEquals("abcd", invoiceLine.JI_CustomsSecondUnitQty);
			});
		}

		public void TestDefaultingCustomsUOMsFromUniversalTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram);
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "LPA");
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.CustomsUOM3Type, "ASVX");
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.CustomsUOM3Type, "HLT");
			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
				AssertEquals("Precondition", 2, invoiceLine.UniversalTariff.UnitsOfMeasure.Count(x => x.ZZ8_Type == Universal.Constants.UnitOfMeasureTypes.CustomsUOM3Type));
				AssertEquals(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, invoiceLine.JI_CustomsUnitQty);
				AssertEquals("LPA", invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("ASVX", invoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("HLT", invoiceLine.JI_CustomsFourthUnitQty);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals(ZString.Empty, invoiceLine.JI_CustomsUnitQty);
				AssertEquals(ZString.Empty, invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals(ZString.Empty, invoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals(ZString.Empty, invoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestJI_NetPrice()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "E01";
				AssertEquals("Procedure is E01", false, invoiceLine.JI_NetPriceInfo.ReadOnly);

				invoiceLine.JI_Procedure = ZString.Empty;
				AssertEquals("Procedure is empty", false, invoiceLine.JI_NetPriceInfo.ReadOnly);

				invoiceLine.JI_Procedure = "1111E01";
				AssertEquals("Concession is E01", true, invoiceLine.JI_NetPriceInfo.ReadOnly);
			});
		}

		public void TestJI_RX_NKNetPriceCurr_ReadOnly()
		{
			AssertEquals(true, invoiceLine.JI_RX_NKNetPriceCurrInfo.ReadOnly);
		}

		public void TestJI_RX_NKNetPriceCurr()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", ZString.Empty, invoiceLine.JI_RX_NKNetPriceCurr);
				invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals("Invoice Currency is Entered", Core.Constants.CurrencyCodes.EuropeanUnion, invoiceLine.JI_RX_NKNetPriceCurr);
			});
		}

		public override void TestChargeTypeList()
		{
			ICommonInvoice commonInvoice = invoiceLine;

			CombineAssertions(() =>
			{
				AssertSame("cached", commonInvoice.ChargeTypeList, commonInvoice.ChargeTypeList);

				AssertEquals("001, 002, 003, 004, 005, 006, 007, 008, 009, 010, 011, 012, 014, 015, 016, 017, 019, AIR, DIS, INP, OPF, SRC, SRN, SRS, STA, TCE", commonInvoice.ChargeTypeList.CodesAsString);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("ADD, AFT, DED, INS, OFT, ONS", commonInvoice.ChargeTypeList.CodesAsString);
			});
		}

		public void TestTaxes()
		{
			AssertType<JobComInvoiceLineTaxCollection>(invoiceLine.Taxes);
		}

		public void TestGetNewValidation_MSC()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobComInvoiceLineValidation>(invoiceLine.Validation);
		}

		public void TestGetNewValidation_IMP()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobComInvoiceLineValidation>(invoiceLine.Validation);
		}

		public void TestGetNewValidation_EXP()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobComInvoiceLineValidation>(invoiceLine.Validation);
		}

		public void TestGetNewValidation_WAD()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			AssertType<WarehouseAdjustmentJobComInvoiceLineValidation>(invoiceLine.Validation);
		}

		public void TestJI_TobaccoStamp_ReadOnly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			var de = Core.Constants.CountryCodes.Germany;
			var start = ZDateTime.MinSmallDateTimeValue;
			var end = ZDateTime.MaxSmallDateTimeValue;

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);

			Factory.Save();

			var tariffType = helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise);
			Factory.Save();

			var tariff = helper.CreateTariff(de, tariffType.PK, "08091998", start, end);
			helper.CreateTariffAttribute("ExciseType", CusLineTariffDetailHelper.ExciseTypes._10, tariff);

			var rateType = helper.CreateCusRateType(de, "10");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "1001", rateType.PK);
			Factory.Save();

			helper.CreateRate(tariff, rateCode.PK, start, end, dataGrouping: de);
			Factory.Save();

			tariffDetail.BZ_Type = Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise;

			CombineAssertions(() =>
			{
				tariffDetail.BZ_Tariff = "12345678";
				Assert("Should be readonly as the invalid tariff has no ExciseType attributes of value '10'", invoiceLine.JI_TobaccoStampInfo.ReadOnly);

				tariffDetail.BZ_Tariff = "08091998";
				Assert("Should NOT be readonly as the tariff has ExciseType attributes of value '10'", !invoiceLine.JI_TobaccoStampInfo.ReadOnly);
			});
		}

		public new void TestSupportingDocuments()
		{
			AssertType<SupportingDocumentCollection>(invoiceLine.SupportingDocuments);
		}

		public void TestEffectiveSupportingDocumentsFallsBack()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			helper.CreateNewOrGetExistingCusCodeType(importCodeType, "Document Type (EU Box 44 Imports)");

			var cusCode = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, importCodeType, "RPTI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Division, "Desc.", importCodeType, Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", importCodeType, Core.Constants.CountryCodes.Germany);
			cusCode.Attributes.AddNew(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Division, UniversalReferenceConstants.RefCusCodeListAttributes.Value.MiscellaneousDocument);
			cusCode.Attributes.AddNew("Level", "ITEM");

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(0, invoiceLine.EffectiveSupportingDocuments().Count);
				var supportDocument = invoiceHeader.SupportingDocuments.AddNew();
				supportDocument.CSI_Code = "RPTI";
				AssertEquals(0, invoiceLine.EffectiveSupportingDocuments().Count);

				supportDocument = invoiceLine.SupportingDocuments.AddNew();
				supportDocument.CSI_Code = "RPTI";
				AssertEquals(1, invoiceLine.EffectiveSupportingDocuments().Count);
			});
		}

		public new void TestIsSupportEmptyPackTypeAndValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Enterprise.Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Enterprise.Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			Factory.Save();

			var pack = declaration.Packages.AddNew();

			CombineAssertions(() =>
			{
				Assert("Precondition Pack is EU Package class", pack is Package);
				Assert("Precondition InvoiceLine is DE InvoiceLine class", invoiceLine is JobComInvoiceLine);
				Assert("Precondition SupportsChcPivot", invoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking);

				var npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
				var npbo = npbos.Cast<InvoiceLineCusLinkPackage>().FirstOrDefault();
				AssertEquals("Precondition IsLinked", false, npbo.IsLinked);

				npbo.IsLinked = true;
				AssertHasMessageErrorContaining(npbo.PackQtyInfo, "have not entered");

				pack.CW_PackType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;
				npbo.Validation.ValidatePackQty();
				AssertNoMessageErrorContaining(npbo.PackQtyInfo, "have not entered");

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				pack.CW_PackType = ZString.Empty;
				npbo.Validation.ValidatePackQty();
				AssertNoMessageErrorContaining(npbo.PackQtyInfo, "have not entered");

				pack.CW_PackType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;
				npbo.Validation.ValidatePackQty();
				AssertNoMessageErrorContaining(npbo.PackQtyInfo, "have not entered");
			});
		}

		public new void TestGetNewPackagesPivotCollection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Enterprise.Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Enterprise.Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			Factory.Save();

			var pack = declaration.Packages.AddNew();

			CombineAssertions(() =>
			{
				Assert("Precondition Pack is EU Package class", pack is Package);
				Assert("Precondition InvoiceLine is DE InvoiceLine class", invoiceLine is JobComInvoiceLine);
				Assert("Precondition SupportsChcPivot", invoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking);

				var npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
				var npbo = npbos.Cast<BaseCusLinkPackage>().FirstOrDefault();

				AssertEquals("Precondition IsLinked", false, npbo?.IsLinked);
				npbo.IsLinked = true;

				invoiceLine.PackagesPivot.RunPreSaveValidation();
				var pivot = invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
				AssertHasMessageErrorContaining(pivot.CHC_NumberOfPacksInfo, "have not entered");

				pack.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;

				invoiceLine.PackagesPivot.RunPreSaveValidation();
				pivot = invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
				AssertNoMessageErrorContaining(pivot.CHC_NumberOfPacksInfo, "have not entered");

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				pack.CW_PackType = ZString.Empty;
				npbo.Validation.ValidatePackQty();
				AssertNoMessageErrorContaining(npbo.PackQtyInfo, "have not entered");

				pack.CW_PackType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;
				npbo.Validation.ValidatePackQty();
				AssertNoMessageErrorContaining(npbo.PackQtyInfo, "have not entered");
			});
		}

		public void TestApportionedCharges()
		{
			AssertType<JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>>(invoiceLine.ApportionedCharges);
		}

		public void TestJI_CountryOfOrigin_UpdateJI_PrimaryPreferenceIfNeeded()
		{
			(var testHelper, var cusTariff, var rateCode, var tradeGroup) = CreateCusRefPreference();

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "123456789";
			CombineAssertions(() =>
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
				AssertEquals("ZG_CountryOfSupply is empty, PrimaryPreferenceList.Count == 1", "100", invoiceLine.JI_PrimaryPreference);

				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.AlandIslands;
				invoiceLine.JI_PrimaryPreference = "101";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
				AssertEquals("ZG_CountryOfSupply isn't empty, PrimaryPreferenceList.Count == 1", "101", invoiceLine.JI_PrimaryPreference);

				var preference150 = testHelper.CreatePreferenceForCountry("150", "150 Desc", Core.Constants.CountryCodes.Germany);
				var testRate2 = testHelper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, preferencePk: preference150.PK);
				testHelper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
				AssertEquals("ZG_CountryOfSupply is empty, PrimaryPreferenceList.Count == 2", "101", invoiceLine.JI_PrimaryPreference);
			});
		}

		public void TestZG_CountryOfSupply_UpdateJI_PrimaryPreferenceIfNeeded()
		{
			CreateCusRefPreference();

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "123456789";
			CombineAssertions(() =>
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
				invoiceLine.JI_PrimaryPreference = "101";
				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.AlandIslands;
				AssertEquals("ZG_CountryOfSupply <> JI_CountryOfOrigin, PrimaryPreferenceList.Count == 1", "100", invoiceLine.JI_PrimaryPreference);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
				invoiceLine.JI_PrimaryPreference = "101";
				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Botswana;
				AssertEquals("ZG_CountryOfSupply == JI_CountryOfOrigin, PrimaryPreferenceList.Count == 1", "101", invoiceLine.JI_PrimaryPreference);
			});
		}

		public void TestJI_PrimaryPreference_DefaultSupportingDocument()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "07095910100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			CreateRefCusCondition(helper, tariff, "115", "115", new ZString[] { UniversalReferenceConstants.SupportingDocumentTypes.N990, UniversalReferenceConstants.SupportingDocumentTypes.C990, UniversalReferenceConstants.SupportingDocumentTypes.D019 });
			CreateRefCusCondition(helper, tariff, "117", "140", new ZString[] { UniversalReferenceConstants.SupportingDocumentTypes.N990, UniversalReferenceConstants.SupportingDocumentTypes.N830 });

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			CombineAssertions(() =>
			{
				AssertEquals("Initially there are no docs", 0, invoiceLine.SupportingDocuments.Count);

				invoiceLine.JI_PrimaryPreference = "140";
				AssertEquals("N990 added", 1, invoiceLine.SupportingDocuments.Count);
				var docN990 = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Single(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.N990);

				invoiceLine.JI_PrimaryPreference = "115";
				AssertContainsExactElementsInAnyOrder("C990 and D019 added",
					new[] { UniversalReferenceConstants.SupportingDocumentTypes.N990, UniversalReferenceConstants.SupportingDocumentTypes.C990, UniversalReferenceConstants.SupportingDocumentTypes.D019 },
					invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Select(x => x.CSI_Code));
				AssertCollectionContains("N990 exists, not added again", docN990, invoiceLine.SupportingDocuments);
			});
		}

		public void TestJI_PrimaryPreference_EmptyTariff()
		{
			invoiceLine.JI_PrimaryPreference = "115";
			AssertEquals("Shouldn't crash when JI_Tariff is empty", 0, invoiceLine.SupportingDocuments.Count);
		}

		public void TestJI_PrimaryPreference_FilteredConditions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "07095910100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			CreateRefCusCondition(helper, tariff, "115", "115", new ZString[] { UniversalReferenceConstants.SupportingDocumentTypes.N990 }, ZDateTime.Today.AddDays(1));
			CreateRefCusCondition(helper, tariff, "115", "115", new ZString[] { UniversalReferenceConstants.SupportingDocumentTypes.N990 }, endDate: ZDateTime.Today.AddDays(-1));

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_PrimaryPreference = "115";
			AssertEquals("No conditions matched", 0, invoiceLine.SupportingDocuments.Count);
		}

		public void TestJI_PrimaryPreference_EmptyPrimaryPreference()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "07095910100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			CreateRefCusCondition(helper, tariff, "117", ZString.Empty, new ZString[] { UniversalReferenceConstants.SupportingDocumentTypes.N990, UniversalReferenceConstants.SupportingDocumentTypes.C990 });

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_PrimaryPreference = "140";
			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N990;
			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;

			CombineAssertions(() =>
			{
				AssertSequencesEqual("Initially there are {N990, N830}", new[] { supportingDocument1, supportingDocument2 }, invoiceLine.SupportingDocuments.Cast<SupportingDocument>());

				invoiceLine.JI_PrimaryPreference = ZString.Empty;
				AssertSequencesEqual("Condition with empty PreferenceCode is not matched, no docs added", new[] { supportingDocument2 }, invoiceLine.SupportingDocuments.Cast<SupportingDocument>());
				AssertEquals("N990 deleted", true, supportingDocument1.IsDeleted);
			});
		}

		public void TestJI_PrimaryPreference_RemoveAndDeleteSupportingDocuments()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Universal.Constants.TariffTypes.Import);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, tariffType.PK, "07095910100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			CreateRefCusCondition(helper, tariff, "117", "140", new ZString[] { UniversalReferenceConstants.SupportingDocumentTypes.C990, UniversalReferenceConstants.SupportingDocumentTypes.D019 });
			CreateRefCusCondition(helper, tariff, "115", "115", new ZString[] { UniversalReferenceConstants.SupportingDocumentTypes.D019 });

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N990;
			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N990;
			var supportingDocument3 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N830;
			var supportingDocument4 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.D019;
			var supportingDocument5 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument5.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.C990;

			CombineAssertions(() =>
			{
				invoiceLine.JI_PrimaryPreference = "140";
				AssertContainsExactElementsInAnyOrder("JI_PrimaryPreference '140', {N830, D019, C990} left", new[] { supportingDocument3, supportingDocument4, supportingDocument5 }, invoiceLine.SupportingDocuments);
				AssertEquals("N990 #1 deleted", true, supportingDocument1.IsDeleted);
				AssertEquals("N990 #2 deleted", true, supportingDocument2.IsDeleted);

				invoiceLine.JI_PrimaryPreference = "115";
				AssertContainsExactElementsInAnyOrder("JI_PrimaryPreference '115', {N830, D019} left", new[] { supportingDocument3, supportingDocument4 }, invoiceLine.SupportingDocuments.Cast<SupportingDocument>());
				AssertEquals("C990 deleted", true, supportingDocument5.IsDeleted);

				invoiceLine.JI_PrimaryPreference = "141";
				AssertSequencesEqual("JI_PrimaryPreference '141', N830 left", new[] { supportingDocument3 }, invoiceLine.SupportingDocuments.Cast<SupportingDocument>());
				AssertEquals("D019 deleted", true, supportingDocument4.IsDeleted);
			});
		}

		public void TestJI_BondedWhsUnitQty_ReadOnly()
		{
			CombineAssertions(() =>
			{
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					invoiceLine.EntryInstruction.CEI_Style = style;
					AssertEquals($"CEI_Style {style}", !entryInstruction.IsInwardMovementApplicable, invoiceLine.JI_BondedWhsUnitQtyInfo.ReadOnly);
				}
			});
		}

		public void TestJI_BondedWhsUnitQty_ReadOnly_WAD()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertEquals("JE_MessageType 'WAD'", false, invoiceLine.JI_BondedWhsUnitQtyInfo.ReadOnly);
		}

		public void TestJI_BondedWhsUnitQty_ReadOnly_IsOutOfWarehouseWarehousingProcedureCode()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
			AssertEquals("isOutOfWarehouseWarehousingProcedureCode", false, invoiceLine.JI_BondedWhsUnitQtyInfo.ReadOnly);
		}

		public void TestJI_BondedWhsUnitQty_ReadOnly_IsOutOfInwardProcessingProcedureCode()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = isOutOfInwardProcessingProcedureCode;
			AssertEquals("isOutOfInwardProcessingProcedureCode", false, invoiceLine.JI_BondedWhsUnitQtyInfo.ReadOnly);
		}

		public void TestJI_BondedWhsQuantity_ReadOnly()
		{
			CombineAssertions(() =>
			{
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					entryInstruction.CEI_Style = style;
					AssertEquals($"CEI_Style {style}", !entryInstruction.IsInwardMovementApplicable, invoiceLine.JI_BondedWhsQuantityInfo.ReadOnly);
				}
			});
		}

		public void TestJI_BondedWhsQuantity_ReadOnly_WAD()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertEquals("JE_MessageType 'WAD'", false, invoiceLine.JI_BondedWhsQuantityInfo.ReadOnly);
		}

		public void TestJI_BondedWhsQuantity_ReadOnly_IsOutOfWarehouseWarehousingProcedureCode()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
			AssertEquals("isOutOfWarehouseWarehousingProcedureCode", false, invoiceLine.JI_BondedWhsQuantityInfo.ReadOnly);
		}

		public void TestClearInwardMovementQuantityIfRequired()
		{
			CombineAssertions(() =>
			{
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					entryInstruction.CEI_Style = style;
					AssertInwardMovementQuantityCleared($"CEI_Style {style}", !entryInstruction.IsInwardMovementApplicable);
				}
			});

			void AssertInwardMovementQuantityCleared(string assertMessage, bool expectedCleared)
			{
				invoiceLine.JI_BondedWhsQuantity = 100m;
				invoiceLine.JI_BondedWhsUnitQty = Core.Constants.Weight.Kilograms;
				invoiceLine.ClearInwardMovementQuantityIfRequired();
				if (expectedCleared)
				{
					AssertEquals($"{assertMessage} -> JI_BondedWhsQuantity", ZDecimal.Zero, invoiceLine.JI_BondedWhsQuantity);
					AssertEquals($"{assertMessage} -> JI_BondedWhsUnitQty", ZString.Empty, invoiceLine.JI_BondedWhsUnitQty);
				}
				else
				{
					AssertEquals($"{assertMessage} -> JI_BondedWhsQuantity", 100m, invoiceLine.JI_BondedWhsQuantity);
					AssertEquals($"{assertMessage} -> JI_BondedWhsUnitQty", Core.Constants.Weight.Kilograms, invoiceLine.JI_BondedWhsUnitQty);
				}
			}
		}

		public void TestJI_DescriptionMaxLength()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(240, invoiceLine.JI_DescriptionInfo.MaxLength);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("Export in transition period", 280, invoiceLine.JI_DescriptionInfo.MaxLength);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Export after transition period", 512, invoiceLine.JI_DescriptionInfo.MaxLength);
				}
			});
		}

		public new void TestCifVsCustomsvalue()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoiceLine.JI_LinePrice = 1000m;

			CombineAssertions(() =>
			{
				var insuranceCharge = invoiceLine.Charges.AddNew();
				insuranceCharge.J7_ChargeType = OverseasInsuranceCode;
				insuranceCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
				insuranceCharge.J7_Amount = 500m;
				Assert("Pre-Req - insurance is dutiable", insuranceCharge.J7_IsDutiable);

				var chargeFactory = (DEImportIncoTermAndCustomsChargeFactory)dec.IncoTermAndChargeFactory;
				var freightOutsideEuCharge = invoiceLine.Charges.AddNew();
				freightOutsideEuCharge.J7_ChargeType = chargeFactory.FreightToEUBorderCode;
				freightOutsideEuCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
				freightOutsideEuCharge.J7_Amount = 100m;
				Assert("Pre-Req - freight before is dutiable", freightOutsideEuCharge.J7_IsDutiable);

				var freightAfterEuCharge = invoiceLine.Charges.AddNew();
				freightAfterEuCharge.J7_ChargeType = chargeFactory.FreightAfterEUBorderCode;
				freightAfterEuCharge.J7_IsDutiable = false;
				freightAfterEuCharge.J7_IsGSTApplicable = true;
				freightAfterEuCharge.J7_IsIncludedInITOT = false;
				freightAfterEuCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
				freightAfterEuCharge.J7_Amount = 50m;
				Assert("Pre-Req - freight after is NOT dutiable", !freightAfterEuCharge.J7_IsDutiable);
				Assert("Pre-Req - freight after is NOT includedInITOT", !freightAfterEuCharge.J7_IsIncludedInITOT);

				AssertEquals("CIF = 1000+500+100+50 = $1650", 1650m, invoiceLine.JI_Calc_CIF);
				AssertEquals("Customs value = 1000+500+100 = $1600", 1600m, invoiceLine.JI_CustomsValue);
			});
		}

		public void TestJI_OA_ExporterAddress_Caption()
		{
			AssertEquals("Consignor", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_OA_ExporterAddressInfo).Caption);
		}

		public void TestZG_ReimportDate()
		{
			var reimportDate = new ZDate(2020, 10, 12);
			invoiceLine.ZG_ReimportDate = reimportDate;
			AssertEquals(invoiceLine.ZG_ReimportDateInfo.Value, reimportDate);
		}

		public void TestZG_ReimportDate_Caption()
		{
			AssertEquals("Reimport Date", invoiceLine.ZG_ReimportDateInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestZG_UsualReplacement()
		{
			invoiceLine.ZG_UsualReplacement = true;
			AssertEquals(invoiceLine.ZG_UsualReplacementInfo.Value, true);
		}

		public void TestZG_UsualReplacement_Caption()
		{
			AssertEquals("Usual Replacement", invoiceLine.ZG_UsualReplacementInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestJI_CustomsValue_Export()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.JZ_InvoiceAmount = 1200.00m;
			invoiceHeader.JZ_Weight = 1200m;
			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			var currCode = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currCode;

			var charge1 = invoiceHeader.Charges.AddNew(OverseasFreightCode, 160.00m, currCode);
			charge1.J7_IsIncludedInITOT = true;
			charge1.J7_IsDutiable = true;

			var charge2 = invoiceHeader.Charges.AddNew(OverseasInsuranceCode, 40.00m, currCode);
			charge2.J7_IsIncludedInITOT = true;
			charge2.J7_IsDutiable = true;

			invoiceLine.JI_LinePrice = 1200m;
			invoiceLine.JI_Weight = 1200m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			declaration.ResumeApportionment();
			AssertEquals(1200m, invoiceLine.JI_CustomsValue);
		}

		public void TestJI_RN_NKCountryOfExport_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_RN_NKCountryOfExportInfo, multipleResourceKey: null, "Country/Region of Export", "Ctry./Rgn. of Exp.", "Ctry./Rgn. of Export");
		}

		public void TestClearCountryOfOriginIfNeeded_Import()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.France;
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				invoiceLine.JI_CEI = entryInstruction2.PK;
				AssertEquals("Not cleared as CEI_Style = LÜZ", Core.Constants.CountryCodes.France, invoiceLine.JI_RN_NKCountryOfExport);

				invoiceLine.JI_CEI = entryInstruction.PK;
				AssertEquals("Cleared as CEI_Style != LÜZ", ZString.Empty, invoiceLine.JI_RN_NKCountryOfExport);

				invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Ireland;
				invoiceLine.JI_CEI = ZGuid.Empty;
				AssertEquals("Cleared as no entry instruction", ZString.Empty, invoiceLine.JI_RN_NKCountryOfExport);
			});
		}

		public void TestClearCountryOfOriginIfNeeded_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.France;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000110;
			invoiceLine.JI_CEI = entryInstruction2.PK;
			AssertEquals("Not cleared for export declaration", Core.Constants.CountryCodes.France, invoiceLine.JI_RN_NKCountryOfExport);
		}

		public void TestJI_CustomDate1_Caption()
		{
			AssertEquals("Decisive Date", InvoiceLine.JI_CustomDate1Info.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestClearDecisiveDateIfNeeded()
		{
			CombineAssertions(() =>
			{
				var currentDate = ZDate.Today;
				invoiceLine.JI_CustomDate1 = currentDate;
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				invoiceLine.JI_CEI = entryInstruction2.PK;
				AssertEquals("Not cleaned as CEI_Style = LÜZ", currentDate, invoiceLine.JI_CustomDate1);

				invoiceLine.JI_CEI = entryInstruction.PK;
				AssertEquals("Cleaned as CEI_Style != LÜZ", ZDate.Empty, invoiceLine.JI_CustomDate1);

				invoiceLine.JI_CustomDate1 = currentDate.AddDays(-1);
				invoiceLine.JI_CEI = ZGuid.Empty;
				AssertEquals("Cleaned as no entry instruction", ZDate.Empty, invoiceLine.JI_CustomDate1);
			});
		}

		public void TestClearFields_WarehouseAdjustment()
		{
			invoiceLine.JI_Tariff = "AA";
			invoiceLine.JI_SupplementaryCode1 = "BB";
			invoiceLine.JI_SupplementaryCode2 = "CC";
			invoiceLine.JI_Description = "DD";
			invoiceLine.JI_CountryOfOrigin = "EE";
			invoiceLine.JI_PrimaryPreference = "FF";
			invoiceLine.JI_StateOrRegionOfOrigin = "12";

			invoiceLine.ZG_CountryOfSupply = "GG";
			invoiceLine.JI_LinePrice = 1.1M;
			invoiceLine.JI_CessionFlag = "HH";
			invoiceLine.JI_PartNo = "II";
			invoiceLine.JI_RH_NKCommodity_Code = "JJ";
			invoiceLine.JI_ConcessionOrder = "KK";
			invoiceLine.ZG_QuotaQty = 2;
			invoiceLine.ZG_QuotaUQ = "LL";
			invoiceLine.SupplementaryInformation = "MM";

			invoiceLine.JI_Weight = 3.3M;
			invoiceLine.JI_WeightUQ = "NN";
			invoiceLine.JI_NetWeight = 4.4M;
			invoiceLine.JI_NetWeightUQ = "OO";
			invoiceLine.JI_CustomsQuantity = 5.5M;
			invoiceLine.JI_CustomsUnitQty = "PP";
			invoiceLine.JI_CustomsSecondQuantity = 6.6M;
			invoiceLine.JI_CustomsSecondUnitQty = "QQ";
			invoiceLine.JI_CustomsThirdQuantity = 7.7M;
			invoiceLine.JI_CustomsThirdUnitQty = "RR";
			invoiceLine.JI_CustomsFourthQuantity = 8.8M;
			invoiceLine.JI_CustomsFourthUnitQty = "SS";
			invoiceLine.JI_TobaccoStamp = "TT";

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
				AssertEquals("JI_Tariff", ZString.Empty, invoiceLine.JI_Tariff);
				AssertEquals("JI_SupplementaryCode1", ZString.Empty, invoiceLine.JI_SupplementaryCode1);
				AssertEquals("JI_SupplementaryCode2", ZString.Empty, invoiceLine.JI_SupplementaryCode2);
				AssertEquals("JI_Description", ZString.Empty, invoiceLine.JI_Description);
				AssertEquals("JI_CountryOfOrigin", ZString.Empty, invoiceLine.JI_CountryOfOrigin);
				AssertEquals("JI_PrimaryPreference", ZString.Empty, invoiceLine.JI_PrimaryPreference);
				AssertEquals("JI_StateOrRegionOfOrigin", ZString.Empty, invoiceLine.JI_StateOrRegionOfOrigin);

				AssertEquals("ZG_CountryOfSupply", ZString.Empty, invoiceLine.ZG_CountryOfSupply);
				AssertEquals("JI_LinePrice", ZDecimal.Zero, invoiceLine.JI_LinePrice);
				AssertEquals("JI_CessionFlag", ZString.Empty, invoiceLine.JI_CessionFlag);
				AssertEquals("JI_PartNo", ZString.Empty, invoiceLine.JI_PartNo);
				AssertEquals("JI_RH_NKCommodity_Code", ZString.Empty, invoiceLine.JI_RH_NKCommodity_Code);
				AssertEquals("JI_ConcessionOrder", ZString.Empty, invoiceLine.JI_ConcessionOrder);
				AssertEquals("ZG_QuotaQty", 0, invoiceLine.ZG_QuotaQty);
				AssertEquals("ZG_QuotaUQ", ZString.Empty, invoiceLine.ZG_QuotaUQ);
				AssertEquals("SupplementaryInformation", ZString.Empty, invoiceLine.SupplementaryInformation);

				AssertEquals("JI_Weight", ZDecimal.Zero, invoiceLine.JI_Weight);
				AssertEquals("JI_WeightUQ", ZString.Empty, invoiceLine.JI_WeightUQ);
				AssertEquals("JI_NetWeight", ZDecimal.Zero, invoiceLine.JI_NetWeight);
				AssertEquals("JI_NetWeightUQ", ZString.Empty, invoiceLine.JI_NetWeightUQ);
				AssertEquals("JI_CustomsQuantity", ZDecimal.Zero, invoiceLine.JI_CustomsQuantity);
				AssertEquals("JI_CustomsUnitQty", ZString.Empty, invoiceLine.JI_CustomsUnitQty);
				AssertEquals("JI_CustomsSecondQuantity", ZDecimal.Zero, invoiceLine.JI_CustomsSecondQuantity);
				AssertEquals("JI_CustomsSecondUnitQty", ZString.Empty, invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("JI_CustomsThirdQuantity", ZDecimal.Zero, invoiceLine.JI_CustomsThirdQuantity);
				AssertEquals("JI_CustomsThirdUnitQty", ZString.Empty, invoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("JI_CustomsFourthQuantity", ZDecimal.Zero, invoiceLine.JI_CustomsFourthQuantity);
				AssertEquals("JI_CustomsFourthUnitQty", ZString.Empty, invoiceLine.JI_CustomsFourthUnitQty);
				AssertEquals("JI_TobaccoStamp", ZString.Empty, invoiceLine.JI_TobaccoStamp);
			});
		}

		public void TestOutwardMRN_Caption()
		{
			AssertEquals("Outward MRN", DataBoundResourceStrings.GetDataForProperty(invoiceLine.OutwardMRNInfo).Caption);
		}

		public void TestOutwardMRN_MaxLength()
		{
			AssertEquals(35, invoiceLine.OutwardMRNInfo.MaxLength);
		}

		public void TestOutwardMRN_Getter()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CusEntryNumber doesn't exist", ZString.Empty, invoiceLine.OutwardMRN);

				var cusEntryNumber = CusEntryNumber.New(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				cusEntryNumber.CE_EntryNum = "DE00012345";
				AssertEquals("CusEntryNumber exists", "DE00012345", invoiceLine.OutwardMRN);

				cusEntryNumber.Delete();
				AssertEquals("CusEntryNumber deleted", ZString.Empty, invoiceLine.OutwardMRN);
			});
		}

		public void TestOutwardMRN_Setter()
		{
			CombineAssertions(() =>
			{
				var cusEntryNumber = CusEntryNumber.Load(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				AssertNull("Initially CusEntryNumber doesn't exist", cusEntryNumber);

				invoiceLine.OutwardMRN = "DE1234567890";
				cusEntryNumber = CusEntryNumber.Load(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				AssertEquals("CusEntryNumber created", "DE1234567890", cusEntryNumber.CE_EntryNum);

				var cusEntryNumberPK = cusEntryNumber.PK;
				invoiceLine.OutwardMRN = "DE0000000000";
				cusEntryNumber = CusEntryNumber.Load(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				AssertEquals("Same CusEntryNumber kept instead of creating a new one", cusEntryNumberPK, cusEntryNumber.PK);
				AssertEquals("CE_EntryNum updated", "DE0000000000", cusEntryNumber.CE_EntryNum);

				cusEntryNumber.Delete();
				invoiceLine.OutwardMRN = "DE1111111111";
				cusEntryNumber = CusEntryNumber.Load(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				AssertNotEquals("New CusEntryNumber is created", cusEntryNumberPK, cusEntryNumber.PK);
				AssertEquals("CE_EntryNum of new CusEntryNumber", "DE1111111111", cusEntryNumber.CE_EntryNum);
				Factory.Save();

				AssertEquals("After saved, invoiceLine.HasChanges = False", false, invoiceLine.HasChanges);
				invoiceLine.OutwardMRN = "DE2222222222";
				AssertEquals("OutwardMRN updated, invoiceLine.HasChanges = True", true, invoiceLine.HasChanges);
			});
		}

		public void TestOutwardDecisiveDate_Caption()
		{
			AssertEquals("Decisive Date", DataBoundResourceStrings.GetDataForProperty(invoiceLine.OutwardDecisiveDateInfo).Caption);
		}

		public void TestOutwardDecisiveDate_Getter()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CusEntryNumber doesn't exist", ZDateTime.Empty, invoiceLine.OutwardDecisiveDate);

				var cusEntryNumber = CusEntryNumber.New(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				cusEntryNumber.CE_ExpiryDate = new ZDateTime(2020, 12, 26);
				AssertEquals("CusEntryNumber exists", new ZDateTime(2020, 12, 26), invoiceLine.OutwardDecisiveDate);

				cusEntryNumber.Delete();
				AssertEquals("CusEntryNumber deleted", ZDateTime.Empty, invoiceLine.OutwardDecisiveDate);
			});
		}

		public void TestOutwardDecisiveDate_Setter()
		{
			CombineAssertions(() =>
			{
				var cusEntryNumber = CusEntryNumber.Load(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				AssertNull("Initially CusEntryNumber doesn't exist", cusEntryNumber);

				invoiceLine.OutwardDecisiveDate = new ZDateTime(2020, 01, 18);
				cusEntryNumber = CusEntryNumber.Load(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				AssertEquals("CusEntryNumber created", new ZDateTime(2020, 01, 18), cusEntryNumber.CE_ExpiryDate);

				var cusEntryNumberPK = cusEntryNumber.PK;
				invoiceLine.OutwardDecisiveDate = new ZDateTime(2021, 05, 21);
				cusEntryNumber = CusEntryNumber.Load(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				AssertEquals("Same CusEntryNumber kept instead of creating a new one", cusEntryNumberPK, cusEntryNumber.PK);
				AssertEquals("CE_ExpiryDate updated", new ZDateTime(2021, 05, 21), cusEntryNumber.CE_ExpiryDate);

				cusEntryNumber.Delete();
				invoiceLine.OutwardDecisiveDate = new ZDateTime(2021, 12, 25);
				cusEntryNumber = CusEntryNumber.Load(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				AssertNotEquals("New CusEntryNumber is created", cusEntryNumberPK, cusEntryNumber.PK);
				AssertEquals("CE_ExpiryDate of new CusEntryNumber", new ZDateTime(2021, 12, 25), cusEntryNumber.CE_ExpiryDate);
				Factory.Save();

				AssertEquals("After saved, invoiceLine.HasChanges = False", false, invoiceLine.HasChanges);
				invoiceLine.OutwardDecisiveDate = new ZDateTime(2022, 01, 10);
				AssertEquals("OutwardDecisiveDate updated, invoiceLine.HasChanges = True", true, invoiceLine.HasChanges);
			});
		}

		public override void TestJI_BondedWhsQuantityCaption()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Warehouse Quantity", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWhsQuantityInfo).Caption);
				AssertEquals("Warehouse Qty.", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWhsQuantityInfo).MediumCaption);
				AssertEquals("Whs. Qty.", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWhsQuantityInfo).ShortCaption);
			});
		}

		public override void TestJI_BondedWhsUnitQtyCaption()
		{
			AssertEquals("Warehouse Unit of Quantity", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWhsUnitQtyInfo).Caption);
		}

		public override void TestJI_PreviousEntryNumberCaption()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Inbound Reg. No.", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_PreviousEntryNumberInfo).Caption);
				AssertEquals("Previous Entry No.", invoiceLine.JI_PreviousEntryNumberBondedWarehouseCaption.Caption);
				AssertEquals("Prev. Entry No.", invoiceLine.JI_PreviousEntryNumberBondedWarehouseCaption.MediumCaption);
				AssertEquals("Prev. Entry", invoiceLine.JI_PreviousEntryNumberBondedWarehouseCaption.ShortCaption);
			});
		}

		public override void TestJI_PreviousEntryLineNumberCaption()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Inbound Reg. Pos.", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_PreviousEntryLineNumberInfo).Caption);
				AssertEquals("Previous Entry Line", invoiceLine.JI_PreviousEntryLineNumberBondedWarehouseCaption.Caption);
				AssertEquals("Prev. Entry Line", invoiceLine.JI_PreviousEntryLineNumberBondedWarehouseCaption.MediumCaption);
				AssertEquals("Prev. Line", invoiceLine.JI_PreviousEntryLineNumberBondedWarehouseCaption.ShortCaption);
			});
		}

		public void TestJI_BondedWHSOrderNumberCaption()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Warehouse Order Number", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWHSOrderNumberInfo).Caption);
				AssertEquals("Whs. Order No.", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWHSOrderNumberInfo).MediumCaption);
				AssertEquals("Whs. Order", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWHSOrderNumberInfo).ShortCaption);
			});
		}

		public void TestJI_BondedWHSOrderLineNumberCaption()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Warehouse Order Line", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWHSOrderLineNumberInfo).Caption);
				AssertEquals("Whs. Order Line", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWHSOrderLineNumberInfo).MediumCaption);
				AssertEquals("Whs. Line", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_BondedWHSOrderLineNumberInfo).ShortCaption);
			});
		}

		public override void TestEffectiveCountryOfOrigin()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_CountryOfOrigin = "AU";
				AssertEquals("IMP and ZG_CountryOfSupply is empty", "AU", invoiceLine.EffectiveCountryOfOrigin);

				invoiceLine.ZG_CountryOfSupply = "NZ";
				AssertEquals("IMP and ZG_CountryOfSupply isn't empty", "NZ", invoiceLine.EffectiveCountryOfOrigin);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				AssertEquals("EXP and ZG_CountryOfSupply isn't empty", "AU", invoiceLine.EffectiveCountryOfOrigin);
			});
		}

		public void TestJI_PreviousEntryNumber()
		{
			AssertEquals(35, invoiceLine.JI_PreviousEntryNumberInfo.MaxLength);
		}

		public void TestBondedWhsQuantityAdjustmentResourceDataString()
		{
			AssertEquals("Outward Qty.", invoiceLine.BondedWhsQuantityAdjustmentResourceDataString.Caption);
		}

		public void TestBondedWhsUnitQuantityAdjustmentResourceDataString()
		{
			AssertEquals("Outward Qty. Unit", invoiceLine.BondedWhsUnitQuantityAdjustmentResourceDataString.Caption);
		}

		public void TestZG_CountryOfSupply_Export()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_PrimaryPreference = "300";
			AssertEquals("JI_PrimaryPreference > 200  & ZG_CountryOfSupply = empty & export type", ZString.Empty, invoiceLine.ZG_CountryOfSupply);
		}

		public void TestZG_CountryOfSupply_Import()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

				invoiceLine.JI_PrimaryPreference = "ABC";
				invoiceLine.JI_CountryOfOrigin = "EU";
				AssertEquals("JI_PrimaryPreference = NAN & ZG_CountryOfSupply = empty, ZG_CountryOfSupply not updated", ZString.Empty, invoiceLine.ZG_CountryOfSupply);

				invoiceLine.JI_PrimaryPreference = "100";
				AssertEquals("JI_PrimaryPreference < 200 & ZG_CountryOfSupply = empty, ZG_CountryOfSupply not updated", ZString.Empty, invoiceLine.ZG_CountryOfSupply);

				invoiceLine.JI_PrimaryPreference = "300";
				AssertEquals("JI_PrimaryPreference > 200 & ZG_CountryOfSupply = empty, ZG_CountryOfSupply updated", "EU", invoiceLine.ZG_CountryOfSupply);

				invoiceLine.JI_PrimaryPreference = "300";
				invoiceLine.JI_CountryOfOrigin = "DE";
				AssertEquals("JI_PrimaryPreference > 200 & ZG_CountryOfSupply <> empty, ZG_CountryOfSupply not updated", "EU", invoiceLine.ZG_CountryOfSupply);

				invoiceLine.JI_PrimaryPreference = "100";
				AssertEquals("JI_PrimaryPreference < 200 & ZG_CountryOfSupply <> empty, ZG_CountryOfSupply not updated", "EU", invoiceLine.ZG_CountryOfSupply);
			});
		}

		public void TestJI_FormattedProcedure()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				invoiceLine.JI_FormattedProcedure = "12345678";
				AssertEquals("Set JI_FormattedProcedure to '12345678', JI_FormattedProcedure", "12 34 567", invoiceLine.JI_FormattedProcedure);

				invoiceLine.JI_FormattedProcedure = "123.4567";
				AssertEquals("Set JI_FormattedProcedure to '123.4567', JI_FormattedProcedure", "12 34 567", invoiceLine.JI_FormattedProcedure);
			});
		}

		public void TestJI_Calc_ValueForVat()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var charge1 = invoiceLine.Charges.AddNew();
			charge1.J7_Amount = 100m;
			charge1.J7_ChargeType = ImportChargeCodeList.Codes.OPF;
			charge1.J7_IsDutiable = false;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_IsIncludedInITOT = true;
			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_Amount = 50m;
			charge2.J7_IsDutiable = false;
			charge2.J7_IsGSTApplicable = true;
			charge1.J7_IsIncludedInITOT = false;
			var charge3 = invoiceLine.Charges.AddNew();
			charge3.J7_Amount = 300m;
			charge3.J7_IsDutiable = true;
			charge3.J7_IsGSTApplicable = true;
			var charge4 = invoiceLine.Charges.AddNew();
			charge4.J7_Amount = 300m;
			charge4.J7_IsDutiable = false;
			charge4.J7_IsGSTApplicable = false;

			CombineAssertions(() =>
			{
				AssertEquals("Has charge 'OPF'", 150m, invoiceLine.JI_Calc_ValueForVat);

				charge1.J7_ChargeType = ImportChargeCodeList.Codes.AIR;
				AssertEquals("No charge 'OPF'", 100m, invoiceLine.JI_Calc_ValueForVat);
			});
		}

		public void TestSupportingDocuments_SequenceNumber()
		{
			CombineAssertions(() =>
			{
				var doc1 = invoiceLine.SupportingDocuments.AddNew();
				AssertEquals("Doc 1", (ZInt)1, doc1.CSI_LineNo);
				var doc2 = invoiceLine.SupportingDocuments.AddNew();
				AssertEquals("Doc 2", (ZInt)2, doc2.CSI_LineNo);
				var doc3 = invoiceLine.SupportingDocuments.AddNew();
				AssertEquals("Doc 3", (ZInt)3, doc3.CSI_LineNo);
				invoiceLine.SupportingDocuments.RemoveAndDelete(doc2);
				AssertEquals("Renumbered doc 3 to 2", (ZInt)2, doc3.CSI_LineNo);
			});
		}

		public void TestEvaluateConditionValue_IsImportIntoBondedWarehouse()
		{
			AssertEvaluateConditionValue(true, x => x.ZZ6_IntoWarehouse = UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
		}

		public void TestEvaluateConditionValue_IsImportIntoInwardProcessing()
		{
			AssertEvaluateConditionValue(true, x => x.ZZ6_IntoInwardProcessing = UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
		}

		public void TestEvaluateConditionValue_IsNotImportIntoBondedWareHouseOrInwardProcessing()
		{
			AssertEvaluateConditionValue(false);
		}

		public void TestAllLinkedPackagesHaveSamePackTypeAndMarks_MultiplePackagesPivots()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "M";
			var packages = declaration.Bills[0].PackingGroups[0].Packages;
			var package1 = packages[0];
			package1.CW_PackType = "CT";
			package1.CW_MarksAndNos = "MARKS AND NUMBERS";
			var package2 = packages.AddNew();
			package2.CW_PackType = "NE";
			package2.CW_MarksAndNos = "MARKS AND NUMBERS";
			var notLinkedPackage = packages.AddNew();
			notLinkedPackage.CW_PackType = "NE";
			notLinkedPackage.CW_MarksAndNos = "MARKS AND NUMBERS2";
			var linePackageCollection = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage1 = linePackageCollection[0];
			lineLinkPackage1.Package = package1;
			lineLinkPackage1.IsLinked = true;
			var lineLinkPackage2 = linePackageCollection.AddNew();
			lineLinkPackage2.Package = package2;
			lineLinkPackage2.IsLinked = true;
			var notLinkedLineLinkPackage = linePackageCollection.AddNew();
			notLinkedLineLinkPackage.Package = notLinkedPackage;

			CombineAssertions(() =>
			{
				AssertEquals("LineLinkPackage1: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'NE', 'MARKS AND NUMBERS'", false, invoiceLine.AllLinkedPackagesHaveSamePackTypeAndMarks);

				package2.CW_PackType = "CT";
				AssertEquals("LineLinkPackage1: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'CT', 'MARKS AND NUMBERS'", true, invoiceLine.AllLinkedPackagesHaveSamePackTypeAndMarks);

				package2.CW_MarksAndNos = "NOT EQUAL";
				AssertEquals("LineLinkPackage1: Package1: 'CT', 'MARKS AND NUMBERS'; Package2: 'CT', 'NOT EQUAL'", false, invoiceLine.AllLinkedPackagesHaveSamePackTypeAndMarks);
			});
		}

		public void TestAllLinkedPackagesHaveSamePackTypeAndMarks_EmptyPackagesPivot()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Assert(invoiceLine.AllLinkedPackagesHaveSamePackTypeAndMarks);
		}

		public void TestAllLinkedPackagesHaveSamePackTypeAndMarks_SinglePackagesPivot()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "M";
			var package = declaration.Bills[0].PackingGroups[0].Packages[0];
			package.CW_PackType = "CT";
			package.CW_MarksAndNos = "MARKS AND NUMBERS";
			var lineLinkPackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			lineLinkPackage.Package = package;
			lineLinkPackage.IsLinked = true;
			Assert(invoiceLine.AllLinkedPackagesHaveSamePackTypeAndMarks);
		}

		public void TestDgSubstance()
		{
			var dgSubstance1001 = Factory.New<UNDGSubstance>();
			dgSubstance1001.DG_UNNO = "1001";
			var dgSubstance1002 = Factory.New<UNDGSubstance>();
			dgSubstance1002.DG_UNNO = "1002";

			CombineAssertions(() =>
			{
				AssertEquals("DgSubstance field is empty", ZString.Empty, invoiceLine.DgSubstance);

				invoiceLine.UNDGs.AddNew().DI_DG = dgSubstance1002.PK;
				AssertEquals("DgSubstance field one substance", "1002", invoiceLine.DgSubstance);

				invoiceLine.UNDGs.AddNew().DI_DG = dgSubstance1001.PK;
				AssertEquals("DgSubstance field two substances", "1001;1002", invoiceLine.DgSubstance);
			});
		}

		public void TestDgSubstanceInfoRefreshBinding()
		{
			bool refreshBindingCalled = false;
			invoiceLine.DgSubstanceInfo.ValueChanged += (s, e) => refreshBindingCalled = true;

			invoiceLine.UNDGs.AddNew();
			AssertEquals(true, refreshBindingCalled);
		}

		public void TestJI_OA_ConsigneeAddress_GetDefaultAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ConsigneeAddress_ZAddress.OrgPK = orgHeader.PK;

			AssertEquals(orgHeader.MainAddress.PK, invoiceLine.JI_OA_ConsigneeAddress);
		}

		public void TestJI_OA_ExporterAddress_GetDefaultAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ExporterAddress_ZAddress.OrgPK = orgHeader.PK;

			AssertEquals(orgHeader.MainAddress.PK, invoiceLine.JI_OA_ExporterAddress);
		}

		public void TestJI_CountryOfOrigin_UpdateJI_StateOrRegionOfOrigin_WhenCountryNotGermany()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Ireland;
				AssertEquals("99", invoiceLine.JI_StateOrRegionOfOrigin);
			});
		}

		public void TestJI_CountryOfOrigin_UpdateJI_StateOrRegionOfOrigin_ValueNotSetWhenCountryIsGermany()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
				AssertEquals(ZString.Empty, invoiceLine.JI_StateOrRegionOfOrigin);
			});
		}

		public void TestJI_StateOrRegionOfOrigin_Caption()
		{
			CombineAssertions(() =>
			{
				var targetInfo = invoiceLine.JI_StateOrRegionOfOriginInfo;
				AssertEquals("Short", "State", targetInfo.GetAttribute<ResourceStringDataAttribute>().ShortCaption);
				AssertEquals("Medium", "Federal State", targetInfo.GetAttribute<ResourceStringDataAttribute>().MediumCaption);
				AssertEquals("Caption", "Origin Federal State", targetInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
			});
		}

		public void TestLanguageForTariffDescription()
		{
			AssertEquals("DE-DE", invoiceLine.LanguageForTariffDescription);
		}

		public void TestIncludedInUniversalXML()
		{
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL5", true, iCancel: false);
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL4", false, iCancel: true);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.ZG_CustomsStatus = "RL5";

			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.CEI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			CombineAssertions(() =>
			{
				AssertEquals("Invoice line is for Out of Warehouse Procedure and linked Entry Line ZG_CustomsStatus == 'RL5'", true, invoiceLine.IncludedInUniversalXML);

				entryLine.ZG_CustomsStatus = "RL4";

				AssertEquals("Invoice line is for Out of Warehouse Procedure and linked Entry Line ZG_CustomsStatus == 'RL4'", false, invoiceLine.IncludedInUniversalXML);

				entryLine.ZG_CustomsStatus = "RL5";
				invoiceLine.JI_Procedure = ZString.Empty;
				invoiceLine.Factory.InvalidateCachedProperties();

				AssertEquals("Invoice Line is not for any warehousing procedure, should be included in UXML by default", true, invoiceLine.IncludedInUniversalXML);

				entryLine.ZG_CustomsStatus = "RL5";
				invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
				invoiceLine.JI_CL = ZGuid.Empty;
				AssertEquals("Invoice Line has no CusEntryLine, should be included in UXML by default", true, invoiceLine.IncludedInUniversalXML);
			});
		}

		public void TestIncludeEntryDetailsInUniversalXMLImport()
		{
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL5", iUpdate: true, iCancel: false);
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL2", iUpdate: false, iCancel: false);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.ZG_CustomsStatus = "RL5";

			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.CEI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			CombineAssertions(() =>
			{
				AssertEquals("Invoice line linked Entry Line ZG_CustomsStatus == 'RL5'", true, invoiceLine.IncludeEntryDetailsInUniversalXML);

				entryLine.ZG_CustomsStatus = "RL2";

				AssertEquals("Invoice line linked Entry Line ZG_CustomsStatus == 'RL2'", false, invoiceLine.IncludeEntryDetailsInUniversalXML);
			});
		}

		public void TestIncludeEntryDetailsInUniversalXMLExport()
		{
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.CEI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			CombineAssertions(() =>
			{
				AssertEquals("Invoice line Import", false, invoiceLine.IncludeEntryDetailsInUniversalXML);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				AssertEquals("Invoice line export", true, invoiceLine.IncludeEntryDetailsInUniversalXML);
			});
		}

		public void TestIncludeEntryDetailsInUniversalXMLExport_WarehouseAdjustment()
		{
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.CEI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			CombineAssertions(() =>
			{
				AssertEquals("Invoice line Import", false, invoiceLine.IncludeEntryDetailsInUniversalXML);

				declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;

				AssertEquals("Invoice line WarehouseAdjustment", true, invoiceLine.IncludeEntryDetailsInUniversalXML);
			});
		}

		public void TestIncludedInUniversalXMLCore()
		{
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL1", iUpdate: false, iCancel: true);
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL2", iUpdate: false, iCancel: false);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = (CusEntryLine)entry.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.ZG_CustomsStatus = "RL2";

			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.CEI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;

			CombineAssertions(() =>
			{
				AssertEquals("Invoice line linked Entry Line ZG_CustomsStatus == 'RL2'", true, invoiceLine.IncludedInUniversalXML);

				entryLine.ZG_CustomsStatus = "RL1";

				AssertEquals("Invoice line linked Entry Line ZG_CustomsStatus == 'RL1'", false, invoiceLine.IncludedInUniversalXML);
			});
		}

		public void TestZG_NetPrice()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = Factory.New<InvoiceLineForTest>();
				invoiceLine.AddInfo.ZG_NetPrice = 1.2;
				AssertEquals("Get value from Addinfo", 1.2m, invoiceLine.ZG_NetPrice);

				invoiceLine.ZG_NetPrice = 2.3;
				AssertEquals("Set value to AddInfo", (ZDecimal)2.3, invoiceLine.AddInfo.ZG_NetPrice);
			});
		}

		public void TestIsBondedWhsQuantityVisibleCore_Import_IsOutOfWarehouseWarehousing()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "7100";
				AssertEquals("IsOutOfWarehouseWarehousing: false, IsInwardMovementApplicable: false", expected: false, invoiceLine.IsBondedWhsQuantityVisible);

				invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
				AssertEquals("IsOutOfWarehouseWarehousing: true, IsInwardMovementApplicable: false", expected: true, invoiceLine.IsBondedWhsQuantityVisible);
			});
		}

		public void TestIsBondedWhsQuantityVisibleCore_Import_IsInwardMovementApplicable()
		{
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			CombineAssertions(() =>
			{
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					entryInstruction.CEI_Style = style;
					AssertEquals($"CEI_Style {style}", entryInstruction.IsInwardMovementApplicable, invoiceLine.IsBondedWhsQuantityVisible);
				}
			});
		}

		public void TestIsBondedWhsQuantityVisibleCore_Export()
		{
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_OutOfWarehouse = "Y";
			procedure.ZZ6_OutOfInwardProcessing = "N";
			procedure.ZZ6_PreviousProcedureCode = "71";
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
			Assert("Precondition", invoiceLine.IsOutOfWarehouseWarehousing);

			CombineAssertions(() =>
			{
				AssertEquals(true, invoiceLine.IsBondedWhsQuantityVisible);

				invoiceLine.JI_Procedure = "4072";
				AssertEquals(false, invoiceLine.IsBondedWhsQuantityVisible);
			});
		}

		public void TestIsBondedWhsQuantityVisibleCore_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			Assert(invoiceLine.IsBondedWhsQuantityVisible);
		}

		public void TestIsBondedWhsQuantityVisibleCore_IntoVATWarehouse()
		{
			procedure.ZZ6_IntoVATWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();

			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = "4071";

			CombineAssertions(() =>
			{
				AssertEquals(true, invoiceLine.IsBondedWhsQuantityVisible);

				invoiceLine.JI_Procedure = "4072";
				AssertEquals(false, invoiceLine.IsBondedWhsQuantityVisible);
			});
		}

		public void TestIsPreviousEntryNumberVisibleCore_Import()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "7100";
				AssertEquals(expected: false, invoiceLine.IsPreviousEntryNumberVisible);

				invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
				AssertEquals(expected: true, invoiceLine.IsPreviousEntryNumberVisible);
			});
		}

		public void TestIsPreviousEntryNumberVisibleCore_Export()
		{
			procedure.ZZ6_ShipmentType = "EXP";
			procedure.ZZ6_OutOfWarehouse = "Y";
			procedure.ZZ6_OutOfInwardProcessing = "N";
			procedure.ZZ6_PreviousProcedureCode = "71";
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "7100";
				AssertEquals(expected: false, invoiceLine.IsPreviousEntryNumberVisible);

				invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
				AssertEquals(expected: true, invoiceLine.IsPreviousEntryNumberVisible);
			});
		}

		public void TestIsIsPreviousEntryNumberVisibleCore_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			Assert(invoiceLine.IsPreviousEntryNumberVisible);
		}

		public override void TestOnCusProcedureChanged()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				declaration.SetSupportsBondedWarehousingForTesting(true);
				CombineAssertions(() =>
				{
					invoiceLine.JI_InvoiceQuantity = 123m;
					invoiceLine.JI_InvoiceUQ = "KGM";
					invoiceLine.JI_Procedure = isOutOfWarehouseWarehousingProcedureCode;
					AssertEquals("Warehouse quantity defaulted from invoice quantity.", 123m, invoiceLine.JI_BondedWhsQuantity);
					AssertEquals("Warehouse unit defaulted from invoice unit.", "KGM", invoiceLine.JI_BondedWhsUnitQty);

					invoiceLine.JI_Procedure = "2345678";
					AssertEquals("Warehouse quantity cleared.", 0m, invoiceLine.JI_BondedWhsQuantity);
					AssertEquals("Warehouse unit cleared.", "", invoiceLine.JI_BondedWhsUnitQty);
				});
			}
		}

		public void TestWarehouseAdjustmentProcedureChangeDoesNotClearBwhQuantityAndUnit() => CombineAssertions(() =>
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			invoiceLine.JI_Procedure = string.Empty;
			invoiceLine.JI_BondedWhsQuantity = 10m;
			invoiceLine.JI_BondedWhsUnitQty = "KGM";

			AssertEquals("pre quantity", 10m, invoiceLine.JI_BondedWhsQuantity);
			AssertEquals("pre quantity unit", "KGM", invoiceLine.JI_BondedWhsUnitQty);

			invoiceLine.JI_Procedure = "01";

			AssertEquals("post quantity", 10m, invoiceLine.JI_BondedWhsQuantity);
			AssertEquals("post quantity unit", "KGM", invoiceLine.JI_BondedWhsUnitQty);
		});

		public override void TestGetNewLinkPackValidation()
		{
			AssertType<InvoiceLinePackageValidation>(InvoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew().Validation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			procedure = Factory.NewWithValidTestData<RefCusProcedure>();
			isOutOfWarehouseWarehousingProcedureCode = "4071";
			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = "DE";
			procedure.ZZ6_ZZZ_NKDataGrouping = "DE";
			procedure.ZZ6_ShipmentType = "IMP";
			procedure.ZZ6_ProcedureCode = isOutOfWarehouseWarehousingProcedureCode.Left(2);
			procedure.ZZ6_Concession = isOutOfWarehouseWarehousingProcedureCode.PadRight(7).Right(3);
			procedure.ZZ6_OutOfWarehouse = "Y";
			procedure.ZZ6_PreviousProcedureCode = "71";

			var procedure2 = Factory.NewWithValidTestData<RefCusProcedure>();
			isOutOfInwardProcessingProcedureCode = "4051";
			procedure2.ZZ6_ZZZ_NKDataGrouping = "DE";
			procedure2.ZZ6_ShipmentType = "IMP";
			procedure2.ZZ6_ProcedureCode = isOutOfInwardProcessingProcedureCode.Left(2);
			procedure2.ZZ6_Concession = isOutOfInwardProcessingProcedureCode.PadRight(7).Right(3);
			procedure2.ZZ6_OutOfInwardProcessing = "Y";
			procedure2.ZZ6_PreviousProcedureCode = "51";

			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
		}
		RefCusProcedure procedure;
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryInstruction entryInstruction;
		ZString isOutOfWarehouseWarehousingProcedureCode;
		ZString isOutOfInwardProcessingProcedureCode;

		protected override string OverseasFreightCode => ImportChargeCodeList.Codes._011;

		protected override string OverseasInsuranceCode => ImportChargeCodeList.Codes._012;

		protected override Type GetExpectedPartType() => typeof(OrgSupplierPart);

		protected override Type GetExpectedCusEntryLineType() => typeof(CusEntryLine);

		protected override string GetLocalPortCode() => "DEFRA";

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(EU.Business.Declaration.InvoiceLineChargeCollection<InvoiceLineCharge>);

		protected override ZString GetJobMessageTypeForTestingSetProductBringsInDocsAndTaxes => Common.Shared.SharedJobMessageTypeList.Codes.Export;

		protected override Type GetExpectedEntryInstructionType()
		{
			return typeof(CusEntryInstruction);
		}

		protected override ZString OFTChargeDescription => "FREIGHT AFTER EU BORDER from Entry";

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			foreach (var invoiceLine in declaration.InvoiceLines.Cast<EU.Business.Declaration.JobComInvoiceLine>())
			{
				invoiceLine.JI_CEI = entryInstruction.PK;
			}
		}

		void CreateRefCusCondition(UniversalReferenceTestDataHelper helper, TariffView tariff, ZString conditionType, ZString preferenceCode, ZString[] supportingDocTypes, ZDateTime? startDate = null, ZDateTime? endDate = null)
		{
			var preferencePK = preferenceCode.IsEmpty ? ZGuid.Empty : helper.CreatePreferenceForCountry(preferenceCode, preferenceCode, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN).PK;
			var supValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument);
			var refConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, conditionType);
			var refCondition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, refConditionType.PK, tariff.PK, ZString.Empty, true, false, startDate ?? ZDateTime.MinSmallDateTimeValue, endDate ?? ZDateTime.MaxSmallDateTimeValue, c => c.ZX1_ZZS_Preference = preferencePK);
			foreach (var docType in supportingDocTypes)
			{
				helper.CreateOrGetExistingRefCusConditionValue(supValueType.PK, refCondition.PK, docType);
			}
		}

		void AssertEvaluateConditionValue(bool isImportIntoBondedWareHouseOrInwardProcessing, Action<RefCusProcedure> setCusProcedureProperty = null)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusProcedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "71", "78", "123", "Desc", "IMP");
			setCusProcedureProperty?.Invoke(cusProcedure);
			Factory.Save();
			const string requiredDocCode = "C640";

			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "7178123";
				AssertEvaluateConditionValue("ConditionType not in (420,465,474,475,477) or starts with 7, no required doc", "400", false);
				AssertEvaluateConditionValue("ConditionType = 420, no required doc", UniversalReferenceConstants.RefCusConditionTypes._420, isImportIntoBondedWareHouseOrInwardProcessing);
				AssertEvaluateConditionValue("ConditionType = 465, no required doc", UniversalReferenceConstants.RefCusConditionTypes._465, isImportIntoBondedWareHouseOrInwardProcessing);
				AssertEvaluateConditionValue("ConditionType = 474, no required doc", UniversalReferenceConstants.RefCusConditionTypes._474, isImportIntoBondedWareHouseOrInwardProcessing);
				AssertEvaluateConditionValue("ConditionType = 475, no required doc", UniversalReferenceConstants.RefCusConditionTypes._475, isImportIntoBondedWareHouseOrInwardProcessing);
				AssertEvaluateConditionValue("ConditionType = 477, no required doc", UniversalReferenceConstants.RefCusConditionTypes._477, isImportIntoBondedWareHouseOrInwardProcessing);
				AssertEvaluateConditionValue("ConditionType starts with 7, no required doc", "711", isImportIntoBondedWareHouseOrInwardProcessing);

				var doc = invoiceLine.SupportingDocuments.AddNew();
				doc.CSI_Code = requiredDocCode;
				AssertEvaluateConditionValue("ConditionType not in (420,465,474,475,477) or starts with 7, has required doc", "400", true);
				AssertEvaluateConditionValue("ConditionType = 420, has required doc", UniversalReferenceConstants.RefCusConditionTypes._420, true);
				AssertEvaluateConditionValue("ConditionType = 465, has required doc", UniversalReferenceConstants.RefCusConditionTypes._465, true);
				AssertEvaluateConditionValue("ConditionType = 474, has required doc", UniversalReferenceConstants.RefCusConditionTypes._474, true);
				AssertEvaluateConditionValue("ConditionType = 475, has required doc", UniversalReferenceConstants.RefCusConditionTypes._475, true);
				AssertEvaluateConditionValue("ConditionType = 477, has required doc", UniversalReferenceConstants.RefCusConditionTypes._477, true);
				AssertEvaluateConditionValue("ConditionType starts with 7, has required doc", "711", true);
			});

			void AssertEvaluateConditionValue(string message, string conditionType, bool expectedResult)
			{
				AssertEquals(message, expectedResult, invoiceLine.EvaluateConditionValue(conditionType, Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, requiredDocCode));
			}
		}

		(UniversalReferenceTestDataHelper, TariffView, CusRefRateCodeView, CusRefTradeGroupView) CreateCusRefPreference()
		{
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var tradeGroup = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.Germany, "TradeGroupTest", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			testHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Botswana);
			testHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.AlandIslands);

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Germany, Constants.RateTypes.Duty, "Duty Desc");
			var rateCode = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var preference100 = testHelper.CreatePreferenceForCountry("100", "100 Desc", Core.Constants.CountryCodes.Germany);
			var cusTariff = testHelper.CreateTariff(Core.Constants.CountryCodes.Germany, hsnTariffType.PK, "123456789", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "123456789 Desc");
			var testRate1 = testHelper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, preferencePk: preference100.PK);
			testHelper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			return (testHelper, cusTariff, rateCode, tradeGroup);
		}
	}

	sealed class InvoiceLineForTest : JobComInvoiceLine
	{
		public InvoiceLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AddInfoJobComInvoiceLine AddInfo => (AddInfoJobComInvoiceLine)base.AddInfo;
	}
}
