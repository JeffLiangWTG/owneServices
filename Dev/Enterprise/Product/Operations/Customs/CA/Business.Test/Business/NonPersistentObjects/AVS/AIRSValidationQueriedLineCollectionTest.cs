using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Services;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AIRSValidationQueriedLineCollectionTest : TestCaseWithFactory
	{
		public void TestAddNewOrUpdateExisting()
		{
			RegistrationNumberHelperTest.PrepareGlobalData(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001001";
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			var invoice = declaration.Invoices.AddNew();

			var collection = new AIRSValidationQueriedLineCollection();
			var invoiceLine1 = CreateJobComInvoiceLine(invoice, "020500111", "21247", "14", "5127", "BC", "US", "TN", "69", "34", "14", "28");
			collection.AddNewOrUpdateExistingOGD(invoiceLine1);
			AssertEquals("1 AIRSValidationQueriedLine should be added", 1, collection.Count);
			var collectionItem0 = (AIRSOGDValidationQueriedLine)collection[0];
			AssertCollectionContains(invoiceLine1.PK, collectionItem0.InvoiceLinePKs);
			AssertEquals("CommodityGroup", "B00001001", collectionItem0.CommodityGroup);
			AssertEquals("Commodity", 1, collectionItem0.Commodity);
			AssertEquals("HSNumber", "020500", collectionItem0.HSNumber);
			AssertEquals("RequirementId", "21247", collectionItem0.RequirementId);
			AssertEquals("RequirementVersion", "14", collectionItem0.RequirementVersion);
			AssertEquals("AirsCode", "005127", collectionItem0.AirsCode);
			AssertEquals("DestinationProvince", "BC", collectionItem0.DestinationProvince);
			AssertEquals("OriginCountry", "US", collectionItem0.OriginCountry);
			AssertEquals("OriginState", "TN", collectionItem0.OriginState);
			AssertEquals("EndUse", "69", collectionItem0.EndUse);
			AssertEquals("Miscellaneous", "34", collectionItem0.Miscellaneous);
			AssertEquals("RegistrationNumbers[0].RegistrationId", "14", collectionItem0.RegistrationNumbers[0].RegistrationId);
			AssertEquals("RegistrationNumbers[1].RegistrationId", "28", collectionItem0.RegistrationNumbers[1].RegistrationId);

			var invoiceLine2 = CreateJobComInvoiceLine(invoice, "020500", "21247", "14", "05127", "BC", "US", "TN", "69", "34", "28", "14");
			collection.AddNewOrUpdateExistingOGD(invoiceLine2);
			AssertEquals("Duplication AIRSValidationQueriedLine should not be added", 1, collection.Count);
			AssertCollectionContains(invoiceLine2.PK, collection[0].InvoiceLinePKs);

			var invoiceLine3 = CreateJobComInvoiceLine(invoice, "020500", "21248", "14", "5127", "BC", "US", "TN", "69", "34", "14", "28");
			collection.AddNewOrUpdateExistingOGD(invoiceLine3);
			AssertEquals("1 AIRSValidationQueriedLine should be added", 2, collection.Count);
			var collectionItem1 = (AIRSOGDValidationQueriedLine)collection[1];
			AssertCollectionContains(invoiceLine3.PK, collectionItem1.InvoiceLinePKs);
			AssertEquals("RequirementId", 2, collectionItem1.Commodity);
			AssertEquals("RequirementId", "21248", collectionItem1.RequirementId);

			var invoiceLine4 = CreateJobComInvoiceLine(invoice, "020500", "21248", "14", "5127", "BC", "US", "TN", "69", "34", "14", "28", "36");
			collection.AddNewOrUpdateExistingOGD(invoiceLine4);
			AssertEquals("1 AIRSValidationQueriedLine should be added", 3, collection.Count);
			var collectionItem2 = (AIRSOGDValidationQueriedLine)collection[2];
			AssertCollectionContains(invoiceLine4.PK, collectionItem2.InvoiceLinePKs);
			AssertEquals("RequirementId", 3, collectionItem2.Commodity);
			AssertEquals("RegistrationNumbers[0].RegistrationId", "14", collectionItem2.RegistrationNumbers[0].RegistrationId);
			AssertEquals("RegistrationNumbers[1].RegistrationId", "28", collectionItem2.RegistrationNumbers[1].RegistrationId);
			AssertEquals("RegistrationNumbers[2].RegistrationId", "36", collectionItem2.RegistrationNumbers[2].RegistrationId);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceLine5 = CreateJobComInvoiceLine(invoice, "020500", "21248", "14", "5127", "BC", "US", "TN", "69", "34", "14", "28", "36");
			var cfia = CreateCFIA(invoiceLine5, "AB", "CA", "BC", "01", "01", "123");
			var regNum1 = cfia.AIRSRegistrationNumbers.AddNew();
			regNum1.CY_Code = "A01";
			var regNum2 = cfia.AIRSRegistrationNumbers.AddNew();
			regNum2.CY_Code = "7001";
			var regNum3 = cfia.AIRSRegistrationNumbers.AddNew();
			regNum3.CY_Code = "7002";
			var lpco1 = cfia.LPCOViews.AddNew();
			lpco1.CLP_Type = "5001";
			var lpco2 = cfia.LPCOViews.AddNew();
			lpco2.CLP_Type = "5002";
			var iidCollection = new AIRSValidationQueriedLineCollection();
			iidCollection.AddNewOrUpdateExistingIID(invoiceLine5);
			AssertEquals("1 AIRSValidationQueriedLine should be added", 1, iidCollection.Count);
			var collectionItemIID = (AIRSIIDValidationQueriedLine)iidCollection[0];
			AssertCollectionContains(invoiceLine5.PK, collectionItemIID.InvoiceLinePKs);
			AssertEquals("CommodityGroup", "B00001001", collectionItemIID.CommodityGroup);
			AssertEquals("Commodity", 1, collectionItemIID.Commodity);
			AssertEquals("HSNumber", "020500", collectionItemIID.HSNumber);
			AssertEquals("AirsCode", "000123", collectionItemIID.AirsCode);
			AssertEquals("DeliveryPartyProvince", "AB", collectionItemIID.DeliveryPartyProvince);
			AssertEquals("OriginCountry", "CA", collectionItemIID.OriginCountry);
			AssertEquals("OriginState", "BC", collectionItemIID.OriginState);
			AssertEquals("EndUse", "01", collectionItemIID.EndUse);
			AssertEquals("Miscellaneous", "01", collectionItemIID.Miscellaneous);
			AssertEquals("5 RegistrationNumbers should be added", 5, collectionItemIID.RegistrationNumbers.Count);
			AssertEquals("RegistrationNumbers[0].RegistrationId", "A01", collectionItemIID.RegistrationNumbers[0].RegistrationId);
			AssertEquals("RegistrationNumbers[0].RegistrationType", AIRSValidationQueriedLineRegistrationTypes.Normal, collectionItemIID.RegistrationNumbers[0].RegistrationType);
			AssertEquals("RegistrationNumbers[1].RegistrationId", "7001", collectionItemIID.RegistrationNumbers[1].RegistrationId);
			AssertEquals("RegistrationNumbers[2].RegistrationId", "7002", collectionItemIID.RegistrationNumbers[2].RegistrationId);
			AssertEquals("RegistrationNumbers[3].RegistrationId", "5001", collectionItemIID.RegistrationNumbers[3].RegistrationId);
			AssertEquals("RegistrationNumbers[4].RegistrationId", "5002", collectionItemIID.RegistrationNumbers[4].RegistrationId);
		}

		internal static JobComInvoiceLine CreateJobComInvoiceLine(JobComInvoiceHeader invoice, string hsNumber, string requirementId, string requirementVersion,
			string airsCode, string destinationProvince, string originCountry, string originState, string endUse, string miscellaneous, params string[] registrationIds)
		{
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = hsNumber;
			invoiceLine.CA_RequirementID = requirementId;
			invoiceLine.CA_RequirementVer = requirementVersion;
			invoiceLine.CA_AirsCode = airsCode;
			invoiceLine.CA_DestinationProvince = destinationProvince;
			invoiceLine.CA_RN_NKCFIAOrigin = originCountry;
			invoiceLine.CA_CFIAUSStateOfOrigin = originState;
			invoiceLine.CA_EndUse = endUse;
			invoiceLine.CA_MiscID = miscellaneous;
			foreach (var registrationId in registrationIds)
			{
				invoiceLine.CFIARegistrationNumbers.AddNew().CY_Code = registrationId;
			}
			return invoiceLine;
		}

		CFIAPGAHeader CreateCFIA(JobComInvoiceLine invoiceLine, string province, string sourceCountry, string sourceState, string endUse, string miscellaneous, string airsCode, params string[] lpcoTypes)
		{
			OrgAddress address = Factory.New<OrgAddress>();
			address.StateCode = province;
			invoiceLine.JI_OA_ConsigneeAddress = address.PK;

			invoiceLine.CA_CFIAInd = "Y";

			var cfia = invoiceLine.CFIAPGAHeader;
			invoiceLine.CA_RN_NKSource = sourceCountry;
			invoiceLine.CA_StateOfSource = sourceState;
			cfia.CA_AIRSExtensionCode = airsCode;
			cfia.CA_AIRSEndUse = endUse;
			cfia.CA_AIRSMiscellaneous = miscellaneous;
			foreach (var type in lpcoTypes)
			{
				cfia.LPCOViews.AddNew().CLP_Type = type;
			}
			return cfia;
		}
	}
}
