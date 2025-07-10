using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class InvalidationSenderProviderTest : AESMessageSenderTest<InvalidationSenderProvider, ICC514CDataProvider>
{
	protected override ZString EntryType => BEExportEntryTypeList.Codes.CancellationRequest;

	protected override ZString EntryStatus => StatusCodes.InvalidationRequest;

	protected override void SetUpMockProviderData(Mock<ICC514CDataProvider> mockProvider)
	{
		mockProvider.Setup(m => m.MessageType).Returns("CC514C");
		mockProvider.Setup(m => m.LanguageCode).Returns("NL");
		mockProvider.Setup(m => m.ExportOperation).Returns(Mock.Of<IExportOperation>);
		mockProvider.Setup(m => m.CustomsOfficeOfExportReferenceNumber).Returns("ReferenceNumber");
		mockProvider.Setup(m => m.Exporter).Returns(Mock.Of<IParty>);
		mockProvider.Setup(m => m.Declarant).Returns(Mock.Of<IParty>);
		mockProvider.Setup(m => m.Representative).Returns(Mock.Of<IAESRepresentative>);
	}
}
