using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceLine))]
	class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
	{
		public void TestNFENumber()
		{
			InvoiceLine.JI_NFeNumber = "JI_NFeNumber";
			AssertEquals("JI_NFeNumber", InvoiceLineWrapper.NFENumber);
		}

		public void TestNetWeightUQ()
		{
			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(Core.Constants.Weight.Kilograms, InvoiceLineWrapper.NetWeightUQ);
		}

		public void TestJI_NetWeight()
		{
			InvoiceLine.JI_NetWeight = 10m;
			AssertEquals("JI_NetWeight should be", 10m, InvoiceLineWrapper.NetWeight);
		}

		public override void TestConcessionOrder()
		{
			InvoiceLine.JI_ConcessionOrder = "11111111111";
			AssertEquals("11111111111", InvoiceLine.JI_ConcessionOrder, InvoiceLineWrapper.ConcessionOrder);
		}

		public void TestUsedMaterial()
		{
			InvoiceLine.JI_UsedMaterialOperationType = GoodsConditionOperationTypeList.Codes.ExTariff;
			InvoiceLine.JI_UsedMaterialRegime = ZString.Empty;
			AssertNullOrEmpty(InvoiceLineWrapper.UsedMaterialRegime.CodeAndDescription);
			AssertNullOrEmpty(InvoiceLineWrapper.UsedMaterialOperationType.CodeAndDescription);

			using var mockLanguageData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.PortugueseBrazil).UseMockData();
			var nationalizationResourceString = (ResourceString)UsedMaterialRegimeList.Descriptions.Nationalization;
			mockLanguageData.Put(nationalizationResourceString.ResourceKey, new ResourceStringData(nationalizationResourceString.ResourceKey, "Nationalization"));

			var containerResourceString = (ResourceString)GoodsConditionOperationTypeList.Descriptions.Container;
			mockLanguageData.Put(containerResourceString.ResourceKey, new ResourceStringData(containerResourceString.ResourceKey, "Container"));

			InvoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
			InvoiceLine.JI_UsedMaterialOperationType = GoodsConditionOperationTypeList.Codes.Container;
			AssertEquals("2 - Nationalization", InvoiceLineWrapper.UsedMaterialRegime.CodeAndDescription);
			AssertEquals("07 - Container", InvoiceLineWrapper.UsedMaterialOperationType.CodeAndDescription);
		}

		public void TestManufacturerAddress()
		{
			var manufacturer = OrgHeader.New(Factory);
			var manufacturerAddress = manufacturer.Addresses.AddNew();

			InvoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("ManufacturerAddress", manufacturer.MainAddress.PK, InvoiceLineWrapper.ManufacturerAddress.OrgAddress.PK);

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.ManufacturerDocAddressPK = manufacturerAddress.PK;
			var wapper = DocJobComInvoiceLine.New(InvoiceLine, Factory);
			AssertEquals("ManufacturerAddress", manufacturerAddress.PK, wapper.ManufacturerAddress.OrgAddress.PK);
		}

		public void TestTariffDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			helper.LoadOrCreateNewTariff(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, tariffType.PK, "123", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "123");
			Factory.Save();

			InvoiceLine.JI_Tariff = "123";
			AssertEquals("TariffDescription should be equal", "123", InvoiceLineWrapper.TariffDescription);
		}

		public void TestUsedMaterialRegime()
		{
			using var mockLanguageData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.PortugueseBrazil).UseMockData();
			var nationalizationResourceString = (ResourceString)UsedMaterialRegimeList.Descriptions.Nationalization;
			mockLanguageData.Put(nationalizationResourceString.ResourceKey, new ResourceStringData(nationalizationResourceString.ResourceKey, "Nationalization"));

			InvoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
			AssertEquals(UsedMaterialRegimeList.Codes.Nationalization, "2 - Nationalization", InvoiceLineWrapper.UsedMaterialRegime.CodeAndDescription);
		}

		public void TestUsedMaterialOperationType()
		{
			using var mockLanguageData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.PortugueseBrazil).UseMockData();
			var containerResourceString = (ResourceString)GoodsConditionOperationTypeList.Descriptions.Container;
			mockLanguageData.Put(containerResourceString.ResourceKey, new ResourceStringData(containerResourceString.ResourceKey, "Container"));

			InvoiceLine.JI_UsedMaterialOperationType = GoodsConditionOperationTypeList.Codes.Container;
			AssertEquals(GoodsConditionOperationTypeList.Codes.Container, "07 - Container", InvoiceLineWrapper.UsedMaterialOperationType.CodeAndDescription);
		}

		public void TestDrawbackModality()
		{
			using var mockLanguageData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.PortugueseBrazil).UseMockData();
			var resKey = ((ResourceString)DrawbackModalityList.Descriptions.GenericSuspension).ResourceKey;
			mockLanguageData.Put(resKey, new ResourceStringData(resKey, "Generic Suspension"));

			InvoiceLine.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			AssertEquals(DrawbackModalityList.Codes.GenericSuspension, "1 - Generic Suspension", InvoiceLineWrapper.DrawbackModality.CodeAndDescription);
		}

		public void TestDrawbackCANumber()
		{
			InvoiceLine.DrawbackCANumber = "12345";
			AssertEquals("12345", InvoiceLine.DrawbackCANumber, InvoiceLineWrapper.DrawbackCANumber);
		}

		public void TestFullGoodsDescription()
		{
			var fullDescription = new string('X', InvoiceLine.FullGoodsDescriptionInfo.MaxLength);
			InvoiceLine.FullGoodsDescription = fullDescription;
			AssertEquals("Full Goods Description", fullDescription, InvoiceLineWrapper.FullGoodsDescription);
		}

		public void TestTariffDetach()
		{
			InvoiceLine.TariffDetachs.AddNew().CY_Code = "001";
			InvoiceLine.TariffDetachs.AddNew().CY_Code = "002";

			AssertEquals("TariffDetach should be", "001,002", InvoiceLineWrapper.TariffDetach);
		}

		public void TestNaladiHs()
		{
			InvoiceLine.NaladiHs = "90908000";
			AssertEquals("NaladiHs should be", InvoiceLine.NaladiHs, InvoiceLineWrapper.NaladiHs);
		}

		public void TestTariffAgreement()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, "Tariff Agreement");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, "ASGPC", "ASGPC DESCRIPTION", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));

			Factory.Save();

			InvoiceLine.JI_SecondaryPreference = "ASGPC";
			AssertEquals("ASGPC - ASGPC DESCRIPTION", InvoiceLineWrapper.TariffAgreement.CodeAndDescription);
		}

		public void TestDutyTaxRegime()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxationRegimeCode, "BR Taxation Regime");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxationRegimeCode, "4", "REDUCAO", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.DutyTaxRegime = "4";
			AssertEquals("4 - REDUCAO", InvoiceLineWrapper.DutyTaxRegime.CodeAndDescription);
		}

		public void TestDutyLegalBase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode, "BR Legal Base Regime");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode, "99", "LIVROS, JORNAIS E PERIODICOS", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForDutyLegalBaseList(Factory, "1");
			Factory.Save();

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			InvoiceLine.DutyLegalBase = "99";
			AssertEquals("99 - LIVROS, JORNAIS E PERIODICOS", InvoiceLineWrapper.DutyLegalBase.CodeAndDescription);

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.Declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			InvoiceLine.DutyTaxRegime = "1";
			InvoiceLine.DutyLegalBase = "01";
			AssertEquals("01 - LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", InvoiceLineWrapper.DutyLegalBase.CodeAndDescription);
		}

		public void TestPisCofinsLegalBase()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory);
			ReferenceTestDataHelper.CreateReferenceDataForPISLegalBaseList(Factory, "1");

			Factory.Save();

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.Declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			InvoiceLine.PisCofinsTaxRegime = "1";
			InvoiceLine.PisCofinsLegalBase = "01";
			AssertEquals("01 - PIS LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", InvoiceLineWrapper.PisCofinsLegalBase.CodeAndDescription);
		}

		public void TestIPITaxBenefitLegalActIssuingBody()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "BR Issuing Body");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "01", "LIVROS, JORNAIS E PERIODICOS", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			Factory.Save();

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.IPITaxBenefitLegalActIssuingBody = "01";
			AssertEquals("01 - LIVROS, JORNAIS E PERIODICOS", InvoiceLineWrapper.IPITaxBenefitLegalActIssuingBody.CodeAndDescription);
		}

		public void TestIPITaxBenefitLegalActType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "BR Legal Act");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "01", "LIVROS, JORNAIS E PERIODICOS", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			Factory.Save();

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.IPITaxBenefitLegalActType = "01";
			AssertEquals("01 - LIVROS, JORNAIS E PERIODICOS", InvoiceLineWrapper.IPITaxBenefitLegalActType.CodeAndDescription);
		}

		public void TestAntidumpingLegalActIssuingBody()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "BR Issuing Body");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "01", "LIVROS, JORNAIS E PERIODICOS", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			Factory.Save();

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.Antidumping).CSI_IssuerType = "01";
			AssertEquals("01 - LIVROS, JORNAIS E PERIODICOS", InvoiceLineWrapper.AntidumpingLegalActIssuingBody.CodeAndDescription);
		}

		public void TestAntidumpingLegalActType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "BR Legal Act");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "01", "LIVROS, JORNAIS E PERIODICOS", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			Factory.Save();

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.Antidumping).CSI_Code = "01";
			AssertEquals("01 - LIVROS, JORNAIS E PERIODICOS", InvoiceLineWrapper.AntidumpingLegalActType.CodeAndDescription);
		}

		public void TestIPITaxRegime()
		{
			ReferenceTestDataHelper.CreateReferenceDataForIPITaxRegimeList(Factory);

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.IPITaxRegime = "2";
			AssertEquals("2 - Reduction", InvoiceLineWrapper.IPITaxRegime.CodeAndDescription);
		}
		public void TestPisCofinsTaxRegime()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTaxRegimeList(Factory);
			Factory.Save();

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.Declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			InvoiceLine.PisCofinsTaxRegime = "2";
			AssertEquals("2 - IMUNIDADE", InvoiceLineWrapper.PisCofinsTaxRegime.CodeAndDescription);
		}

		public void TestICMSTaxRegime()
		{
			ReferenceTestDataHelper.CreateReferenceDataForICMSTaxRegimeList(Factory);

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.ICMSTaxRegime = "5";
			AssertEquals("5 - Suspension", InvoiceLineWrapper.ICMSTaxRegime.CodeAndDescription);
		}

		public void TestIPITaxBenefitLegalActNumber()
		{
			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.IPITaxBenefitLegalActNumber = "5";
			AssertEquals("5", InvoiceLineWrapper.IPITaxBenefitLegalActNumber);
		}

		public void TestIPITaxBenefitLegalActYear()
		{
			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.IPITaxBenefitLegalActYear = "2023";
			AssertEquals("2023", InvoiceLineWrapper.IPITaxBenefitLegalActYear);
		}

		public void TestAntidumpingLegalActNumber()
		{
			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.Antidumping).CSI_ReferenceNumber = "6";
			AssertEquals("6", InvoiceLineWrapper.AntidumpingLegalActNumber);
		}

		public void TestAntidumpingLegalActYear()
		{
			var year = ZDateTime.Today.Year.ToString();
			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.LegalActInfos.AddNew(AdditionalTaxTypeList.Codes.Antidumping).CSI_YearOfIssue = year;
			AssertEquals(year, InvoiceLineWrapper.AntidumpingLegalActYear);
		}

		public void TestManufacturerIndicator()
		{
			InvoiceLine.JI_ManufacturerIndicator = ZString.Empty;
			AssertNullOrEmpty(InvoiceLineWrapper.ManufacturerIndicator.CodeAndDescription);

			InvoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
			AssertEquals("1 - O fabricante é o exportador", InvoiceLineWrapper.ManufacturerIndicator.CodeAndDescription);
		}

		public void TestICMSLegalBase()
		{
			ReferenceTestDataHelper.CreateReferenceDataForICMSLegalBaseList(Factory);

			InvoiceLine.Declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			InvoiceLine.ICMSLegalBase = "01";
			AssertEquals("01 - ICMS Legal Base 01", InvoiceLineWrapper.ICMSLegalBase.CodeAndDescription);
		}

		public void TestNFeLinePrice()
		{
			InvoiceLine.JI_NFeLinePrice = ZDecimal.Zero;
			AssertEquals("NFeLinePrice", ZDecimal.Zero, InvoiceLineWrapper.NFeLinePrice);

			InvoiceLine.JI_NFeLinePrice = 10m;
			AssertEquals("NFeLinePrice", 10m, InvoiceLineWrapper.NFeLinePrice);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Brazil; }
		}

		protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLineInternal)
		{
			return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
		}

		#endregion
	}
}
