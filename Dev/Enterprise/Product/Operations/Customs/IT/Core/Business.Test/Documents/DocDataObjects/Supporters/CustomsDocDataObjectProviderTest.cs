using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Moq;

namespace Enterprise.Customs.IT.Business.Documents.DocDataObjects.Testing;

sealed class CustomsDocDataObjectProviderTest : TestCaseWithFactory
{
	public void TestGetJobDeclarationDocDataObjectDataProviderForEntry()
	{
		var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		var provider = new CustomsDocDataObjectProviderForTest();

		CombineAssertions(() =>
		{
			var parameters = new Mock<IDocDataObjectParameters>().Object;
			AssertType<EURCertificateOfOriginWrapper>("JobDeclarationDocDataObjectDataProvider (for entry header) Type", provider.GetEURCertificateOfOriginForEntryExposed(entryHeader, parameters));
		});
	}

	public void TestGetEUR1CertificateOfOriginForEntryFromXml()
	{
		var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		var xcoMessage = entryHeader.Messages.AddNew();
		xcoMessage.EM_MessageType = EDIMessageTypeList.Codes.CertificateOfOrigin;
		xcoMessage.EM_MessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.CertificateOfOriginXml.TestFiles.A21ITQS31T0003580T1_Eur1.xml");
		xcoMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("123456");

		var provider = new CustomsDocDataObjectProviderForTest();
		var parametersMock = new Mock<IDocDataObjectParameters>();
		parametersMock.Setup(x => x.DataStoreName).Returns("EUR1 Certificate XML");
		AssertType<XmlEURCertificateOfOriginWrapper>("JobDeclarationDocDataObjectDataProvider (for entry header) Type", provider.GetEURCertificateOfOriginForEntryExposed(entryHeader, parametersMock.Object));
	}

	public void TestGetEURMEDCertificateOfOriginForEntryFromXml()
	{
		var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		var xcoMessage = entryHeader.Messages.AddNew();
		xcoMessage.EM_MessageType = EDIMessageTypeList.Codes.CertificateOfOrigin;
		xcoMessage.EM_MessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.CertificateOfOriginXml.TestFiles.C21ITQVG2T0000060E1_Eur1Med.xml");
		xcoMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("123456");

		var provider = new CustomsDocDataObjectProviderForTest();
		var parametersMock = new Mock<IDocDataObjectParameters>();
		parametersMock.Setup(x => x.DataStoreName).Returns("EURMED Certificate XML");
		AssertType<XmlEURCertificateOfOriginWrapper>("JobDeclarationDocDataObjectDataProvider (for entry header) Type", provider.GetEURCertificateOfOriginForEntryExposed(entryHeader, parametersMock.Object));
	}

	public void TestGetATRCertificateDataObjectForEntry()
	{
		var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		var provider = new CustomsDocDataObjectProviderForTest();

		CombineAssertions(() =>
		{
			var parameters = new Mock<IDocDataObjectParameters>().Object;
			AssertType<ITATRCertificateOfOriginWrapper>("ITATRCertificateOfOriginWrapper (for entry header) Type", provider.GetATRCertificateOfOriginForEntryExposed(entryHeader, parameters));
		});
	}

	public void TestGetATRCertificateOfOriginForEntryFromXml()
	{
		var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		var xcoMessage = entryHeader.Messages.AddNew();
		xcoMessage.EM_MessageType = EDIMessageTypeList.Codes.CertificateOfOrigin;
		xcoMessage.EM_MessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.CertificateOfOriginXml.TestFiles.B21ITQS31T0012765E0_Atr.xml");
		xcoMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("123456");

		var provider = new CustomsDocDataObjectProviderForTest();
		var parametersMock = new Mock<IDocDataObjectParameters>();
		parametersMock.Setup(x => x.DataStoreName).Returns("ATR Certificate XML");
		AssertType<XmlATRCertificateOfOriginWrapper>("ATRCertificateDocDataObject (for entry header) Type", provider.GetATRCertificateOfOriginForEntryExposed(entryHeader, parametersMock.Object));
	}

	public void TestDV1CertificateForEntryType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var provider = new CustomsDocDataObjectProviderForTest();

		var parameters = new DocDataObjectParameters("DataObjectParameters", "a");

		var dV1CertificateWithParameters = provider.GetDV1CertificateForEntryExposed(entryHeader, parameters);
		AssertType<EU.Business.Documents.CertificateOfOrigin.DV1CertificateWrapper>("DV1Certificate type", dV1CertificateWithParameters);
	}

	sealed class CustomsDocDataObjectProviderForTest : CustomsDocDataObjectProvider
	{
		public EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetEURCertificateOfOriginForEntryExposed(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => GetEURCertificateOfOriginForEntry(entryHeader, parameters);

		public EU.Business.Documents.CertificateOfOrigin.IATRCertificateOfOrigin GetATRCertificateOfOriginForEntryExposed(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => GetATRCertificateOfOriginForEntry(entryHeader, parameters);

		public EU.Business.Documents.CertificateOfOrigin.IDV1Certificate GetDV1CertificateForEntryExposed(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => GetDV1CertificateForEntry(entryHeader, parameters);
	}
}
