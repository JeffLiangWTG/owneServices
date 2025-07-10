using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPDATLineProvider))]
	sealed class EXPDATLineProviderTest : AESLineProviderAbstractTest<EXPDATLineProvider>
	{
		public void TestTransactionType()
		{
			header.Setup(m => m.TransactionType).Returns("3");
			invoice.JZ_ValuationCode = "2";
			AssertEquals(ZString.Empty, Provider.TransactionType);
		}

		public void TestTransactionType_EmptyInHeader()
		{
			header.Setup(m => m.TransactionType).Returns(ZString.Empty);
			invoice.JZ_ValuationCode = "2";
			AssertEquals("2", Provider.TransactionType);
		}

		public void TestCountryOfExport_Style4thDigitIsNot9()
		{
			header.Setup(m => m.ExportCountry).Returns(Core.Constants.CountryCodes.Germany);
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			AssertEquals("CEI_Style != '***9**', Header is not empty, CountryOfExport schould be empty", ZString.Empty, Provider.CountryOfExport);
		}

		public void TestCountryOfExport_Style4thDigitIs9()
		{
			header.Setup(m => m.ExportCountry).Returns(Core.Constants.CountryCodes.Germany);
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000902;
			invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			AssertEquals("CEI_Style == '***9**', Header is not empty, CountryOfExport schould be empty", ZString.Empty, Provider.CountryOfExport);
		}

		public void TestCountryOfExport_EmptyInHeader_Style4thDigitIs9()
		{
			header.Setup(m => m.ExportCountry).Returns(ZString.Empty);
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000902;
			invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			AssertEquals("CEI_Style == '***9**', Header is empty, CountryOfExport schould be mapped from JI_RN_NKCountryOfExport", Core.Constants.CountryCodes.Australia, Provider.CountryOfExport);
		}

		public void TestCommercialReferenceNumber()
		{
			header.Setup(m => m.CommercialReferenceNumber).Returns("ABC");
			invoice.JZ_UCR = "123";
			AssertEquals(ZString.Empty, Provider.CommercialReferenceNumber);
		}

		public void TestCommercialReferenceNumber_EmptyInHeader()
		{
			header.Setup(m => m.CommercialReferenceNumber).Returns(ZString.Empty);
			invoice.JZ_UCR = "123";
			AssertEquals("123", Provider.CommercialReferenceNumber);
		}

		public void TestAuthorisations()
		{
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = AdditionalDocTypeList.Codes.Authorization;
			AssertEquals(2, Provider.Authorisations.Count);
		}

		public void TestRequestedProcedure()
		{
			invoiceLine.JI_Procedure = "1041A51";
			AssertEquals("10", Provider.RequestedProcedure);
		}

		public void TestPreviousProcedure()
		{
			invoiceLine.JI_Procedure = "1041A51";
			AssertEquals("41", Provider.PreviousProcedure);
		}

		public void TestAdditionalProcedure()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Procedure = "1041A51";
				AssertEquals("Has AdditionalProcedure", "A51", Provider.AdditionalProcedure);
				invoiceLine.JI_Procedure = "1040";
				AssertEquals("No AdditionalProcedure", ZString.Empty, Provider.AdditionalProcedure);
			});
		}

		public void TestConsignor()
		{
			header.Setup(m => m.Consignor).Returns(new Mock<IAESParty>().Object);
			var address = CreateAddress1();
			invoiceLine.JI_OA_ExporterAddress = address.PK;
			AssertNull(Provider.Consignor);
		}

		public void TestConsignor_EmptyInHeader()
		{
			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			header.Setup(m => m.Consignor).Returns((IAESParty)null);
			var address = CreateAddress1();
			invoiceLine.JI_OA_ExporterAddress = address.PK;
			AssertPartyWithoutContactPerson(Provider.Consignor
					, "GREOR1"
					, "EBS1"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany);
		}

		public void TestConsignor_EmptyInHeader_ExporterAddressIsNull()
		{
			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var address2 = CreateAddress2();
			header.Setup(m => m.Consignor).Returns((IAESParty)null);
			declaration.SupplierDocumentaryAddress.E2_OA_Address = address2.PK;
			AssertPartyWithoutContactPerson(Provider.Consignor
					, "GREOR2"
					, "EBS2"
					, "MAX MUSTERMANN2"
					, "TESTSTRASSE 2"
					, "MAINZ"
					, "55122"
					, Core.Constants.CountryCodes.Germany);
		}

		public void TestConsignee()
		{
			header.Setup(m => m.Consignee).Returns(new Mock<IAESParty>().Object);
			var address = CreateAddress1();
			invoiceLine.JI_OA_ConsigneeAddress = address.PK;
			AssertNull(Provider.Consignee);
		}

		public void TestConsignee_EmptyInHeader()
		{
			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			header.Setup(m => m.Consignee).Returns((IAESParty)null);
			var address = CreateAddress1();
			invoiceLine.JI_OA_ConsigneeAddress = address.PK;
			AssertPartyWithoutContactPerson(Provider.Consignee
					, "GREOR1"
					, "EBS1"
					, "MAX MUSTERMANN"
					, "TESTSTRASSE 1"
					, "MAINZ"
					, "55126"
					, Core.Constants.CountryCodes.Germany);
		}

		public void TestConsignee_EmptyInHeader_ConsigneeAddressIsNull()
		{
			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			header.Setup(m => m.Consignee).Returns((IAESParty)null);
			var address2 = CreateAddress2();
			invoice.JZ_OA_ConsigneeAddress = address2.PK;
			AssertPartyWithoutContactPerson(Provider.Consignee
					, "GREOR2"
					, "EBS2"
					, "MAX MUSTERMANN2"
					, "TESTSTRASSE 2"
					, "MAINZ"
					, "55122"
					, Core.Constants.CountryCodes.Germany);
		}

		public void TestAdditionalSupplyChainActors()
		{
			var reference1 = invoiceLine.CusSupplyChainActorReferences.AddNew();
			reference1.CFR_Code = "FR1";
			var reference2 = invoiceLine.CusSupplyChainActorReferences.AddNew();
			reference2.CFR_Code = "FR2";
			AssertEquals(2, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestCountryOfOrigin()
		{
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
			AssertEquals(Core.Constants.CountryCodes.Germany, Provider.CountryOfOrigin);
		}

		public void TestOriginFederalState()
		{
			invoiceLine.JI_StateOrRegionOfOrigin = OriginFederalStateList.Codes.SchleswigHolstein;
			AssertEquals("01", Provider.OriginFederalState);
		}

		public void TestGoodsDescription()
		{
			CombineAssertions("GoodsDescription", () =>
			{
				invoiceLine.JI_NDescription = string.Empty;
				invoiceLine.JI_Description = "Description";
				AssertEquals("GoodsDescription is JI_Description", "Description", Provider.GoodsDescription);
				invoiceLine.JI_NDescription = "NDescription";
				AssertEquals("GoodsDescription is JI_NDescription", "NDescription", Provider.GoodsDescription);
			});
		}

		public void TestCusCode()
		{
			invoiceLine.ZG_CusNumber = "AA";
			AssertEquals("AA", Provider.CusCode);
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			invoiceLine.JI_Tariff = "27101231";
			AssertEquals("271012", Provider.HarmonizedSystemSubHeadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			invoiceLine.JI_Tariff = "27101231";
			AssertEquals("31", Provider.CombinedNomenclatureCode);
		}

		public void TestTaricFirstAdditionalCode()
		{
			invoiceLine.JI_SupplementaryCode1 = "1234";
			AssertEquals("1234", Provider.TaricFirstAdditionalCode);
		}

		public void TestTaricSecondAdditionalCode()
		{
			invoiceLine.JI_SupplementaryCode2 = "5678";
			AssertEquals("5678", Provider.TaricSecondAdditionalCode);
		}

		public void TestTaricOtherAdditionalCodes()
		{
			invoiceLine.AdditionalSupplementaryCodes.AddNew("TF03");
			invoiceLine.AdditionalSupplementaryCodes.AddNew("TF04");

			var actual = Provider.TaricOtherAdditionalCodes;
			AssertContainsExactElementsInAnyOrder("Has 2 codes", new[] { "TF03", "TF04" }, actual);
		}

		public void TestTaricOtherAdditionalCodesSpecified()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, true))
				{
					AssertEquals("Functionality is enabled", false, Provider.TaricOtherAdditionalCodesSpecified);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, false))
				{
					AssertEquals("Functionality is disabled", true, Provider.TaricOtherAdditionalCodesSpecified);
				}
			});
		}

		public void TestDangerousGoodsCodes_Empty()
		{
			invoiceLine.UNDGs.RemoveAllFromRelationship();
			AssertEquals(0, Provider.DangerousGoodsCodes.Count);
		}

		public void TestDangerousGoodsCodes_Multiple()
		{
			for (int i = 0; i < 99; i++)
			{
				invoiceLine.UNDGs.AddNew();
			}

			AssertEquals("The amount of DangerousGoodsCode should be 99", 99, Provider.DangerousGoodsCodes.Count);
		}

		public void TestGrossMass()
		{
			invoiceLine.JI_Weight = 3.2512m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			var secondInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			secondInvoiceLine.JI_Weight = 0.00325m;
			secondInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			secondInvoiceLine.JI_CL = entryLine.PK;

			var thirdInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			thirdInvoiceLine.JI_Weight = ZDecimal.Zero;
			thirdInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			thirdInvoiceLine.JI_CL = entryLine.PK;

			AssertEquals(6.251m, Provider.GrossMass);
		}

		public void TestGrossMass_Normalize()
		{
			invoiceLine.JI_Weight = 2.2000m;

			AssertEquals(2.2m, Provider.GrossMass);
		}

		public void TestGrossMass_Invalid()
		{
			invoiceLine.JI_Weight = 2.2000m;
			invoiceLine.JI_WeightUQ = ZString.Empty;

			AssertEquals(ZDecimal.Zero, Provider.GrossMass);
		}

		public void TestGrossMass_ZeroWeight()
		{
			invoice.JZ_Weight = 100m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_Weight = ZDecimal.Zero;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_LinePrice = 1m;

			AssertEquals(ZDecimal.Zero, Provider.GrossMass);
		}

		public void TestNetMass()
		{
			invoiceLine.JI_CustomsQuantity = 2.2512m;

			var secondInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			secondInvoiceLine.JI_CustomsQuantity = 2.2521m;
			secondInvoiceLine.JI_CL = entryLine.PK;

			AssertEquals(4.503m, Provider.NetMass);
		}

		public void TestNetMass_Round3()
		{
			invoiceLine.JI_CustomsQuantity = 2.2514m;
			AssertEquals(2.251m, Provider.NetMass);
		}

		public void TestNetMass_Normalize()
		{
			invoiceLine.JI_CustomsQuantity = 2.2000m;
			AssertEquals(2.2m, Provider.NetMass);
		}

		public void TestPackages()
		{
			var package = invoiceLine.Declaration.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = "CT";
			package.CW_MarksAndNos = "MARKS AND NUMBERS";

			AssertEquals("No linked Package", 0, Provider.Packages.Count);
		}

		public void TestPackages_OnePackage()
		{
			var package = invoiceLine.Declaration.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = "CT";
			package.CW_MarksAndNos = "MARKS AND NUMBERS";
			var npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var npbo = npbos.Cast<BaseCusLinkPackage>().FirstOrDefault();
			npbo.IsLinked = true;
			var pack = Provider.Packages.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 10, pack.Quantity);
				AssertEquals("Kind", "CT", pack.Kind);
				AssertEquals("Marks", "MARKS AND NUMBERS", pack.MarksNumbers);
			});
		}

		public void TestPackages_TwoPackages()
		{
			var package = invoiceLine.Declaration.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = "CT";
			package.CW_MarksAndNos = "MARKS AND NUMBERS";
			var npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var npbo = npbos.Cast<BaseCusLinkPackage>().First();
			npbo.IsLinked = true;
			invoiceLine.Declaration.Packages.AddNew();
			npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			npbo = npbos.Cast<BaseCusLinkPackage>().Last();
			npbo.IsLinked = true;

			AssertEquals("Packages Count = 2", 2, Provider.Packages.Count);
		}

		public void TestPackages_Order()
		{
			var package1 = invoiceLine.Declaration.Packages.AddNew();
			package1.CW_MarksAndNos = "81";
			var package2 = invoiceLine.Declaration.Packages.AddNew();
			package2.CW_MarksAndNos = "86";
			var package3 = invoiceLine.Declaration.Packages.AddNew();
			package3.CW_MarksAndNos = "1-80";
			var package4 = invoiceLine.Declaration.Packages.AddNew();
			package4.CW_MarksAndNos = "84";
			var npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			npbos.Cast<BaseCusLinkPackage>().ForEach(x => x.IsLinked = true);
			AssertSequencesEqual("Packages are sorted by CW_MarksAndNos", new string[] { "1-80", "81", "84", "86" }, Provider.Packages.Select(x => x.MarksNumbers));
		}

		public void TestPreviousDocuments()
		{
			var doc1 = invoiceLine.PreviousDocuments.AddNew();
			doc1.CSI_Code = "AAA";
			doc1.CSI_ReferenceNumber = "REFERENCE1";
			doc1.CSI_Description = "Description 1";

			var secondInvoiceLine = AddSecondInvoiceWithInvoiceLineToDeclaration();

			var doc2 = secondInvoiceLine.PreviousDocuments.AddNew();
			doc2.CSI_Code = "BBB";
			doc2.CSI_ReferenceNumber = "REFERENCE2";
			doc2.CSI_Description = "Description 2";

			var doc3 = secondInvoiceLine.PreviousDocuments.AddNew();
			doc3.CSI_Code = "AAA";
			doc3.CSI_ReferenceNumber = "REFERENCE1";
			doc3.CSI_Description = "Description 3";

			var previousDocuments = Provider.PreviousDocuments;
			AssertEquals("should distinct by code and reference", 2, previousDocuments.Count);
		}

		public void TestDocuments()
		{
			invoiceLine.SupportingDocuments.AddNew();
			var doc = invoiceLine.SupportingDocuments.AddNew();
			doc.CSI_FullType = "3LNACF";
			doc.CSI_ReferenceNumber = "REFERENCE";
			doc.CSI_ItemNumber = 2;
			doc.CSI_Description = "COMPLEMENTARY INFORMATION";
			doc.CSI_ReferenceNumber2 = "REFERENCE2";
			doc.CSI_AdditionalDescription = "ADDITIONAL DESCRIPTION";
			doc.CSI_DateOfIssue = new ZDate(2020, 1, 1);
			doc.CSI_DateOfExpiry = new ZDate(2021, 1, 1);
			doc.CSI_UnitOfQuantity2 = "UQ2";
			doc.CSI_UnitOfQuantity = "kg";
			doc.CSI_Quantity = 1.2345;
			doc.CSI_RX_NKCurrency = "EUR";
			doc.CSI_Value = 2;
			CreateCodeWithAttributes(doc.CSI_FullType);

			CombineAssertions(() =>
			{
				AssertEquals("2 Documents - Count", 2, Provider.Documents.Count);
				var wrappedDoc = Provider.Documents.Last();
				AssertEquals("Type", "3LNA", wrappedDoc.Type);
				AssertEquals("Qualifier", "CF", wrappedDoc.Qualifier);
				AssertEquals("ReferenceNumber", "REFERENCE", wrappedDoc.ReferenceNumber);
				AssertEquals("DocumentLineItemNumber", 2, wrappedDoc.DocumentLineItemNumber);
				AssertEquals("Complement", "COMPLEMENTARY INFORMATION", wrappedDoc.Complement);
				AssertEquals("Detail", "REFERENCE2", wrappedDoc.Detail);
				AssertEquals("IssuingAuthorityName", "ADDITIONAL DESCRIPTION", wrappedDoc.IssuingAuthorityName);
				AssertEquals("IssuingDate", new ZDate(2020, 1, 1), wrappedDoc.IssuingDate);
				AssertEquals("ValidityDate", new ZDate(2021, 1, 1), wrappedDoc.ValidityDate);
				AssertEquals("MeasurementUnitAndQualifier", "UQ2", wrappedDoc.MeasurementUnitAndQualifier);
				AssertEquals("ComplementaryUnit", "kg", wrappedDoc.ComplementaryUnit);
				AssertEquals("Qty", 1.2345m, wrappedDoc.Quantity);
				AssertEquals("Currency", "EUR", wrappedDoc.Currency);
				AssertEquals("Amount", 2m, wrappedDoc.Amount);
			});
		}

		void CreateCodeWithAttributes(string code)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocument, "Supporting Document");

			var refCusCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, code, code, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Detail, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Authority, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.IssuingDate, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.ValidityDate, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.ComplementaryUnit, UniversalReferenceConstants.RefCusCodeListAttributes.Value.No);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode.PK, UniversalReferenceConstants.RefCusCodeListAttributes.Name.Value, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			Factory.Save();
		}

		public void TestAdditionalReferences()
		{
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = "ABC1";
			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			additionalInfo2.CSI_Code = "ABC2";
			var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			additionalInfo3.CSI_Code = "ABC3";
			AssertEquals(2, Provider.AdditionalReferences.Count);
		}

		public void TestAdditionalReferences_Combined()
		{
			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			additionalInfo1.CSI_Code = "Y908";
			additionalInfo1.CSI_ReferenceNumber = "ABC123";
			additionalInfo1.CSI_ReferenceNumber2 = "ABC456";
			additionalInfo1.CSI_RX_NKCurrency = "EUR";
			additionalInfo1.CSI_Value = 100;

			var secondInvoiceLine = AddSecondInvoiceWithInvoiceLineToDeclaration();
			var additionalInfo2 = secondInvoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			additionalInfo2.CSI_Code = "Y908";
			additionalInfo2.CSI_ReferenceNumber = "ABC123";
			additionalInfo2.CSI_ReferenceNumber2 = "ABC456";
			additionalInfo2.CSI_RX_NKCurrency = "EUR";
			additionalInfo2.CSI_Value = 50;

			CombineAssertions(() =>
			{
				var provider = GetProvider();
				AssertEquals("Combined", 1, provider.AdditionalReferences.Count);
				AssertEquals("Combined Amount", 150m, provider.AdditionalReferences.First().Amount);

				additionalInfo2.CSI_Code = "Y901";
				AssertEquals("Different CSI_Code", 2, GetProvider().AdditionalReferences.Count);

				additionalInfo2.CSI_Code = "Y908";
				additionalInfo2.CSI_ReferenceNumber = "ABC124";
				AssertEquals("Different CSI_ReferenceNumber", 2, GetProvider().AdditionalReferences.Count);

				additionalInfo2.CSI_ReferenceNumber = "ABC123";
				additionalInfo2.CSI_ReferenceNumber2 = "ABC457";
				AssertEquals("Different CSI_ReferenceNumber2", 2, GetProvider().AdditionalReferences.Count);

				additionalInfo2.CSI_ReferenceNumber2 = "ABC456";
				additionalInfo2.CSI_RX_NKCurrency = "USD";
				AssertEquals("Different CSI_RX_NKCurrency", 2, GetProvider().AdditionalReferences.Count);
			});
		}

		public void TestAdditionalInformations()
		{
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
			var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
			AssertEquals(2, Provider.AdditionalInformations.Count);
		}

		public void TestTransportChargesPaymentMethod()
		{
			header.Setup(m => m.TransportChargesPaymentMethod).Returns("B");
			invoice.ZG_TransportChargesMethodOfPayment = "A";
			AssertEquals(ZString.Empty, Provider.TransportChargesPaymentMethod);
		}

		public void TestTransportChargesPaymentMethod_EmptyInHeader()
		{
			header.Setup(m => m.TransportChargesPaymentMethod).Returns(ZString.Empty);
			invoice.ZG_TransportChargesMethodOfPayment = "A";
			AssertEquals("A", Provider.TransportChargesPaymentMethod);
		}

		public void TestOutwardProcessingReplacement()
		{
			invoiceLine.ZG_UsualReplacement = ZBool.True;
			AssertEquals(string.Empty, Provider.OutwardProcessingReplacement);
		}

		public void TestOutwardProcessingReplacement_Style1stDigitIs1()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110410;
			CombineAssertions(() =>
			{
				invoiceLine.ZG_UsualReplacement = ZBool.True;
				AssertEquals("ZG_UsualReplacement is true", "1", Provider.OutwardProcessingReplacement);
				invoiceLine.ZG_UsualReplacement = ZBool.False;
				AssertEquals("ZG_UsualReplacement is false", "0", Provider.OutwardProcessingReplacement);
			});
		}

		public void TestOutwardProcessingReimportDate()
		{
			invoiceLine.ZG_ReimportDate = new ZDateTime(2021, 08, 03);
			AssertEquals(default(DateTime), Provider.OutwardProcessingReimportDate);
		}

		public void TestOutwardProcessingReimportDate_Style1stDigitIs1()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110410;
			invoiceLine.ZG_ReimportDate = new ZDateTime(2021, 08, 03);
			AssertEquals(new DateTime(2021, 08, 03), Provider.OutwardProcessingReimportDate);
		}

		public void TestIsWarehouseProcedure()
		{
			CombineAssertions(() =>
			{
				AssertEquals("JI_Procedure is empty", false, Provider.IsWarehouseProcedure);

				invoiceLine.JI_Procedure = "4071";
				AssertEquals("JI_Procedure is 'XX71'", true, Provider.IsWarehouseProcedure);

				invoiceLine.JI_Procedure = "4000";
				AssertEquals("JI_Procedure isn't empty or 'XX71'", false, Provider.IsWarehouseProcedure);
			});
		}

		public void TestWarehouseLocalReferenceNumber()
		{
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.PreviousProcedureMaster.CSI_ReferenceNumber2 = "localreferencenumber";
			AssertEquals(ZString.Empty, Provider.WarehouseLocalReferenceNumber);
		}

		public void TestWarehouseLocalReferenceNumber_IsWarehouseProcedure()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			invoiceLine.PreviousProcedureMaster.CSI_ReferenceNumber2 = "localreferencenumber";
			AssertEquals(ZString.Empty, Provider.WarehouseLocalReferenceNumber);
		}

		public void TestWarehouseLocalReferenceNumber_IsWarehouseProcedure_ATZL()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			invoiceLine.PreviousProcedureMaster.CSI_ReferenceNumber2 = "localreferencenumber";
			AssertEquals("localreferencenumber", Provider.WarehouseLocalReferenceNumber);
		}

		public void TestCustomsWarehousingAuthorisation()
		{
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DECW15864LA000068";
			AssertNull(Provider.CustomsWarehousingAuthorisation);
		}

		public void TestCustomsWarehousingAuthorisation_IsWarehouseProcedure()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DECW15864LA000068";
			AssertNull(Provider.CustomsWarehousingAuthorisation);
		}

		public void TestCustomsWarehousingAuthorisation_IsWarehouseProcedure_ATZL_CWP()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DECWP5864LA000068";
			CombineAssertions(() =>
			{
				AssertEquals("C517", Provider.CustomsWarehousingAuthorisation.Type);
				AssertEquals("DECWP5864LA000068", Provider.CustomsWarehousingAuthorisation.ReferenceNumber);
			});
		}

		public void TestCustomsWarehousingAuthorisation_IsWarehouseProcedure_ATZL_CW1()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DECW15864LA000068";
			CombineAssertions(() =>
			{
				AssertEquals("C518", Provider.CustomsWarehousingAuthorisation.Type);
				AssertEquals("DECW15864LA000068", Provider.CustomsWarehousingAuthorisation.ReferenceNumber);
			});
		}

		public void TestCustomsWarehousingAuthorisation_IsWarehouseProcedure_ATZL_CW2()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DECW25864LA000068";
			CombineAssertions(() =>
			{
				AssertEquals("C519", Provider.CustomsWarehousingAuthorisation.Type);
				AssertEquals("DECW25864LA000068", Provider.CustomsWarehousingAuthorisation.ReferenceNumber);
			});
		}

		public void TestWarehouseProcedures()
		{
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.PreviousProcedures.AddNew();

			AssertEquals(0, Provider.WarehouseProcedures.Count);
		}

		public void TestWarehouseProcedures_IsWarehouseProcedure()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			invoiceLine.PreviousProcedures[0].CSI_Tariff = "98001234001";

			var secondInvoiceLine = AddSecondInvoiceWithInvoiceLineToDeclaration();
			secondInvoiceLine.JI_Procedure = "4071";
			secondInvoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			secondInvoiceLine.PreviousProcedures[0].CSI_Tariff = "98001234000";

			var warehouseProcedures = Provider.WarehouseProcedures;
			AssertEquals(0, warehouseProcedures.Count);
		}

		public void TestWarehouseProcedures_IsWarehouseProcedure_ATZL()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			invoiceLine.PreviousProcedures[0].CSI_Tariff = "98001234001";

			var secondInvoiceLine = AddSecondInvoiceWithInvoiceLineToDeclaration();
			secondInvoiceLine.JI_Procedure = "4071";
			secondInvoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			secondInvoiceLine.PreviousProcedures[0].CSI_Tariff = "98001234000";

			var warehouseProcedures = Provider.WarehouseProcedures;
			AssertEquals(2, warehouseProcedures.Count);
		}

		public void TestIsInwardProcessingProcedure()
		{
			CombineAssertions(() =>
			{
				AssertEquals("JI_Procedure is empty", false, Provider.IsInwardProcessingProcedure);

				invoiceLine.JI_Procedure = "4051";
				AssertEquals("JI_Procedure is 'XX51'", true, Provider.IsInwardProcessingProcedure);

				invoiceLine.JI_Procedure = "4000";
				AssertEquals("JI_Procedure isn't empty or 'XX51'", false, Provider.IsInwardProcessingProcedure);
			});
		}

		public void TestInwardProcessingSimplyGrantedAuthorisation()
		{
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag = ZBool.True;
			AssertEquals(string.Empty, Provider.InwardProcessingSimplyGrantedAuthorisation);
		}

		public void TestInwardProcessingSimplyGrantedAuthorisation_IsInwardProcessingProcedure()
		{
			invoiceLine.JI_Procedure = "4051";
			invoiceLine.PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag = ZBool.True;
			AssertEquals(string.Empty, Provider.InwardProcessingSimplyGrantedAuthorisation);
		}

		public void TestInwardProcessingSimplyGrantedAuthorisation_IsInwardProcessingProcedure_ATAV()
		{
			invoiceLine.JI_Procedure = "4051";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;

			CombineAssertions(() =>
			{
				invoiceLine.PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag = ZBool.True;
				AssertEquals("SimplifiedGrantAuthorizationFlag is true", "1", Provider.InwardProcessingSimplyGrantedAuthorisation);
				invoiceLine.PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag = ZBool.False;
				AssertEquals("SimplifiedGrantAuthorizationFlag is false", "0", Provider.InwardProcessingSimplyGrantedAuthorisation);
			});
		}

		public void TestInwardProcessingAuthorisation()
		{
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DEIPO5863AV000044";
			AssertNull(Provider.InwardProcessingAuthorisation);
		}

		public void TestInwardProcessingAuthorisation_IsInwardProcessingProcedure()
		{
			invoiceLine.JI_Procedure = "4051";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DEIPO5863AV000044";
			AssertNull(Provider.InwardProcessingAuthorisation);
		}

		public void TestInwardProcessingAuthorisation_IsInwardProcessingProcedure_ATAV()
		{
			invoiceLine.JI_Procedure = "4051";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DEIPO5863AV000044";
			CombineAssertions(() =>
			{
				AssertEquals("C601", Provider.InwardProcessingAuthorisation.Type);
				AssertEquals("DEIPO5863AV000044", Provider.InwardProcessingAuthorisation.ReferenceNumber);
			});
		}

		public void TestInwardProcessingCustomsOfficeOfSupervision()
		{
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.PreviousProcedureMaster.CSI_CustomsOffice = "DE00567";
			AssertEquals(string.Empty, Provider.InwardProcessingCustomsOfficeOfSupervision);
		}

		public void TestInwardProcessingCustomsOfficeOfSupervision_IsInwardProcessingProcedure()
		{
			invoiceLine.JI_Procedure = "4051";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			invoiceLine.PreviousProcedureMaster.CSI_CustomsOffice = "DE00567";
			AssertEquals(string.Empty, Provider.InwardProcessingCustomsOfficeOfSupervision);
		}

		public void TestInwardProcessingCustomsOfficeOfSupervision_IsInwardProcessingProcedure_ATAV()
		{
			invoiceLine.JI_Procedure = "4051";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			invoiceLine.PreviousProcedureMaster.CSI_CustomsOffice = "DE00567";
			AssertEquals("DE00567", Provider.InwardProcessingCustomsOfficeOfSupervision);
		}

		public void TestInwardProcessingProcedures()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedures.AddNew();
			AssertEquals(0, Provider.InwardProcessingProcedures.Count);
		}

		public void TestInwardProcessingProcedures_IsInwardProcessingProcedure()
		{
			invoiceLine.JI_Procedure = "4051";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			invoiceLine.PreviousProcedures[0].CSI_Tariff = "98001234001";
			AssertEquals(0, Provider.InwardProcessingProcedures.Count);
		}

		public void TestInwardProcessingProcedures_IsInwardProcessingProcedure_ATAV()
		{
			invoiceLine.JI_Procedure = "4051";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			invoiceLine.PreviousProcedures[0].CSI_Tariff = "98001234001";
			AssertEquals(1, Provider.InwardProcessingProcedures.Count);
		}

		public void TestProcedureTransferenceSpecified()
		{
			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				AssertEquals("CEI_SubStyle isn't '1X' or '2X'", false, Provider.ProcedureTransferenceSpecified);

				invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

				invoiceLine.JI_Procedure = "4051";
				AssertEquals("IsWarehouseProcedure is true, procedure is not empty", true, Provider.ProcedureTransferenceSpecified);

				invoiceLine.JI_Procedure = "4071";
				AssertEquals("IsInwardProcessingProcedure is true, procedure is not empty", true, Provider.ProcedureTransferenceSpecified);

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
				invoiceLine.JI_Procedure = "4051";
				AssertEquals("CEI_SubStyle is '1X'", false, Provider.ProcedureTransferenceSpecified);

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				invoiceLine.JI_Procedure = "4071";
				AssertEquals("IsInwardProcessingProcedure is true", true, Provider.ProcedureTransferenceSpecified);

				invoiceLine.JI_Procedure = "4000";
				AssertEquals("IsInwardProcessingProcedure and IsWarehouseProcedure is false", false, Provider.ProcedureTransferenceSpecified);

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
				invoiceLine.JI_Procedure = "4071";
				AssertEquals("CEI_SubStyle is '2X'", false, Provider.ProcedureTransferenceSpecified);

				entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				invoiceLine.PreviousProcedureMaster.CSI_Procedure = ZString.Empty;

				invoiceLine.JI_Procedure = "4051";
				AssertEquals("IsWarehouseProcedure is true, procedure is empty", false, Provider.ProcedureTransferenceSpecified);

				invoiceLine.JI_Procedure = "4071";
				AssertEquals("IsInwardProcessingProcedure is true, procedure is empty", false, Provider.ProcedureTransferenceSpecified);

				invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				AssertEquals("IsInwardProcessingProcedure is true, procedure is not empty, CEI_SubStyle isn't '1X' or '2X', CEI_Style isn't XXX4XX ", true, Provider.ProcedureTransferenceSpecified);

				entryInstruction.CEI_Style = "222422";
				AssertEquals("CEI_Style is XXX4XX ", false, Provider.ProcedureTransferenceSpecified);
			});
		}

		public void TestAnnotation()
		{
			invoiceLine.AdditionalInfoDescription = "ADDITIONAL INFORMATION";
			AssertEquals("ADDITIONAL INFORMATION", Provider.Annotation);
		}

		public void TestContainerIdentificationNumbers_NoContainer()
		{
			AssertEquals("No linked Container", 0, Provider.ContainerIdentificationNumbers.Count);
		}

		public void TestContainerIdentificationNumbers_1Container()
		{
			var container = invoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "ABC12345";
			var invoiceLineContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			var containerLine = invoiceLineContainer.Cast<NonPersistentCusContainer>().First();
			containerLine.IsForInvoiceLine = true;
			CombineAssertions(() =>
			{
				AssertEquals("Container Count: 1", 1, Provider.ContainerIdentificationNumbers.Count);
				AssertEquals("ContainerNumber", "ABC12345", Provider.ContainerIdentificationNumbers.First());
			});
		}

		public void TestContainerIdentificationNumbers_2Containers()
		{
			var container = invoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "ABC12345";
			var invoiceLineContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			var containerLine = invoiceLineContainer.Cast<NonPersistentCusContainer>().First();
			containerLine.IsForInvoiceLine = true;
			invoiceLine.Declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC23456";
			invoiceLineContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containerLine = invoiceLineContainer.Cast<NonPersistentCusContainer>().Last();
			containerLine.IsForInvoiceLine = true;
			AssertEquals("Container Count: 2", 2, Provider.ContainerIdentificationNumbers.Count);
		}

		public void TestContainerIdentificationNumbers_3ContainersMultipleInvoiceLines()
		{
			invoiceLine.Declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC12345";
			var invoiceLineContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			var containerLine = invoiceLineContainer.Cast<NonPersistentCusContainer>().First();
			containerLine.IsForInvoiceLine = true;

			invoiceLine.Declaration.CusContainers.AddNew().CO_ContainerNumber = "ABC23456";
			invoiceLineContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly;
			containerLine = invoiceLineContainer.Cast<NonPersistentCusContainer>().Single(c => c.ContainerNumber == "ABC23456");
			containerLine.IsForInvoiceLine = true;

			var invoiceLine2 = AddSecondInvoiceWithInvoiceLineToDeclaration();
			invoiceLine2.Declaration.CusContainers.AddNew().CO_ContainerNumber = "TKH12345";
			var invoiceLineContainer3 = invoiceLine2.ContainersForInvoiceLinesForBindingOnly;
			var containerLine3 = invoiceLineContainer3.Cast<NonPersistentCusContainer>().Single(c => c.ContainerNumber == "TKH12345");
			containerLine3.IsForInvoiceLine = true;

			AssertEquals("Container Count: 3", 3, Provider.ContainerIdentificationNumbers.Count);
		}

		public void TestDeliveryTerms()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_IncoTermPlace = "PLACE";
			var deliveryTerms = Provider.DeliveryTerms;
			CombineAssertions(() =>
			{
				AssertEquals("Code", Core.Constants.IncoTerms.FreeOnBoard, deliveryTerms.IncotermCode);
				AssertEquals("Place", "PLACE", deliveryTerms.Location);
			});
		}

		public void TestWarehouseOwnerEmptyWhenPreviousProcedureIsNotWarehouse()
		{
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = "AT-ZL";
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "WarehouseOwner";
			AssertEquals(ZString.Empty, Provider.WarehouseOwner);
		}

		public void TestWarehouseOwner()
		{
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = "AT-ZL";
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "WarehouseOwner";
			AssertEquals("WarehouseOwner", Provider.WarehouseOwner);
		}

		public void TestProcessingOwnerEmpty()
		{
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DECW15864LA000068";
			AssertEquals(ZString.Empty, Provider.ProcessingOwner);
		}

		public void TestProcessingOwner_4051()
		{
			invoiceLine.JI_Procedure = "4051";
			invoiceLine.PreviousProcedureMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
			invoiceLine.PreviousProcedureMaster.AuthorizationNumber = "DECW15864LA000068";
			AssertEquals("DECW15864LA000068", Provider.ProcessingOwner);
		}

		public void TestCountryOfDestination() => AssertNullOrEmpty(Provider.CountryOfDestination);

		public void TestTotalDutiesAndTaxesAmount() => AssertEquals(0.0M, Provider.TotalDutiesAndTaxesAmount);

		public void TestTaxType() => AssertNullOrEmpty(Provider.TaxType);

		public void TestPayableTaxAmount() => AssertEquals(0.0M, Provider.PayableTaxAmount);

		public void TestTaxPaymentMethod() => AssertNullOrEmpty(Provider.TaxPaymentMethod);

		public void TestTaxBaseTaxRate() => AssertEquals(0.0M, Provider.TaxBaseTaxRate);

		public void TestTaxBaseMeasurementUnitAndQualifier() => AssertNullOrEmpty(Provider.TaxBaseMeasurementUnitAndQualifier);

		public void TestTaxBaseQuantity() => AssertEquals(0.0M, Provider.TaxBaseQuantity);

		public void TestTaxBaseAmount() => AssertEquals(0.0M, Provider.TaxBaseAmount);

		public void TestTaxBaseTaxAmount() => AssertEquals(0.0M, Provider.TaxBaseTaxAmount);

		protected override IEnumerable<Expression<Func<EXPDATLineProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Consignor;
			yield return x => x.Consignee;
			yield return x => x.CustomsWarehousingAuthorisation;
			yield return x => x.InwardProcessingAuthorisation;
			yield return x => x.DeliveryTerms;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = new Mock<IEXPDATHeader>();
		}
		Mock<IEXPDATHeader> header;

		protected override EXPDATLineProvider GetProvider() => new EXPDATLineProvider(entryLine, header.Object);

		new IEXPDATLine Provider => base.Provider;

		JobComInvoiceLine AddSecondInvoiceWithInvoiceLineToDeclaration()
		{
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			return invoiceLine2;
		}

		OrgAddress CreateAddress1()
		{
			var orgAddress1 = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			orgAddress1.Header.OH_FullName = "MAX MUSTERMANN";
			orgAddress1.OA_Address1 = "TESTSTRASSE 1";
			orgAddress1.OA_City = "MAINZ";
			orgAddress1.OA_PostCode = "55126";
			orgAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			return orgAddress1;
		}

		OrgAddress CreateAddress2()
		{
			var orgAddress2 = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS2");
			orgAddress2.Header.OH_FullName = "MAX MUSTERMANN2";
			orgAddress2.OA_Address1 = "TESTSTRASSE 2";
			orgAddress2.OA_City = "MAINZ";
			orgAddress2.OA_PostCode = "55122";
			orgAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			return orgAddress2;
		}
	}
}
