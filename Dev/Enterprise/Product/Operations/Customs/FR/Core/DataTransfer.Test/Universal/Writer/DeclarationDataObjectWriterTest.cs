using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
{
	class DeclarationDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestCommercialInvoiceHeaderDataObjectWriterType()
		{
			((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var writer = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration)));
			writer.GetDataObject(declaration);
			AssertType<CommercialInvoiceHeaderDataObjectWriter>("CommercialInvoiceHeader writer type", writer.GetNewCommercialInvoiceHeaderDataObjectWriterExposed(entryHeader));
		}

		public void TestCustomsEntryHeaderDataObjectWriterType()
		{
			((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			var declaration = Factory.New<JobDeclaration>();
			var writer = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration)));
			writer.GetDataObject(declaration);
			AssertType<CustomsEntryHeaderDataObjectWriter>("CustomsEntryHeader writer type", writer.GetNewCustomsEntryHeaderDataObjectWriterExposed());
		}

		public void TestBothModelViewAndBaseEUAddInfoAreWritten()
		{
			((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.JE_AirRouteType = AirRouteTypeList.Codes._6_CountryDOMDir;
			declaration.JE_TariffType = CusTariffTypes.ImportTariff;
			var writer = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			CombineAssertions(() =>
			{
				AssertEquals("VATDeferType", VATProcedureList.Codes._2, declarationData.AddInfoCollection.GetZStringValue("VATDeferType"));
				AssertEquals("AirRouteType", AirRouteTypeList.Codes._6_CountryDOMDir, declarationData.AddInfoCollection.GetZStringValue("AirRouteType"));
				AssertEquals("TariffType", CusTariffTypes.ImportTariff, declarationData.AddInfoCollection.GetZStringValue("TariffType"));
			});
		}
	}

	class DeclarationDataObjectWriterForTest : DeclarationDataObjectWriter
	{
		public DeclarationDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
		{
		}

		public Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriterExposed(Customs.Business.CusEntryHeader relatedEntry) => GetNewCommercialInvoiceHeaderDataObjectWriter(relatedEntry);

		public Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriterExposed() => GetNewCustomsEntryHeaderDataObjectWriter();
	}
}
