using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class TradeNetDeclarationCreatorTest : TestCaseWithFactory
	{
		[TestDate(2018, 04, 17, 15, 30, 0)]
		public void TestCreateTradeNetDeclaration()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "49030000", new ZDateTime(2011, 11, 26), new ZDateTime(2018, 6, 23));
			helper.CreateTariffUOM(tariff1, Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.NMB);
			Factory.Save();
			var level1DataImport = new Level1DataImport(Factory);
			level1DataImport.FlightNumber = "QF656";
			level1DataImport.DepartureDate = ZDateTime.Today.AddDays(-5);
			level1DataImport.ArrivalDate = ZDateTime.Today;
			level1DataImport.PortOfLoading = "AUSYD";
			level1DataImport.PortOfDischarge = "SGSIN";
			level1DataImport.IsSurplus = true;
			level1DataImport.MasterBill = "08122222222";
			level1DataImport.CoLoadMasterBill = "08144444444";
			var billRecord = new Level1Record();
			billRecord.AddRecordLines(new ZString[] { "US2795AU9639040422              D3824ARFY9JH2000003824ARFY9JH           N 1 19   LBS US          USDNNNN           USDAKE32156QF    09642             USD         USD5100      USDN0 N   NEDI  19APR200419 LBS         USDD1    NNN NN  NN  N USD           USD    T1                      19APR20040000           5100       P/PNRE       19   LBSNNC0000051901QF12            N N 1   N ", "US2795AU9639040422              D3824ARFY9JH2020001Z3824AR6640806327                 19     19     2    N                                                            7709705048E                AU09639  S1AU9639TF.113B2004-04-21                              Y                                                                             AUDNNNNNBI                                  ", "US2795AU9639040422              D3824ARFY9JH300000073706003824AR    BIOTECH CORP                       107 OAKWOOD DRIVE                                                     GLASTONBURY                                            CT060332481US 18606338111                                                                               11644                                         ", "US2795AU9639040422              D3824ARFY9JH400000170520138AU0495926SNOWSILL, SONYA                                             15 LEANDER ST                                                         FALCON                                                   6210     AU 0895343637                  SNOWSILL    SNOWSILL                                                 237           ", "US2795AU9639040422              DA4W82133K3G401000        8AU0596227WENDY PATERSON                     ContactName              GPO BOX 1609                       ABC                                SYDNEY                                                 NS2001     AU 123456789                                                                                        237           ", "US2795AU9639040422              D3824ARFY9JH5000006   EA SHEN MIN EXTRA STRENGTH                                                                                 600       USD11644               US                                   AUTEST PART                                                                                                                                        ", "US2795AU9639040422              D3947808NNSR60000099999999999    11 LBS         USD         USD          USD                             C0000092779UPS6901         NY1ZAT27736792092649                 10     AAY89758UPS                                                                                                                                                               ", });
			var lineTariffs = new Dictionary<_500000Line, TariffView>();
			lineTariffs.Add(billRecord._500000Lines.OfType<_500000Line>().First(), tariff1);
			var declaration = new TradeNetDeclarationCreator(Factory, level1DataImport, billRecord, false, consignor, consignee, 1000m, 100m, "SGD", lineTariffs).CreateTradeNet(new ReadOnlyBusinessObjectFactory());
			CombineAssertions("For Import TradeNet", () =>
			{
				AssertEquals("declaration.JE_OH_Importer", consignee.PK, declaration.JE_OH_Importer);
				AssertEquals("declaration.JE_OH_Supplier", consignor.PK, declaration.JE_OH_Supplier);
				AssertEquals("declaration.JE_MessageType", MessageTypeCodeList.Codes.INP, declaration.JE_MessageType);
				AssertEquals("declaration.JE_MessageSubType", DeclarationTypeCodeList.Codes.SFZ, declaration.JE_MessageSubType);
				AssertEquals("declaration.JE_TransportMode", "AIR", declaration.JE_TransportMode);
				AssertEquals("declaration.JE_VoyageFlightNo", "QF656", declaration.JE_VoyageFlightNo);
				AssertEquals("declaration.JE_ContainerMode", CargoPackingCodeList.Codes.PackingType5, declaration.JE_ContainerMode);
				AssertEquals("declaration.JE_RS_NKServiceLevel", "STD", declaration.JE_RS_NKServiceLevel);
				AssertEquals("declaration.JE_GS_NKCusAgent", GlbStaff.CurrentUser.GS_Code, declaration.JE_GS_NKCusAgent);
				AssertEquals("declaration.JE_MasterBill", "08122222222", declaration.JE_MasterBill);
				AssertEquals("declaration.JE_DateAtOrigin", ZDateTime.Today.AddDays(-5), declaration.JE_DateAtOrigin);
				AssertEquals("declaration.JE_DateAtFinalDestination", ZDateTime.Today, declaration.JE_DateAtFinalDestination);
				AssertEquals("declaration.JE_HouseBill", "3824ARFY9JH", declaration.JE_HouseBill);
				AssertEquals("declaration.JE_TotalWeight", 8.618m, declaration.JE_TotalWeight);
				AssertEquals("declaration.JE_TotalWeightUnit", "KG", declaration.JE_TotalWeightUnit);
				AssertEquals("declaration.JE_TotalVolume", 0m, declaration.JE_TotalVolume);
				AssertEquals("declaration.JE_TotalVolumeUnit", "M3", declaration.JE_TotalVolumeUnit);
				AssertEquals("declaration.JE_RL_NKPortOfLoading", "AUSYD", declaration.JE_RL_NKPortOfLoading);
				AssertEquals("declaration.JE_RL_NKPortOfArrival", "SGSIN", declaration.JE_RL_NKPortOfArrival);
				AssertEquals("declaration.JE_RL_NKOrigin", "USONT", declaration.JE_RL_NKOrigin);
				AssertEquals("declaration.JE_RL_NKFinalDestination", "AUSYD", declaration.JE_RL_NKFinalDestination);
				AssertEquals("declaration.Invoices.Count", 1, declaration.Invoices.Count);
				var invoice = declaration.Invoices[0];
				AssertEquals("invoice.JZ_InvoiceNumber", "3824ARFY9JH", invoice.JZ_InvoiceNumber);
				AssertEquals("invoice.JZ_InvoiceAmount", 1000m, invoice.JZ_InvoiceAmount);
				AssertEquals("invoice.JZ_RX_NKInvoice_Currency", "SGD", invoice.JZ_RX_NKInvoice_Currency);
				AssertEquals("invoice.Charges.Count", 1, invoice.Charges.Count);
				AssertEquals("invoice.Charges[0].J7_ChargeType", "DIS", invoice.Charges[0].J7_ChargeType);
				AssertEquals("invoice.Charges[0].J7_Amount", 100m, invoice.Charges[0].J7_Amount);
				AssertEquals("invoice.JobComInvoiceLines.Count", 1, invoice.JobComInvoiceLines.Count);
				var invoiceLine = invoice.JobComInvoiceLines[0];
				AssertEquals("invoiceLine.JI_PartNo", "TEST PART", invoiceLine.JI_PartNo);
				AssertEquals("invoiceLine.JI_Description", "SHEN MIN EXTRA STRENGTH", invoiceLine.JI_Description);
				AssertEquals("invoiceLine.JI_Tariff", "49030000", invoiceLine.JI_Tariff);
				AssertEquals("invoiceLine.JI_InvoiceQuantity", 6m, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("invoiceLine.JI_InvoiceUQ", "PCE", invoiceLine.JI_InvoiceUQ);
				AssertEquals("invoiceLine.JI_LinePrice", 4.32m, invoiceLine.JI_LinePrice);
				AssertEquals("invoiceLine.JI_CountryOfOrigin", "US", invoiceLine.JI_CountryOfOrigin);
				AssertEquals("invoiceLine.JI_CustomsQuantity", 6m, invoiceLine.JI_CustomsQuantity);
				AssertEquals("invoiceLine.JI_CustomsUnitQty", "NMB", invoiceLine.JI_CustomsUnitQty);
			});
			level1DataImport.PortOfLoading = "SGSIN";
			level1DataImport.PortOfDischarge = "AUSYD";
			level1DataImport.IsRoad = true;
			declaration = new TradeNetDeclarationCreator(Factory, level1DataImport, billRecord, true, consignor, consignee, 1000m, 100m, "SGD", lineTariffs).CreateTradeNet(new ReadOnlyBusinessObjectFactory());
			CombineAssertions("For Export TradeNet", () =>
			{
				AssertEquals("declaration.JE_OH_Importer", consignor.PK, declaration.JE_OH_Importer);
				AssertEquals("declaration.JE_OH_Supplier", consignee.PK, declaration.JE_OH_Supplier);
				AssertEquals("declaration.SG_OutwardTransportMode", "ROA", declaration.SG_OutwardTransportMode);
				AssertEquals("declaration.JE_MessageType", MessageTypeCodeList.Codes.OUT, declaration.JE_MessageType);
				AssertEquals("declaration.JE_MessageSubType", DeclarationTypeCodeList.Codes.DRT, declaration.JE_MessageSubType);
				AssertEquals("declaration.SG_OutwardVoyageFlightNo", "ROAD", declaration.SG_OutwardVoyageFlightNo);
				AssertEquals("declaration.JE_ContainerMode", CargoPackingCodeList.Codes.PackingType5, declaration.JE_ContainerMode);
				AssertEquals("declaration.JE_RS_NKServiceLevel", "STD", declaration.JE_RS_NKServiceLevel);
				AssertEquals("declaration.JE_GS_NKCusAgent", GlbStaff.CurrentUser.GS_Code, declaration.JE_GS_NKCusAgent);
				AssertEquals("declaration.JE_MasterBill", "08122222222", declaration.JE_MasterBill);
				AssertEquals("declaration.JE_DateAtOrigin", ZDateTime.Today.AddDays(-5), declaration.JE_DateAtOrigin);
				AssertEquals("declaration.JE_DateAtFinalDestination", ZDateTime.Today, declaration.JE_DateAtFinalDestination);
				AssertEquals("declaration.JE_HouseBill", "3824ARFY9JH", declaration.JE_HouseBill);
				AssertEquals("declaration.JE_RL_NKPortOfLoading", "SGSIN", declaration.JE_RL_NKPortOfLoading);
				AssertEquals("declaration.JE_RL_NKPortOfArrival", "AUSYD", declaration.JE_RL_NKPortOfArrival);
				AssertEquals("declaration.JE_RL_NKOrigin", "USONT", declaration.JE_RL_NKOrigin);
				AssertEquals("declaration.JE_RL_NKFinalDestination", "AUSYD", declaration.JE_RL_NKFinalDestination);
				AssertEquals("declaration.Invoices.Count", 1, declaration.Invoices.Count);
				var invoice = declaration.Invoices[0];
				AssertEquals("invoice.JZ_InvoiceNumber", "3824ARFY9JH", invoice.JZ_InvoiceNumber);
				AssertEquals("invoice.JZ_InvoiceAmount", 1000m, invoice.JZ_InvoiceAmount);
				AssertEquals("invoice.JZ_RX_NKInvoice_Currency", "SGD", invoice.JZ_RX_NKInvoice_Currency);
				AssertEquals("invoice.Charges.Count", 1, invoice.Charges.Count);
				AssertEquals("invoice.Charges[0].J7_ChargeType", "DIS", invoice.Charges[0].J7_ChargeType);
				AssertEquals("invoice.Charges[0].J7_Amount", 100m, invoice.Charges[0].J7_Amount);
				AssertEquals("invoice.JobComInvoiceLines.Count", 1, invoice.JobComInvoiceLines.Count);
				var invoiceLine = invoice.JobComInvoiceLines[0];
				AssertEquals("invoiceLine.JI_PartNo", "TEST PART", invoiceLine.JI_PartNo);
				AssertEquals("invoiceLine.JI_Description", "SHEN MIN EXTRA STRENGTH", invoiceLine.JI_Description);
				AssertEquals("invoiceLine.JI_Tariff", "49030000", invoiceLine.JI_Tariff);
				AssertEquals("invoiceLine.JI_InvoiceQuantity", 6m, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("invoiceLine.JI_InvoiceUQ", "PCE", invoiceLine.JI_InvoiceUQ);
				AssertEquals("invoiceLine.JI_LinePrice", 4.32m, invoiceLine.JI_LinePrice);
				AssertEquals("invoiceLine.JI_CountryOfOrigin", "US", invoiceLine.JI_CountryOfOrigin);
				AssertEquals("invoiceLine.JI_CustomsQuantity", 6m, invoiceLine.JI_CustomsQuantity);
				AssertEquals("invoiceLine.JI_CustomsUnitQty", "NMB", invoiceLine.JI_CustomsUnitQty);
			});
		}

		public void TestIUnitConverterDataProviderType()
		{
			var level1DataImport = new Level1DataImport(Factory);
			var lineTariffs = new Dictionary<_500000Line, TariffView>();
			var tradeNetCreator = new TradeNetDeclarationCreator(Factory, level1DataImport, null, false, consignor, consignee, 1000m, 100m, "SGD", lineTariffs);
			AssertEquals("Pack Conversion Type should be CommercialInvoice", RPTypeList.Codes.CommercialInvoice, ((IUnitConverterDataProvider)tradeNetCreator).Type);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore);
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			var sGCountryPK = new ZGuid("DCCBAC74-137B-459B-A6AE-8A7CF7AD5A63");
			PopulateRefLocoMap("2795", sGCountryPK, "USONT");
			PopulateRefLocoMap("9639", sGCountryPK, "AUSYD");
			consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "TEST CONSIGNEE";
			consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "TEST CONSIGNOR";
			var part = Factory.New<Customs.SG.V4.Business.OrgSupplierPart>();
			part.OP_PartNum = "TEST PART";
			part.OP_Desc = "TEST PART DESC";
			part.RelatedOrganisations.AddOrganisationIfNotExist(consignee.PK, "BTH");
			part.RelatedOrganisations.AddOrganisationIfNotExist(consignor.PK, "BTH");
			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = Classification.ClassificationType.Both;
			classification.CC_LookupCode = "LookupCode1";
			classification.CC_Description = "Lookup 1 DESCRIPTION";
			classification.CC_TariffNum = "49030000";
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var importPivot = Factory.New<CusClassPartPivot>();
			importPivot.CI_CC = classification.PK;
			importPivot.CI_OP = part.PK;
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			var testDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = testDataHelper.CreateTariffType(Core.Constants.CountryCodes.Singapore, Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			testDataHelper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "49030000", new ZDateTime(2011, 11, 26), new ZDateTime(2018, 6, 23));
			Factory.Save();
		}

		OrgHeader consignee;
		OrgHeader consignor;
		void PopulateRefLocoMap(ZString localPortCode, ZGuid countryGuid, ZString uNLOCO)
		{
			var locoMap = Factory.New<MasterFiles.Business.RefLocoMap>();
			locoMap.RY_LocalPortCode = localPortCode.SubstringSafe(0, locoMap.RY_LocalPortCodeInfo.MaxLength);
			locoMap.RY_RN = countryGuid;
			locoMap.RY_SystemUsage = "UPS";
			locoMap.RY_RL_NKLocoPort = uNLOCO;
		}
		#endregion
	}
}
