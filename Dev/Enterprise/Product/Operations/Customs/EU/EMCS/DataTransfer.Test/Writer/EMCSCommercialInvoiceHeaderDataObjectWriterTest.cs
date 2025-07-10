using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.DataTransfer.Testing
{
	class EMCSCommercialInvoiceHeaderDataObjectWriterTest : OrganizationAddressTestHelper
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
				var classificationJonno = Factory.New<CusClassification>();
				classificationJonno.FillWithValidTestData();
				classificationJonno.CC_TariffNum = "0002.02.02 1";
				classificationJonno.CC_LookupCode = "JONNO";
				classificationJonno.CC_ClassificationType = "BTH";

				var job = Factory.New<EMCSJobDeclaration>();
				job.JE_DeclarationReference = "EMC123";
				job.JE_MessageType = EMCSEntryTypeList.Codes.Consignor;
				job.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
				job.JE_TransportMode = Core.Constants.TransportModes.Air;

				job.InvoiceNumber = "INV1234";
				job.InvoiceDate = new ZDateTime(2018, 09, 09, 09, 09, 09);

				job.OwnerDocumentaryAddress.OrganisationPK = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;

				var invoice = job.Invoices.Cast<EMCSJobComInvoiceHeader>().First();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_LineNo = 1;
				invoiceLine1.JI_BrandName = "brand";
				invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

				invoiceLine1.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
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

				var writer = new EMCSCommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new EMCSUniversalDataObjectWriterHelper(Factory.BOFactory, ""));
				var invoiceData = writer.GetDataObject(invoice);

				CombineAssertions(() =>
				{
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
					AssertEquals("invoiceLineData.AddInfoCollection/ExciseProductCode", EMCSJobComInvoiceLine.ExciseProductCode_W200, invoiceLineData.AddInfoCollection.Single(x => x.Key.GetValueOrDefault() == "ExciseProductCode").Value);
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

					var operationCodesData = invoiceLineData.CustomsReferenceCollection.Where(x => x.Type.Code.GetValueOrDefault() == CusCodeDataTypeList.Codes.WineCode).ToArray();
					AssertEquals("invoiceLineData.CustomsReferenceCollection.Count", 2, operationCodesData.Length);
					AssertEquals("invoiceLineData.CustomsReferenceCollection[0].SubType.Code", "123", operationCodesData[0].SubType.Code);
					AssertEquals("invoiceLineData.CustomsReferenceCollection[1].SubType.Code", "456", operationCodesData[1].SubType.Code);
				});

				AssertOrganizationBO_INTHEMSYD("invoiceData.Supplier", invoiceData.Supplier, AddressTypes.Supplier);
			}
		}
	}
}
