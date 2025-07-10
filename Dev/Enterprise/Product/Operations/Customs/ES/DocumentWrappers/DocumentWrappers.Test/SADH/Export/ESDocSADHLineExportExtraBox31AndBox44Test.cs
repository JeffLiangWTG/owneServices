using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	sealed class ESDocSADHLineExportExtraBox31AndBox44Test : DocBaseWrapperTest
	{
		public void TestConstructor()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1234.00m;
			invoiceLine.JI_Weight = 160222.7777m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_Procedure = "4000APC";
			invoiceLine.JI_NetWeight = 1005.3465657m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsSecondQuantity = 1005.3465657m;
			invoiceLine.JI_CustomsSecondUnitQty = "ABC";
			invoiceLine.JI_ConcessionOrder = "AA";
			invoiceLine.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];

				ESDocSADHLineExportExtraBox31AndBox44 line = ESDocSADHLineExportExtraBox31AndBox44.New(entryLine, Factory, "Box31", "Box44");
				AssertEquals("Box31PackagesAndDescriptionOfGoods is the first parameter", "Box31", line.Box31PackagesAndDescriptionOfGoods);
				AssertEquals("Box32ItemNumber is filled", 1, line.Box32ItemNumber);
				AssertEquals("Box33CommodityCode Empty", ZString.Empty, line.Box33CommodityCode);
				AssertEquals("Box33ECSupplement Empty", ZString.Empty, line.Box33ECSupplement);
				AssertEquals("Box33ECSupplement2 Empty", ZString.Empty, line.Box33ECSupplement2);
				AssertEquals("Box34CountryOfOrigin Empty", ZString.Empty, line.Box34CountryOfOrigin);
				AssertEquals("Box34StateOfOrigin Empty", ZString.Empty, line.Box34StateOfOrigin);
				AssertEquals("Box35GrossWeightInKG Empty", ZString.Empty, line.Box35GrossWeightInKG);
				AssertEquals("Box37Procedure Empty", ZString.Empty, line.Box37Procedure);
				AssertEquals("Box37_2Procedure Empty", ZString.Empty, line.Box37_2Procedure);
				AssertEquals("Box38NetWeightInKG Empty", ZString.Empty, line.Box38NetWeightInKG);
				AssertEquals("Box39Quota Empty", ZString.Empty, line.Box39Quota);
				AssertEquals("Box40PreviousDocuments Empty", ZString.Empty, line.Box40PreviousDocuments);
				AssertEquals("Box41SupplementaryUnits Empty", ZString.Empty, line.Box41SupplementaryUnits);
				AssertEquals("Box42ItemPrice Empty", ZString.Empty, line.Box42ItemPrice);
				AssertEquals("Box44AddInfoAndDocuments is the second parameter", "Box44", line.Box44AddInfoAndDocuments);
				AssertEquals("ShowBox46StatisticalValue is false", false, line.ShowBox46StatisticalValue);
			});
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			return ESDocSADHLineExportExtraBox31AndBox44.New(entryLine, Factory);
		}
	}
}
