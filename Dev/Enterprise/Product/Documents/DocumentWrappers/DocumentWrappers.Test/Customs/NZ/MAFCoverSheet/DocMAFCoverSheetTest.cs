using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	sealed class DocMAFCoverSheetTest : TestCaseWithFactory
	{
		public void TestBusinessObjectToLogAgainst()
		{
			var declaration = Factory.New<JobDeclaration>();
			var coversheetWrapper = new DocMAFCoverSheet(Factory, new NZDocsMAFCoverSheet(TestDataBuilder.GetMAFMessaging(declaration)));
			AssertEquals("((IBODocDataProvider)coversheetWrapper).BusinessObjectToLogAgainst", declaration, ((IBODocDataProvider)coversheetWrapper).BusinessObjectToLogAgainst);
		}

		#region TestFieldsDirectlyExposedFromNZDocsMAFCoverSheet
		public void TestFieldsDirectlyExposedFromNZDocsMAFCoverSheet()
		{
			CoverSheet.D0_AccountHolder = "AccountHolder";
			CoverSheet.D0_AgentCompanyName = "AgentCompanyName";
			CoverSheet.D0_AgentContactName = "AgentContactName";
			CoverSheet.D0_AgentEmail = "AgentEmail";
			CoverSheet.D0_AgentFaxNumber = "AgentFaxNumber";
			CoverSheet.D0_AgentPhoneNumber = "AgentPhoneNumber";
			CoverSheet.D0_ClientReference = "ClientReference";
			CoverSheet.D0_DateOfArrival = new ZDateTime(2006, 4, 22);
			CoverSheet.D0_DateSigned = new ZDateTime(2006, 4, 25);
			CoverSheet.D0_EDITariffCodes = "EDITariffCodes";
			CoverSheet.D0_EntryNumber = "EntryNum";
			CoverSheet.D0_ExporterName = "ExporterName";
			CoverSheet.D0_HouseBill = "HouseBill";
			CoverSheet.D0_ImporterName = "ImporterName";
			CoverSheet.D0_MasterBill = "MasterBill";
			CoverSheet.D0_PayBeCash = true;
			CoverSheet.D0_PayBeCheque = false;
			CoverSheet.D0_PayByAccount = true;
			CoverSheet.D0_QENumber = "QENumber";
			CoverSheet.D0_ShippingOrAirLine = "ShippingOrAirLine";
			CoverSheet.D0_SignatoryCompanyName = "SignatoryCompanyName";
			CoverSheet.D0_SignatoryName = "SignatoryFullName";
			CoverSheet.D0_SignatoryPhoneNumber = "SignatoryPhoneNumber";
			CoverSheet.D0_SuppliedBillOfLading = false;
			CoverSheet.D0_SuppliedCertificates = true;
			CoverSheet.D0_SuppliedComplianceAgreement = false;
			CoverSheet.D0_SuppliedComplianceCheckCompleted = true;
			CoverSheet.D0_SuppliedIHS = false;
			CoverSheet.D0_SuppliedImportPermit = true;
			CoverSheet.D0_SuppliedOtherDocumentation = "DocsSuppliedOtherDocumentation";
			CoverSheet.D0_SuppliedQuarantineDeclaration = false;
			CoverSheet.D0_SuppliedRelevantInvoices = true;
			CoverSheet.D0_TotalPages = 6;
			CoverSheet.D0_ToMAFQuarantineService = "ToMAFQuarantineService";
			CoverSheet.D0_TransitionalFacility = "TransitionalFacility";
			CoverSheet.D0_TreatmentSupplier = "TreatmentSupplier";
			CoverSheet.D0_Vessel = "Vessel";
			CoverSheet.D0_VoyageOrFlight = "VoyageOrFlight";
			CoverSheet.D0_RL_NKDestination = "NZAKL";
			CoverSheet.D0_RL_NKPortOfDischarge = "NZCHC";
			CoverSheet.D0_RN_NKCountryOfOrigin = "AU";

			AssertEquals("CoverSheetWrapper.AccountHolder", "AccountHolder", CoverSheetWrapper.AccountHolder);
			AssertEquals("CoverSheetWrapper.AgentCompanyName", "AgentCompanyName", CoverSheetWrapper.AgentCompanyName);
			AssertEquals("CoverSheetWrapper.AgentContactName", "AgentContactName", CoverSheetWrapper.AgentContactName);
			AssertEquals("CoverSheetWrapper.AgentEmail", "AgentEmail", CoverSheetWrapper.AgentEmail);
			AssertEquals("CoverSheetWrapper.AgentFaxNumber", "AgentFaxNumber", CoverSheetWrapper.AgentFaxNumber);
			AssertEquals("CoverSheetWrapper.AgentPhoneNumber", "AgentPhoneNumber", CoverSheetWrapper.AgentPhoneNumber);
			AssertEquals("CoverSheetWrapper.ClientReference", "ClientReference", CoverSheetWrapper.ClientReference);
			AssertEquals("CoverSheetWrapper.DateOfArrival", new ZDateTime(2006, 4, 22), CoverSheetWrapper.DateOfArrival);
			AssertEquals("CoverSheetWrapper.DateSigned", new ZDateTime(2006, 4, 25), CoverSheetWrapper.DateSigned);
			AssertEquals("CoverSheetWrapper.EDITariffCodes", "EDITariffCodes", CoverSheetWrapper.EDITariffCodes);
			AssertEquals("CoverSheetWrapper.EntryNumber", "EntryNum", CoverSheetWrapper.EntryNumber);
			AssertEquals("CoverSheetWrapper.ExporterName", "ExporterName", CoverSheetWrapper.ExporterName);
			AssertEquals("CoverSheetWrapper.HouseBill", "HouseBill", CoverSheetWrapper.HouseBill);
			AssertEquals("CoverSheetWrapper.ImporterName", "ImporterName", CoverSheetWrapper.ImporterName);
			AssertEquals("CoverSheetWrapper.MasterBill", "MasterBill", CoverSheetWrapper.MasterBill);
			AssertEquals("CoverSheetWrapper.PayByCash", "X", CoverSheetWrapper.PayByCash);
			AssertEquals("CoverSheetWrapper.PayByCheque", " ", CoverSheetWrapper.PayByCheque);
			AssertEquals("CoverSheetWrapper.PayByAccount", "X", CoverSheetWrapper.PayByAccount);
			AssertEquals("CoverSheetWrapper.QENumber", "QENumber", CoverSheetWrapper.QENumber);
			AssertEquals("CoverSheetWrapper.ShippingOrAirLine", "ShippingOrAirLine", CoverSheetWrapper.ShippingOrAirLine);
			AssertEquals("CoverSheetWrapper.SignatoryCompanyName", "SignatoryCompanyName", CoverSheetWrapper.SignatoryCompanyName);
			AssertEquals("CoverSheetWrapper.SignatoryFullName", "SignatoryFullName", CoverSheetWrapper.SignatoryFullName);
			AssertEquals("CoverSheetWrapper.SignatoryPhoneNumber", "SignatoryPhoneNumber", CoverSheetWrapper.SignatoryPhoneNumber);
			AssertEquals("CoverSheetWrapper.DocsSuppliedBillOfLading", " ", CoverSheetWrapper.DocsSuppliedBillOfLading);
			AssertEquals("CoverSheetWrapper.DocsSuppliedCertificates", "X", CoverSheetWrapper.DocsSuppliedCertificates);
			AssertEquals("CoverSheetWrapper.DocsSuppliedComplianceAgreement", " ", CoverSheetWrapper.DocsSuppliedComplianceAgreement);
			AssertEquals("CoverSheetWrapper.DocsSuppliedComplianceCheckCompleted", "X", CoverSheetWrapper.DocsSuppliedComplianceCheckCompleted);
			AssertEquals("CoverSheetWrapper.DocsSuppliedIHS", " ", CoverSheetWrapper.DocsSuppliedIHS);
			AssertEquals("CoverSheetWrapper.DocsSuppliedImportPermit", "X", CoverSheetWrapper.DocsSuppliedImportPermit);
			AssertEquals("CoverSheetWrapper.DocsSuppliedOtherDocumentation", "DocsSuppliedOtherDocumentation", CoverSheetWrapper.DocsSuppliedOtherDocumentation);
			AssertEquals("CoverSheetWrapper.DocsSuppliedQuarantineDeclaration", " ", CoverSheetWrapper.DocsSuppliedQuarantineDeclaration);
			AssertEquals("CoverSheetWrapper.DocsSuppliedRelevantInvoices", "X", CoverSheetWrapper.DocsSuppliedRelevantInvoices);
			AssertEquals("CoverSheetWrapper.ToMAFQuarantineService", "ToMAFQuarantineService", CoverSheetWrapper.ToMAFQuarantineService);
			AssertEquals("CoverSheetWrapper.TransitionalFacility", "TransitionalFacility", CoverSheetWrapper.TransitionalFacility);
			AssertEquals("CoverSheetWrapper.TreatmentSupplier", "TreatmentSupplier", CoverSheetWrapper.TreatmentSupplier);
			AssertEquals("CoverSheetWrapper.Vessel", "Vessel", CoverSheetWrapper.Vessel);
			AssertEquals("CoverSheetWrapper.VoyageOrFlight", "VoyageOrFlight", CoverSheetWrapper.VoyageOrFlight);
			AssertEquals("CoverSheetWrapper.RL_NKDestination", "NZAKL - Auckland", CoverSheetWrapper.Destination);
			AssertEquals("CoverSheetWrapper.RL_NKPortOfDischarge", "NZCHC - Christchurch", CoverSheetWrapper.PortOfDischarge);
			AssertEquals("CoverSheetWrapper.RN_NKCountryOfOrigin", "AU - Australia", CoverSheetWrapper.CountryOfOrigin);
			AssertEquals("CoverSheetWrapper.TotalPages", 6, CoverSheetWrapper.TotalPages);
		}
		#endregion

		#region TestFieldsExposedFromTheContainerForTheFirstPageGetDataFromContainers
		public void TestFieldsExposedFromTheContainerForTheFirstPageGetDataFromContainers()
		{
			NZDocsMAFCSContainer container1 = CoverSheet.Containers.AddNew();
			container1.D2_ContainerNumber = "CONTAINER1";
			container1.D2_IsFCL = true;
			container1.D2_IsLCL = false;

			NZDocsMAFCSContainer container2 = CoverSheet.Containers.AddNew();
			container2.D2_ContainerNumber = "CONTAINER2";
			container2.D2_IsFCL = false;
			container2.D2_IsLCL = true;

			NZDocsMAFCSContainer container3 = CoverSheet.Containers.AddNew();
			container3.D2_ContainerNumber = "CONTAINER3";
			container3.D2_IsFCL = true;
			container3.D2_IsLCL = false;

			NZDocsMAFCSContainer container4 = CoverSheet.Containers.AddNew();
			container4.D2_ContainerNumber = "CONTAINER4";
			container4.D2_IsFCL = false;
			container4.D2_IsLCL = true;

			NZDocsMAFCSContainer container5 = CoverSheet.Containers.AddNew();
			container5.D2_ContainerNumber = "CONTAINER5";
			container5.D2_IsFCL = true;
			container5.D2_IsLCL = false;

			NZDocsMAFCSContainer container6 = CoverSheet.Containers.AddNew();
			container6.D2_ContainerNumber = "CONTAINER6";
			container6.D2_IsFCL = false;
			container6.D2_IsLCL = true;

			AssertEquals("CoverSheetWrapper.ContainerNumber1", "CONTAINER1", CoverSheetWrapper.Page[0].Containers[0].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL1", true, CoverSheetWrapper.Page[0].Containers[0].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL1", false, CoverSheetWrapper.Page[0].Containers[0].IsLCL);

			AssertEquals("CoverSheetWrapper.ContainerNumber2", "CONTAINER2", CoverSheetWrapper.Page[0].Containers[1].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL2", false, CoverSheetWrapper.Page[0].Containers[1].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL2", true, CoverSheetWrapper.Page[0].Containers[1].IsLCL);

			AssertEquals("CoverSheetWrapper.ContainerNumber3", "CONTAINER3", CoverSheetWrapper.Page[0].Containers[2].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL3", true, CoverSheetWrapper.Page[0].Containers[2].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL3", false, CoverSheetWrapper.Page[0].Containers[2].IsLCL);

			AssertEquals("CoverSheetWrapper.ContainerNumber4", "CONTAINER4", CoverSheetWrapper.Page[0].Containers[3].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL4", false, CoverSheetWrapper.Page[0].Containers[3].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL4", true, CoverSheetWrapper.Page[0].Containers[3].IsLCL);

			AssertEquals("CoverSheetWrapper.ContainerNumber5", "CONTAINER5", CoverSheetWrapper.Page[0].Containers[4].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL5", true, CoverSheetWrapper.Page[0].Containers[4].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL5", false, CoverSheetWrapper.Page[0].Containers[4].IsLCL);
		}
		#endregion

		#region TestFieldsExposedFromTheContainerForTheFirstPageDontBarfWithNoContainers
		public void TestFieldsExposedFromTheContainerForTheFirstPageDontBarfWithNoContainers()
		{
			AssertEquals("CoverSheetWrapper.ContainerNumber1", "", CoverSheetWrapper.Page[0].Containers[0].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL1", false, CoverSheetWrapper.Page[0].Containers[0].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL1", false, CoverSheetWrapper.Page[0].Containers[0].IsLCL);

			AssertEquals("CoverSheetWrapper.ContainerNumber2", "", CoverSheetWrapper.Page[0].Containers[1].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL2", false, CoverSheetWrapper.Page[0].Containers[1].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL2", false, CoverSheetWrapper.Page[0].Containers[1].IsLCL);

			AssertEquals("CoverSheetWrapper.ContainerNumber3", "", CoverSheetWrapper.Page[0].Containers[2].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL3", false, CoverSheetWrapper.Page[0].Containers[2].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL3", false, CoverSheetWrapper.Page[0].Containers[2].IsLCL);

			AssertEquals("CoverSheetWrapper.ContainerNumber4", "", CoverSheetWrapper.Page[0].Containers[3].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL4", false, CoverSheetWrapper.Page[0].Containers[3].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL4", false, CoverSheetWrapper.Page[0].Containers[3].IsLCL);

			AssertEquals("CoverSheetWrapper.ContainerNumber5", "", CoverSheetWrapper.Page[0].Containers[4].ContainerNumber);
			AssertEquals("CoverSheetWrapper.ContainerIsFCL5", false, CoverSheetWrapper.Page[0].Containers[4].IsFCL);
			AssertEquals("CoverSheetWrapper.ContainerIsLCL5", false, CoverSheetWrapper.Page[0].Containers[4].IsLCL);
		}
		#endregion

		#region TestFieldsExposedFromTheCommoditiesForTheFirstPageGetDataFromCommodities
		public void TestFieldsExposedFromTheCommoditiesForTheFirstPageGetDataFromCommodities()
		{
			NZDocsMAFCSCommodity commodity1 = CoverSheet.Commodities.AddNew();
			commodity1.D1_CommodityOrSpecies = "SPECIES1";
			commodity1.D1_QuantityWithUnit = "10 PC";
			commodity1.D1_MeasureWithUnit = "1 M3";

			NZDocsMAFCSCommodity commodity2 = CoverSheet.Commodities.AddNew();
			commodity2.D1_CommodityOrSpecies = "SPECIES2";
			commodity2.D1_QuantityWithUnit = "20 PC";
			commodity2.D1_MeasureWithUnit = "2 M3";

			NZDocsMAFCSCommodity commodity3 = CoverSheet.Commodities.AddNew();
			commodity3.D1_CommodityOrSpecies = "SPECIES3";
			commodity3.D1_QuantityWithUnit = "30 PC";
			commodity3.D1_MeasureWithUnit = "3 M3";

			NZDocsMAFCSCommodity commodity4 = CoverSheet.Commodities.AddNew();
			commodity4.D1_CommodityOrSpecies = "SPECIES4";
			commodity4.D1_QuantityWithUnit = "40 PC";
			commodity4.D1_MeasureWithUnit = "4 M3";

			NZDocsMAFCSCommodity commodity5 = CoverSheet.Commodities.AddNew();
			commodity5.D1_CommodityOrSpecies = "SPECIES5";
			commodity5.D1_QuantityWithUnit = "50 PC";
			commodity5.D1_MeasureWithUnit = "5 M3";

			NZDocsMAFCSCommodity commodity6 = CoverSheet.Commodities.AddNew();
			commodity6.D1_CommodityOrSpecies = "SPECIES6";
			commodity6.D1_QuantityWithUnit = "60 PC";
			commodity6.D1_MeasureWithUnit = "6 M3";

			AssertEquals("CoverSheetWrapper.CommoditySpecies1", "SPECIES1", CoverSheetWrapper.Page[0].Commodities[0].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity1", "10 PC", CoverSheetWrapper.Page[0].Commodities[0].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure1", "1 M3", CoverSheetWrapper.Page[0].Commodities[0].Measure);

			AssertEquals("CoverSheetWrapper.CommoditySpecies2", "SPECIES2", CoverSheetWrapper.Page[0].Commodities[1].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity2", "20 PC", CoverSheetWrapper.Page[0].Commodities[1].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure2", "2 M3", CoverSheetWrapper.Page[0].Commodities[1].Measure);

			AssertEquals("CoverSheetWrapper.CommoditySpecies3", "SPECIES3", CoverSheetWrapper.Page[0].Commodities[2].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity3", "30 PC", CoverSheetWrapper.Page[0].Commodities[2].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure3", "3 M3", CoverSheetWrapper.Page[0].Commodities[2].Measure);

			AssertEquals("CoverSheetWrapper.CommoditySpecies4", "SPECIES4", CoverSheetWrapper.Page[0].Commodities[3].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity4", "40 PC", CoverSheetWrapper.Page[0].Commodities[3].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure4", "4 M3", CoverSheetWrapper.Page[0].Commodities[3].Measure);

			AssertEquals("CoverSheetWrapper.CommoditySpecies5", "SPECIES5", CoverSheetWrapper.Page[0].Commodities[4].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity5", "50 PC", CoverSheetWrapper.Page[0].Commodities[4].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure5", "5 M3", CoverSheetWrapper.Page[0].Commodities[4].Measure);
		}
		#endregion

		#region TestFieldsExposedFromTheCommoditiesForTheFirstPageDontBarfWithNoCommodities
		public void TestFieldsExposedFromTheCommoditiesForTheFirstPageDontBarfWithNoCommodities()
		{
			AssertEquals("CoverSheetWrapper.CommoditySpecies1", "", CoverSheetWrapper.Page[0].Commodities[0].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity1", "", CoverSheetWrapper.Page[0].Commodities[0].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure1", "", CoverSheetWrapper.Page[0].Commodities[0].Measure);

			AssertEquals("CoverSheetWrapper.CommoditySpecies2", "", CoverSheetWrapper.Page[0].Commodities[1].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity2", "", CoverSheetWrapper.Page[0].Commodities[1].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure2", "", CoverSheetWrapper.Page[0].Commodities[1].Measure);

			AssertEquals("CoverSheetWrapper.CommoditySpecies3", "", CoverSheetWrapper.Page[0].Commodities[2].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity3", "", CoverSheetWrapper.Page[0].Commodities[2].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure3", "", CoverSheetWrapper.Page[0].Commodities[2].Measure);

			AssertEquals("CoverSheetWrapper.CommoditySpecies4", "", CoverSheetWrapper.Page[0].Commodities[3].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity4", "", CoverSheetWrapper.Page[0].Commodities[3].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure4", "", CoverSheetWrapper.Page[0].Commodities[3].Measure);

			AssertEquals("CoverSheetWrapper.CommoditySpecies5", "", CoverSheetWrapper.Page[0].Commodities[4].Name);
			AssertEquals("CoverSheetWrapper.CommodityQuantity5", "", CoverSheetWrapper.Page[0].Commodities[4].Qty);
			AssertEquals("CoverSheetWrapper.CommodityMeasure5", "", CoverSheetWrapper.Page[0].Commodities[4].Measure);
		}
		#endregion

		#region Implementation
		#region CoverSheetWrapper
		DocMAFCoverSheet CoverSheetWrapper
		{
			get
			{
				if (fCoverSheetWrapper == null)
				{
					fCoverSheetWrapper = new DocMAFCoverSheet(Factory, CoverSheet);
				}
				return fCoverSheetWrapper;
			}
		}
		DocMAFCoverSheet fCoverSheetWrapper;
		#endregion

		#region CoverSheet
		NZDocsMAFCoverSheet CoverSheet
		{
			get
			{
				if (fCoverSheet == null)
				{
					fCoverSheet = new NZDocsMAFCoverSheet(TestDataBuilder.GetMAFMessaging(Declaration));
				}
				return fCoverSheet;
			}
		}
		NZDocsMAFCoverSheet fCoverSheet;
		#endregion

		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion
		#endregion
	}
}
