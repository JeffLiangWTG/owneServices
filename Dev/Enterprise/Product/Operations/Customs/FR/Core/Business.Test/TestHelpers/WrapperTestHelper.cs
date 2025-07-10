using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using SharedBusiness = Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Testing
{
	public class WrapperTestHelper : TestCaseWithFactory
	{
		#region Constants

		public const string VesselCode = "APL FRANCE VESSEL";
		public const string CustomsClientID = "1234";
		public const string OfficeOfExit = "A123456";
		public const string ExportExitTypeReason = "BLABLA";
		public const string ExportTransportOrigin = "FR";
		public const string ExportTransportDestination = "GB";
		public const string CINNumLta = "05783013825";

		#endregion

		public CusEntryHeader CreateTestCusEntryHeader()
		{
			return CreateTestCusEntryHeader(true, false);
		}

		public CusEntryHeader CreateTestCusEntryHeader(bool import)
		{
			return CreateTestCusEntryHeader(import, false);
		}

		public CusEntryHeader CreateTestCusEntryHeader(bool import, bool cinMessage)
		{
			var factory = new BusinessObjectFactory();

			SetupMasterFilesDatas(factory);

			var declaration = factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_MasterBill = "UnitTest";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.ImporterDeliveryAddress.OrganisationPK = importerDelivery.PK;

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_SubStyle = "Y";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;
			#region Invoice Line charges

			var invoiceLineCharges = invoiceLine.Charges;
			var commissionCharge = invoiceLineCharges.AddNew(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, 0.45m, Enterprise.Core.Constants.CurrencyCodes.France);
			var containersAndPackingCharge = invoiceLineCharges.AddNew(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, 0.49m, Enterprise.Core.Constants.CurrencyCodes.France);

			#endregion
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.FillWithValidTestData();

			var customOffice = declaration.CustomsOffices.AddNew();
			customOffice.CY_Code = "ENT";
			customOffice.CY_Type = "EUO";
			customOffice.CY_Data = "FR000130";

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			if (import == ZBool.False)
			{
				declaration.JE_MessageType = SharedBusiness.JobMessageTypeList.Codes.Export;
				declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.RoadModeOfTransport;

				#region Vessel

				vessel = factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, VesselCode);
				if (vessel == null)
				{
					vessel = Factory.New<RefVessel>();
					vessel.RV_Code = VesselCode;
				}

				declaration.JE_VesselName = vessel.RV_Code;

				#endregion

				#region Office of Exit

				var officeOfExit = declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(o => o.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit);
				officeOfExit.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
				officeOfExit.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
				officeOfExit.CY_Data = OfficeOfExit;

				declaration.JE_ExportExitType = Enterprise.Customs.FR.Business.ExportExitTypeList.Codes.STC;
				declaration.JE_ExportExitTypeReason = ExportExitTypeReason;

				var refUNLOCOLoading = Factory.New<RefUNLOCO>();
				refUNLOCOLoading.RL_Code = ExportTransportOrigin;
				refUNLOCOLoading.RL_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				var refCountryStatesLoading = Factory.New<RefCountryStates>();
				refCountryStatesLoading.RW_Code = Core.Constants.CountryCodes.France;
				refUNLOCOLoading.RL_RW = refCountryStatesLoading.PK;

				var refUNLOCOArrival = Factory.New<RefUNLOCO>();
				refUNLOCOArrival.RL_Code = ExportTransportDestination;
				refUNLOCOArrival.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				var refCountryStatesArrival = Factory.New<RefCountryStates>();
				refCountryStatesArrival.RW_Code = Core.Constants.CountryCodes.Australia;
				refUNLOCOArrival.RL_RW = refCountryStatesArrival.PK;

				declaration.JE_RL_NKPortOfLoading = refUNLOCOLoading.RL_Code;
				declaration.JE_RL_NKPortOfArrival = refUNLOCOArrival.RL_Code;

				#endregion

				#region Transport

				invoice.ZG_TransportChargesMethodOfPayment = SharedBusiness.TransportChargesModeOfPayment.Codes.CreditCard;

				Transport trans1 = declaration.Transports.AddNew();
				Transport trans2 = declaration.Transports.AddNew();
				Transport trans3 = declaration.Transports.AddNew();
				Transport trans4 = declaration.Transports.AddNew();
				trans1.JW_ETA = ZDate.Today.AddDays(1);
				trans2.JW_ETA = ZDate.Today.AddDays(2);
				trans3.JW_ETA = ZDate.Today.AddDays(3);
				trans4.JW_ETA = ZDate.Today.AddDays(4);
				trans1.JW_RL_NKLoadPort = ExportTransportOrigin;
				trans1.JW_RL_NKDiscPort = "INBOM";
				trans2.JW_RL_NKLoadPort = "INBOM";
				trans2.JW_RL_NKDiscPort = "TRIST";
				trans3.JW_RL_NKLoadPort = "TRIST";
				trans3.JW_RL_NKDiscPort = "DEHAM";
				trans4.JW_RL_NKLoadPort = "DEHAM";
				trans4.JW_RL_NKDiscPort = ExportTransportDestination;
				declaration.JE_RL_NKOrigin = ExportTransportOrigin;
				declaration.JE_RL_NKFinalDestination = ExportTransportDestination;

				#endregion

				#region CIN

				if (cinMessage == ZBool.True)
				{
					declaration.JE_MasterBill = CINNumLta;
				}

				#endregion
			}

			var shutUp = new SharedBusiness.SendsMessagesToCustomsShutterUpperer();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var mergeResult = declaration.DoMerge(shutUp);

			var groupCharges = declaration.CustomsEntryHeaders[0].InvoiceHeaders[0].GroupCharges;

			var charge2 = groupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 1000m, declaration.LocalCurrencyCode);
			charge2.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Value;

			var charge3 = groupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 2000m, Enterprise.Core.Constants.CurrencyCodes.UnitedStates);
			charge3.J7_DistributeBy = Customs.Common.ChargeDistributeByList.Codes.Value;

			#region Air transport charges

			var airThirdCountryToEUTransportCostCharge = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 100m, Enterprise.Core.Constants.CurrencyCodes.France);
			airThirdCountryToEUTransportCostCharge.J7_IsDutiable = true;
			airThirdCountryToEUTransportCostCharge.J7_IsStatisticalValueApplicable = true;

			var airEUtoFRTransportCostCharge = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 200m, Enterprise.Core.Constants.CurrencyCodes.France);
			airEUtoFRTransportCostCharge.J7_IsDutiable = false;
			airEUtoFRTransportCostCharge.J7_IsStatisticalValueApplicable = true;

			var airFRTransportCostCharge = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, 300m, Enterprise.Core.Constants.CurrencyCodes.France);
			airFRTransportCostCharge.J7_IsDutiable = false;
			airFRTransportCostCharge.J7_IsStatisticalValueApplicable = false;

			#endregion

			#region Other transport charges

			var thirdCountryTransportCostCharge = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 400m, Enterprise.Core.Constants.CurrencyCodes.France);
			thirdCountryTransportCostCharge.J7_IsDutiable = true;
			thirdCountryTransportCostCharge.J7_IsStatisticalValueApplicable = true;

			var eUTransportCostChargeInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 500m, Enterprise.Core.Constants.CurrencyCodes.France);
			eUTransportCostChargeInInvoice.J7_IsDutiable = false;
			eUTransportCostChargeInInvoice.J7_IsStatisticalValueApplicable = true;
			eUTransportCostChargeInInvoice.J7_IsNotIncludedInInvoice = false;

			var eUTransportCostChargeNotInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 600m, Enterprise.Core.Constants.CurrencyCodes.France);
			eUTransportCostChargeNotInInvoice.J7_IsDutiable = false;
			eUTransportCostChargeNotInInvoice.J7_IsStatisticalValueApplicable = true;
			eUTransportCostChargeNotInInvoice.J7_IsNotIncludedInInvoice = true;

			var frTransportCostChargeInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 700m, Enterprise.Core.Constants.CurrencyCodes.France);
			frTransportCostChargeInInvoice.J7_IsDutiable = false;
			frTransportCostChargeInInvoice.J7_IsStatisticalValueApplicable = false;
			frTransportCostChargeInInvoice.J7_IsNotIncludedInInvoice = false;

			var frTransportCostChargeNotInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 800m, Enterprise.Core.Constants.CurrencyCodes.France);
			frTransportCostChargeNotInInvoice.J7_IsDutiable = false;
			frTransportCostChargeNotInInvoice.J7_IsStatisticalValueApplicable = false;
			frTransportCostChargeNotInInvoice.J7_IsNotIncludedInInvoice = true;

			#endregion

			#region Insurance charges

			var thirdCountryInsuranceChargeIncludedInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 10m, Enterprise.Core.Constants.CurrencyCodes.France);
			thirdCountryInsuranceChargeIncludedInInvoice.J7_IsDutiable = true;
			thirdCountryInsuranceChargeIncludedInInvoice.J7_IsStatisticalValueApplicable = true;
			thirdCountryInsuranceChargeIncludedInInvoice.J7_IsNotIncludedInInvoice = false;

			var thirdCountryInsuranceChargeNotIncludedInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 20m, Enterprise.Core.Constants.CurrencyCodes.France);
			thirdCountryInsuranceChargeNotIncludedInInvoice.J7_IsDutiable = true;
			thirdCountryInsuranceChargeNotIncludedInInvoice.J7_IsStatisticalValueApplicable = true;
			thirdCountryInsuranceChargeNotIncludedInInvoice.J7_IsNotIncludedInInvoice = true;

			var euInsuranceChargeIncludedInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 30m, Enterprise.Core.Constants.CurrencyCodes.France);
			euInsuranceChargeIncludedInInvoice.J7_IsDutiable = false;
			euInsuranceChargeIncludedInInvoice.J7_IsStatisticalValueApplicable = true;
			euInsuranceChargeIncludedInInvoice.J7_IsNotIncludedInInvoice = false;

			var euCountryInsuranceChargeNotIncludedInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 40m, Enterprise.Core.Constants.CurrencyCodes.France);
			euCountryInsuranceChargeNotIncludedInInvoice.J7_IsDutiable = false;
			euCountryInsuranceChargeNotIncludedInInvoice.J7_IsStatisticalValueApplicable = true;
			euCountryInsuranceChargeNotIncludedInInvoice.J7_IsNotIncludedInInvoice = true;

			var frCountryInsuranceChargeIncludedInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 50m, Enterprise.Core.Constants.CurrencyCodes.France);
			frCountryInsuranceChargeIncludedInInvoice.J7_IsDutiable = false;
			frCountryInsuranceChargeIncludedInInvoice.J7_IsStatisticalValueApplicable = false;
			frCountryInsuranceChargeIncludedInInvoice.J7_IsNotIncludedInInvoice = false;

			var frCountryInsuranceChargeNotIncludedInInvoice = groupCharges.AddNew(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, 60m, Enterprise.Core.Constants.CurrencyCodes.France);
			frCountryInsuranceChargeNotIncludedInInvoice.J7_IsDutiable = false;
			frCountryInsuranceChargeNotIncludedInInvoice.J7_IsStatisticalValueApplicable = false;
			frCountryInsuranceChargeNotIncludedInInvoice.J7_IsNotIncludedInInvoice = true;

			var thirdCountryAirInsuranceCharge = groupCharges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 70m, Enterprise.Core.Constants.CurrencyCodes.France);
			thirdCountryAirInsuranceCharge.J7_IsDutiable = true;
			thirdCountryAirInsuranceCharge.J7_IsStatisticalValueApplicable = false;
			thirdCountryAirInsuranceCharge.J7_IsNotIncludedInInvoice = false;

			var frAirInsuranceCharge = groupCharges.AddNew(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, 80m, Enterprise.Core.Constants.CurrencyCodes.France);
			frAirInsuranceCharge.J7_IsDutiable = false;
			frAirInsuranceCharge.J7_IsStatisticalValueApplicable = false;
			frAirInsuranceCharge.J7_IsNotIncludedInInvoice = false;

			#endregion

			AssertEquals("Invoice header has 18 charges apportioned", 20, groupCharges.Count);

			Assert("Merge failed", mergeResult);
			declaration.ResumeApportionment();
			factory.Save();

			return declaration.CustomsEntryHeaders[0];
		}

		void SetupMasterFilesDatas(BusinessObjectFactory factory)
		{
			if (declarantAddress != null)
			{
				return;
			}
			declarantAddress = factory.New<OrgAddress>();
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = OrgCusCode.FranceCodeTypes.Siret;
			orgHeader.CustomsClientID = CustomsClientID;

			declarantAddress.OA_OH = orgHeader.PK;
			declarantAddress.OA_Address1 = "Eugene Leroy Street ";

			#region Organisation Registration number : SRT

			var orgCusCodeSrt = factory.New<OrgCusCode>();
			orgCusCodeSrt.OK_OA_PremisesAddress = declarantAddress.PK;
			orgCusCodeSrt.OK_CodeType = OrgCusCode.FranceCodeTypes.Siret;
			orgCusCodeSrt.OK_CustomsRegNo = "FR33159700500064";
			orgCusCodeSrt.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			orgCusCodeSrt.OK_OH = declarantAddress.OA_OH;

			orgHeader.CustomsCodes.Add(orgCusCodeSrt);

			#endregion

			#region Organisation Registration number : CBR

			var orgCusCodeCbr = factory.New<OrgCusCode>();
			orgCusCodeCbr.OK_OA_PremisesAddress = declarantAddress.PK;
			orgCusCodeCbr.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
			orgCusCodeCbr.OK_CustomsRegNo = "FR33159700500064";
			orgCusCodeCbr.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			orgCusCodeCbr.OK_OH = declarantAddress.OA_OH;

			orgHeader.CustomsCodes.Add(orgCusCodeCbr);

			#endregion

			importer = factory.NewWithValidTestData<OrgHeader>();
			importer.MainAddress.OA_PostCode = "24130";

			var countryCodeImporter = factory.NewWithValidTestData<RefCountry>();

			supplier = factory.NewWithValidTestData<OrgHeader>();

			arrivalAt = factory.New<OrgHeader>();
			arrivalAt.FillWithValidTestData();
			arrivalAt.OH_RL_NKClosestPort = "!ZZ";
			var arrivalLocation1 = arrivalAt.Addresses.AddNew();
			arrivalLocation1.FillWithValidTestData();
			arrivalLocation1.Address1 = "Addresss1";
			var arrivalLocation2 = arrivalAt.Addresses.AddNew();
			arrivalLocation2.FillWithValidTestData();
			arrivalLocation2.Address1 = "Addresss2";
			arrivalAt.CustomsCodes.AddNew("CCP", "1111", Core.Constants.CountryCodes.France).OK_OA_PremisesAddress = arrivalLocation1.PK;
			arrivalAt.CustomsCodes.AddNew("CCP", "2222", Core.Constants.CountryCodes.UnitedStates).OK_OA_PremisesAddress = arrivalLocation2.PK;

			importerDelivery = factory.New<OrgHeader>();
			importerDelivery.FillWithValidTestData();
			var importerDeliveryAddress = importerDelivery.Addresses.AddNew();
			importerDeliveryAddress.FillWithValidTestData();
			importerDeliveryAddress.Address1 = "Importer Delivery Address";
		}
		OrgAddress declarantAddress;
		OrgHeader importer;
		OrgHeader supplier;
		OrgHeader arrivalAt;
		OrgHeader importerDelivery;
		RefVessel vessel;
	}
}
