using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.DataTransfer.Testing
{
	class EMCSDeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		[TestDate(2018, 3, 19, 12, 12, 12)]
		public void TestMappings()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "EMCS Pack Types");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");

			var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "VQ", "BulkLiquidGas", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Countable, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "KG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));

			Factory.SaveForTesting();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var job = Factory.New<EMCSJobDeclaration>();

				job.JE_OH_Supplier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;
				job.JE_OH_Importer = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
				job.SupplierDocumentaryAddress.OrganisationPK = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;
				job.ImporterDocumentaryAddress.OrganisationPK = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
				job.OwnerDocumentaryAddress.OrganisationPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
				job.DispatchWarehouseDocumentaryAddress.OrganisationPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
				job.DestinationWarehouseDocumentaryAddress.OrganisationPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
				job.CarrierAgentDocumentaryAddress.OrganisationPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;
				job.TransporterDocumentaryAddress.OrganisationPK = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).PK;

				job.JE_DeclarationReference = "EMC123";
				job.JE_MessageType = "EMC";
				job.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				job.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
				job.JE_TransportMode = Core.Constants.TransportModes.Air;
				job.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration;

				job.EADNumber = "EADNO";
				job.JE_EntryStatus = EntryStatusList.Codes.REG;
				job.JE_MessageStatus = Enterprise.Customs.Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
				job.JE_EntryAuthorisationDate = new ZDateTime(2018, 09, 09, 11, 11, 11);
				job.JE_EntrySubmittedDate = new ZDateTime(2018, 09, 09, 10, 10, 10);

				job.JourneyTimeNumericPart = 1;
				job.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Hours;
				job.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingSameMemberStateMovements;
				job.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignor;
				job.ZG_OriginType = EMCSOriginTypeList.Codes.TaxWarehouse;
				job.ZG_DeferredSubmission = EMCSDeferredSubmissionList.Codes.Yes;

				job.InvoiceNumber = "INV1234";
				job.InvoiceDate = new ZDateTime(2018, 09, 09, 09, 09, 09);
				job.JE_DateAtOrigin = new ZDateTime(2018, 09, 09, 12, 12, 12);
				var sad1 = job.ImportSADNumbers.AddNew();
				sad1.CSI_Description = "SAD123";
				var sad2 = job.ImportSADNumbers.AddNew();
				sad2.CSI_Description = "SAD456";
				job.SpecialInstructions = "SpecialInst";

				var officeCode1 = job.CustomsOffices.Cast<EuOfficeCode>().First();
				officeCode1.CY_Code = "CAD";
				officeCode1.CY_Data = "office1";
				var officeCode2 = job.CustomsOffices.AddNew();
				officeCode2.CY_Code = "DEL";
				officeCode2.CY_Data = "office2";
				var officeCode3 = job.CustomsOffices.AddNew();
				officeCode3.CY_Code = "DIS";
				officeCode3.CY_Data = "office3";

				var certificate1 = job.Documents.AddNew();
				certificate1.CSI_Description = "cert1 desc";
				certificate1.CSI_ReferenceNumber = "cert1 ref";
				var certificate2 = job.Documents.AddNew();
				certificate2.CSI_Description = "cert2 desc";
				certificate2.CSI_ReferenceNumber = "cert2 ref";

				var transport1 = job.CusContainers.AddNew();
				transport1.ZG_UnitCode = EMCSTransportUnitCodeList.Codes.Container;
				transport1.CO_ContainerNumber = "transport1";
				transport1.CO_Seal = "seal1";
				transport1.SealDetails = "seal1 details";
				transport1.Comment = "seal1 comment";
				var transport2 = job.CusContainers.AddNew();
				transport2.ZG_UnitCode = EMCSTransportUnitCodeList.Codes.Tractor;
				transport2.CO_ContainerNumber = "transport2";
				transport2.CO_Seal = "seal2";
				transport2.SealDetails = "seal2 details";
				transport2.Comment = "seal2 comment";

				var classificationJonno = Factory.New<CusClassification>();
				classificationJonno.FillWithValidTestData();
				classificationJonno.CC_TariffNum = "0002.02.02 1";
				classificationJonno.CC_LookupCode = "JONNO";
				classificationJonno.CC_ClassificationType = "BTH";

				var invoice = job.Invoices.Cast<EMCSJobComInvoiceHeader>().First();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_LineNo = 1;
				invoiceLine1.JI_BrandName = "brand";
				invoiceLine1.ZG_ExciseProductCode = "TX";
				invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				invoiceLine1.JI_CustomsQuantity = 100m;
				invoiceLine1.JI_CustomsUnitQty = EMCSCustomsQuantityTypeList.Codes.FifteenLitre;
				invoiceLine1.JI_NDescription = "line desc";
				invoiceLine1.JI_InvoiceQuantity = 111m;
				invoiceLine1.JI_InvoiceUQ = "VQ";
				invoiceLine1.JI_NetWeight = 121m;
				invoiceLine1.JI_NetWeightUQ = "KG";
				invoiceLine1.JI_PartNo = "partno";
				invoiceLine1.JI_Weight = 122m;
				invoiceLine1.JI_WeightUQ = "KG";
				invoiceLine1.JI_CC = classificationJonno.PK;

				invoiceLine1.ZG_FiscalMark = "mark";
				invoiceLine1.ZG_FiscalMarkUsed = ZBool.True;
				invoiceLine1.ZG_AlcoholicStrength = 1m;
				invoiceLine1.ZG_DegreePlato = 2m;
				invoiceLine1.ZG_SizeOfProducer = 3m;
				invoiceLine1.ZG_Density = 4m;
				invoiceLine1.ZG_WineCategory = EMCSWineCategoryList.Codes.ImportedWine;
				invoiceLine1.ZG_GrowingZone = EMCSGrowingZoneList.Codes.A;
				invoiceLine1.ZG_WineCountryOrigin = Core.Constants.CountryCodes.Canada;
				invoiceLine1.JI_WineDetailsComments = "Wine Details Comments";

				invoiceLine1.OperationCodeDataCollection.AddNew("123");
				invoiceLine1.OperationCodeDataCollection.AddNew("456");

				Factory.SaveForTesting();

				var writer = new EMCSDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, job)));
				var jobData = writer.GetDataObject(job);

				var invoiceData = jobData.CommercialInfo.CommercialInvoiceCollection.Single();
				CombineAssertions(() =>
				{
					AssertEquals("jobData/DataContext/DataSourceCollection/DataSource/Type", nameof(DataContextType.EMCSJobDeclaration), jobData.DataContext.DataSourceCollection.First().Type);
					AssertEquals("jobData/DataContext/DataSourceCollection/DataSource/Key", "EMC123", jobData.DataContext.DataSourceCollection.First().Key);
					AssertEquals("jobData/Branch/Code", "BNE", jobData.Branch.Code);
					AssertEquals("jobData/Branch/Name", "BN - AUBNE", jobData.Branch.Name);

					AssertEquals("jobData/EntryStatus/Code", "REG", jobData.EntryStatus.Code);
					AssertEquals("jobData/EntryStatus/Description", "e-AD Registered", jobData.EntryStatus.Description);
					AssertEquals("jobData/MessageStatus/Code", "AWO", jobData.MessageStatus.Code);
					AssertNull("jobData/MessageStatus/Description", jobData.MessageStatus.Description);
					AssertEquals("jobData/MessageSubType/Code", "1", jobData.MessageSubType.Code);
					AssertEquals("jobData/MessageSubType/Description", "Destination - Tax Warehouse", jobData.MessageSubType.Description);
					AssertEquals("jobData/MessageType/Code", "EMC", jobData.MessageType.Code);
					AssertNull("jobData/MessageType/Description", jobData.MessageType.Description);
					AssertEquals("jobData/MessagingApplicationCode/Code", "EMC", jobData.MessagingApplicationCode.Code);
					AssertEquals("jobData/MessagingApplicationCode/Description", "EMC", jobData.MessagingApplicationCode.Description);
					AssertEquals("jobData/PaymentMethod/Code", DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration, jobData.PaymentMethod.Code);
					AssertEquals("jobData/PaymentMethod/Description", DefermentMethodList.Descriptions.ConsigneesAccountConsigneeCompletingTheDeclaration, jobData.PaymentMethod.Description);
					AssertEquals("jobData/TransportMode/Code", "AIR", jobData.TransportMode.Code);
					AssertEquals("jobData/TransportMode/Description", "Air Freight", jobData.TransportMode.Description);
					AssertEquals("jobData/DeclarantType/Code", "1", jobData.DeclarantType.Code);
					AssertEquals("jobData/DeclarantType/Description", "Consignor", jobData.DeclarantType.Description);

					AssertEquals("jobData/AddInfoCollection/AddInfo[Key='JourneyTime']/Value", "1H", jobData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "JourneyTime").Value);
					AssertEquals("jobData/AddInfoCollection/AddInfo[Key='TransportArrangement']/Value", "1", jobData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "TransportArrangement").Value);
					AssertEquals("jobData/AddInfoCollection/AddInfo[Key='OriginType']/Value", "1", jobData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "OriginType").Value);
					AssertEquals("jobData/AddInfoCollection/AddInfo[Key='DeferredSubmission']/Value", EMCSDeferredSubmissionList.Codes.Yes, jobData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "DeferredSubmission").Value);
					AssertEquals("jobData/AddInfoCollection/AddInfo[Key='GuarantorType']/Value", EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingSameMemberStateMovements, jobData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "GuarantorType").Value);

					AssertEquals("jobData/NoteCollection/Note[Description='Special Instructions']/NoteText", "SpecialInst", jobData.NoteCollection.Single(x => x.Description.GetValueOrDefault() == "Special Instructions").NoteText);

					var importSadsData = jobData.CustomsSupportingInformationCollection.Where(x => x.Type.Code.GetValueOrDefault() == "SAD").ToArray();
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='SAD'] Count", 2, importSadsData.Length);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='SAD'][0]/Category/Description", "Import SAD", importSadsData[0].Category.Description);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='SAD'][0]/Description", "SAD123", importSadsData[0].Description);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='SAD'][1]/Category/Description", "Import SAD", importSadsData[1].Category.Description);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='SAD'][1]/Description", "SAD456", importSadsData[1].Description);

					var customsOfficesData = jobData.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == "EUO").ToArray();
					AssertEquals("jobData/CustomsReferenceCollection/CustomsReference", 3, customsOfficesData.Length);
					AssertEquals("jobData/CustomsReferenceCollection[0].SubType.Code", "CAD", customsOfficesData[0].SubType.Code);
					AssertEquals("jobData/CustomsReferenceCollection[0].Reference", "office1", customsOfficesData[0].Reference);
					AssertEquals("jobData/CustomsReferenceCollection[1].SubType.Code", "DEL", customsOfficesData[1].SubType.Code);
					AssertEquals("jobData/CustomsReferenceCollection[1].Reference", "office2", customsOfficesData[1].Reference);
					AssertEquals("jobData/CustomsReferenceCollection[2].SubType.Code", "DIS", customsOfficesData[2].SubType.Code);
					AssertEquals("jobData/CustomsReferenceCollection[2].Reference", "office3", customsOfficesData[2].Reference);

					var certificatesData = jobData.CustomsSupportingInformationCollection.Where(x => x.Type.Code.GetValueOrDefault() == "CER").ToArray();
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='CER'] Count", 2, certificatesData.Length);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='CER'][0]/Category/Description", "Certificate", certificatesData[0].Category.Description);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='CER'][0]/Description", "cert1 desc", certificatesData[0].Description);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='CER'][0]/ReferenceNumber", "cert1 ref", certificatesData[0].ReferenceNumber);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='CER'][1]/Category/Description", "Certificate", certificatesData[1].Category.Description);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='CER'][1]/Description", "cert2 desc", certificatesData[1].Description);
					AssertEquals("jobData/CustomsSupportingInformationCollection/CustomsSupportingInformation[Type/Code='CER'][1]/ReferenceNumber", "cert2 ref", certificatesData[1].ReferenceNumber);

					var transportDetailsData = jobData.ContainerCollection.ToArray();
					AssertEquals("jobData/ContainerCollection Count", 2, transportDetailsData.Length);
					AssertEquals("jobData/ContainerCollection[0]/AddInfoCollection[Key='UnitCode']/Value", EMCSTransportUnitCodeList.Codes.Container, transportDetailsData[0].AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "UnitCode").Value);
					AssertEquals("jobData/ContainerCollection[0]/ContainerNumber", "transport1", transportDetailsData[0].ContainerNumber);
					AssertEquals("jobData/ContainerCollection[0]/Seal", "seal1", transportDetailsData[0].Seal);
					AssertEquals("jobData/ContainerCollection[0]/AddInfoCollection[Key='SealDetails']/Value", "seal1 details", transportDetailsData[0].AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "SealDetails").Value);
					AssertEquals("jobData/ContainerCollection[0]/AddInfoCollection[Key='Comment']/Value", "seal1 comment", transportDetailsData[0].AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "Comment").Value);

					AssertEquals("jobData/ContainerCollection[1]/AddInfoCollection[Key='UnitCode']/Value", EMCSTransportUnitCodeList.Codes.Tractor, transportDetailsData[1].AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "UnitCode").Value);
					AssertEquals("jobData/ContainerCollection[1]/AddInfoCollection[Key='TransportID']/Value", "transport2", transportDetailsData[1].ContainerNumber);
					AssertEquals("jobData/ContainerCollection[1]/AddInfoCollection[Key='SealNumber']/Value", "seal2", transportDetailsData[1].Seal);
					AssertEquals("jobData/ContainerCollection[1]/AddInfoCollection[Key='SealDetails']/Value", "seal2 details", transportDetailsData[1].AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "SealDetails").Value);
					AssertEquals("jobData/ContainerCollection[1]/AddInfoCollection[Key='Comment']/Value", "seal2 comment", transportDetailsData[1].AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "Comment").Value);

					AssertEquals("jobData/DateCollection/Date[Type='Dispatch']", ZBool.False, jobData.DateCollection.Single(x => x.Type.GetValueOrDefault() == DateType.Departure).IsEstimate);
					AssertEquals("jobData/DateCollection/Date[Type='Dispatch']", new ZDateTime(2018, 09, 09, 12, 12, 12), jobData.DateCollection.Single(x => x.Type.GetValueOrDefault() == DateType.Departure).Value);
					AssertEquals("jobData/DateCollection/Date[Type='EntryAuthorisation']", ZBool.False, jobData.DateCollection.Single(x => x.Type.GetValueOrDefault() == DateType.EntryAuthorisation).IsEstimate);
					AssertEquals("jobData/DateCollection/Date[Type='EntryAuthorisation']", new ZDateTime(2018, 09, 09, 11, 11, 11), jobData.DateCollection.Single(x => x.Type.GetValueOrDefault() == DateType.EntryAuthorisation).Value);

					AssertEquals("invoiceData.InvoiceNumber", "INV1234", invoiceData.InvoiceNumber);
					AssertEquals("invoiceData.InvoiceDate", new ZDateTime(2018, 09, 09, 09, 09, 09), invoiceData.InvoiceDate);
					AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);
					var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
					AssertEquals("invoiceLineData.LineNo", 1, invoiceLineData.LineNo);
					AssertEquals("invoiceLineData.BrandName", "brand", invoiceLineData.BrandName);
					AssertEquals("invoiceLineData.ClassificationCode", "JONNO", invoiceLineData.ClassificationCode);
					AssertEquals("invoiceLineData.CountryOfOrigin.Code", "AU", invoiceLineData.CountryOfOrigin.Code);
					AssertEquals("invoiceLineData.CountryOfOrigin.Name", "Australia", invoiceLineData.CountryOfOrigin.Name);
					AssertEquals("invoiceLineData.CustomsQuantity", 100m, invoiceLineData.CustomsQuantity);
					AssertEquals("invoiceLineData.CustomsQuantityUnit.Code", EMCSCustomsQuantityTypeList.Codes.FifteenLitre, invoiceLineData.CustomsQuantityUnit.Code);
					AssertEquals("invoiceLineData.CustomsQuantityUnit.Description", EMCSCustomsQuantityTypeList.Descriptions.FifteenLitre, invoiceLineData.CustomsQuantityUnit.Description);
					AssertEquals("invoiceLineData.Description", "line desc", invoiceLineData.LocalDescription);
					AssertEquals("invoiceLineData.HarmonisedCode", "000202021", invoiceLineData.HarmonisedCode);
					AssertEquals("invoiceLineData.InvoiceQuantity", 111m, invoiceLineData.InvoiceQuantity);
					AssertEquals("invoiceLineData.InvoiceQuantityUnit.Code", "VQ", invoiceLineData.InvoiceQuantityUnit.Code);
					AssertEquals("invoiceLineData.InvoiceQuantityUnit.Description", "BulkLiquidGas", invoiceLineData.InvoiceQuantityUnit.Description);
					AssertEquals("invoiceLineData.NetWeight", 121m, invoiceLineData.NetWeight);
					AssertEquals("invoiceLineData.NetWeightUnit.Code", "KG", invoiceLineData.NetWeightUnit.Code);
					AssertEquals("invoiceLineData.NetWeightUnit.Description", "KG", invoiceLineData.NetWeightUnit.Description);
					AssertEquals("invoiceLineData.PartNo", "partno", invoiceLineData.PartNo);
					AssertEquals("invoiceLineData.Weight", 122m, invoiceLineData.Weight);
					AssertEquals("invoiceLineData.WeightUnit.Code", "KG", invoiceLineData.WeightUnit.Code);
					AssertEquals("invoiceLineData.WeightUnit.Description", "KG", invoiceLineData.WeightUnit.Description);
					AssertEquals("invoiceLineData.AddInfoCollection/ExciseProductCode", "TX", invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "ExciseProductCode").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/FiscalMark", "mark", invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "FiscalMark").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/FiscalMarkUsed", "Y", invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "FiscalMarkUsed").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/AlcoholicStrength", "1", invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "AlcoholicStrength").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/DegreePlato", "2", invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "DegreePlato").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/SizeOfProducer", "3", invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "SizeOfProducer").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/Density", "4", invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "Density").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/WineCategory", EMCSWineCategoryList.Codes.ImportedWine, invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "WineCategory").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/GrowingZone", EMCSGrowingZoneList.Codes.A, invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "GrowingZone").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/WineCountryOrigin", Core.Constants.CountryCodes.Canada, invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "WineCountryOrigin").Value);
					AssertEquals("invoiceLineData.AddInfoCollection/WineDetailsComments", "Wine Details Comments", invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "WineDetailsComments").Value);

					var operationCodesData = invoiceLineData.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == Business.CusCodeDataTypeList.Codes.WineCode).ToArray();
					AssertEquals("invoiceLineData.CustomsReferenceCollection.Count", 2, operationCodesData.Length);
					AssertEquals("invoiceLineData.CustomsReferenceCollection[0].SubType.Code", "123", operationCodesData[0].SubType.Code);
					AssertEquals("invoiceLineData.CustomsReferenceCollection[1].SubType.Code", "456", operationCodesData[1].SubType.Code);
				});

				AssertOrganizationBO_INTHEMSYD("Supplier", jobData.OrganizationAddressCollection.Single(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.SupplierDocumentaryAddress)), nameof(DocAddressType.SupplierDocumentaryAddress), true);
				AssertOrganizationBO_WUFSHIJNB("Importer", jobData.OrganizationAddressCollection.Single(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ImporterDocumentaryAddress)), nameof(DocAddressType.ImporterDocumentaryAddress), true);
				AssertOrganizationBO_CRAHOLSYD("GoodsOwner", jobData.OrganizationAddressCollection.Single(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.GoodsOwner)), nameof(DocAddressType.GoodsOwner));
				AssertOrganizationBO_CRAHOLSYD("CarrierAgent", jobData.OrganizationAddressCollection.Single(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.CarrierAgent)), nameof(DocAddressType.CarrierAgent));
				AssertOrganizationBO_CRAHOLSYD("Transporter", jobData.OrganizationAddressCollection.Single(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.Transporter)), nameof(DocAddressType.Transporter));
				AssertOrganizationBO_CRAHOLSYD("DispatchWarehouse", jobData.OrganizationAddressCollection.Single(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.DispatchWarehouse)), nameof(DocAddressType.DispatchWarehouse));
				AssertOrganizationBO_CRAHOLSYD("DestinationWarehouse", jobData.OrganizationAddressCollection.Single(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.DestinationWarehouse)), nameof(DocAddressType.DestinationWarehouse));

				AssertOrganizationBO_CRAHOLSYD("invoiceData.Supplier", invoiceData.Supplier, AddressTypes.Supplier);
			}
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
