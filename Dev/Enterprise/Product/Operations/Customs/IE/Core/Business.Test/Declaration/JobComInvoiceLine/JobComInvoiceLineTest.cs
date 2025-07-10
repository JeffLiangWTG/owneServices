using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
	{
		public void TestJI_ValuationCodeCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().JI_ValuationCodeInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "Val. Method", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[4/16] Val. Method", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[4/16] Valuation Method", captionResourceString.Caption);
		}

		public void TestJI_LinePriceCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().JI_LinePriceInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "Item Price", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[4/14] Item Price", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[4/14] Item Price / Amount", captionResourceString.Caption);
		}

		public void TestJI_PrimaryPreferenceCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().JI_PrimaryPreferenceInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "Pref.", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[4/17] Pref.", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[4/17] Preference", captionResourceString.Caption);
		}

		public void TestJI_CountryOfOriginCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().JI_CountryOfOriginInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "[5/15] Orig. Ctry.", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[5/15] Orig. Country", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[5/15] Origin Country", captionResourceString.Caption);
			AssertEquals("FullDescription", "[5/15] Country of Origin code", captionResourceString.FullDescription);
		}

		public void TestJI_DescriptionCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().JI_DescriptionInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "Goods Desc.", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[6/8] Goods Desc.", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[6/8] Goods Description", captionResourceString.Caption);
			AssertEquals("FullDescription", "[6/8] Description of Goods", captionResourceString.FullDescription);
		}

		public void TestZG_CountryOfDestinationCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().ZG_CountryOfDestinationInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "Dest. Country", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[5/8] Dest. Country", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[5/8] Destination Country", captionResourceString.Caption);
			AssertEquals("FullDescription", "[5/8] Country of Destination Code", captionResourceString.FullDescription);
		}

		public void TestZG_CountryOfDispatchCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().ZG_CountryOfDispatchInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "[5/14] Disp. Ctry.", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[5/14] Disp. Country", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[5/14] Dispatch Country", captionResourceString.Caption);
			AssertEquals("FullDescription", "[5/14] Country of Dispatch/ Export", captionResourceString.FullDescription);
		}

		public void TestZG_CountryOfSupplyCaption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().ZG_CountryOfSupplyInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "Pref. Orig.", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[5/16] Pref. Orig.", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[5/16] Preferential Orig. Country", captionResourceString.Caption);
			AssertEquals("FullDescription", "[5/16] Country of preferential origin code", captionResourceString.FullDescription);
		}

		public void TestHasMutuallyExclusiveSupportingDocument()
		{
			AssertEquals("empty collection", false, invoiceLine.HasMutuallyExclusiveSupportingDocument);

			var supportingDoc = invoiceLine.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = "666";
			AssertEquals("without target supportingDocument", false, invoiceLine.HasMutuallyExclusiveSupportingDocument);

			supportingDoc.CSI_Code = "U164";
			var supportingDoc2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = "U165";
			AssertEquals(true, invoiceLine.HasMutuallyExclusiveSupportingDocument);

			supportingDoc.CSI_Code = "U167";
			AssertEquals("Specific case", false, invoiceLine.HasMutuallyExclusiveSupportingDocument);
		}

		public override void TestIAdditionalProcedureParent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure("IE", "A", "11", "11", "111", "One", "IMP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure("IE", "A", "11", "11", "222", "Two", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure("IE", "B", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure("IE", "A", "44", "44", "444", "Four", "EXP", group: "EFD");

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "EFD";
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_RN_NKCountryOfExport = "IE";
			invLine.JI_CEI = cei.PK;

			IAdditionalProcedureParent cpcParent = invLine;

			Assert("CPC List", cpcParent.AdditionalProcedureCodeList.ContainsCode("4444444"));
		}

		public void TestHasAdditionalProcedureCodeConcessionF15()
		{
			AssertEquals("When AdditionalProcedureCodes is empty", false, invoiceLine.HasAdditionalProcedureCodeConcessionF15);
			invoiceLine.AdditionalProcedureCodes.AddNew("4000C01");
			AssertEquals("When no F15 AdditionalProcedureCode", false, invoiceLine.HasAdditionalProcedureCodeConcessionF15);
			invoiceLine.AdditionalProcedureCodes.AddNew("4000F15");
			AssertEquals("When has F15 AdditionalProcedureCode", true, invoiceLine.HasAdditionalProcedureCodeConcessionF15);
		}

		public void TestHasChargeType()
		{
			invoiceLine.Charges.AddNew().J7_ChargeType = AISChargeCodeList.Codes._1X;
			invoiceLine.Charges.AddNew().J7_ChargeType = AISChargeCodeList.Codes._2X;
			invoiceLine.ApportionedCharges.AddNew().J7_ChargeType = AISChargeCodeList.Codes._2X;
			invoiceLine.ApportionedCharges.AddNew().J7_ChargeType = AISChargeCodeList.Codes.AJ;
			CombineAssertions(() =>
			{
				AssertEquals("1X is in Charges", true, invoiceLine.HasChargeType(AISChargeCodeList.Codes._1X));
				AssertEquals("2X is in Charges and ApportionedCharges", true, invoiceLine.HasChargeType(AISChargeCodeList.Codes._2X));
				AssertEquals("AJ is in ApportionedCharges", true, invoiceLine.HasChargeType(AISChargeCodeList.Codes.AJ));
				AssertEquals("BF is not in Charges nor ApportionedCharges", false, invoiceLine.HasChargeType(AISChargeCodeList.Codes.BF));
			});
		}

		public void TestIAdditionalProcedureParent_IMP()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure("IE", "A", "11", "11", "111", "One", "IMP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure("IE", "A", "11", "11", "222", "Two", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure("IE", "B", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure("IE", "A", "44", "44", "444", "Four", "EXP", group: "EFD");

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "IFD";
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_RN_NKCountryOfExport = "IE";
			invLine.JI_CEI = cei.PK;

			IAdditionalProcedureParent cpcParent = invLine;

			AssertEquals(2, cpcParent.AdditionalProcedureCodeList.Count);
			Assert("CPC List", cpcParent.AdditionalProcedureCodeList.ContainsCode("1111111"));
			Assert("CPC List", cpcParent.AdditionalProcedureCodeList.ContainsCode("1111222"));
		}

		public void TestShouldCheckMissingPreviousDocumentsCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals("EXP should check missing previous documents", true, line.ShouldCheckMissingPreviousDocuments);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertEquals("REX should not check missing previous documents", false, line.ShouldCheckMissingPreviousDocuments);
		}

		public override void TestChargeTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			ICommonInvoice line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList = line.ChargeTypeList;
			var chargeTypeList2 = line.ChargeTypeList;
			AssertSame("Cached", chargeTypeList, chargeTypeList2);
			AssertEquals("ADD, AFT, DED, INS, OFT, ONS", chargeTypeList.CodesAsString);
		}

		public void TestEvaluateConditionValue_Export()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var doc1 = invoiceLine.EntryInstruction.AdditionalInfos.AddNew();
			doc1.CSI_Code = "C641";
			doc1.CSI_SubType = "REF";

			CombineAssertions(() =>
			{
				AssertEquals("Test SUP/C641", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "C641"));
				AssertEquals("Test SNR/C641", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "C641"));
			});
		}

		public void TestIsProcedureCodeStartsWith76Or77()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "1200";
				AssertEquals("Not start with 76 or 77", false, invoiceLine.IsProcedureCodeStartsWith76Or77);
				invoiceLine.JI_Procedure = "7600";
				AssertEquals("Start with 76 or 77", true, invoiceLine.IsProcedureCodeStartsWith76Or77);
				invoiceLine.JI_Procedure = "7700";
				AssertEquals("Start with 76 or 77", true, invoiceLine.IsProcedureCodeStartsWith76Or77);
			});
		}

		public void TestEffectiveSupportingDocumentsFallsBack()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "C400", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.EffectiveSupportingDocuments().Count);
			var supportDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportDocument.CSI_Code = "C400";
			AssertEquals(0, invoiceLine.EffectiveSupportingDocuments().Count);

			supportDocument = invoiceLine.SupportingDocuments.AddNew();
			supportDocument.CSI_Code = "C400";
			AssertEquals(1, invoiceLine.EffectiveSupportingDocuments().Count);
		}

		public void TestAddInfos()
		{
			var lineForTest = Factory.New<JobComInvoiceLineForTest>();
			AssertType<AddInfoJobComInvoiceLine>(lineForTest.AddInfo);
		}

		public void TestZG_CountryOfDispatch()
		{
			CombineAssertions(() =>
			{
				declaration.JE_RL_NKOrigin = "FR001";
				AssertEquals("Value comes from the effective parent (declaration).", Core.Constants.CountryCodes.France, invoiceLine.ZG_CountryOfDispatch);

				invoiceLine.ZG_CountryOfDispatch = Core.Constants.CountryCodes.Italy;
				AssertEquals("Value comes from self (invoice line).", Core.Constants.CountryCodes.Italy, invoiceLine.ZG_CountryOfDispatch);
			});
		}

		public void TestJI_RN_NKCountryOfExport()
		{
			CombineAssertions(() =>
			{
				declaration.JE_RL_NKOrigin = "FR001";
				AssertEquals("Value comes from the effective parent (declaration).", Core.Constants.CountryCodes.France, invoiceLine.JI_RN_NKCountryOfExport);

				invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Italy;
				AssertEquals("Value comes from self (invoice line).", Core.Constants.CountryCodes.Italy, invoiceLine.JI_RN_NKCountryOfExport);
			});
		}

		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be IE", Core.Constants.CountryCodes.Ireland, InvoiceLine.CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				CombineAssertions(() =>
				{
					AssertEquals("CustomsCountryCode", Core.Constants.CountryCodes.Ireland, partDetails.CustomsCountryCode);
					AssertEquals("TypeOfPartUsed", typeof(MasterFiles.OrgSupplierPart), partDetails.TypeOfPartUsed);
				});
			}
		}

		public void TestJI_FormattedTariff_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_FormattedTariffInfo, (string)null, "Tariff");
		}

		public void TestJI_FormattedTariff_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_FormattedTariffInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[6/14 & 6/15] Commodity Code – Combined Nom. & TARIC", captionResourceString.Caption);
				AssertEquals("ShortCaption", "[6/14] Commodity", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/14 & 6/15] Commodity & TARIC", captionResourceString.MediumCaption);
				AssertEquals("FullDescription", "[6/14 & 6/15] Commodity Code – Combined Nomenclature Code & TARIC Code", captionResourceString.FullDescription);
			});
		}

		public void TestJI_Procedure_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_ProcedureInfo, (string)null, "Procedure Code", shortCaption: "CPC");
		}

		public void TestJI_Procedure_CaptionIMPUCC5()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				info: invoiceLine.JI_ProcedureInfo,
				multipleResourceKey: declaration.MultipleKeysToUse[0],
				shortCaption: "Proc. Code",
				mediumCaption: "[1/10 & 1/11] Proc. Code",
				caption: "[1/10 & 1/11] Procedure Code"
			);
		}

		public void TestJI_FormattedProcedure_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_FormattedProcedureInfo, (string)null, "Procedure Code", shortCaption: "CPC");
		}

		public void TestJI_FormattedProcedure_CaptionIMPUCC5()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				info: invoiceLine.JI_FormattedProcedureInfo,
				multipleResourceKey: declaration.MultipleKeysToUse[0],
				shortCaption: "[1/10]Proc. Code",
				mediumCaption: "[1/10 & 1/11] Proc. Code",
				caption: "[1/10 & 1/11] Procedure Code"
			);
		}

		public void TestJI_ConcessionOrder_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_ConcessionOrderInfo, (string)null, "Quota Order Number", shortCaption: "Quota", fullDescription: "Quota. Box 39. The Quota Order Number of the quota against which a claim for relief from Customs Duty is to be applied.");
		}

		public void TestJI_ConcessionOrder_Caption_ImportUCC5()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceLine>().JI_ConcessionOrderInfo, JobDeclaration.CaptionKeyImportUCC5);
			AssertEquals("ShortCaption", "[8/1] Quota", captionResourceString.ShortCaption);
			AssertEquals("MediumCaption", "[8/1] Quota", captionResourceString.MediumCaption);
			AssertEquals("Caption", "[8/1] Quota order number", captionResourceString.Caption);
		}

		public void TestAdditionalProcedureCodesWith3CharactersAsStringCaptionIMPUCC5()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(
				info: invoiceLine.AdditionalProcedureCodesAsStringInfo,
				multipleResourceKey: declaration.MultipleKeysToUse[0],
				shortCaption: "[1/11]Add. Proc.",
				mediumCaption: "[1/11] Add. Procedure",
				caption: "[1/11] Additional Procedure"
			);
		}

		#region Procedure

		public void TestRequestedProcedure()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "1200";
				AssertEquals("Not start with 76 or 77", false, invoiceLine.IsCustomsWarehousingProcedure76Or77);
				invoiceLine.JI_Procedure = "76";
				AssertEquals("Start with 76 or 77", true, invoiceLine.IsCustomsWarehousingProcedure76Or77);
				invoiceLine.JI_Procedure = "77";
				AssertEquals("Start with 76 or 77", true, invoiceLine.IsCustomsWarehousingProcedure76Or77);

				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._44;
				AssertEquals(true, invoiceLine.IsProcedureCode44);

				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76;
				AssertEquals(false, invoiceLine.IsProcedureCode44);

				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._53;
				AssertEquals(true, invoiceLine.IsProcedureCode53);

				invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76;
				AssertEquals(false, invoiceLine.IsProcedureCode53);
			});
		}

		#endregion

		public void TestJI_CustomsThirdQuantity_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_CustomsThirdQuantityInfo, (string)null, "Third Qty", shortCaption: "3rd Qty");
		}

		public void TestZG_CountryOfDestination_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.ZG_CountryOfDestinationInfo, (string)null, "Destination", shortCaption: "Dest.");
		}

		public void TestZG_CountryOfDispatch_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.ZG_CountryOfDispatchInfo, (string)null, "Country of Dispatch", shortCaption: "Dispatch Ctry.");
		}

		public void TestZG_CountryOfSupply_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.ZG_CountryOfSupplyInfo, (string)null, "Pref. Orig.", shortCaption: "Origin", fullDescription: "Country of (Preferential) Origin");
		}

		public void TestJI_ValuationCode_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_ValuationCodeInfo, (string)null, "Valuation Method");
		}

		public void TestJI_LinePrice_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_LinePriceInfo, (string)null, "Price");
		}

		public void TestJI_PrimaryPreference_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_PrimaryPreferenceInfo, (string)null, "Preference");
		}

		public void TestZG_RegionOfDestination_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.ZG_RegionOfDestinationInfo, (string)null, "Region of Destination", shortCaption: "Dest. Region");
		}

		public void TestJI_CountryOfOrigin()
		{
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.France;
			AssertEquals("Value comes from the effective parent (declaration).", Core.Constants.CountryCodes.France, invoiceLine.JI_CountryOfOrigin);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
			AssertEquals("Value comes from self (invoice line).", Core.Constants.CountryCodes.Italy, invoiceLine.JI_CountryOfOrigin);
		}

		public void TestJI_CustomsQuantity_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_CustomsQuantityInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Net Weight in KG", captionResourceString.Caption);
		}

		public void TestJI_CustomsQuantity_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_CustomsQuantityInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("FullDescription", "[6/1] Net Mass (kg)", captionResourceString.FullDescription);
				AssertEquals("ShortCaption", "[6/1] Net Mass", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/1] Net Mass (kg)", captionResourceString.MediumCaption);
			});
		}

		public void TestJI_Weight_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_WeightInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "Gross Weight", captionResourceString.Caption);
			AssertEquals("FullDescription", "[18 04 001 000] Gross Mass (KG)", captionResourceString.FullDescription);
		}

		public void TestJI_Weight_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_WeightInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[6/5] Gross Mass (kg)", captionResourceString.Caption);
				AssertEquals("ShortCaption", "[6/5] Gross Mass", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/5] Gross Mass (kg)", captionResourceString.MediumCaption);
			});
		}

		public void TestJI_CustomsSecondQuantity_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_CustomsSecondQuantityInfo, declaration.MultipleKeysToUse);
			AssertEquals("ShortCaption", "Suppl. Units", captionResourceString.ShortCaption);
			AssertEquals("Caption", "Supplementary Units", captionResourceString.Caption);
			AssertEquals("FullDescription", "[18 02 001 000] Supplementary Units", captionResourceString.FullDescription);
		}

		public void TestJI_CustomsSecondQuantity_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_CustomsSecondQuantityInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[6/2] Supplementary Units", captionResourceString.Caption);
				AssertEquals("ShortCaption", "[6/2] Suppl. Units", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/2] Suppl. Units", captionResourceString.MediumCaption);
			});
		}

		public void TestJI_Description_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_DescriptionInfo, (string)null, "Goods Description", shortCaption: "Goods Desc.", fullDescription: "[18 05 001 000] Description of Goods");
		}

		public void TestJI_Tariff_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_TariffInfo, declaration.MultipleKeysToUse);
				AssertEquals("Caption", "Tariff", captionResourceString.Caption);
				AssertEquals("FullDescription", "[18 09 056 000] Harmonized system sub-heading code and [18 09 057 000] Combined nomenclature code", captionResourceString.FullDescription);
			});
		}

		public void TestJI_Tariff_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_TariffInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[6/14 & 6/15] Commodity Code – Combined Nom. & TARIC", captionResourceString.Caption);
				AssertEquals("ShortCaption", "[6/14] Commodity", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/14 & 6/15] Commodity & TARIC", captionResourceString.MediumCaption);
				AssertEquals("FullDescription", "[6/14 & 6/15] Commodity Code – Combined Nomenclature Code & TARIC Code", captionResourceString.FullDescription);
			});
		}

		public void TestJI_OA_ExporterAddress_Caption()
		{
			var resourceStringDataAttribute = Factory.New<JobComInvoiceLine>().JI_OA_ExporterAddressInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Caption", "Consignor", resourceStringDataAttribute.Caption);
		}

		public new void TestZG_CusNumber_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.ZG_CusNumberInfo, declaration.MultipleKeysToUse);
			AssertEquals("Caption", "CUS Code", captionResourceString.Caption);
			AssertEquals("FullDescription", "[18 08 001 000] CUS Code", captionResourceString.FullDescription);
		}

		public void TestZG_CusNumber_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.ZG_CusNumberInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[6/13] CUS code", captionResourceString.Caption);
				AssertEquals("ShortCaption", "[6/13] CUS code", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/13] CUS code", captionResourceString.MediumCaption);
			});
		}

		public void TestJI_SupplementaryCode1_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_SupplementaryCode1Info, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[6/16] Commodity Code – First TARIC additional code", captionResourceString.Caption);
				AssertEquals("ShortCaption", "[6/16] Add. Code 1", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/16] First Add. Code", captionResourceString.MediumCaption);
			});
		}

		public void TestJI_SupplementaryCode2_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_SupplementaryCode2Info, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[6/16] Commodity Code – Second TARIC additional code", captionResourceString.Caption);
				AssertEquals("ShortCaption", "[6/16] Add. Code 2", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/16] Second Add. Code", captionResourceString.MediumCaption);
			});
		}

		public void TestJI_AdditionalSupplements_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_AdditionalSupplementsInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[6/16] Commodity Code – Further TARIC additional codes", captionResourceString.Caption);
				AssertEquals("ShortCaption", "[6/16] Fur. Codes", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/16] Further Add. Codes", captionResourceString.MediumCaption);
			});
		}

		public void TestJI_ZZF_NKTaxType_Caption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(invoiceLine.JI_ZZF_NKTaxTypeInfo, declaration.MultipleKeysToUse);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[6/17] Commodity Code – National additional code(s)", captionResourceString.Caption);
				AssertEquals("ShortCaption", "[6/17] VAT", captionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "[6/17] VAT", captionResourceString.MediumCaption);
			});
		}

		public void TestMaxNumberOfAdditionalProcedureCode()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Import", 99, invoiceLine.MaxNumberOfAdditionalProcedureCode);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export", 99, invoiceLine.MaxNumberOfAdditionalProcedureCode);
			});
		}

		public override void TestMaxSupportingDocuments()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Import MaxSupportingDocuments default value", -1, invoiceLine.MaxSupportingDocuments);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export MaxSupportingDocuments default value", 99, invoiceLine.MaxSupportingDocuments);
			});
		}

		public void TestCheckSupportingDocumentCount()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supportingDocumentInInvoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().SupportingDocuments.AddNew();
			var invoiceLine = (JobComInvoiceLine)supportingDocumentInInvoiceLine.Parent;
			var supportingDocuments = invoiceLine.SupportingDocuments;
			for (int i = 0; i < 99; i++)
			{
				supportingDocuments.AddNew();
			}
			AssertEquals("Precondition", 100, supportingDocuments.Count);

			var expectedMessageError = "The maximum number of supporting documents allowed is 99.";
			invoiceLine.Validation.ValidateAll();
			Assert("When supporting document count > 99", invoiceLine.SupportingDocuments.GetMessageErrors().ContainsNotificationContaining(expectedMessageError));

			supportingDocuments.RemoveAndDelete(supportingDocuments[99]);
			invoiceLine.Validation.ValidateAll();
			AssertEquals("When supporting document count <= 99", false, invoiceLine.SupportingDocuments.GetMessageErrors().ContainsNotificationContaining(expectedMessageError));
		}

		public void TestNeedsCustomsQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, Universal.Constants.TariffTypes.Export);
			Factory.Save();

			var tariffCode = "08091998";
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, Core.Constants.Weight.Kilograms);
			Factory.Save();

			invoiceLine.JI_Tariff = tariffCode;
			AssertEquals("Precondition: base.NeedsCustomsQuantity is true", false, invoiceLine.CustomsUQ.IsEmpty);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertEquals("EXS", false, invoiceLine.NeedsCustomsQuantity);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertEquals("EXP", true, invoiceLine.NeedsCustomsQuantity);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertEquals("REX", false, invoiceLine.NeedsCustomsQuantity);

			declaration.JE_ApplicationCode = "V1";
			var instrction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instrction.PK;
			instrction.CEI_Style = "H2";
			AssertEquals("UCC5 and H2 or H3", false, invoiceLine.NeedsCustomsQuantity);
		}

		public void TestGetCusSupportingInfoTypes_PreviousDocument()
		{
			AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)invoiceLine).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		}

		public void TestGetCusSupportingInfoTypes_AdditionalInfo()
		{
			AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)invoiceLine).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestGetCusSupportingInfoTypes_SupportingDocument()
		{
			AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)invoiceLine).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		}

		public void TestPreviousDocuments()
		{
			AssertType<PreviousDocumentCollection>(invoiceLine.PreviousDocuments);
		}

		public void TestAdditionalInfos()
		{
			AssertType<AdditionalInfoCollection>(invoiceLine.AdditionalInfos);
		}

		public void TestSupportingDocuments_Type()
		{
			AssertType<SupportingDocumentCollection>(invoiceLine.SupportingDocuments);
		}

		public void TestLookups()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<JobComInvoiceLineLookups>(IEJobMessageTypeList.Codes.Import, invoiceLine.Lookups);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<JobComInvoiceLineLookups>(IEJobMessageTypeList.Codes.Export, invoiceLine.Lookups);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobComInvoiceLineLookups>(IEJobMessageTypeList.Codes.MiscellaneousCustoms, invoiceLine.Lookups);
		}

		public void TestValidation()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				AssertType<JobComInvoiceLineValidation>("no header", invoiceLine.Validation);
				var invoice = Factory.New<JobComInvoiceHeader>();
				invoiceLine.JI_JZ = invoice.PK;
				var list = new List<string>(invoice.Lookups.MessageTypes.GetAllCodes());
				list.Remove(IEJobMessageTypeList.Codes.Export);
				invoice.JZ_StandAloneInvoiceDirection = IEJobMessageTypeList.Codes.Export;
				AssertType<ExportJobComInvoiceLineValidation>("Standalone EXP", invoiceLine.Validation);
				foreach (var code in list)
				{
					invoice.JZ_StandAloneInvoiceDirection = code;
					AssertType<JobComInvoiceLineValidation>("Standalone " + code, invoiceLine.Validation);
				}
				var declaration = Factory.New<JobDeclaration>();
				invoice.JZ_JE = declaration.PK;
				list = new List<string>(invoice.Lookups.MessageTypes.GetAllCodes());
				list.Remove(IEJobMessageTypeList.Codes.Export);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertType<ExportJobComInvoiceLineValidation>("EXP", invoiceLine.Validation);
				list.Remove(IEJobMessageTypeList.Codes.ReExport);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
				AssertType<ReExportJobComInvoiceLineValidation>("REX", invoiceLine.Validation);
				list.Remove(IEJobMessageTypeList.Codes.ExitSummary);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
				AssertType<ExitSummaryJobComInvoiceLineValidation>("EXS", invoiceLine.Validation);
				list.Remove(IEJobMessageTypeList.Codes.Import);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertType<ImportJobComInvoiceLineValidation>("IMP", invoiceLine.Validation);
				foreach (var code in list)
				{
					declaration.JE_MessageType = code;
					AssertType<JobComInvoiceLineValidation>(code, invoiceLine.Validation);
				}
			});
		}

		public void TestFiscalReferencesType()
		{
			AssertType<CusFiscalReference>(invoiceLine.FiscalReferences.AddNew());
		}

		public void TestJI_RN_NKCountryOfDestination()
		{
			CombineAssertions(() =>
			{
				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				AssertEquals("Value comes from the effective parent (declaration).", Core.Constants.CountryCodes.France, invoiceLine.ZG_CountryOfDestination);

				invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Italy;
				AssertEquals("Value comes from self (invoice line).", Core.Constants.CountryCodes.Italy, invoiceLine.ZG_CountryOfDestination);
			});
		}

		public void TestJI_FormattedProcedure()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_FormattedProcedure = "12345678";
				AssertEquals("Input without dot, JI_FormattedProcedure", "12345678", invoiceLine.JI_FormattedProcedure);

				invoiceLine.JI_FormattedProcedure = "123.4567";
				AssertEquals("Input with dot, JI_FormattedProcedure", "123.4567", invoiceLine.JI_FormattedProcedure);
			});
		}

		public void TestJI_OA_ExporterAddress_GetDefaultAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ExporterAddress_ZAddress.OrgPK = orgHeader.PK;

			AssertEquals(orgHeader.MainAddress.PK, invoiceLine.JI_OA_ExporterAddress);
		}

		public void TestJI_OA_ConsigneeAddress_GetDefaultAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ConsigneeAddress_ZAddress.OrgPK = orgHeader.PK;

			AssertEquals(orgHeader.MainAddress.PK, invoiceLine.JI_OA_ConsigneeAddress);
		}

		public override void TestGetNewPackagesPivotCollection()
		{
			Assert("Having TestIsSupportEmptyPackTypeAndValidation, skipping the base case.", true);
		}

		public override void TestIsSupportEmptyPackTypeAndValidation()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var pkgVG = declaration.Packages.AddNew();
			pkgVG.CW_PackType = "VG";
			pkgVG.CW_MarksAndNos = "VGMark";
			var pkgNE = declaration.Packages.AddNew();
			pkgNE.CW_PackType = "NE";
			pkgNE.CW_MarksAndNos = "NEMark";
			var pkg1A = declaration.Packages.AddNew();
			pkg1A.CW_PackType = "1A";
			pkg1A.CW_MarksAndNos = "1AMark";

			var vgLink = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.Package == pkgVG);
			vgLink.IsLinked = true;
			AssertNoMessageErrorContaining("QTY not required for Bulk packages.", vgLink.PackQtyInfo, MandatoryValidation.YouHaveNotEntered);

			var neLink = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.Package == pkgNE);
			neLink.IsLinked = true;
			AssertHasMessageErrorContaining("QTY required for BreakBulk packages.", neLink.PackQtyInfo, MandatoryValidation.YouHaveNotEntered);

			var otherLink = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().First(link => link.Package == pkg1A);
			otherLink.IsLinked = true;
			AssertNoMessageErrorContaining("QTY not required for normal packages.", otherLink.PackQtyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public override void TestGetNewLinkPackValidation()
		{
			var supporter = (ICusLinkPackageSupporter)invoiceLine;
			var linkPackage = new InvoiceLineCusLinkPackageCollection(invoiceLine).AddNew();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportInvoiceLinePackageValidation>(supporter.GetNewLinkPackValidation(linkPackage));

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<InvoiceLinePackageValidation>(supporter.GetNewLinkPackValidation(linkPackage));
		}

		public void TestJI_ZZF_NKTaxType()
		{
			var declaration = GetJobDeclaration(Factory, DeclarationImportMessageType);
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			AssertEquals("Default VAT", "VATS", line.JI_ZZF_NKTaxType);

			var expDeclaration = GetJobDeclaration(Factory, DeclarationExportMessageType);
			var expInvoice = expDeclaration.Invoices.AddNew();
			var expLine = expInvoice.InvoiceLines.AddNew();
			AssertEquals("Default VAT", string.Empty, expLine.JI_ZZF_NKTaxType);
		}

		public void TestJI_Tariff_RefreshCusLineTariffDetails()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			var ieDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euDataGrouping);

			var cusTariffTypeX2 = helper.CreateNewOrGetExistingTariffType(ieDataGrouping.ZZZ_DataGrouping, "X2");
			var cusTariffTypeIMP = helper.CreateNewOrGetExistingTariffType(ieDataGrouping.ZZZ_DataGrouping, Universal.Constants.TariffTypes.Import);

			var cusTariffX203 = helper.LoadOrCreateNewTariff(ieDataGrouping.ZZZ_DataGrouping, cusTariffTypeX2.PK, "X203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Cigarettes containing tobaco (a)");
			var cusTariff2402209000 = helper.LoadOrCreateNewTariff(ieDataGrouping.ZZZ_DataGrouping, cusTariffTypeIMP.PK, "2402209000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Others");

			var cusTariffRelationship = helper.CreateTariffRelationship(cusTariffX203.PK, cusTariffTypeIMP.PK, cusTariff2402209000.ZZ1_TariffCode);
			Factory.Save();

			var declaration = GetJobDeclaration(Factory, DeclarationImportMessageType);
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = cusTariff2402209000.ZZ1_TariffCode;

			CombineAssertions(() =>
			{
				AssertEquals("1 line should be added to the CusLineTariffDetails", 1, invoiceLine.CusLineTariffDetails.Count);
				var cusLineTariffDetail = invoiceLine.CusLineTariffDetails.FirstOrDefault();
				AssertEquals("BZ_Type of CusLineTariffDetail should be 'X2'", "X2", cusLineTariffDetail?.BZ_Type);
				AssertEquals("BZ_Tariff of CusLineTariffDetail should be 'X203'", "X203", cusLineTariffDetail?.BZ_Tariff);

				invoiceLine.JI_Tariff = "1234567890";
				AssertEquals("CusLineTariffDetail will be cleared when another tariff is entered.", 0, invoiceLine.CusLineTariffDetails.Count);

				invoiceLine.JI_Tariff = cusTariff2402209000.ZZ1_TariffCode;
				AssertEquals("CusLineTariffDetail should be recreated.", 1, invoiceLine.CusLineTariffDetails.Count);
			});
		}

		public void TestCusLineTariffDetails()
		{
			var declaration = GetJobDeclaration(Factory, DeclarationImportMessageType);
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			AssertType<CusLineTariffDetailCollection>(line.CusLineTariffDetails);
		}

		public void TestGetBR8011Key()
		{
			invoiceLine.JI_Tariff = "0303001010";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_ConcessionOrder = "100001";

			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
			AssertEquals("AU_0303001010_100001", invoiceLine.GetBR8011Key());
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
			AssertEquals("US_0303001010_100001", invoiceLine.GetBR8011Key());
		}

		public void TestIsBR8011PreferenceUsed()
		{
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
			AssertEquals(true, invoiceLine.IsBR8011PreferenceUsed());
			invoiceLine.JI_PrimaryPreference = "600";
			AssertEquals(false, invoiceLine.IsBR8011PreferenceUsed());
		}

		public void TestHasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			CombineAssertions(() =>
			{
				AssertEquals(false, invoiceLine.HasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader);

				var preDoc = entryInstruction.PreviousDocuments.AddNew();
				preDoc.CSI_Code = Constants.PreviousDocumentTypeCodes.MRN;
				AssertEquals(true, invoiceLine.HasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader);

				preDoc.CSI_Code = Constants.SupportingDocumentCodes._N018;
				AssertEquals(false, invoiceLine.HasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader);

				preDoc = invoiceLine.InvoiceHeader.PreviousDocuments.AddNew();
				preDoc.CSI_Code = Constants.PreviousDocumentTypeCodes.MRN;
				AssertEquals(true, invoiceLine.HasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader);

				preDoc.CSI_Code = Constants.SupportingDocumentCodes._N990;
				AssertEquals(false, invoiceLine.HasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader);

				preDoc = invoiceLine.PreviousDocuments.AddNew();
				preDoc.CSI_Code = Constants.PreviousDocumentTypeCodes.MRN;
				AssertEquals(true, invoiceLine.HasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader);

				preDoc.CSI_Code = Constants.SupportingDocumentCodes._C990;
				AssertEquals(false, invoiceLine.HasMRNPreviousDocumentsIncludingEntryInstructionAndInvoiceHeader);
			});
		}

		public void TestIsBR2001PreviousProcedureCodeUsed()
		{
			var procedureCodeForBR2001 = new[] { "07", "41", "43", "45", "51", "53", "54", "71", "76", "77", "78", "10", "11", "21", "22", "23" };
			foreach (var code in procedureCodeForBR2001)
			{
				invoiceLine.JI_Procedure = $"40{code}C01";
				AssertEquals($"Previous procedure is {code}", true, invoiceLine.IsBR2001PreviousProcedureCodeUsed);
			}

			var procedureCodeIsNotForBR2001 = new[] { "01", "08", "09" };
			foreach (var code in procedureCodeIsNotForBR2001)
			{
				invoiceLine.JI_Procedure = $"40{code}C01";
				AssertEquals($"Previous procedure is {code}", false, invoiceLine.IsBR2001PreviousProcedureCodeUsed);
			}
		}

		public void TestPreferenceCode()
		{
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.NormalThirdCountryTariffDuty;
			AssertEquals(Constants.PreferenceCode.Code1, invoiceLine.PreferenceCode);
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Codes.TemporaryTariffQuota;
			AssertEquals(Constants.PreferenceCode.Code3, invoiceLine.PreferenceCode);
		}

		public void TestC100SupportingDocReferences()
		{
			var sup1 = invoiceLine.SupportingDocuments.AddNew();
			sup1.CSI_Code = SupportingDocumentCodes._C100;
			sup1.CSI_ReferenceNumber = "AAA1";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var sup2 = invoiceLine2.SupportingDocuments.AddNew();
			sup2.CSI_Code = SupportingDocumentCodes._C100;
			sup2.CSI_ReferenceNumber = "AAA2";
			var sup3 = invoiceLine2.SupportingDocuments.AddNew();
			sup3.CSI_Code = SupportingDocumentCodes._C100;
			sup3.CSI_ReferenceNumber = "AAA1";
			var sup4 = invoiceLine2.SupportingDocuments.AddNew();
			sup4.CSI_Code = SupportingDocumentCodes._C503;
			sup4.CSI_ReferenceNumber = "AAA3";
			var sup5 = invoiceLine2.SupportingDocuments.AddNew();
			sup5.CSI_Code = SupportingDocumentCodes._C100;
			sup5.CSI_ReferenceNumber = "AAA2";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			var sup6 = invoiceLine3.SupportingDocuments.AddNew();
			sup6.CSI_Code = SupportingDocumentCodes._C100;
			sup6.CSI_ReferenceNumber = "AAA4";
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder("invoiceLine", new[] { "AAA1" }, invoiceLine.C100SupportingDocReferences);
				AssertContainsExactElementsInExactOrder("invoiceLine2", new[] { "AAA2", "AAA1", "AAA2" }, invoiceLine2.C100SupportingDocReferences);
				AssertContainsExactElementsInExactOrder("invoiceLine3", new[] { "AAA4" }, invoiceLine3.C100SupportingDocReferences);
			});
		}

		public void TestGetValuationCalculatorType()
		{
			var lineForTest = Factory.New<JobComInvoiceLineForTest>();
			AssertType<IeCustomsValuationCalculator>(lineForTest.ValuationCalculator);
		}

		public override void TestEffectiveCountryOfOrigin()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				invoiceLine.JI_CountryOfOrigin = "AU";
				AssertEquals("IMP and ZG_CountryOfSupply is empty", "AU", invoiceLine.EffectiveCountryOfOrigin);

				invoiceLine.ZG_CountryOfSupply = "NZ";
				AssertEquals("IMP and ZG_CountryOfSupply isn't empty", "NZ", invoiceLine.EffectiveCountryOfOrigin);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				AssertEquals("EXP and ZG_CountryOfSupply isn't empty", "AU", invoiceLine.EffectiveCountryOfOrigin);
			});
		}

		protected override Type GetExpectedCusEntryLineType() => typeof(CusEntryLine);

		protected override Type GetExpectedPartType() => typeof(MasterFiles.OrgSupplierPart);

		protected override BaseJobDeclaration GetJobDeclaration() => GetJobDeclaration(Factory, IEJobMessageTypeList.Codes.Import);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => invoiceLine;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetInvoiceLine(factory).InvoiceLine;

		protected override ZString OFTChargeDescription => "INTERNATIONAL FREIGHT (DEDUCT BEFORE ADDING PORTION TO BORDER) from Entry";

		protected override Type ExpectedTypeOfCharges => typeof(EU.Business.Declaration.InvoiceLineChargeCollection<InvoiceLineCharge>);

		protected override Type ExpectedTypeOfApportionedCharges => typeof(Common.JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type GetExpectedEntryInstructionType() => typeof(CusEntryInstruction);

		protected override string OverseasInsuranceCode => AISChargeCodeList.Codes.AK;

		protected override void CreateEUCountry()
		{
			var refCountry = Factory.New<RefCountry>();
			refCountry.RN_Code = "EU";
			refCountry.RN_Desc = euDescription;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "Country");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "EU", euDescription, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var list = (ZZRefCusCodeListCombinedCollection)invoiceLine.Lookups.CountryOfOrigins;
			list.Load();
		}

		protected override string euDescription => "European Community";

		protected override void SetUp()
		{
			base.SetUp();
			(declaration, invoiceLine) = GetInvoiceLine(Factory);
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;

		static (JobDeclaration Declaration, JobComInvoiceLine InvoiceLine) GetInvoiceLine(BusinessObjectFactory factory)
		{
			var declaration = GetJobDeclaration(factory);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			return (declaration, invoiceLine);
		}

		static JobDeclaration GetJobDeclaration(BusinessObjectFactory factory, string messageType = null)
		{
			var declaration = factory.New<JobDeclaration>();
			if (!string.IsNullOrEmpty(messageType))
			{
				declaration.JE_MessageType = messageType;
			}
			return declaration;
		}

		class JobComInvoiceLineForTest : JobComInvoiceLine
		{
			public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new AddInfoJobComInvoiceLine AddInfo => (AddInfoJobComInvoiceLine)base.AddInfo;

			public new ICustomsValuationCalculator ValuationCalculator => base.ValuationCalculator;
		}
	}
}
