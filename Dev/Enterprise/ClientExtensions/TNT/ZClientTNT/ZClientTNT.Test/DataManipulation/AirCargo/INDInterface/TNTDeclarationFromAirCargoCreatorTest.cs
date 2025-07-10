using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.Testing
{
	public class TNTDeclarationFromAirCargoCreatorTest : TestCaseWithFactory
	{
		public void TestHouseBillIsLinkedToNewDeclaration()
		{
			AssertEquals("PreCondition: HouseBill is not linked to a declaration", ZGuid.Empty, HouseBill.CS_JE_CustomsFormalEntry);
			TNTDeclarationFromAirCargoCreator creator = new TNTDeclarationFromAirCargoCreator(HouseBill);
			JobDeclaration declaration = (JobDeclaration)creator.CreateIgnoreWarnings();
			AssertNotNull("New Declaration should not be null", declaration);
			AssertEquals("HouseBill should be linked to a declaration", declaration.PK, HouseBill.CS_JE_CustomsFormalEntry);
		}

		public void TestNewDeclarationCreatedWithOverrideData()
		{
			HouseBill.CS_ShipmentType = "DOC";
			NotificationBuffer buffer = new NotificationBuffer();
			TNTDeclarationFromAirCargoCreator creator = new TNTDeclarationFromAirCargoCreator(HouseBill);
			JobDeclaration declaration = (JobDeclaration)creator.Create(buffer);
			AssertNotNull("Declaration should not be null", declaration);
			AssertEquals("Declaration's Message Type should be Import", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("Declaration's JE_AgentsReference", HouseBill.CS_HAWB, declaration.JE_AgentsReference);
			AssertEquals("Declaration's JE_TotalNoOfPacksPackType", Core.Constants.PkgUnit.Piece, declaration.JE_TotalNoOfPacksPackType);
			AssertEquals("Declaration's JE_MergeBy", OrgConstants.MergeInvoiceLines.Tariff, declaration.JE_MergeBy);
			AssertNotNull("Declaration's JobDocsAndCartage", declaration.DocsAndCartage);
			AssertEquals("Declaration's ImporterDeliveryAddress.E2_AddressOverride", true, declaration.ImporterDeliveryAddress.E2_AddressOverride);
			AssertEquals("Declaration's Payment Method", JobDeclaration.PaymentMethods.Broker, declaration.JE_PaymentMethod);
			HouseBill.CS_ShipmentType = "STD";
			declaration = (JobDeclaration)creator.CreateIgnoreWarnings();
			AssertNotNull("Declaration should not be null", declaration);
			AssertEquals("Declaration's JE_TotalNoOfPacksPackType", Core.Constants.PkgUnit.Carton, declaration.JE_TotalNoOfPacksPackType);
		}

		#region TestCreateCommercialInvoiceDetails
		public void TestCreateCommercialInvoiceDetails()
		{
			HouseBill.CS_RX_NKGoodsCurrency = Helper.USDCurrency.RX_Code;
			TNTDeclarationFromAirCargoCreator creator = new TNTDeclarationFromAirCargoCreator(HouseBill);
			JobDeclaration declaration = (JobDeclaration)creator.CreateIgnoreWarnings();
			AssertNotNull("Declaration should not be null", declaration);
			AssertEquals("Declaration should have 1 invoice", 1, declaration.Invoices.Count);
			JobComInvoiceHeader invoice = declaration.Invoices[0];
			AssertNotNull("Invoice should not be null", invoice);
			AssertEquals("Invoice Number", "1", invoice.JZ_InvoiceNumber);
			AssertEquals("Invoice Supplier should be empty", ZGuid.Empty, invoice.JZ_OH_Supplier);
			AssertEquals("Invoice Amount", HouseBill.CS_GoodsValue, invoice.JZ_InvoiceAmount);
			AssertEquals("Invoice Currency", Helper.USDCurrency.RX_Code, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("Invoice Weight", declaration.JE_TotalWeight, invoice.JZ_Weight);
			AssertEquals("Invoice Weight Unit Quantiy", declaration.JE_TotalWeightUnit, invoice.JZ_WeightUQ);
			AssertEquals("Invoice Valuation Basis", "TV", invoice.JZ_ValuationBasis);
			AssertEquals("Invoice Related Transaction", "N", invoice.AddInfo.ZA_HeaderREL_Hidden);
			RefUNLOCO origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, declaration.JE_RL_NKOrigin);
			AssertNotNull("PreCondition: Declaration Origin should not be null", origin);
			AssertEquals("Invoice Origin", origin.RL_RN_NKCountryCode, invoice.ZA_ORG);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice.JobComInvoiceLines.Count);
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines[0];
			AssertNotNull("InvoiceLine should not be null", invoiceLine);
			AssertEquals("InvoiceLine Quantity", new ZDecimal(declaration.JE_TotalNoOfPacks), invoiceLine.JI_InvoiceQuantity);
			AssertEquals("InvoiceLine Unit Quantity", declaration.JE_TotalNoOfPacksPackType, invoiceLine.JI_InvoiceUQ);
			AssertEquals("InvoiceLine Price", invoice.JZ_InvoiceAmount, invoiceLine.JI_LinePrice);
			AssertEquals("InvoiceLine Weight", invoice.JZ_Weight, invoiceLine.JI_Weight);
			AssertEquals("InvoiceLine Unit Weight", invoice.JZ_WeightUQ, invoiceLine.JI_WeightUQ);
			AssertNotNull("InvoiceLine AddInfo should not be null", invoiceLine.AddInfo);
			AssertEquals("InvoiceLine Valuation Basis", "", invoiceLine.AddInfo.ZA_ValuationBasis_Hidden);
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			AssertNotNull("Declaration's Invoice Group Header should not be null", groupHeader);
			AssertEquals("GroupHeader should have 2 Group Charges", 2, groupHeader.Charges.Count);
			GroupInvoiceCharge overseasInsuranceCharge = groupHeader.Charges[AUChargeCodeList.Codes.OverseasInsurance];
			AssertNotNull("OverseasInsuranceCharge should not be null", overseasInsuranceCharge);
			AssertEquals("Group Charge is Oversea Insurance", AUChargeCodeList.Codes.OverseasInsurance, overseasInsuranceCharge.J7_ChargeType);
			AssertEquals("Group Charge's Oversea Insurance Amount", new ZDecimal(invoice.JZ_Calc_FOBAmount / 400m).Round(2), overseasInsuranceCharge.J7_Amount);
			AssertEquals("Group Charge's Oversea Insurance Currency", invoice.Invoice_Currency.RX_Code, overseasInsuranceCharge.J7_RX_NKCurrency);
			GroupInvoiceCharge overseasFreightCharge = groupHeader.Charges[AUChargeCodeList.Codes.OverseasFreight];
			AssertNotNull("OverseasFreightCharge should not be null", overseasFreightCharge);
			AssertEquals("Group Charge is Oversea Freight", AUChargeCodeList.Codes.OverseasFreight, overseasFreightCharge.J7_ChargeType);
			AssertEquals("Group Charge's Oversea Freight Amount", 0m, overseasFreightCharge.J7_Amount);
			AssertEquals("Group Charge's Oversea Freight Currency", GlbCompany.CurrentCompany.LocalCurrency.RX_Code, overseasFreightCharge.J7_RX_NKCurrency);
		}

		public void TestCreateCommercialInvoiceDetails_EmptyOriginAndCurrency()
		{
			HouseBill.CS_RX_NKGoodsCurrency = ZString.Empty;
			HouseBill.CS_RL_NKOrigin = ZString.Empty;
			Factory.Save();
			TNTDeclarationFromAirCargoCreator creator = new TNTDeclarationFromAirCargoCreator(HouseBill);
			JobDeclaration declaration = (JobDeclaration)creator.CreateIgnoreWarnings();
			AssertNotNull("Declaration should not be null", declaration);
			AssertEquals("Declaration should have 1 invoice", 1, declaration.Invoices.Count);
			JobComInvoiceHeader invoice = declaration.Invoices[0];
			AssertNotNull("Invoice should not be null", invoice);
			AssertEquals("Invoice Currency", ZString.Empty, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("Invoice Origin", ZString.Empty, invoice.ZA_ORG);
		}

		#endregion
		#region Test Tempoarary Organisation Creation
		public void TestTemporaryConsigneeIsCreatedIfAirCargoIsNotLinkedToAConsignee()
		{
			OrgHeader consignee = HouseBill.Consignee;
			AssertNotNull("PreCondition: Consignee should not be null", consignee);
			HouseBill.CS_OA_ConsigneeAddress = ZGuid.Empty;
			HouseBill.CS_ConsigneeContactName = "DUMMY CONTACT";
			HouseBill.CS_ConsigneeName = consignee.OH_FullName;
			HouseBill.CS_ConsigneeStreet = consignee.MainAddress.OA_Address1;
			HouseBill.CS_ConsigneeCity = consignee.MainAddress.OA_City;
			HouseBill.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			HouseBill.CS_ConsigneeState = "NSW";
			HouseBill.CS_ConsigneePostcode = consignee.MainAddress.OA_PostCode;
			HouseBill.CS_ConsigneePhone = consignee.MainAddress.OA_Phone;
			Factory.Save();
			int consigneeCount = Factory.GetDatabaseCount(typeof(OrgHeader), ConsigneeTempAccountFilter);
			TNTDeclarationFromAirCargoCreator creator = new TNTDeclarationFromAirCargoCreator(HouseBill);
			JobDeclaration declaration = (JobDeclaration)creator.CreateIgnoreWarnings();
			Factory.Save();
			AssertNotNull("Declaration should not be null", declaration);
			AssertEquals("Number of Consignee Temporary Organisation", consigneeCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader), ConsigneeTempAccountFilter));
			AssertNotNull("Declaration's Consignee should not be null", declaration.Consignee);
			AssertEquals("Declaration's Consignee is a temporary organisation", true, declaration.Consignee.OH_IsTempAccount);
			AssertEquals("Declaration's Consignee is mark as Consignee", true, declaration.Consignee.OH_IsConsignee);
			AssertEquals("Declaration's Consignee is not the same as Old Housebill", false, consignee.PK == declaration.Consignee.PK);
			AssertEquals("Declaration's Consignee Name", HouseBill.CS_ConsigneeName, declaration.Consignee.OH_FullName);
			AssertEquals("Declaration's Consignee should have 1 address type", 1, declaration.Consignee.Addresses.Count);
			AssertEquals("Declaration's Consignee Address 1", HouseBill.CS_ConsigneeStreet, declaration.Consignee.MainAddress.OA_Address1);
			AssertEquals("Declaration's Consignee City", HouseBill.CS_ConsigneeCity, declaration.Consignee.MainAddress.OA_City);
			AssertEquals("Declaration's Consignee State", HouseBill.CS_ConsigneeState, declaration.Consignee.MainAddress.OA_State);
			AssertEquals("Declaration's Consignee PostCode", HouseBill.CS_ConsigneePostcode, declaration.Consignee.MainAddress.OA_PostCode);
			AssertEquals("Declaration's Consignee Phone", HouseBill.CS_ConsigneePhone, declaration.Consignee.MainAddress.OA_Phone);
			AssertEquals("Declaration's Consignee Closest Port", "AUSYD", declaration.Consignee.OH_RL_NKClosestPort);
			AssertEquals("Declaration's Consignee should have 1 Contact", 1, declaration.Consignee.Contacts.Count);
			OrgContact contact = declaration.Consignee.Contacts[0];
			AssertNotNull("Declaration's Consignee's Contact should not be null", contact);
			AssertEquals("Declaration's Consignee's Contact Name", HouseBill.CS_ConsigneeContactName, contact.OC_ContactName);
		}

		public void TestTemporaryConsignorIsCreatedIfAirCargoIsNotLinkedToAConsignor()
		{
			OrgHeader consignor = HouseBill.Consignor;
			AssertNotNull("PreCondition: Consignor should not be null", consignor);
			HouseBill.CS_OA_ConsignorAddress = ZGuid.Empty;
			HouseBill.CS_ConsignorContactName = "DUMMY CONTACT";
			HouseBill.CS_ConsignorName = consignor.OH_FullName;
			HouseBill.CS_ConsignorStreet = consignor.MainAddress.OA_Address1;
			HouseBill.CS_ConsignorCity = consignor.MainAddress.OA_City;
			HouseBill.CS_RN_NKConsignorCountry = "KK";
			HouseBill.CS_ConsignorState = consignor.MainAddress.OA_State;
			HouseBill.CS_ConsignorPostcode = consignor.MainAddress.OA_PostCode;
			HouseBill.CS_ConsignorPhone = consignor.MainAddress.OA_Phone;
			Factory.Save();
			int consignorCount = Factory.GetDatabaseCount(typeof(OrgHeader), ConsignorTempAccountFilter);
			TNTDeclarationFromAirCargoCreator creator = new TNTDeclarationFromAirCargoCreator(HouseBill);
			JobDeclaration declaration = (JobDeclaration)creator.CreateIgnoreWarnings();
			Factory.Save();
			AssertNotNull("Declaration should not be null", declaration);
			AssertEquals("Number of Consignor Temporary Organisation", consignorCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader), ConsignorTempAccountFilter));
			AssertNotNull("Declaration's Consignor should not be null", declaration.Consignor);
			AssertEquals("Declaration's Consignor is a temporary organisation", true, declaration.Consignor.OH_IsTempAccount);
			AssertEquals("Declaration's Consignor is mark as Consignor", true, declaration.Consignor.OH_IsConsignor);
			AssertEquals("Declaration's Consignor is not the same as Old Housebill", false, consignor.PK == declaration.Consignor.PK);
			AssertEquals("Declaration's Consignor Name", HouseBill.CS_ConsignorName, declaration.Consignor.OH_FullName);
			AssertEquals("Declaration's Consignor should have 1 address type", 1, declaration.Consignor.Addresses.Count);
			AssertEquals("Declaration's Consignor Address 1", HouseBill.CS_ConsignorStreet, declaration.Consignor.MainAddress.OA_Address1);
			AssertEquals("Declaration's Consignor City", HouseBill.CS_ConsignorCity, declaration.Consignor.MainAddress.OA_City);
			AssertEquals("Declaration's Consignor State", HouseBill.CS_ConsignorState, declaration.Consignor.MainAddress.OA_State);
			AssertEquals("Declaration's Consignor PostCode", HouseBill.CS_ConsignorPostcode, declaration.Consignor.MainAddress.OA_PostCode);
			AssertEquals("Declaration's Consignor Phone", HouseBill.CS_ConsignorPhone, declaration.Consignor.MainAddress.OA_Phone);
			AssertEquals("Declaration's Consignor Closest Port", "KKZZZ", declaration.Consignor.OH_RL_NKClosestPort);
			AssertEquals("Declaration's Consignor should have 1 Contact", 1, declaration.Consignor.Contacts.Count);
			OrgContact contact = declaration.Consignor.Contacts[0];
			AssertNotNull("Declaration's Consignor's Contact should not be null", contact);
			AssertEquals("Declaration's Consignor's Contact Name", HouseBill.CS_ConsignorContactName, contact.OC_ContactName);
		}

		public void TestTemporaryConsigneeIsCreatedWithExtraAddress()
		{
			OrgHeader consignee = HouseBill.Consignee;
			AssertNotNull("PreCondition: Consignee should not be null", consignee);
			HouseBill.CS_OA_ConsigneeAddress = ZGuid.Empty;
			HouseBill.CS_ConsigneeContactName = "DUMMY CONTACT";
			HouseBill.CS_ConsigneeName = consignee.OH_FullName;
			HouseBill.CS_ConsigneeStreet = consignee.MainAddress.OA_Address1;
			HouseBill.CS_ConsigneeCity = consignee.MainAddress.OA_City;
			HouseBill.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			HouseBill.CS_ConsigneeState = "NSW";
			HouseBill.CS_ConsigneePostcode = consignee.MainAddress.OA_PostCode;
			HouseBill.CS_ConsigneePhone = consignee.MainAddress.OA_Phone;
			JobDocAddress delivery = AddDeliveryOrganisationToAirCargo(HouseBill);
			JobDocAddress pickUp = AddPickUpOrganisationToAirCargo(HouseBill);
			Factory.Save();
			int consigneeCount = Factory.GetDatabaseCount(typeof(OrgHeader), ConsigneeTempAccountFilter);
			TNTDeclarationFromAirCargoCreator creator = new TNTDeclarationFromAirCargoCreator(HouseBill);
			JobDeclaration declaration = (JobDeclaration)creator.CreateIgnoreWarnings();
			Factory.Save();
			AssertNotNull("Declaration should not be null", declaration);
			AssertEquals("Number of Consignee Temporary Organisation", consigneeCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader), ConsigneeTempAccountFilter));
			AssertNotNull("Declaration's Consignee should not be null", declaration.Consignee);
			AssertEquals("Declaration's Consignee is a temporary organisation", true, declaration.Consignee.OH_IsTempAccount);
			AssertEquals("Declaration's Consignee is mark as Consignee", true, declaration.Consignee.OH_IsConsignee);
			AssertEquals("Declaration's Consignee is not the same as Old Housebill", false, consignee.PK == declaration.Consignee.PK);
			AssertEquals("Declaration's Consignee Name", HouseBill.CS_ConsigneeName, declaration.Consignee.OH_FullName);
			AssertEquals("Declaration's Consignee should have 1 address types", 1, declaration.Consignee.Addresses.Count);
			OrgAddressList deliveyList = declaration.Consignee.Addresses.AddressesOfType(OrgAddressType.Delivery);
			AssertEquals("Declaration's Consignee shouldn't have  Delivery Address ", 0, deliveyList.Count);
			AssertEquals("Declaration 's Delivery Address line 1", delivery.E2_Address1, declaration.ImporterDeliveryAddress.E2_Address1);
			AssertEquals("Declaration 's Delivery Address line 2", delivery.E2_Address2, declaration.ImporterDeliveryAddress.E2_Address2);
			AssertEquals("Declaration 's Delivery Address city", delivery.E2_City, declaration.ImporterDeliveryAddress.E2_City);
			AssertEquals("Declaration 's Delivery Address contact", delivery.E2_Contact, declaration.ImporterDeliveryAddress.E2_Contact);
			AssertEquals("Declaration 's Delivery Address postcode", delivery.E2_Postcode, declaration.ImporterDeliveryAddress.E2_Postcode);
			AssertEquals("Declaration 's Delivery Address phone", delivery.E2_Phone, declaration.ImporterDeliveryAddress.E2_Phone);
			AssertEquals("Declaration 's Delivery Address state", delivery.E2_State, declaration.ImporterDeliveryAddress.E2_State);
			AssertEquals("Declaration 's Delivery Address company name", delivery.E2_CompanyName, declaration.ImporterDeliveryAddress.E2_CompanyName);
		}

		public void TestTemporaryConsignorIsCreatedWithExtraAddress()
		{
			OrgHeader consignor = HouseBill.Consignor;
			AssertNotNull("PreCondition: Consignor should not be null", consignor);
			HouseBill.CS_OA_ConsignorAddress = ZGuid.Empty;
			HouseBill.CS_ConsignorContactName = "DUMMY CONTACT";
			HouseBill.CS_ConsignorName = consignor.OH_FullName;
			HouseBill.CS_ConsignorStreet = consignor.MainAddress.OA_Address1;
			HouseBill.CS_ConsignorCity = consignor.MainAddress.OA_City;
			HouseBill.CS_RN_NKConsignorCountry = Core.Constants.CountryCodes.Australia;
			HouseBill.CS_ConsignorState = "NSW";
			HouseBill.CS_ConsignorPostcode = consignor.MainAddress.OA_PostCode;
			HouseBill.CS_ConsignorPhone = consignor.MainAddress.OA_Phone;
			JobDocAddress delivery = AddDeliveryOrganisationToAirCargo(HouseBill);
			JobDocAddress pickUp = AddPickUpOrganisationToAirCargo(HouseBill);
			Factory.Save();
			JobDocAddress pickupAddessForTest = Factory.New<JobDocAddress>();
			pickupAddessForTest = (JobDocAddress)pickUp.Clone();
			int consignorCount = Factory.GetDatabaseCount(typeof(OrgHeader), ConsignorTempAccountFilter);
			TNTDeclarationFromAirCargoCreator creator = new TNTDeclarationFromAirCargoCreator(HouseBill);
			JobDeclaration declaration = (JobDeclaration)creator.CreateIgnoreWarnings();
			Factory.Save();
			AssertNotNull("Declaration should not be null", declaration);
			AssertEquals("Number of Consignor Temporary Organisation", consignorCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader), ConsignorTempAccountFilter));
			AssertNotNull("Declaration's Consignor should not be null", declaration.Consignor);
			AssertEquals("Declaration's Consignor is a temporary organisation", true, declaration.Consignor.OH_IsTempAccount);
			AssertEquals("Declaration's Consignor is mark as Consignor", true, declaration.Consignor.OH_IsConsignor);
			AssertEquals("Declaration's Consignor is not the same as Old Housebill", false, consignor.PK == declaration.Consignor.PK);
			AssertEquals("Declaration's Consignor Name", HouseBill.CS_ConsignorName, declaration.Consignor.OH_FullName);
			AssertEquals("Declaration's Consignor should have 1 address type", 1, declaration.Consignor.Addresses.Count);
			OrgAddressList pickupList = declaration.Consignor.Addresses.AddressesOfType(OrgAddressType.Pickup);
			AssertEquals("Declaration's Consignor shouldn't have Pickup Address", 0, pickupList.Count);
		}

		public void TestTemporaryCreatedWithCorrectClosestPort()
		{
			OrgHeader consignor = HouseBill.Consignor;
			AssertNotNull("PreCondition: Consignor should not be null", consignor);
			RefUNLOCO nZCHCPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZCHC");
			AssertNotNull("PreCondition: NZCHC UNLoco should exist", nZCHCPort);
			HouseBill.CS_OA_ConsignorAddress = ZGuid.Empty;
			HouseBill.CS_ConsignorContactName = "DUMMY CONTACT";
			HouseBill.CS_ConsignorName = consignor.OH_FullName;
			HouseBill.CS_ConsignorStreet = consignor.MainAddress.OA_Address1;
			HouseBill.CS_ConsignorCity = nZCHCPort.RL_PortName;
			HouseBill.CS_RN_NKConsignorCountry = nZCHCPort.RL_RN_NKCountryCode;
			HouseBill.CS_ConsignorState = nZCHCPort.CountryStates.RW_Code;
			HouseBill.CS_ConsignorPostcode = consignor.MainAddress.OA_PostCode;
			HouseBill.CS_ConsignorPhone = consignor.MainAddress.OA_Phone;
			Factory.Save();
			int consignorCount = Factory.GetDatabaseCount(typeof(OrgHeader), ConsignorTempAccountFilter);
			TNTDeclarationFromAirCargoCreator creator = new TNTDeclarationFromAirCargoCreator(HouseBill);
			JobDeclaration declaration = (JobDeclaration)creator.CreateIgnoreWarnings();
			Factory.Save();
			AssertNotNull("Declaration should not be null", declaration);
			AssertEquals("Number of Consignor Temporary Organisation", consignorCount + 1, Factory.GetDatabaseCount(typeof(OrgHeader), ConsignorTempAccountFilter));
			AssertNotNull("Declaration's Consignor should not be null", declaration.Consignor);
			AssertEquals("Declaration's Consignor is a temporary organisation", true, declaration.Consignor.OH_IsTempAccount);
			AssertEquals("Declaration's Consignor is mark as Consignor", true, declaration.Consignor.OH_IsConsignor);
			AssertEquals("Declaration's Consignor is not the same as Old Housebill", false, consignor.PK == declaration.Consignor.PK);
			AssertEquals("Declaration's Consignor Name", HouseBill.CS_ConsignorName, declaration.Consignor.OH_FullName);
			AssertEquals("Declaration's Consignor should have 1 address type", 1, declaration.Consignor.Addresses.Count);
			AssertEquals("Declaration's Consignor Address 1", HouseBill.CS_ConsignorStreet, declaration.Consignor.MainAddress.OA_Address1);
			AssertEquals("Declaration's Consignor City", HouseBill.CS_ConsignorCity, declaration.Consignor.MainAddress.OA_City);
			AssertEquals("Declaration's Consignor State", HouseBill.CS_ConsignorState, declaration.Consignor.MainAddress.OA_State);
			AssertEquals("Declaration's Consignor PostCode", HouseBill.CS_ConsignorPostcode, declaration.Consignor.MainAddress.OA_PostCode);
			AssertEquals("Declaration's Consignor Phone", HouseBill.CS_ConsignorPhone, declaration.Consignor.MainAddress.OA_Phone);
			AssertEquals("Declaration's Consignor Closest Port", nZCHCPort.Code, declaration.Consignor.OH_RL_NKClosestPort);
			AssertEquals("Declaration's Consignor should have 1 Contact", 1, declaration.Consignor.Contacts.Count);
			OrgContact contact = declaration.Consignor.Contacts[0];
			AssertNotNull("Declaration's Consignor's Contact should not be null", contact);
			AssertEquals("Declaration's Consignor's Contact Name", HouseBill.CS_ConsignorContactName, contact.OC_ContactName);
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			HouseBill = Helper.CreateTestHouseBill();
			Factory.Save();
		}

		#region ConsigneeTempAccountFilter
		ZQuery ConsigneeTempAccountFilter
		{
			get
			{
				if (fConsigneeTempAccountFilter == null)
				{
					fConsigneeTempAccountFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True);
					fConsigneeTempAccountFilter.AddToFilter(OrgHeaderSchema.OH_IsTempAccount, ZBool.True);
				}

				return fConsigneeTempAccountFilter;
			}
		}

		ZQuery fConsigneeTempAccountFilter;
		#endregion
		#region ConsignorTempAccountFilter
		ZQuery ConsignorTempAccountFilter
		{
			get
			{
				if (fConsignorTempAccountFilter == null)
				{
					fConsignorTempAccountFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, ZBool.True);
					fConsignorTempAccountFilter.AddToFilter(OrgHeaderSchema.OH_IsTempAccount, ZBool.True);
				}

				return fConsignorTempAccountFilter;
			}
		}

		ZQuery fConsignorTempAccountFilter;
		#endregion
		JobDocAddress AddDeliveryOrganisationToAirCargo(CusHAWB houseBill)
		{
			JobDocAddress delivery = Factory.New<JobDocAddress>();
			delivery.E2_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(CusHAWBSchema.Constants.TableName);
			delivery.E2_ParentID = houseBill.PK;
			delivery.E2_AddressType = DocAddressTypes.Codes.ImporterPickupDeliveryAddress;
			delivery.E2_CompanyName = "DELIVERY COMPANY";
			delivery.E2_Address1 = "DELIVERY ADDRESS 1";
			delivery.E2_Address2 = "DELIVERY ADDRESS 2";
			delivery.E2_City = "CITY";
			delivery.E2_Postcode = "324234";
			delivery.E2_State = "STATE";
			delivery.E2_Phone = "32 23432 2342";
			delivery.E2_Fax = "3423 23432 2343";
			delivery.E2_Email = "adssdf@dummy.com";
			delivery.E2_Contact = "DELIVERY CONTACT";
			delivery.E2_AddressOverride = true;
			return delivery;
		}

		JobDocAddress AddPickUpOrganisationToAirCargo(CusHAWB houseBill)
		{
			JobDocAddress pickUp = Factory.New<JobDocAddress>();
			pickUp.E2_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(CusHAWBSchema.Constants.TableName);
			pickUp.E2_ParentID = houseBill.PK;
			pickUp.E2_AddressType = DocAddressTypes.Codes.SupplierPickupDeliveryAddress;
			pickUp.E2_CompanyName = "PICKUP COMPANY";
			pickUp.E2_Address1 = "PICKUP ADDRESS 1";
			pickUp.E2_Address2 = "PICKUP ADDRESS 2";
			pickUp.E2_City = "CITY";
			pickUp.E2_Postcode = "324234";
			pickUp.E2_State = "STATE";
			pickUp.E2_Phone = "32 23432 2342";
			pickUp.E2_Fax = "3423 23432 2343";
			pickUp.E2_Email = "adssdf@dummy.com";
			pickUp.E2_Contact = "PICKUP CONTACT";
			pickUp.E2_AddressOverride = true;
			return pickUp;
		}

		#region Helper
		ZTestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new ZTestHelper(Factory);
				}

				return fHelper;
			}
		}

		ZTestHelper fHelper;
		#endregion
		CusHAWB HouseBill;
		#endregion
	}
}
