using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.GB.Business.GBCommonConstants;
using DataBoundResourceStrings = CargoWise.EntityFramework.DataBoundResourceStrings;
using DeclarationApplicationCodeList = Enterprise.Customs.GB.Registry.Business.DeclarationApplicationCodeList;
using DefaultDataGroupingType = Enterprise.Customs.Business.DefaultDataGroupingType;
using EUJobComInvoiceLine = Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineBaseOnlyTest : JobComInvoiceLineTest<JobComInvoiceLine>
	{
		public void TestJI_ZZF_NKTaxTypeReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("JI_ZZF_NKTaxType should be read only", false, invoiceLine.JI_ZZF_NKTaxTypeInfo.ReadOnly);

			declaration.JE_ApplicationCode = "CHF";
			AssertEquals("JI_ZZF_NKTaxType should not be read only", false, invoiceLine.JI_ZZF_NKTaxTypeInfo.ReadOnly);
		}

		public override void TestFetchStrategy()
		{
			AssertType(typeof(JobComInvoiceLineFetchStrategy), InvoiceLine.FetchStrategy);
		}

		public override void TestComponentPrice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_LinePrice = 50m;
			invoiceLine.JI_ValuationMarkup = 10m;
			AssertEquals("Prerequisite.", false, Declaration.Configuration.InvoiceLineConfiguration.InflateItemPriceByValuationMarkup(Declaration));
			AssertEquals("JI_ValuationMarkup should have no effect on ComponentPrice calculation in EU, because InflateItemPriceByValuationMarkup is false.", 50m, invoiceLine.ComponentPrice);
		}

		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be GB", Constants.CountryCodes.UnitedKingdom, InvoiceLine.CustomsCountryCode);
		}

		public void TestRelatedIndicator_Caption()
		{
			AssertEquals("Party relationship, whether there is price influence or not", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.RelatedIndicator)).Caption);
		}

		public void TestRelatedIndicator2_Caption()
		{
			AssertEquals("Restrictions as to the disposal or use of the goods by the buyer in accordance with Article 70(3)(a) of the Code", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.RelatedIndicator2)).Caption);
		}

		public void TestRelatedIndicator3_Caption()
		{
			AssertEquals("Sale or price is subject to some condition or consideration in accordance with Article 70(3)(b) of the Code", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.RelatedIndicator3)).Caption);
		}

		public void TestRelatedIndicator4_Caption()
		{
			AssertEquals("The sale is subject to an arrangement under which part of the proceeds of any subsequent resale, disposal or use accrues directly or indirectly to the seller", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.RelatedIndicator4)).Caption);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Constants.CountryCodes.UnitedKingdom, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestJI_Calc_InstructionDisplaySequence_Caption()
		{
			AssertEquals("Entry Instruction Display Sequence", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_Calc_InstructionDisplaySequence)).Caption);
		}

		public void TestJI_Calc_InstructionDisplaySequence()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Not linked to instruction", ZShort.Zero, invoiceLine.JI_Calc_InstructionDisplaySequence);
				invoiceLine.JI_CEI = instruction.PK;
				instruction.CEI_DisplaySequence = 1;
				AssertEquals("Linked to Instruction", (short)1, invoiceLine.JI_Calc_InstructionDisplaySequence);
			});
		}

		public void TestIsUpdatingDetailsFromPart()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			var invoiceLine = invoiceLineMock.Object;
			invoice.InvoiceLines.Add(invoiceLine);
			invoiceLineMock.Setup(l => l.UpdateDetailsFromProductOnPartChangeCore())
				.Callback(() => AssertEquals(true, invoiceLine.IsUpdatingDetailsFromPart));
			CombineAssertions(() =>
			{
				AssertEquals(false, invoiceLine.IsUpdatingDetailsFromPart);
				invoiceLine.JI_PartNo = "P1234";
				AssertEquals(false, invoiceLine.IsUpdatingDetailsFromPart);
				invoiceLineMock.Verify(l => l.UpdateDetailsFromProductOnPartChangeCore(), Times.Once);
			});
		}

		public void TestTariffUsedForNorthernIrelandImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals(ZString.Empty, invoiceLine.TariffUsedForNorthernIrelandImport);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = AdditonalInfoCodes.NIAID;
			Assert(!invoiceLine.IsEuTariffToBeUsedForNorthernIreland);
			AssertEquals(WindsorFrameworkNIProtocolTariffCodes.GB, invoiceLine.TariffUsedForNorthernIrelandImport);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIDOM;
			Assert(invoiceLine.IsEuTariffToBeUsedForNorthernIreland);
			AssertEquals(WindsorFrameworkNIProtocolTariffCodes.EUN, invoiceLine.TariffUsedForNorthernIrelandImport);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobDeclaration jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			OrgHeader orgHeader = factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			jobDeclaration.JE_OH_Importer = orgHeader.PK;
			jobDeclaration.JE_MessageType = "IMP";
			BaseJobComInvoiceHeader baseJobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
			BaseJobComInvoiceLine baseJobComInvoiceLine = baseJobComInvoiceHeader.InvoiceLines.AddNew();
			CusEntryHeader cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			CusEntryLine cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			if (jobDeclaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine)
			{
				AdditionalInvoiceLineEntryLineLink additionalInvoiceLineEntryLineLink = ((baseJobComInvoiceLine.AdditionalEntryLineLinks.Count > 0) ? baseJobComInvoiceLine.AdditionalEntryLineLinks[0] : null) ?? baseJobComInvoiceLine.AdditionalEntryLineLinks.AddNew();
				additionalInvoiceLineEntryLineLink.BU_CL = cusEntryLine.PK;
				additionalInvoiceLineEntryLineLink.BU_JI = baseJobComInvoiceLine.PK;
			}

			jobDeclaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			return baseJobComInvoiceLine;
		}
	}

	public abstract class JobComInvoiceLineTest<T> : EU.Business.Declaration.Testing.JobComInvoiceLineTest<T>
		where T : JobComInvoiceLine
	{
		public new void TestDefaultDataGroupingCode()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Constants.CountryCodes.Argentina;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			AssertEquals(Constants.CountryCodes.UnitedKingdom, invoiceLine1.GetDefaultDataGroupingCode());
			AssertEquals(Constants.CountryCodes.UnitedKingdom, invoiceLine1.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));//test
			AssertEquals(Constants.CountryCodes.UnitedKingdom, invoiceLine1.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals(Constants.CountryCodes.UnitedKingdom, invoiceLine.GetDefaultDataGroupingCode());
			AssertEquals(Constants.CountryCodes.UnitedKingdom, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
			AssertEquals(Constants.CountryCodes.UnitedKingdom, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));

			declaration.JE_GB = branch.PK;
			AssertEquals(Constants.CountryCodes.Argentina, invoiceLine.GetDefaultDataGroupingCode());
			AssertEquals(Constants.CountryCodes.Argentina, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
			AssertEquals(Constants.CountryCodes.Argentina, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
		}

		public void TestIsNorthernIrelandDomestic()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = AdditonalInfoCodes.NIAID;
			Assert(!invoiceLine.IsNorthernIrelandDomestic);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIDOM;
			Assert(invoiceLine.IsNorthernIrelandDomestic);
		}

		public void TestIsNorthernIrelandImportFromRow()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = AdditonalInfoCodes.NIAID;
			Assert(!invoiceLine.IsNorthernIrelandImportFromRow);
			Assert(!invoiceLine.IsNorthernIrelandImport);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIIMP;
			Assert(invoiceLine.IsNorthernIrelandImportFromRow);
			Assert(invoiceLine.IsNorthernIrelandImport);
		}

		public void TestIsNorthernIrelandDeRiskStatement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = AdditonalInfoCodes.NIAID;
			Assert(!invoiceLine.IsNorthernIrelandDeRiskStatement);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIIMP;
			Assert(invoiceLine.IsNorthernIrelandDeRiskStatement);

			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIDOM;
			Assert(invoiceLine.IsNorthernIrelandDeRiskStatement);
		}

		public void TestIsNorthernIrelandRemainOrQuotaDeRiskStatement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = AdditonalInfoCodes.NIAID;
			Assert(!invoiceLine.IsNorthernIrelandRemainOrQuotaDeRiskStatement);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIREM;
			Assert(invoiceLine.IsNorthernIrelandRemainOrQuotaDeRiskStatement);

			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIQUO;
			Assert(invoiceLine.IsNorthernIrelandRemainOrQuotaDeRiskStatement);

			var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo3.CSI_Code = AdditonalInfoCodes.NIQUO;
			Assert(!invoiceLine.IsNorthernIrelandRemainOrQuotaDeRiskStatement);
		}

		public void TestIsAtRisk()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = AdditonalInfoCodes.NIAID;
			Assert(invoiceLine.IsAtRisk);

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIREM;
			Assert(!invoiceLine.IsAtRisk);
		}

		public void TestIsEuTariffToBeUsedForNorthernIreland()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = AdditonalInfoCodes.NIAID;
			Assert(!invoiceLine.IsEuTariffToBeUsedForNorthernIreland);
			AssertEquals(Constants.CountryCodes.UnitedKingdom, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIDOM;
			Assert(invoiceLine.IsEuTariffToBeUsedForNorthernIreland);
			AssertEquals(Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));

			var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo3.CSI_Code = AdditonalInfoCodes.NIREM;
			Assert(invoiceLine.IsEuTariffToBeUsedForNorthernIreland);
			AssertEquals(Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));

			declaration.JE_NorthernIrelandMode = NIModeList.Codes.MovementFromGreatBritainToNi;
			Assert(invoiceLine.IsEuTariffToBeUsedForNorthernIreland);
			AssertEquals(Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));

			additionalInfo2.CSI_Code = AdditonalInfoCodes.NIIMP;
			Assert(!invoiceLine.IsEuTariffToBeUsedForNorthernIreland);
			AssertEquals(Constants.CountryCodes.UnitedKingdom, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
		}

		public void TestDefaultCustomsWeightUnitIsKGMIsInList()
		{
			string countryCodeUK = Constants.CountryCodes.UnitedKingdom;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeUK))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(countryCodeUK, "United Kingdom");
				helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Unit of Quantity");
				helper.CreateCusCodeList(countryCodeUK, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "Kilogram", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("Default should be KGM", "KGM", invoiceLine.JI_CustomsUnitQty);
					invoiceLine.Validation.ValidateJI_CustomsUnitQty();
					AssertEquals("Default KGM should produce no list errors", 0, invoiceLine.JI_CustomsUnitQtyInfo.Notifications.Count());
				});
			}
		}

		public virtual void TestCustomsUnitDefaultingStrategy()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = jobDeclaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			invoiceLine.JI_JZ = invoice.PK;
			var strategy = invoiceLine.GetCustomsUnitDefaultingStrategyExposed();
			AssertType<UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>>(strategy);
			AssertNotNull(((UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>)strategy).IsConvertibleFrom);
		}

		public void TestRemoveNorthernIrelandAddInfoDoesNotLeaveItOrphaned()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = "IMP";
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var ai = invoiceLine.AddAdditionalInfoByCodeIfNotExists("POOPY");
			Factory.Save();
			Assert(ai.IsInDatabase);
			invoiceLine.RemoveAdditionalInfoByCode("POOPY");
			Factory.Save();
			Assert(ai.IsDeleted);
		}

		public void TestZG_MethodOfPayment_Defaulting()
		{
			var dec = GetJobDeclarationForTesting();
			dec.JE_MessageType = "IMP";
			dec.JE_PaymentMethod = "A";
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals(ZString.Empty, invoiceLine.ZG_MethodOfPayment);

			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("E", invoiceLine.ZG_MethodOfPayment);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals("E", invoiceLine2.ZG_MethodOfPayment);
		}

		public void TestCustomsUnitDefaultingStrategy_ApplicationExtender()
		{
			SetupCustomsUnitDefaultingStrategyTestData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9999.00.10 00";
			AssertDefaults();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertDefaults();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			AssertDefaults();

			void AssertDefaults()
			{
				CombineAssertions(() =>
				{
					AssertEquals("should default UQ1 from ZZ1_ZZ8_UQ1", "KGX", invoiceLine.JI_CustomsUnitQty);
					AssertEquals("should default UQ2 from ZZ1_ZZ8_UQ2", "LTX", invoiceLine.JI_CustomsSecondUnitQty);
					AssertEquals("should default UQ3 to blank", "", invoiceLine.JI_CustomsThirdUnitQty);
				});
			}
		}

		public void TestCustomsUnitDefaultingStrategy_AttachingInvoiceLineToInvoiceHeader()
		{
			SetupCustomsUnitDefaultingStrategyTestData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			AssertType<UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>>(invoiceLine.GetCustomsUnitDefaultingStrategyExposed());

			invoiceLine.JI_Tariff = "9999.00.10 00";
			AssertEquals("Unattached invoice line does not default UQ1", "KGM", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("Unattached invoice line does not default UQ2", "", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("Unattached invoice line does not default UQ3", "", invoiceLine.JI_CustomsThirdUnitQty);

			invoiceLine.JI_JZ = invoice.PK;
			AssertType<UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>>(invoiceLine.GetCustomsUnitDefaultingStrategyExposed());

			declaration.InvoiceLines.Add(invoiceLine);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertType<UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>>(invoiceLine.GetCustomsUnitDefaultingStrategyExposed());
			AssertEquals("CDS should default UQ1 from ZZ1_ZZ8_UQ1", "KGX", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("CDS should default UQ2 from ZZ1_ZZ8_UQ2", "LTX", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("CDS should default UQ3 to blank", "", invoiceLine.JI_CustomsThirdUnitQty);
		}

		public void TestCustomsUnitDefaultingStrategy_ShouldFillDefaultUOMsFromAllApplicableRates_WhenAnySupplementaryCodeStartsWithX()
		{
			var (tariff, _, _, _, _, _, rate5, rate6) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			invoiceLineMock.Protected().Setup<ZString>("DefaultDataGroupingForTaxOrFee").Returns(GlbCompany.CurrentCompany.Country.Code);
			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.AllApplicableRates).Returns(new[] { rate5, rate6 });
			var invoiceLine = invoiceLineMock.Object;

			invoiceLine.JI_SupplementaryCode1 = "X123";
			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			AssertCollectionContains("Precondition", invoiceLine.SupplementaryCodes, sc => sc.CY_Code.StartsWith("X"));
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CU2", invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "UM4", invoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "UM5", invoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestCustomsUnitDefaultingStrategy_ShouldFillDefaultUOMsFromUniversalDutyRate_WhenNoSupplementaryCodeStartsWithX()
		{
			var (tariff, rate, _, _, _, _, _, _) = UniversalRateCustomsUnitDefaultingStrategyTestingHelper.SetupTariffData();

			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			invoiceLineMock.Protected().Setup<ZString>("DefaultDataGroupingForTaxOrFee").Returns(GlbCompany.CurrentCompany.Country.Code);
			invoiceLineMock.Setup(x => x.UniversalTariff).Returns(tariff);
			invoiceLineMock.Setup(x => x.UniversalDutyRate).Returns(rate);
			var invoiceLine = invoiceLineMock.Object;

			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;

			AssertCollectionNotContains("Precondition", invoiceLine.SupplementaryCodes, sc => sc.CY_Code.StartsWith("X"));
			CombineAssertions(() =>
			{
				AssertEquals("First UQ", "CU1", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ", "CU2", invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("Third UQ", "UM1", invoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("Fourth UQ", "UM2", invoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		void SetupCustomsUnitDefaultingStrategyTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(
				Constants.CountryCodes.UnitedKingdom,
				UniversalReferenceConstants.CusTariffTypes.ImportTariff
			);
			var cdsTariffType = helper.CreateNewOrGetExistingTariffType(
				DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
				UniversalReferenceConstants.CusTariffTypes.ImportTariff
			);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(
				Constants.CountryCodes.UnitedKingdom,
				tariffType.PK,
				"9999001000",
				ZDateTime.Now.AddYears(-1),
				ZDateTime.Now.AddYears(1)
			);
			var cdsTariff = helper.LoadOrCreateNewTariff(
				DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
				cdsTariffType.PK,
				"9999001000",
				ZDateTime.Now.AddYears(-1),
				ZDateTime.Now.AddYears(1)
			);
			helper.CreateTariffUOM(tariff, "CU1", "KGX");
			helper.CreateTariffUOM(tariff, "CU2", "LTX");
			helper.CreateTariffUOM(cdsTariff, "CU1", "KGX");
			helper.CreateTariffUOM(cdsTariff, "CU2", "LTX");
		}

		public void TestApportionedCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertType<JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>>(invoiceLine.ApportionedCharges);
		}

		public void TestJI_FormattedProcedure()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			invoiceLine.JI_FormattedProcedure = "12345678";
			AssertEquals("1234567", invoiceLine.JI_Procedure);
			AssertEquals("12 34 567", invoiceLine.JI_FormattedProcedure);

			invoiceLine.JI_FormattedProcedure = "123 4567";
			AssertEquals("1234567", invoiceLine.JI_Procedure);
			AssertEquals("12 34 567", invoiceLine.JI_FormattedProcedure);
		}

		public void TestUseUniversalTariff()
		{
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			Assert(invoiceLine.UseUniversalTariffExposed);
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLineForTest)invoice.InvoiceLines.AddNew(typeof(JobComInvoiceLineForTest));
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			Assert(invoiceLine.UseUniversalTariffExposed);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			Assert(invoiceLine.UseUniversalTariffExposed);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals("ADD, DED", chargeTypeList1.CodesAsString);
		}

		public void TestGetCusProcedureReturnsCorrectCPCForCDS()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "11", "", "   ", "One", "EXP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure("CDS", "A", "22", "", "   ", "Two", "EXP", group: "IFD");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			AssertNull(invoiceLine.CusProcedure);
			invoiceLine.JI_Procedure = "11";
			AssertEquals(procedure1, invoiceLine.CusProcedure);

			invoiceLine.JI_Procedure = "22";
			AssertNull(invoiceLine.CusProcedure);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals(procedure2, invoiceLine.CusProcedure);
		}

		public void TestMaxNumberOfAdditionalProcedureCode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.MaxNumberOfAdditionalProcedureCode);
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals(98, invoiceLine.MaxNumberOfAdditionalProcedureCode);
		}

		public void TestIsAdditionalProcedureCodesApplicable()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			var invoice = dec.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			AssertEquals(false, line.IsAdditionalProcedureCodesApplicable);
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals(true, line.IsAdditionalProcedureCodesApplicable);
		}

		public void TestSupportingDocumentsCorrectType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<SupportingDocumentCollection>(line.SupportingDocuments);
		}

		public void TestAdditionalInfoCorrectType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<AdditionalInfoCollection>(line.AdditionalInfos);
		}

		public void TestPreviousDocumentsCorrectType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<PreviousDocumentCollection>(line.PreviousDocuments);
		}

		public void TestFiscalReferences()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<CusFiscalReferenceCollection<CusFiscalReference>>(line.FiscalReferences);
		}

		public void TestSetTariffEtcDataFromProductsPivot()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "TestTEST";
			var orgRelation = product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.IMP;
			pivot.CI_OH = orgRelation.OU_OH;
			pivot.CI_TariffNum = "10101010";
			var sdPart1 = pivot.SupportingDocuments.AddNew();
			var sdPart2 = pivot.SupportingDocuments.AddNew();
			var pdPart1 = pivot.PreviousDocuments.AddNew();
			var pdPart2 = pivot.PreviousDocuments.AddNew();
			var aiPart1 = pivot.AdditionalInfos.AddNew();
			var aiPart2 = pivot.AdditionalInfos.AddNew();
			var taxPartA00 = pivot.Taxes.AddNew();
			var taxPartB00 = pivot.Taxes.AddNew();
			sdPart1.CSI_Code = "SD01";
			sdPart1.CSI_ReferenceNumber = "FromPart";
			sdPart2.CSI_Code = "SD02";
			pdPart1.CSI_Code = "PD01";
			pdPart2.CSI_Code = "PD02";
			aiPart1.CSI_Code = "AI01";
			aiPart2.CSI_Code = "AI02";
			taxPartA00.Data.G4_Type = "A00";
			taxPartA00.Data.G4_RateDuty = "NEW";
			taxPartB00.Data.G4_Type = "B00";
			pivot.CI_TariffNum = "1234";
			pivot.CI_CPC = "4000001";
			pivot.CI_ZZF_NKTaxType = "RED";
			pivot.PreferenceCode = "400";
			pivot.CI_RN_NKCountryOfOrigin = "US";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var sdExisting1 = invoiceLine.SupportingDocuments.AddNew();
			var pdExisting1 = invoiceLine.PreviousDocuments.AddNew();
			var aiExisting1 = invoiceLine.AdditionalInfos.AddNew();
			var taxExistingA00 = invoiceLine.Taxes.AddNew();
			var taxExistingD10 = invoiceLine.Taxes.AddNew();
			sdExisting1.CSI_Code = "SD01";
			pdExisting1.CSI_Code = "PD01";
			aiExisting1.CSI_Code = "AI01";
			taxExistingA00.Data.G4_Type = "A00";
			taxExistingA00.Data.G4_RateDuty = "OLD";
			taxExistingD10.Data.G4_Type = "D10";
			taxExistingD10.Data.G4_RateDuty = "OLD";

			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("CPC set from product's pivot", "4000001", invoiceLine.JI_Procedure);
		}

		public void TestTariffQuantitiesSetFromProductsPivot_ExportDeclarationBothProduct() =>
			AssertTariffQuantitiesSetFromProductsPivot(
				"2nd, 4th and 5th quantities should not be set from Product Pivot when Message Type and Classification type do not match or Classification is both",
				SharedJobMessageTypeList.Codes.Export, ClassificationType.Both, 12.34m, 23.45m, 67.89m);

		public void TestTariffQuantitiesSetFromProductsPivot_ExportDeclarationImportProduct() =>
			AssertTariffQuantitiesSetFromProductsPivot(
				"2nd, 4th and 5th quantities should not be set from Product Pivot when Message Type and Classification type do not match",
				SharedJobMessageTypeList.Codes.Export, ClassificationType.IMP, 0m, 0m, 0m);

		public void TestTariffQuantitiesSetFromProductsPivot_ExportDeclarationExportProduct() =>
			AssertTariffQuantitiesSetFromProductsPivot(
				"2nd, 4th and 5th quantities should be set from Product Pivot when Message Type and Classification type match",
				SharedJobMessageTypeList.Codes.Export, ClassificationType.EXP, 12.34m, 23.45m, 67.89m);

		public void TestTariffQuantitiesSetFromProductsPivot_ImportDeclarationBothProduct() =>
			AssertTariffQuantitiesSetFromProductsPivot(
				"2nd, 4th and 5th quantities should be set from Product Pivot when Message Type and Classification type do not match or Classification is both",
				SharedJobMessageTypeList.Codes.Import, ClassificationType.Both, 12.34m, 23.45m, 67.89m);

		public void TestTariffQuantitiesSetFromProductsPivot_ImportDeclarationImportProduct() =>
			AssertTariffQuantitiesSetFromProductsPivot(
				"2nd, 4th and 5th quantities should be set from Product Pivot when Message Type and Classification type match",
				SharedJobMessageTypeList.Codes.Import, ClassificationType.IMP, 12.34m, 23.45m, 67.89m);

		public void TestTariffQuantitiesSetFromProductsPivot_ImportDeclarationExportProduct() =>
			AssertTariffQuantitiesSetFromProductsPivot(
				"2nd, 4th and 5th quantities should not be set from Product Pivot when Message Type and Classification type do not match",
				SharedJobMessageTypeList.Codes.Import, ClassificationType.EXP, 0, 0m, 0m);

		public void TestGoodsCategorySetFromProductsPivot_ImportDeclarationFromGreatBritainToNi() =>
			AssertGoodsCategorySetFromProductsPivot(
				"Goods Category should be set for G2N import", SharedJobMessageTypeList.Codes.Import, NIModeList.Codes.MovementFromGreatBritainToNi, GoodsCategoryList.Codes.Category1);

		public void TestGoodsCategorySetFromProductsPivot_ImportDeclarationFromNiToGreatBritain() =>
			AssertGoodsCategorySetFromProductsPivot(
				"Goods Category should not be set for N2G import", SharedJobMessageTypeList.Codes.Import, NIModeList.Codes.MovementFromNiToGreatBritain, ZString.Empty);

		public void TestGoodsCategorySetFromProductsPivot_ExportDeclarationFromGreatBritainToNi() =>
			AssertGoodsCategorySetFromProductsPivot(
				"Goods Category should not be set for G2N export", SharedJobMessageTypeList.Codes.Export, NIModeList.Codes.MovementFromGreatBritainToNi, ZString.Empty);

		public void TestGoodsCategorySetFromProductsPivot_ExportDeclarationFromNiToGreatBritain() =>
			AssertGoodsCategorySetFromProductsPivot(
				"Goods Category should not be set for N2G export", SharedJobMessageTypeList.Codes.Export, NIModeList.Codes.MovementFromNiToGreatBritain, ZString.Empty);

		public void TestTaxes()
		{
			var dec = Factory.New<JobDeclaration>();
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<JobComInvoiceLineTaxCollection>(invLine.Taxes);
		}

		public override void TestProcedureLookupsAndValidation()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(Constants.CountryCodes.UnitedKingdom, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure(Constants.CountryCodes.UnitedKingdom, "A", "22", "22", "222", "Two", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure(Constants.CountryCodes.UnitedKingdom, "B", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure(Constants.CountryCodes.UnitedKingdom, "A", "44", "44", "444", "Four", "EXP", group: "EFD");

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_MessageType = "IMP";
			AssertEquals("IFD", dec.JE_DeclarationType);
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals("JI-CEI automatically set upon creation", dec.CusEntryInstruction.PK, invLine.JI_CEI);
			AssertEquals(null, invLine.CusProcedure);

			// NB:
			var cpcs = invLine.Lookups.Procedures;
			Assert("GB lookups has all CPCs filters by shipmentType=DJC and declaraitonType IFD", cpcs.ContainsCode("1111111"));
			Assert("GB lookups has all CPCs filters by shipmentType=DJC and declaraitonType IFD", cpcs.ContainsCode("2222222"));
			invLine.JI_Procedure = "1111111";
			AssertNoMessageErrorContaining(invLine.JI_ProcedureInfo, "list");
			AssertEquals(procedure1, invLine.CusProcedure);
			invLine.JI_Procedure = "2222222";
			AssertNoMessageErrorContaining(invLine.JI_ProcedureInfo, "list");
			AssertEquals(procedure2, invLine.CusProcedure);
			invLine.JI_Procedure = "3333333";
			AssertHasMessageErrorContaining(invLine.JI_ProcedureInfo, "list"); // NB different to base
			AssertEquals(procedure3, invLine.CusProcedure);
			invLine.JI_Procedure = "xxxxxxx";
			AssertHasMessageErrorContaining(invLine.JI_ProcedureInfo, "list");
			AssertEquals(null, invLine.CusProcedure);

			var invLineTwo = dec.Invoices.AddNew().InvoiceLines.AddNew();
			AssertNoMessageErrorContaining(invLine.JI_ProcedureInfo, "series");
			invLine.JI_Procedure = "1111111";
			invLineTwo.JI_Procedure = "3333333";
			AssertHasMessageErrorContaining(invLineTwo.JI_ProcedureInfo, "series");
		}

		public void TestInvoiceLineType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType(typeof(JobComInvoiceLine), line);
		}

		public void TestInvoiceLineVatTaxLineCreated()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = helper.CreateTaxOrFee("654", 0.00, Constants.CountryCodes.UnitedKingdom);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;

			var line1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			line1.JI_ZZF_NKTaxType = taxOrFee.ZZF_Code;

			AssertEquals("CHIEF InvoiceLine should have one tax row", 1, line1.Taxes.Count);
			if (line1.Taxes.Count > 0)
			{
				AssertEquals("E", line1.Taxes[0].G4_RateDuty);
			}

			var line2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			line2.JI_ZZF_NKTaxType = "ABC";

			AssertEquals("CHIEF InvoiceLine should not have any tax rows", 0, line2.Taxes.Count);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var line3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			line3.JI_ZZF_NKTaxType = taxOrFee.ZZF_Code;

			AssertEquals("CDS InvoiceLine should have no tax rows", 0, line3.Taxes.Count);
		}

		public void IsValidToApportionTo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Assert(!((IChargeApportionee)invoiceLine).IsValidToApportionTo);

			invoiceLine.JI_Procedure = "4000E01";
			Assert(!((IChargeApportionee)invoiceLine).IsValidToApportionTo);

			invoiceLine.JI_Procedure = "4000E02";
			Assert(!((IChargeApportionee)invoiceLine).IsValidToApportionTo);

			invoiceLine.JI_Procedure = "4000E03";
			Assert(((IChargeApportionee)invoiceLine).IsValidToApportionTo);

			invoiceLine.JI_Procedure = string.Empty;
			var additionalCode = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalCode.CY_Code = "4000E01";
			Assert(!((IChargeApportionee)invoiceLine).IsValidToApportionTo);

			additionalCode.CY_Code = "4000E02";
			Assert(!((IChargeApportionee)invoiceLine).IsValidToApportionTo);

			additionalCode.CY_Code = "4000E03";
			Assert(!((IChargeApportionee)invoiceLine).IsValidToApportionTo);
		}

		public void TestSetCpcBringsInGuarantees()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.SetTemporaryValue(Guid.Empty, declaration.Branch.EntityPK.ToGuid(), Guid.Empty, false))
			{
				// Deferring all to one DAN
				declaration.JE_PaymentMethod = "A";
				declaration.JE_DefermentAccountNumber = "1111111";
				var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				line.JI_Procedure = "5100001";
				AssertOneGuarantee("1111111", line);
				AssertTaxes(line);

				// Deferring to two DANs
				declaration.ZG_VATDeferType = "A";
				declaration.ZG_VATDeferNumber = "2222222";
				line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				line.JI_Procedure = "5100001";
				AssertTwoGuarantees("1111111", "2222222", line);
				AssertTaxes(line);

				// Already have an AI statement for another guarantee - do nothing
				line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				var existingAiForAnotherGuarantee = line.AdditionalInfos.AddNew();
				existingAiForAnotherGuarantee.CSI_Code = GRNTR;
				existingAiForAnotherGuarantee.CSI_Description = "3333333";
				line.JI_Procedure = "5100001";
				AssertOneGuarantee("3333333", line);
				AssertEquals(0, line.Taxes.Count);

				// OK to reuse an existing AI statement GRNTR is the description is not set
				line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				var existingAiButBlank = line.AdditionalInfos.AddNew();
				existingAiButBlank.CSI_Code = GRNTR;
				line.JI_Procedure = "5100001";
				AssertTwoGuarantees("1111111", "2222222", line);
				AssertTaxes(line);

				// Don't do anything for irrelevant CPCs
				line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				line.JI_Procedure = "9999999";
				AssertEquals(0, line.AdditionalInfos.Count);
				AssertEquals(0, line.Taxes.Count);
			}
		}

		static void AssertOneGuarantee(string expectedDan, JobComInvoiceLine line)
		{
			var guars = line.AdditionalInfos.OfType<AdditionalInfo>().Where(g => g.CSI_Code == GRNTR).ToArray();
			AssertEquals(1, guars.Length);
			AssertEquals(expectedDan, guars.First().CSI_Description);
		}

		static void AssertTwoGuarantees(string expectedFirstDan, string expectedVatDan, JobComInvoiceLine line)
		{
			var guars = (from AdditionalInfo g
						 in line.AdditionalInfos.OfType<AdditionalInfo>()
						 where g.CSI_Code == GRNTR
						 orderby g.CSI_Description
						 select g).ToArray();
			AssertEquals(2, guars.Length);
			AssertEquals(expectedFirstDan, guars.First().CSI_Description);
			AssertEquals(expectedVatDan, guars.Last().CSI_Description);
		}

		static void AssertTaxes(JobComInvoiceLine line)
		{
			var taxes = line.Taxes.OfType<JobComInvoiceLineTax>().ToArray();
			var a00 = taxes.First(t => t.Data.G4_Type == "A00").Data;
			AssertEquals("Q", a00.G4_MethodOfPayment);
			AssertEquals("F", a00.G4_RateDuty);
			var b00 = taxes.First(t => t.Data.G4_Type == "B00").Data;
			AssertEquals("Q", b00.G4_MethodOfPayment);
			AssertEquals("", b00.G4_RateDuty);
		}

		const string GRNTR = "GRNTR";

		public override void TestJI_DescriptionDefaultingFromTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Constants.CountryCodes.UnitedKingdom, "EXP").PK;
			var cdsTariffTypePK = helper.CreateNewOrGetExistingTariffType("CDS", "EXP").PK;
			Factory.Save();
			_ = helper.CreateTariff(Constants.CountryCodes.UnitedKingdom, tariffTypePK, "99990010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "BEER2");
			_ = helper.CreateTariff("CDS", cdsTariffTypePK, "99990010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "BEER2");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			var invoice = dec.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "99990010";
			AssertEquals("BEER2", line.JI_Description);

			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			line.JI_Description = "";
			line.JI_Tariff = "";
			line.JI_Tariff = "99990010";
			AssertEquals("BEER2", line.JI_Description);
		}

		public void TestGetCountryCodeForSupplementaryCodeProvider()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();

			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("When JE_ApplicationCode is CDS, GetCountryCodeForSupplementaryCodeProvide", "GBCDS", invoiceLine.GetCountryCodeForCodeProvider());

			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals("When JE_ApplicationCode is CHF, GetCountryCodeForSupplementaryCodeProvide", "GBCHF", invoiceLine.GetCountryCodeForCodeProvider());

			var invoiceLineWithNoDeclaration = Factory.New<JobComInvoiceLine>();
			AssertEquals("When Invoice Line has no parent Declaration, GetCountryCodeForSupplementaryCodeProvide", "GB", invoiceLineWithNoDeclaration.GetCountryCodeForCodeProvider());
		}

		public void TestGetAddInfoJobComInvoiceLineLookups()
		{
			CombineAssertions(() =>
			{
				var cdsDeclaration = Factory.New<JobDeclaration>();
				cdsDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				var invoiceLineWithCDSDeclaration = cdsDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
				AssertType<AddInfoJobComInvoiceLineLookups>("When JE_ApplicationCode is CDS", invoiceLineWithCDSDeclaration.AddInfoLookups);

				var invoiceLineWithNoDeclaration = Factory.New<JobComInvoiceLine>();
				AssertType<AddInfoJobComInvoiceLineLookups>("When Invoice Line has no parent Declaration", invoiceLineWithNoDeclaration.AddInfoLookups);
			});
		}

		JobComInvoiceLine GetInvoiceLine(ZString appCode, ZString msgType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = appCode;
			declaration.JE_MessageType = msgType;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			return invoiceLine;
		}

		public void TestSetDefaultTaxOrFeeCode_NoRelatedTaxOrFeeCode()
		{
			var (tariff, expectedTaxType) = SetupTariffsAndFeeDetails(0);
			AssertDefaultTaxOrFeeCode(tariff, expectedTaxType);
		}

		public void TestSetDefaultTaxOrFeeCode_OneRelatedTaxOrFeeCode()
		{
			var (tariff, expectedTaxType) = SetupTariffsAndFeeDetails(1);
			AssertDefaultTaxOrFeeCode(tariff, expectedTaxType);
		}

		public void TestSetDefaultTaxOrFeeCode_MoreThanOneRelatedTaxOrFeeCode()
		{
			var (tariff, expectedTaxType) = SetupTariffsAndFeeDetails(2);
			AssertDefaultTaxOrFeeCode(tariff, expectedTaxType);
		}

		void AssertDefaultTaxOrFeeCode(ZString tariff, ZString expectedTaxType)
		{
			var cdsImp = GetInvoiceLine(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "IMP");
			var cdsExp = GetInvoiceLine(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "EXP");
			var chfImp = GetInvoiceLine(DeclarationApplicationCodeList.Codes.CHIEF, "IMP");
			var chfExp = GetInvoiceLine(DeclarationApplicationCodeList.Codes.CHIEF, "EXP");

			CombineAssertions(() =>
			{
				AssertEquals("CDS Imp Use Universal", true, cdsImp.UseUniversalTariff);
				AssertEquals("CDS Exp Use Universal", true, cdsExp.UseUniversalTariff);
				AssertEquals("CHF Imp Use Universal", true, chfImp.UseUniversalTariff);
				AssertEquals("CHF Exp Use Universal", true, chfExp.UseUniversalTariff);

				cdsImp.JI_Tariff = tariff;
				AssertEquals("CDS IMP", expectedTaxType, cdsImp.JI_ZZF_NKTaxType);

				cdsExp.JI_Tariff = tariff;
				AssertEquals("CDS EXP", ZString.Empty, cdsExp.JI_ZZF_NKTaxType);

				chfImp.JI_Tariff = tariff;
				AssertEquals("CHF IMP", expectedTaxType, chfImp.JI_ZZF_NKTaxType);

				chfExp.JI_Tariff = tariff;
				AssertEquals("CHF EXP", ZString.Empty, chfExp.JI_ZZF_NKTaxType);
			});
		}

		(ZString tariffCode, ZString expectedTaxType) SetupTariffsAndFeeDetails(int vatApplicablityNumber)
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var countryCode = GlbCompany.CurrentCompany.Country.Code;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var importTariffTypePK = helper.CreateNewOrGetExistingTariffType(countryCode, UniversalReferenceConstants.CusTariffTypes.ImportTariff).PK;
			var exportTariffTypePK = helper.CreateNewOrGetExistingTariffType(countryCode, UniversalReferenceConstants.CusTariffTypes.ExportTariff).PK;

			var gbGrp = helper.CreateNewOrGetExistingDataGrouping(countryCode);
			helper.CreateNewOrGetExistingDataGrouping(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, parent: gbGrp);
			Factory.Save();
			var taxOrFee1 = helper.CreateTaxOrFee("ZZ1", 0.02m, countryCode);
			taxOrFee1.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			var taxOrFee2 = helper.CreateTaxOrFee("ZZ2", 0.01m, countryCode);
			taxOrFee2.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			var taxOrFee3 = helper.CreateTaxOrFee("ZZ3", 9999m, countryCode);
			taxOrFee3.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			Factory.Save();

			var tariffCode = "99999999";
			var tariffImp = helper.CreateTariff(countryCode, importTariffTypePK, tariffCode, startDate, endDate, taxOrFeeCode: "ZZ4");
			var tariffExp = helper.CreateTariff(countryCode, exportTariffTypePK, tariffCode, startDate, endDate, taxOrFeeCode: "ZZ4");
			var expectedTaxType = "ZZ4";

			if (vatApplicablityNumber > 0)
			{
				helper.CreateNewOrGetExistingVATApplicability(tariffImp, countryCode, "ZZ1", "ZZ1", startDate, endDate);
				helper.CreateNewOrGetExistingVATApplicability(tariffExp, countryCode, "ZZ1", "ZZ1", startDate, endDate);
				expectedTaxType = "ZZ1";
			}

			if (vatApplicablityNumber > 1)
			{
				helper.CreateNewOrGetExistingVATApplicability(tariffImp, countryCode, "ZZ2", ZString.Empty, startDate, endDate);
				helper.CreateNewOrGetExistingVATApplicability(tariffExp, countryCode, "ZZ2", ZString.Empty, startDate, endDate);
				expectedTaxType = "ZZ3";
			}

			return (tariffCode, expectedTaxType);
		}

		protected override Type GetExpectedPartType()
		{
			return typeof(OrgSupplierPart);
		}

		protected override Type GetExpectedCusEntryLineType()
		{
			return typeof(CusEntryLine);
		}

		protected override string GetLocalPortCode()
		{
			return "GBLON";
		}

		public void TestJI_SupplementaryCodeIsSetWhenJI_ZZF_NKTaxTypeChanges()
		{
			JI_SupplementaryCodeIsSetWhenJI_ZZF_NKTaxTypeChangesTest("CDS", true);
		}

		public void JI_SupplementaryCodeIsSetWhenJI_ZZF_NKTaxTypeChangesTest(ZString applicationCode, ZBool setSupplementaryCode)
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var dataGrouping = applicationCode == "CDS" ? "CDS" : "GB";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("EUN").PK;

			var gbGrp = helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, parent: gbGrp);
			Factory.Save();

			var expTariffTypePK = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP").PK;
			Factory.Save();
			helper.CreateTaxOrFee("IT1", 0.02m, dataGrouping);
			helper.CreateTaxOrFee("IT2", 0.01m, dataGrouping);
			helper.CreateTaxOrFee("IT3", 9999m, dataGrouping);
			Factory.Save();

			var tariff = helper.CreateTariff(dataGrouping, expTariffTypePK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			helper.CreateNewOrGetExistingVATApplicability(tariff, dataGrouping, "IT1", additionalCode: "Add1", startDate: startDate, endDate: endDate);
			helper.CreateNewOrGetExistingVATApplicability(tariff, dataGrouping, "IT2", additionalCode: "Add2", startDate: startDate, endDate: endDate);

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = applicationCode;
			declaration.JE_MessageType = "EXP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99999999";
			invoiceLine.JI_ZZF_NKTaxType = "IT1";
			AssertEquals($"SupplementaryCode1 should be {(setSupplementaryCode ? "Add1" : string.Empty)} for {applicationCode} declarations", setSupplementaryCode ? "Add1" : string.Empty, invoiceLine.JI_SupplementaryCode1);

			invoiceLine.JI_ZZF_NKTaxType = "IT2";
			AssertEquals($"SupplementaryCode1 should be {(setSupplementaryCode ? "Add2" : string.Empty)} for {applicationCode} declarations", setSupplementaryCode ? "Add2" : string.Empty, invoiceLine.JI_SupplementaryCode1);
		}

		public void TestJI_RN_NKCountryOfExport_Default()
		{
			var dec = GetJobDeclarationForTesting();
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine.JI_RN_NKCountryOfExport);
		}

		public override void TestWipeNKTaxType()
		{
			TestWipeNKTaxType("CDS");
			TestWipeNKTaxType("CHF");
		}

		void TestWipeNKTaxType(ZString applicationCode)
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = applicationCode == "CDS" ? "CDS" : "GB";
			var procedure1 = helper.CreateRefCusProcedure(dataGrouping, "A", "11", "11", "111", "One", "IMP", group: "IFD");

			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = applicationCode;
			var cei = dec.CustomsEntryInstructions.AddNew();
			dec.JE_MessageType = "IMP";
			cei.CEI_Style = "IFD";

			var header = dec.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.JI_ZZF_NKTaxType = "605";
			line.JI_Procedure = "1111111";
			line.JI_CEI = cei.PK;
			Assert(line.ShouldWipeNKTaxType);
			AssertEquals(ZString.Empty, line.JI_ZZF_NKTaxType);

			line.JI_ZZF_NKTaxType = "605";
			line.JI_Procedure = "1111222";
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals("605", line.JI_ZZF_NKTaxType);

			var additionalProcedure = line.AdditionalProcedureCodes.AddNew();
			additionalProcedure.CY_Code = "1111111";
			AssertNotNull(line.AdditionalProcedureWithCalculateVATIsFalse);
			Assert(line.ShouldWipeNKTaxType);
			AssertEquals(ZString.Empty, line.JI_ZZF_NKTaxType);

			line.JI_ZZF_NKTaxType = "605";
			additionalProcedure.CY_Code = "1111222";
			AssertNull(line.AdditionalProcedureWithCalculateVATIsFalse);
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals("605", line.JI_ZZF_NKTaxType);

			procedure1.ZZ6_CalculateVAT = true;
			Factory.Save();
			line.JI_Procedure = "1111111";
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals("605", line.JI_ZZF_NKTaxType);
		}

		void AssertTariffQuantitiesSetFromProductsPivot(string assertionMessage, string messageType, string childType, ZDecimal expected2ndQty, ZDecimal expected4thQty, ZDecimal expected5thQty)
		{
			var product = (Customs.Business.OrgSupplierPart)OrgSupplierPart.New(Factory);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "~~~";
			product.OP_PartNum = "DEF";
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_ChildType = childType;
			pivot.CI_SecondQty = 12.34m;
			pivot.CI_FourthQty = 23.45m;
			pivot.CI_FifthQty = 67.89m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = messageType;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CC = ZGuid.Invalid;

			invoiceLine.JI_PartNo = product.OP_PartNum;

			CombineAssertions(assertionMessage, () =>
			{
				AssertEquals(expected2ndQty, invoiceLine.JI_CustomsSecondQuantity);
				AssertEquals(expected4thQty, invoiceLine.JI_CustomsFourthQuantity);
				AssertEquals(expected5thQty, invoiceLine.JI_CustomsFifthQuantity);
			});
		}

		void AssertGoodsCategorySetFromProductsPivot(string assertionMessage, string messageType, string northernIrelandMode, ZString expectedGoodsCategory)
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "~~~";

			var product = (Customs.Business.OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "DEF";
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_GoodsCategory = GoodsCategoryList.Codes.Category1;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = messageType;
			declaration.JE_NorthernIrelandMode = northernIrelandMode;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(assertionMessage, expectedGoodsCategory, invoiceLine.ZG_GoodsCategory);
		}

		public override void TestDutyAmountsAsStringCore()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();
			var chfDeclaration = Factory.New<JobDeclaration>();
			var chfInvoiceLine = chfDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cdsDeclaration = Factory.New<JobDeclaration>();
			cdsDeclaration.JE_ApplicationCode = "CDS";
			var cdsInvoiceLine = cdsDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => { var dutyAmount = chfInvoiceLine.DutyAmountsAsString; });
				AssertNoExceptionThrown(() => { var dutyAmount = cdsInvoiceLine.DutyAmountsAsString; });
				AssertNoExceptionThrown(() => { var dutyAmount = Factory.New<JobComInvoiceLine>().DutyAmountsAsString; });
			});
		}

		public override void TestNationalRateSelectionCriteria()
		{
			var nationalRateSelectionCriteria = InvoiceLine.NationalRateSelectionCriteria.ToArray();

			AssertEquals(1, nationalRateSelectionCriteria.Length);
			AssertType<EUJobComInvoiceLine.RateSelectionCriteriaNoPrimaryPreference<JobComInvoiceLine>>(nationalRateSelectionCriteria[0]);
			AssertEquals(UniversalReferenceConstants.RefCusRateTypes.Excise, nationalRateSelectionCriteria[0].RateType);
			AssertNullOrEmpty(nationalRateSelectionCriteria[0].RateCode);
		}

		public void TestInvoiceHeaderType() => AssertEquals(typeof(JobComInvoiceHeader), Factory.New<JobComInvoiceLineForTest>().InvoiceHeaderTypeExposed);

		protected override string OverseasFreightCode => ChargesProvider.AirFreightCode;

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		protected new JobComInvoiceLine InvoiceLine => base.InvoiceLine;

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceLineChargeCollection<InvoiceLineCharge>);

		protected override Type GetExpectedEntryInstructionType() => typeof(CusEntryInstruction);

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTesting()
		{
			var dec = base.GetJobDeclarationForTesting();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			return dec;
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var dec = base.GetJobDeclaration();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			return dec;
		}

		public class JobComInvoiceLineForTest : JobComInvoiceLine
		{
			public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{ }

			public bool UseUniversalTariffExposed => UseUniversalTariff;
			public Type InvoiceHeaderTypeExposed => base.InvoiceHeaderType;
			public new ZDecimal ComponentPrice => base.ComponentPrice;

			public ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategyExposed() => GetCustomsUnitDefaultingStrategy();
		}
	}
}
