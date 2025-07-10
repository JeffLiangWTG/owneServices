using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseLoadingObjectNcmDetailsCollection))]
	class ImportLicenseLoadingObjectNcmDetailsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportLicenseLoadingObjectNcmDetailsCollection>
	{
		public void TestAddNewItems()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "08264406000193";

			var messageBuilder = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = importer.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1";

			var loadedXml = XmlObjectSerializer.Deserialize<respostaconsultali>(ImportLicenseLoadingObjectParentTest.RightXML);
			var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
			var collection = new ImportLicenseLoadingObjectNcmDetailsCollection(new ImportLicenseLoadingObject(importLicenseParent));
			collection.AddNewItems(((listalicompletatype)loadedXml.Item).licompleta[0].GrupoMercadoria.listadetalhencm);
			CombineAssertions(() =>
			{
				AssertEquals("SequencialProductNumber must be", (ZShort)1, collection[0].SequencialProductNumber);
				AssertEquals("ComercialMeasureUnitName must be", "CAIXA", collection[0].ComercialMeasureUnitName);
				AssertEquals("NetWeight must be", 5625m, collection[0].NetWeight);
				AssertEquals("ComercialMerchandiseQuantity must be", 1250m, collection[0].ComercialMerchandiseQuantity);
				AssertEquals("StatisticMerchandiseQuantity must be", 5625m, collection[0].StatisticMerchandiseQuantity);
				AssertEquals("ShipmentTotalValue must be", 20000m, collection[0].ShipmentTotalValue);
				AssertEquals("ProductDescription must be", "1250 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO ROSADO MEIO SECO, ROSE (50% SYRAH - 50% CABERNET SAUVIGNON), MARCA: QUE BELLA RESERVE, SAFRA: 2021, TEOR ALC.: 13,1, LOTE: L-22071, INDICACAO GEOGRAFICA: VALLE CENTRAL", collection[0].ProductDescription);

				AssertEquals("SequencialProductNumber must be", (ZShort)2, collection[1].SequencialProductNumber);
				AssertEquals("ComercialMeasureUnitName must be", "CAIXA", collection[1].ComercialMeasureUnitName);
				AssertEquals("NetWeight must be", 4410m, collection[1].NetWeight);
				AssertEquals("ComercialMerchandiseQuantity must be", 980m, collection[1].ComercialMerchandiseQuantity);
				AssertEquals("StatisticMerchandiseQuantity must be", 4410m, collection[1].StatisticMerchandiseQuantity);
				AssertEquals("ShipmentTotalValue must be", 11760m, collection[1].ShipmentTotalValue);
				AssertEquals("ProductDescription must be", "980 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CARMENERE, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,6, LOTE: L-22073, INDICACAO GEOGRAFICA: VALLE CENTRAL", collection[1].ProductDescription);

				AssertEquals("SequencialProductNumber must be", (ZShort)3, collection[2].SequencialProductNumber);
				AssertEquals("ComercialMeasureUnitName must be", "CAIXA", collection[2].ComercialMeasureUnitName);
				AssertEquals("NetWeight must be", 3780m, collection[2].NetWeight);
				AssertEquals("ComercialMerchandiseQuantity must be", 840m, collection[2].ComercialMerchandiseQuantity);
				AssertEquals("StatisticMerchandiseQuantity must be", 3780m, collection[2].StatisticMerchandiseQuantity);
				AssertEquals("ShipmentTotalValue must be", 10080m, collection[2].ShipmentTotalValue);
				AssertEquals("ProductDescription must be", "840 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CABERNET SAUVIGNON, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,7, LOTE: L-22074, INDICACAO GEOGRAFICA: VALLE CENTRAL", collection[2].ProductDescription);
			});
		}

		#region Implementantion

		protected override ImportLicenseLoadingObjectNcmDetailsCollection GetCollectionToTest()
		{
			return new ImportLicenseLoadingObjectNcmDetailsCollection(new ImportLicenseLoadingObject(Parent));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ImportLicenseLoadingObjectNcmDetails(Factory);
		}

		ImportLicenseLoadingObjectParent Parent => fParent ?? (fParent = new ImportLicenseLoadingObjectParent(Factory.New<JobDeclaration>()));
		ImportLicenseLoadingObjectParent fParent;

		#endregion
	}
}
