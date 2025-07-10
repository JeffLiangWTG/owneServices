using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobComInvoiceHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNew()
		{
			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			var countrySetup = typeDeciderContextMock.Setup(k => k.Country);
			var typeDecider = new JobComInvoiceHeaderTypeDecider();
			AssertEquals("Type for EU must be Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader", "Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader", typeDecider.GetTypeForNew(typeDeciderContextMock.Object).FullName);

			countrySetup.Returns(Core.Constants.CountryCodes.Congo);
			AssertEquals("Type for CG must be Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader", "Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader", typeDecider.GetTypeForNew(typeDeciderContextMock.Object).FullName);

			countrySetup.Returns(Core.Constants.CountryCodes.Germany);
			AssertEquals("Type for DE must be Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader", ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceHeader>().FullName, typeDecider.GetTypeForNew(typeDeciderContextMock.Object).FullName);
		}

		public void TestGetTypeForBinding()
		{
			var typeDecider = new JobComInvoiceHeaderTypeDecider();
			AssertEquals("Type for EU must be Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader", "Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader", typeDecider.GetTypeForBinding().FullName);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				AssertEquals("Type for CG must be Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader", "Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader", typeDecider.GetTypeForBinding().FullName);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AssertEquals("Type for DE must be Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader", ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceHeader>().FullName, typeDecider.GetTypeForBinding().FullName);
			}
		}
	}
}
