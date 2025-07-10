using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CustomsDocDataObjectProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetDocDataObjectJobDeclarationDataContext()
		{
			AssertDocDataObjectBasedOnDataContext<JobDeclarationDocDataObject>(DataContext.JobDeclaration);
		}

		[ExpectNoExceptions]
		public void TestGetDocDataObjectEURMEDCertificateDataContext()
		{
			AssertDocDataObjectBasedOnDataContext<JobDeclarationDocDataObject>(DataContext.EURMEDCertificate);
		}

		[ExpectNoExceptions]
		public void TestGetDocDataObjectDV1DataContext()
		{
			AssertDocDataObjectBasedOnDataContext<DV1DocDataObject>(DataContext.DV1Certificate);
		}

		[ExpectNoExceptions]
		public void TestGetDocDataObjectATRCertificateDataContext()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var parameters = new CustomsDocDataObjectProviderParametersTest { DocumentTitle = "ORIGINAL" };

			var dataObject = customsDocDataObjectProvider.GetDocDataObject(entryHeader, DataContext.ATRCertificate, parameters);
			CombineAssertions($"When input is {nameof(CusEntryHeader)}", () => AssertDocDataObject<ATRCertificateDocDataObject>(dataObject));
		}

		[ExpectNoExceptions]
		public void TestGetEURCertificateOfOriginForEntry()
		{
			var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var provider = new CustomsDocDataObjectProviderForTest();
			var parameters = new Mock<IDocDataObjectParameters>().Object;
			NUnit.Framework.Assert.That(provider.GetEURCertificateOfOriginForEntryExposed(entryHeader, parameters), NUnit.Framework.Is.TypeOf<EURCertificateOfOriginWrapper>(), "EUR Certificate of origin wrapper (for entry header) Type");
		}

		[ExpectNoExceptions]
		public void TestGetATRCertificateOfOriginForEntry()
		{
			var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var provider = new CustomsDocDataObjectProviderForTest();
			var parameters = new Mock<IDocDataObjectParameters>().Object;
			NUnit.Framework.Assert.That(provider.GetATRCertificateOfOriginForEntryExposed(entryHeader, parameters), NUnit.Framework.Is.TypeOf<ATRCertificateOfOriginWrapper>(), "ATR Certificate of origin wrapper (for entry header) Type");
		}

		protected override void SetUp()
		{
			base.SetUp();
			customsDocDataObjectProvider = new CustomsDocDataObjectProvider();
		}

		CustomsDocDataObjectProvider customsDocDataObjectProvider;

		#region Implementation

		const string ATRCertificateDocumentTitle = "ATR Certificate";

		[ExpectNoExceptions]
		void AssertDocDataObjectBasedOnDataContext<TDocDataObject>(string dataContext)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var parameters = new CustomsDocDataObjectProviderParametersTest { DocumentTitle = "ORIGINAL" };

			var dataObject = customsDocDataObjectProvider.GetDocDataObject(entryHeader, dataContext, parameters);
			CombineAssertions($"When input is {nameof(CusEntryHeader)}", () => AssertDocDataObject<TDocDataObject>(dataObject));
		}

		[ExpectNoExceptions]
		void AssertDocDataObject<TDocDataObject>(object dataObject)
		{
			NUnit.Framework.Assert.That(dataObject, NUnit.Framework.Is.Not.EqualTo(default(object)), "nameof(dataObject) - should not be [null]");
			NUnit.Framework.Assert.That(dataObject, NUnit.Framework.Is.TypeOf<TDocDataObject>(), $"{nameof(dataObject)} type");
		}

		#endregion
	}

	class CustomsDocDataObjectProviderParametersTest : IDocDataObjectParameters
	{
		public string DocumentTitle { get; set; }
		public string DataStoreName { get; set; }
		public object Data { get; set; }
		public IStmALogProvider LogProvider { get; set; }

		IStmALogProvider IDocDataObjectParameters.LogProvider => LogProvider;
	}

	#region CustomsDocDataObjectProviderForTest

	public class CustomsDocDataObjectProviderForTest : CustomsDocDataObjectProvider
	{
		public IEURCertificateOfOrigin GetEURCertificateOfOriginForEntryExposed(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => GetEURCertificateOfOriginForEntry(entryHeader, parameters);

		public IATRCertificateOfOrigin GetATRCertificateOfOriginForEntryExposed(CusEntryHeader entryHeader, IDocDataObjectParameters parameters) => GetATRCertificateOfOriginForEntry(entryHeader, parameters);
	}

	#endregion
}
