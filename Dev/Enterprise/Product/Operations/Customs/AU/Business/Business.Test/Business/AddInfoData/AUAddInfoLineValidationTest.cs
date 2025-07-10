using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class AUAddInfoLineValidationTest : AUAddInfoValidationTest
	{
		[TestDate(2005, 1, 1)]
		public void TestCheckJI_CustomsUnitOfQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "0000000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "AA");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "BB");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var declaration = JobDeclaration.New(Factory);
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "00000000 00";

				AssertEquals("UQ is defaulted", "AA", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ is defaulted", "BB", invoiceLine.JI_SecondQuantityUQ);

				invoiceLine.AddInfo.ZA_UQ2 = "CC";
				AssertHasWarning(invoiceLine.AddInfo.ZA_UQ2Info, string.Format("The tariff requires a different second unit of quantity, {0}", "BB"));
			}
		}

		[TestDate(2005, 1, 1)]
		public void TestCheckJI_CustomsUnitOfQty_AUCAHECC()
		{
			var statClassification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassification.SC_TariffClassificationNumber = "00000000";
			statClassification.SC_StatisticalClassificationCode = "00";
			statClassification.SC_StartDate = new ZDateTime(2005, 1, 1);
			statClassification.SC_EndDate = new ZDateTime(2005, 1, 1);
			statClassification.SC_QuantityUnit = "AA";
			statClassification.SC_SecondQuantityUnit = "BB";

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000 00";

			AssertEquals("UQ is defaulted", "AA", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("Second UQ is defaulted", "BB", invoiceLine.JI_SecondQuantityUQ);

			invoiceLine.AddInfo.ZA_UQ2 = "CC";
			AssertHasWarning(invoiceLine.AddInfo.ZA_UQ2Info, string.Format("The tariff requires a different second unit of quantity, {0}", "BB"));
		}

		public void TestValidateZA_PRT()
		{
			invoiceLine.AddInfo.ZA_PRT = "";
			AssertEquals("ZA_PRT", false, invoiceLine.AddInfo.ZA_PRTInfo.HasNotifications());
			invoiceLine.JI_IsPackToBondForLine = ZBool.True;
			invoiceLine.AddInfo.ZA_PRT = "POM";
			AssertHasWarningContaining(invoiceLine.AddInfo.ZA_PRTInfo, "Since this line is Nature 20 this Rule won't be used in the message.");
		}

		public void TestValidateZA_VID()
		{
			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.JE_OH_Importer = GetValidImporterWithABN().PK;

			var part = Factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = "Part1";
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation1.OU_OH = declaration.Importer.PK;
			invoiceLine.JI_OP = part.PK;
			invoiceLine.JI_PartNo = "Part1";

			addInfo.ZA_VID = "abc";
			AssertHasError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnImporter);
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnPart);
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.EnterVINOnLine);

			declaration.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;
			addInfo.Validation.ValidateZA_VID();
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnImporter);
			AssertHasError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnPart);
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.EnterVINOnLine);

			declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part, 1, true);
			addInfo.Validation.ValidateZA_VID();
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnImporter);
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnPart);
			AssertHasError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.EnterVINOnLine);

			addInfo.ZA_VID = "";
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnImporter);
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnPart);
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.EnterVINOnLine);

			declaration.SetSupportsBondedWarehousingForTesting(false);
			addInfo.ZA_VID = "blah";
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnImporter);
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.SetupVINOnPart);
			AssertNoError(addInfo.ZA_VIDInfo, AUAddInfoLineValidation.VIDErrorMessages.EnterVINOnLine);
		}

		public void TestZA_WRNAndZA_WRLValidationForEntryThatIsNotNature30()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			JobComInvoiceHeader invHeader = testDec.Invoices.AddNew();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.AddInfo.ZA_WRN = "TT";
			invLine.AddInfo.ZA_WRL = 12;
			AssertEquals("WRN", true, invLine.AddInfo.ZA_WRNInfo.HasMessageErrors());
			AssertEquals("WRL", true, invLine.AddInfo.ZA_WRLInfo.HasMessageErrors());

			invLine.AddInfo.ZA_WRN = "";
			invLine.AddInfo.ZA_WRL = 0;
			AssertEquals("WRN", false, invLine.AddInfo.ZA_WRNInfo.HasMessageErrors());
			AssertEquals("WRL", false, invLine.AddInfo.ZA_WRLInfo.HasMessageErrors());

			testDec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			invLine.AddInfo.ZA_WRN = "TT";
			invLine.AddInfo.ZA_WRL = 12;
			AssertEquals("WRN", false, invLine.AddInfo.ZA_WRNInfo.HasMessageErrors());
			AssertEquals("WRL", false, invLine.AddInfo.ZA_WRLInfo.HasMessageErrors());
		}

		public void TestEFDForInvoiceLineError()
		{
			invoiceLine.AddInfo.AddInfoLine = "EFD=400104";
			Assert("Invoice Line cannot have EFD", invoiceLine.AddInfo.ZA_EFDInfo.HasMessageErrors());
		}

		public void TestWETExemptions()
		{
			addInfo.ZA_WETE = "000";
			AssertHasMessageErrors(addInfo.ZA_WETEInfo);
			addInfo.ZA_WETE = addInfo.Lookups.ZA_WETE_List[0].ToString();
			AssertNoNotifications(addInfo.ZA_WETInfo);
		}

		public void TestInvoiceLinesPriceAdjustment()
		{
			addInfo.ZA_ADJ = "";
			AssertEquals("ZA_ADJ Notifications [" + addInfo.ZA_ADJ + "]", false, addInfo.ZA_ADJInfo.HasNotifications());

			addInfo.ZA_ADJ = "Invalid";
			AssertEquals("ZA_ADJ Errors [" + addInfo.ZA_ADJ + "]", true, addInfo.ZA_ADJInfo.HasMessageErrors());

			addInfo.ZA_ADJ = "151.21 USD";
			AssertEquals("ZA_ADJ Notifications [" + addInfo.ZA_ADJ + "]", false, addInfo.ZA_ADJInfo.HasNotifications());

			addInfo.ZA_ADJ = "+151.21 USD";
			AssertEquals("ZA_ADJ Notifications [" + addInfo.ZA_ADJ + "]", false, addInfo.ZA_ADJInfo.HasNotifications());

			addInfo.ZA_ADJ = "151.32 XXX";
			AssertEquals("ZA_ADJ Errors [" + addInfo.ZA_ADJ + "]", true, addInfo.ZA_ADJInfo.HasMessageErrors());

			addInfo.ZA_ADJ = "-151.21AUD";
			AssertEquals("ZA_ADJ Notifications [" + addInfo.ZA_ADJ + "]", false, addInfo.ZA_ADJInfo.HasNotifications());

			addInfo.ZA_ADJ = "+/4321.23/6";
			AssertEquals("ZA_ADJ Errors [" + addInfo.ZA_ADJ + "]", true, addInfo.ZA_ADJInfo.HasMessageErrors());
		}

		public void TestPriceAdjustmentWithPercentage()
		{
			addInfo.ZA_ADJ = "";
			AssertEquals("ZA_ADJ Notifications [" + addInfo.ZA_ADJ + "]", false, addInfo.ZA_ADJInfo.HasNotifications());

			addInfo.ZA_ADJ = "10%";
			AssertEquals("No errors", false, addInfo.ZA_ADJInfo.HasNotifications());

			addInfo.ZA_ADJ = "-10%";
			AssertEquals("No errors", false, addInfo.ZA_ADJInfo.HasNotifications());

			addInfo.ZA_ADJ = "10";
			AssertEquals("Errors", true, addInfo.ZA_ADJInfo.HasNotifications());
		}

		public void TestISSGeneratesNoMessageErrorForNature30()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			addInfo.ZA_ISS = 12;
			AssertEquals("Precondition - IsNature30", true, declaration.IsNature30);
			AssertNoNotifications(addInfo.ZA_ISSInfo);
		}

		public void TestValidateWUV()
		{
			JobComInvoiceLine invLine = DefaultData.InvoiceLine;
			invLine.AddInfo.ZA_WUV = 12.0m;
			Assert("MessageError", invLine.AddInfo.ZA_WUVInfo.HasMessageErrors());
			invLine.AddInfo.ZA_WUV = 0m;
			Assert("NoMessageError", !invLine.AddInfo.ZA_WUVInfo.HasMessageErrors());
			invLine.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			invLine.AddInfo.ZA_WUV = 12.0m;
			Assert("NoMessageError", !invLine.AddInfo.ZA_WUVInfo.HasMessageErrors());
			invLine.AddInfo.ZA_WUV = 0m;
			invLine.AddInfo.ZA_IsPackToBondForLine_Hidden = ZString.Empty;
			DefaultData.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			invLine.AddInfo.ZA_WUV = 12.0m;
			Assert("NoMessageError", !invLine.AddInfo.ZA_WUVInfo.HasMessageErrors());
			DefaultData.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.WarehousedByExternalAgent;
			invLine.AddInfo.ZA_WUV = 15.0m;
			Assert("NoMessageError", !invLine.AddInfo.ZA_WUVInfo.HasMessageErrors());
		}

		public void TestDumpingDuty()
		{
			addInfo.ZA_DMP = 23.24m;
			AssertEquals("ZA_DMP notifications [" + addInfo.ZA_DMP + "]", false, addInfo.ZA_DMPInfo.HasNotifications());
			addInfo.ZA_DMP = 2000;
			AssertEquals("ZA_DMP Errors [" + addInfo.ZA_DMP + "]", false, addInfo.ZA_DMPInfo.HasMessageErrors());
			addInfo.ZA_DMP = 33000.24m;
			AssertEquals("ZA_DMP notifications [" + addInfo.ZA_DMP + "]", false, addInfo.ZA_DMPInfo.HasNotifications());
		}

		public void TestDumpingRateOfExchange()
		{
			addInfo.ZA_DRE = 23.24m;
			AssertEquals("ZA_DRE notifications [" + addInfo.ZA_DRE + "]", false, addInfo.ZA_DREInfo.HasNotifications());
			addInfo.ZA_DRE = 2000;
			AssertEquals("ZA_DRE Errors [" + addInfo.ZA_DRE + "]", false, addInfo.ZA_DREInfo.HasMessageErrors());
			addInfo.ZA_DRE = 33000.24m;
			AssertEquals("ZA_DRE notifications [" + addInfo.ZA_DRE + "]", false, addInfo.ZA_DREInfo.HasNotifications());
		}

		public void TestSetDXPAcceptsInputValuesCorrectly()
		{
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_ORG = Enterprise.Core.Constants.CountryCodes.Australia;
			invoiceLine.AddInfo.ZA_DRC = "18";
			invoiceLine.AddInfo.ZA_DSN = "4";
			invoiceLine.AddInfo.ZA_DXP = "1802.82";
			invoiceLine.AddInfo.ZA_ValuationBasis_Hidden = "TV";
			AssertEquals("ZA_DXP Warnings", "No currency code detected, defaulting to AUD", invoiceLine.AddInfo.ZA_DXPInfo.GetWarnings().GetFirstMessage());
			invoiceLine.AddInfo.ZA_DXP = "1808.45 AUD";
			invoiceLine.AddInfo.RunPreSaveValidation();
			AssertNoMessageErrors(addInfo);
			AssertEquals("Add Info Notifications : " + invoiceLine.AddInfo.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString(), false, addInfo.HasNotifications());
		}

		public void TestInvoiceSpiritStrength()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			addInfo.ZA_ISS = 0;
			AssertEquals("ZA_ISS Notifications [" + addInfo.ZA_ISS + "]", false, addInfo.ZA_ISSInfo.HasNotifications());
			addInfo.ZA_ISS = 121;
			AssertEquals("ZA_ISS Errors [" + addInfo.ZA_ISS + "]", true, addInfo.ZA_ISSInfo.HasMessageErrors());
			addInfo.ZA_ISS = 21.4m;
			AssertEquals("ZA_ISS Notifications [" + addInfo.ZA_ISS + "]", false, addInfo.ZA_ISSInfo.HasNotifications());
			addInfo.ZA_ISS = 1230.0m;
			AssertEquals("ZA_ISS Notifications [" + addInfo.ZA_ISS + "]", true, addInfo.ZA_ISSInfo.HasNotifications());
			addInfo.ZA_ISS = -123;
			AssertEquals("ZA_ISS Notifications [" + addInfo.ZA_ISS + "]", true, addInfo.ZA_ISSInfo.HasNotifications());
			addInfo.ZA_ISS = 12.34m;
			AssertEquals("ZA_ISS Notifications [" + addInfo.ZA_ISS + "]", false, addInfo.ZA_ISSInfo.HasNotifications());
		}

		public void TestDumpingExportPrice()
		{
			addInfo.ZA_DXP = "";
			AssertEquals("ZA_DXP notifications [" + addInfo.ZA_DXP + "]", false, addInfo.ZA_DXPInfo.HasNotifications());

			addInfo.ZA_DXP = "USD";
			AssertEquals("ZA_DXP Errors [" + addInfo.ZA_DXP + "] no amount", true, addInfo.ZA_DXPInfo.HasMessageErrors());
			AssertHasMessageError(addInfo.ZA_DXPInfo, AUAddInfoLineValidation.DXPZeroAmountErrorMessage);

			addInfo.ZA_DXP = "AUSD";
			AssertEquals("ZA_DXP Errors [" + addInfo.ZA_DXP + "] no amount", true, addInfo.ZA_DXPInfo.HasMessageErrors());
			AssertHasMessageError(addInfo.ZA_DXPInfo, "A" + AUAddInfoLineValidation.DXPInvalidAmountErrorMessage);

			addInfo.ZA_DXP = "2324 USD";
			AssertEquals("ZA_DXP Notifications [" + addInfo.ZA_DXP + "]", false, addInfo.ZA_DXPInfo.HasNotifications());

			addInfo.ZA_DXP = "2324USD";
			AssertEquals("ZA_DXP Notifications [" + addInfo.ZA_DXP + "]", false, addInfo.ZA_DXPInfo.HasNotifications());

			addInfo.ZA_DXP = "1234.23 ";
			AssertEquals("ZA_DXP Warning [" + addInfo.ZA_DXP + "] no currency", true, addInfo.ZA_DXPInfo.HasWarnings());

			addInfo.ZA_DXP = "252.32  USD";
			AssertEquals("ZA_DXP Notifications [" + addInfo.ZA_DXP + "]", false, addInfo.ZA_DXPInfo.HasNotifications());

			addInfo.ZA_DXP = "AUD 1231.23";
			AssertEquals("ZA_DXP Errors [" + addInfo.ZA_DXP + "] bad format", true, addInfo.ZA_DXPInfo.HasMessageErrors());

			addInfo.ZA_DXP = "-1231.23 AUD";
			AssertEquals("ZA_DXP Errors [" + addInfo.ZA_DXP + "] bad format", true, addInfo.ZA_DXPInfo.HasMessageErrors());
		}

		public void TestDuty()
		{
			addInfo.ZA_DTY = 23.24m;
			AssertEquals("ZA_DTY notifications [" + addInfo.ZA_DTY + "]", false, addInfo.ZA_DTYInfo.HasNotifications());
			addInfo.ZA_DTY = 2000;
			AssertEquals("ZA_DTY Errors [" + addInfo.ZA_DTY + "]", false, addInfo.ZA_DTYInfo.HasMessageErrors());
			addInfo.ZA_DTY = 33000.24m;
			AssertEquals("ZA_DTY notifications [" + addInfo.ZA_DTY + "]", false, addInfo.ZA_DTYInfo.HasNotifications());
			addInfo.ZA_DTY = -100m;
			AssertEquals("ZA_DTY notifications [" + addInfo.ZA_DTY + "]", true, addInfo.ZA_DTYInfo.HasNotifications());
		}

		public void TestLCTQuoting()
		{
			addInfo.ZA_LCTQ = "";
			AssertEquals("ZA_LCTQ Notifications [" + addInfo.ZA_LCTQ + "]", false, addInfo.ZA_LCTQInfo.HasNotifications());

			addInfo.ZA_LCTQ = "K";
			AssertEquals("ZA_LCTQ Errors [" + addInfo.ZA_LCTQ + "]", true, addInfo.ZA_LCTQInfo.HasMessageErrors());

			addInfo.ZA_LCTQ = "Y";
			AssertEquals("ZA_LCTQ Notifications [" + addInfo.ZA_LCTQ + "]", false, addInfo.ZA_LCTQInfo.HasNotifications());

			addInfo.ZA_LCTQ = "K";
			AssertEquals("ZA_LCTQ Errors [" + addInfo.ZA_LCTQ + "]", true, addInfo.ZA_LCTQInfo.HasMessageErrors());
		}

		public void TestLCTExemptions()
		{
			addInfo.ZA_LCTE = "415";
			AssertNoNotifications(addInfo.ZA_LCTEInfo);
			addInfo.ZA_LCTE = "NIL";
			AssertHasMessageErrors(addInfo.ZA_LCTEInfo);
		}

		public void TestManualLineProcessing()
		{
			// Format is MLP = nnna

			addInfo.ZA_MLP = "";
			AssertNoNotifications(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "1234";
			AssertHasMessageErrors(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "123A";
			AssertNoNotifications(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "123";
			AssertHasMessageErrors(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "999Z";
			AssertNoNotifications(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "5713A";
			AssertHasMessageErrors(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "13A";
			AssertHasMessageErrors(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "3A";
			AssertHasMessageErrors(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "A";
			AssertHasMessageErrors(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "A223B";
			AssertHasMessageErrors(addInfo.ZA_MLPInfo);
			addInfo.ZA_MLP = "222Ba";
			AssertHasMessageErrors(addInfo.ZA_MLPInfo);
		}

		public void TestOtherDutyFactor()
		{
			// Format ODF=nnnn

			addInfo.ZA_ODF = 0m;
			AssertNoNotifications(addInfo.ZA_ODFInfo);
			addInfo.ZA_ODF = 142;
			AssertNoNotifications(addInfo.ZA_ODFInfo);
			addInfo.ZA_ODF = 475.21m;
			AssertNoNotifications(addInfo.ZA_ODFInfo);
			addInfo.ZA_ODF = -475.21m;
			AssertHasMessageErrors(addInfo.ZA_ODFInfo);
		}

		public void TestSecondMinisterialDetermination()
		{
			// May only be used in conjunction with MD1
			// Format MD2=nnnnnn

			addInfo.ZA_MD2 = "";
			AssertNoNotifications(addInfo.ZA_MD2Info);
			addInfo.ZA_MD2 = "123456";
			AssertNoNotifications(addInfo.ZA_MD2Info);
			addInfo.ZA_MD2 = "1234";
			AssertHasMessageErrors(addInfo.ZA_MD2Info);
			addInfo.ZA_MD2 = "abcdef";
			AssertHasMessageErrors(addInfo.ZA_MD2Info);
		}

		public void TestSecondTariffConcessionOrder()
		{
			// May only be used in conjunction with TC1
			// Format TC2=nnnnnnn

			addInfo.ZA_TC2 = "";
			AssertNoNotifications(addInfo.ZA_TC2Info);
			addInfo.ZA_TC2 = "8840003";
			AssertNoNotifications(addInfo.ZA_TC2Info);
			addInfo.ZA_TC2 = "12345678";
			AssertHasMessageErrors(addInfo.ZA_TC2Info);
			addInfo.ZA_TC2 = "abcdefg";
			AssertHasMessageErrors(addInfo.ZA_TC2Info);
		}

		public void TestSecondQuantity()
		{
			// Format QT2=nnnn

			addInfo.ZA_QT2 = 0;
			AssertEquals("Has no message errors", false, addInfo.ZA_QT2Info.HasMessageErrors());

			addInfo.ZA_QT2 = 400;
			AssertEquals("Has a message error", true, addInfo.ZA_QT2Info.HasMessageErrors());

			addInfo.ZA_UQ2 = "KG";
			AssertEquals("Has no message errors", false, addInfo.ZA_QT2Info.HasMessageErrors());

			addInfo.ZA_QT2 = -400;
			AssertEquals("Has a message error", true, addInfo.ZA_QT2Info.HasMessageErrors());
		}

		public void TestUnitOfSecondQuantity()
		{
			// Format UQ2=aa

			addInfo.ZA_UQ2 = "";
			AssertEquals("No error message", false, addInfo.ZA_UQ2Info.HasMessageErrors());

			addInfo.ZA_UQ2 = "kg";
			AssertEquals("No error message-kg", false, addInfo.ZA_UQ2Info.HasMessageErrors());

			addInfo.ZA_UQ2 = "88";
			AssertEquals("Error message-88", true, addInfo.ZA_UQ2Info.HasMessageErrors());

			addInfo.ZA_UQ2 = "XX";
			AssertEquals("Error message-XX", true, addInfo.ZA_UQ2Info.HasMessageErrors());

			addInfo.ZA_UQ2 = "KG";
			AssertEquals("No error message-KG", false, addInfo.ZA_UQ2Info.HasMessageErrors());
		}

		public void TestStandardDuty()
		{
			addInfo.ZA_STD = 0;
			AssertNoNotifications(addInfo.ZA_STDInfo);
			addInfo.ZA_STD = 234.56m;
			AssertNoNotifications(addInfo.ZA_STDInfo);
			addInfo.ZA_STD = 12344578.90m;
			AssertNoNotifications(addInfo.ZA_STDInfo);
			addInfo.ZA_STD = 1234m;
			AssertNoNotifications(addInfo.ZA_STDInfo);
			addInfo.ZA_STD = .90m;
			AssertNoNotifications(addInfo.ZA_STDInfo);
			addInfo.ZA_STD = 123456789.10m;
			AssertHasMessageErrors(addInfo.ZA_STDInfo);
			addInfo.ZA_STD = 1234.5678m;
			AssertEquals(1234.57m, addInfo.ZA_STD);
			addInfo.ZA_STD = -1234.56m;
			AssertHasMessageErrors(addInfo.ZA_STDInfo);
		}

		public void TestTariffAdviceNumber()
		{
			addInfo.ZA_TAN = "";
			AssertNoNotifications(addInfo.ZA_TANInfo);
			addInfo.ZA_TAN = "1234567";
			AssertNoNotifications(addInfo.ZA_TANInfo);
			addInfo.ZA_TAN = "1234";
			AssertNoNotifications(addInfo.ZA_TANInfo);
			addInfo.ZA_TAN = "12345678901";
			AssertHasMessageErrors(addInfo.ZA_TANInfo);
			addInfo.ZA_TAN = "-123456";
			AssertHasMessageErrors(addInfo.ZA_TANInfo);
			addInfo.ZA_TAN = "abcdefg";
			AssertHasMessageErrors(addInfo.ZA_TANInfo);
		}

		public void TestWineEqualisationTax()
		{
			addInfo.ZA_WET = 0;
			AssertNoNotifications(addInfo.ZA_WETInfo);
			addInfo.ZA_WET = Convert.ToDecimal(12345678.50);
			AssertNoNotifications(addInfo.ZA_WETInfo);
			addInfo.ZA_WET = 1234;
			AssertNoNotifications(addInfo.ZA_WETInfo);
			addInfo.ZA_WET = Convert.ToDecimal(.50);
			AssertNoNotifications(addInfo.ZA_WETInfo);
			addInfo.ZA_WET = Convert.ToDecimal(123456789.25);
			AssertHasMessageErrors(addInfo.ZA_WETInfo);
			addInfo.ZA_WET = 340.2525m;
			AssertEquals(340.25m, addInfo.ZA_WET);
			addInfo.ZA_WET = -340.25m;
			AssertHasMessageErrors(addInfo.ZA_WETInfo);
		}

		public void TestWETQAndImporterValidABN()
		{
			var declaration = Factory.New<JobDeclaration>();

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_WETQ = "Y";
			AssertHasMessageErrors(invoiceLine.AddInfo.ZA_WETQInfo);

			declaration.JE_OH_Importer = GetValidImporterWithABN().PK;
			invoiceLine.AddInfo.Validation.ValidateZA_WETQ();
			AssertNoNotifications(invoiceLine.AddInfo.ZA_WETQInfo);
		}

		public void TestValidateJI_AUStateForExport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_AUState = string.Empty;
			AssertHasMessageError(invoiceLine.AddInfo.ZA_AUState_HiddenInfo, "An AU State is required.");
			invoiceLine.JI_AUState = "ZZ";
			AssertHasMessageError(invoiceLine.AddInfo.ZA_AUState_HiddenInfo, "The code you have selected is not in the list.");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_AUState = string.Empty;
			AssertNoMessageError(invoiceLine.AddInfo.ZA_AUState_HiddenInfo, "An AU State is required.");
			invoiceLine.JI_AUState = "ZZ";
			AssertNoMessageError(invoiceLine.AddInfo.ZA_AUState_HiddenInfo, "The code you have selected is not in the list.");
		}

		public void TestValidateJI_AUStateForQuarantine()
		{
			declaration.JE_MessageType = AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoice.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			invoiceLine.JI_AUState = string.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_AUStateInfo, "You have not entered an AU State");
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			invoiceLine.JI_AUState = "GAX";
			invoiceLine.JI_AUState = string.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_AUStateInfo, "You have not entered an AU State");
			invoice.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = false;
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			invoiceLine.JI_AUState = "GAX";
			invoiceLine.JI_AUState = string.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_AUStateInfo, "Please do not enter an AU State");
			invoiceLine.JI_AUState = "NSW";
			AssertHasMessageErrorContaining(invoiceLine.JI_AUStateInfo, "Please do not enter an AU State");
		}

		public void TestCheckZA_AQISCustomsWt_Hidden()
		{
			declaration.JE_MessageType = AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			invoice.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeight = 1m;
			AssertHasMessageErrorContaining(invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightInfo, "Please do not enter a Customs Weight");
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			invoiceLine.AddInfo.Validation.ValidateZA_AQISCustomsWt_Hidden();
			AssertNoMessageErrorContaining(invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightInfo, "Please do not enter a Customs Weight");
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			invoiceLine.AddInfo.Validation.ValidateZA_AQISCustomsWt_Hidden();
			AssertNoMessageErrorContaining(invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightInfo, "Please do not enter a Customs Weight");
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			invoiceLine.AddInfo.Validation.ValidateZA_AQISCustomsWt_Hidden();
			AssertNoMessageErrorContaining(invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightInfo, "Please do not enter a Customs Weight");
			invoice.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = false;
			invoiceLine.AddInfo.Validation.ValidateZA_AQISCustomsWt_Hidden();
			AssertHasMessageErrorContaining(invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightInfo, "Please do not enter a Customs Weight");
		}

		public void TestCheckZA_AQISCustomsWtUQ_Hidden()
		{
			declaration.JE_MessageType = AU.Declaration.Business.JobMessageTypeList.Codes.Quarantine;
			invoice.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightUQ = "XXX";
			AssertHasMessageError(invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightUQInfo, "The code you have selected is not in the list.");
			invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightUQ = "KGM";
			AssertNoMessageError(invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightUQInfo, "The code you have selected is not in the list.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var header = declaration.JobComInvoiceGroupHeaders[0];
			invoice = header.JobComInvoiceHeaders.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			addInfo = invoiceLine.AddInfo;
		}

		protected JobComInvoiceLine invoiceLine;
		protected JobComInvoiceHeader invoice;

		protected DefaultDataRig defaultData;
		protected DefaultDataRig DefaultData => defaultData ?? (defaultData = new DefaultDataRig(Factory));

		protected OrgHeader GetValidImporterWithABN()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "21 003 980 130");
			return importer;
		}
	}
}
