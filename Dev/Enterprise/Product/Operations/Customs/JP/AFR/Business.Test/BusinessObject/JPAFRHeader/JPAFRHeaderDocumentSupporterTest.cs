using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRHeaderDocumentSupporter))]
	sealed class JPAFRHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			AssertEquals("AFR Header cannot be found.", header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(CoreConstants.DataContext.JPAFRHeader), null));
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.JPAFRHeader, header.DocumentSupporter.BusinessContext);
		}

		public void TestDataContexts()
		{
			AssertEquals(true, header.DocumentSupporter.ListOfSupportedDataContexts.ContainsCode(CoreConstants.DataContext.JPAFRHeader));
		}

		public void TestGetDocumentWrappersInternal_JPAFRHeader()
		{
			var wrapper = header.DocumentSupporter.GetDocumentWrappers(CoreConstants.DataContext.JPAFRHeader, null).Single();
			AssertEquals("Enterprise.Customs.JP.AFR.DocumentWrappers.DocJPAFRHeader", wrapper.GetType().FullName);
			AssertEquals(header, wrapper.WrappedObject);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<JPAFRHeader>();

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<JPAFRHeader>();
		}
		JPAFRHeader header;
	}
}
