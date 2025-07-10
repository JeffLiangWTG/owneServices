using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	class ExportEInvoiceXmlBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<ExportEInvoiceXmlBuilder>(new ArgentinaEInvoiceXmlWriter().ExportEInvoiceXmlBuilder_ExposedForTestOnly);
		}

		public void TestClsFEXAuthRequestBuilder()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { Branch = new Branch() };
			var builderMock = new Mock<IClsFEXAuthRequestBuilder>();
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			var exportEInvoiceBuilder = new ExportEInvoiceXmlBuilder();
			exportEInvoiceBuilder.SubstituteClsFEXAuthRequestBuilder_ForTestOnly(builderMock.Object);
			exportEInvoiceBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);
			var builder = exportEInvoiceBuilder as IEInvoiceXmlBuilder;

			var organizationAddress = new OrganizationAddress();
			var countryCode = CountryCodes.Argentina;
			var taxRegistrationCode = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;

			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()))
				.Callback<OrganizationAddress, ZString, ZString>((x, y, z) =>
				{
					organizationAddress = x;
					countryCode = y;
					taxRegistrationCode = z;
				});

			var actualXmlResult = builder.BuildXml(transaction, accBatch).ToString();
			builderMock.Verify(x => x.BuildXML(transaction, "http://ar.gov.afip.dif.fexv1/", It.IsAny<ZString>()), Times.Once);
			transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Once);

			AssertEquals(transaction.OrganizationAddress, organizationAddress);
			AssertEquals(CountryCodes.Argentina, countryCode);
			AssertEquals(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, taxRegistrationCode);
		}

		public void ClsFEXRequestBuilder()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var builderMock = new Mock<IClsFEXRequestBuilder>();
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			var exportEInvoiceBuilder = new ExportEInvoiceXmlBuilder();
			exportEInvoiceBuilder.SubstituteClsFEXRequestBuilder_ForTestOnly(builderMock.Object);
			exportEInvoiceBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);
			var builder = exportEInvoiceBuilder as IEInvoiceXmlBuilder;

			var organizationAddress = new OrganizationAddress();
			var countryCode = CountryCodes.Argentina;
			var taxRegistrationCode = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;

			transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()))
				.Callback<OrganizationAddress, ZString, ZString>((x, y, z) =>
				{
					organizationAddress = x;
					countryCode = y;
					taxRegistrationCode = z;
				});

			builder.BuildXml(transaction, accBatch);
			builderMock.Verify(x => x.BuildXML(transaction, "http://ar.gov.afip.dif.fexv1/", string.Empty, Factory), Times.Once);

			transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Once);

			AssertEquals(transaction.OrganizationAddress, organizationAddress);
			AssertEquals(CountryCodes.Argentina, countryCode);
			AssertEquals(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, taxRegistrationCode);
		}
	}
}
