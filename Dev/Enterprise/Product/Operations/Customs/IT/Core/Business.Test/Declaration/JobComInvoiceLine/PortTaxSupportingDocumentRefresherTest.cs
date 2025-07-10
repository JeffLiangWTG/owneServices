using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PortTaxSupportingDocumentRefresherTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When parameter is null",
			() => new PortTaxSupportingDocumentRefresher(provider: null));

		var providerMock = new Mock<ISupportingDocumentsWithHarbourRateProvider>();
		AssertExceptionThrown<ArgumentNullException>(
			"When harbourRateProvider is null",
			() => new PortTaxSupportingDocumentRefresher(providerMock.Object));

		providerMock.Setup(x => x.HarbourRateProvider.HarbourRate);
		AssertExceptionThrown<ArgumentNullException>(
			"When SupportingDocumentsMaster is null",
			() => new PortTaxSupportingDocumentRefresher(providerMock.Object));
	}

	[ExpectNoExceptions]
	public void TestRefreshDocument_AlwaysDeleteExisting39YYDocuments()
	{
		portTaxSupportingDocumentRefresher.RefreshDocument();

		supportingDocumentsMock.Verify(x => x.DeleteAllDocumentsHavingCode("39YY"), Times.Once);
	}

	[ExpectNoExceptions]
	public void TestRefreshDocument_Create39YYDocumentIfHarbourRateFound()
	{
		var harbourRate = Factory.New<RefHarbourRate>();
		harbourRate.ZXF_Port = "ITVCE";

		supportingDocumentsProviderWithHarbourRateMock
			.Setup(x => x.HarbourRateProvider.HarbourRate)
			.Returns(harbourRate);

		portTaxSupportingDocumentRefresher.RefreshDocument();

		supportingDocumentsMock.Verify(x => x.AddNew("39YY", "--ITVCE"), Times.Once);
	}

	[ExpectNoExceptions]
	public void TestRefreshDocument_DoesNotCreate39YYDocumentIfHarbourRateNotFound()
	{
		supportingDocumentsProviderWithHarbourRateMock
			.Setup(x => x.HarbourRateProvider.HarbourRate)
			.Returns(value: null);

		portTaxSupportingDocumentRefresher.RefreshDocument();

		supportingDocumentsMock.Verify(x => x.AddNew(It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Never);
	}

	protected override void SetUp()
	{
		base.SetUp();

		supportingDocumentsMock = new Mock<ISupportingDocumentCollection<SupportingDocument>>();

		supportingDocumentsProviderWithHarbourRateMock = new Mock<ISupportingDocumentsWithHarbourRateProvider>();
		supportingDocumentsProviderWithHarbourRateMock
			.Setup(x => x.HarbourRateProvider.HarbourRate)
			.Returns(value: null);
		supportingDocumentsProviderWithHarbourRateMock
			.Setup(x => x.SupportingDocumentsMaster.SupportingDocuments)
			.Returns(supportingDocumentsMock.Object);

		portTaxSupportingDocumentRefresher = new PortTaxSupportingDocumentRefresher(supportingDocumentsProviderWithHarbourRateMock.Object);
	}

	Mock<ISupportingDocumentCollection<SupportingDocument>> supportingDocumentsMock;
	Mock<ISupportingDocumentsWithHarbourRateProvider> supportingDocumentsProviderWithHarbourRateMock;
	PortTaxSupportingDocumentRefresher portTaxSupportingDocumentRefresher;
}
