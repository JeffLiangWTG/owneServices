using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseLoadingObjectNcmDetails))]
	class ImportLicenseLoadingObjectNcmDetailsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadRightXMLFile()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "08264406000193";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = importer.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1";

			using (var rightFile = new MemoryStream(Encoding.UTF8.GetBytes(ImportLicenseLoadingObjectParentTest.RightXML)))
			{
				var messageBuilder = new ZStringBuilder();
				var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
				importLicenseParent.AddLog = (completedCount, totalCount, messageText) => messageBuilder.Append(messageText);
				importLicenseParent.LoadAndValidateXML("Response.xml", rightFile);
				var importLicenseLoadingObject = importLicenseParent.Collection.FirstOrDefault() as ImportLicenseLoadingObject;

				CombineAssertions(() =>
				{
					AssertEquals("SequencialProductNumber must be", (ZShort)1, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[0].SequencialProductNumber);
					AssertEquals("ComercialMeasureUnitName must be", "CAIXA", importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[0].ComercialMeasureUnitName);
					AssertEquals("NetWeight must be", 5625m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[0].NetWeight);
					AssertEquals("ComercialMerchandiseQuantity must be", 1250m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[0].ComercialMerchandiseQuantity);
					AssertEquals("StatisticMerchandiseQuantity must be", 5625m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[0].StatisticMerchandiseQuantity);
					AssertEquals("ShipmentTotalValue must be", 20000m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[0].ShipmentTotalValue);
					AssertEquals("ProductDescription must be", "1250 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO ROSADO MEIO SECO, ROSE (50% SYRAH - 50% CABERNET SAUVIGNON), MARCA: QUE BELLA RESERVE, SAFRA: 2021, TEOR ALC.: 13,1, LOTE: L-22071, INDICACAO GEOGRAFICA: VALLE CENTRAL", importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[0].ProductDescription);

					AssertEquals("SequencialProductNumber must be", (ZShort)2, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[1].SequencialProductNumber);
					AssertEquals("ComercialMeasureUnitName must be", "CAIXA", importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[1].ComercialMeasureUnitName);
					AssertEquals("NetWeight must be", 4410m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[1].NetWeight);
					AssertEquals("ComercialMerchandiseQuantity must be", 980m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[1].ComercialMerchandiseQuantity);
					AssertEquals("StatisticMerchandiseQuantity must be", 4410m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[1].StatisticMerchandiseQuantity);
					AssertEquals("ShipmentTotalValue must be", 11760m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[1].ShipmentTotalValue);
					AssertEquals("ProductDescription must be", "980 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CARMENERE, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,6, LOTE: L-22073, INDICACAO GEOGRAFICA: VALLE CENTRAL", importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[1].ProductDescription);

					AssertEquals("SequencialProductNumber must be", (ZShort)3, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[2].SequencialProductNumber);
					AssertEquals("ComercialMeasureUnitName must be", "CAIXA", importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[2].ComercialMeasureUnitName);
					AssertEquals("NetWeight must be", 3780m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[2].NetWeight);
					AssertEquals("ComercialMerchandiseQuantity must be", 840m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[2].ComercialMerchandiseQuantity);
					AssertEquals("StatisticMerchandiseQuantity must be", 3780m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[2].StatisticMerchandiseQuantity);
					AssertEquals("ShipmentTotalValue must be", 10080m, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[2].ShipmentTotalValue);
					AssertEquals("ProductDescription must be", "840 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CABERNET SAUVIGNON, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,7, LOTE: L-22074, INDICACAO GEOGRAFICA: VALLE CENTRAL", importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection[2].ProductDescription);
				});
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportLicenseLoadingObjectNcmDetails(Factory);
		}

		#endregion
	}
}
