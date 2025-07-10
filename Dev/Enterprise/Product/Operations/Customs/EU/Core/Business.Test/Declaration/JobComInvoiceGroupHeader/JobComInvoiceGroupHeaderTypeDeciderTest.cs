using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobComInvoiceGroupHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNew()
		{
			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			var countrySetup = typeDeciderContextMock.Setup(k => k.Country);
			var typeDecider = new JobComInvoiceGroupHeaderTypeDecider();
			AssertEquals("Type for EU must be Enterprise.Customs.EU.Business.Declaration.JobComInvoiceGroupHeader", "Enterprise.Customs.EU.Business.Declaration.JobComInvoiceGroupHeader", typeDecider.GetTypeForNew(typeDeciderContextMock.Object).FullName);

			countrySetup.Returns(Core.Constants.CountryCodes.Congo);
			AssertEquals("Type for CG must be Enterprise.Customs.EU.Business.Declaration.JobComInvoiceGroupHeader", "Enterprise.Customs.EU.Business.Declaration.JobComInvoiceGroupHeader", typeDecider.GetTypeForNew(typeDeciderContextMock.Object).FullName);

			countrySetup.Returns(Core.Constants.CountryCodes.Germany);
			AssertEquals("Type for DE must be Enterprise.Customs.DE.Business.Declaration.JobComInvoiceGroupHeader", ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceGroupHeader>().FullName, typeDecider.GetTypeForNew(typeDeciderContextMock.Object).FullName);
		}

		public void TestGetTypeForBinding()
		{
			var typeDecider = new JobComInvoiceGroupHeaderTypeDecider();
			AssertEquals("Type for EU must be Enterprise.Customs.EU.Business.Declaration.JobComInvoiceGroupHeader", "Enterprise.Customs.EU.Business.Declaration.JobComInvoiceGroupHeader", typeDecider.GetTypeForBinding().FullName);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				AssertEquals("Type for CG must be Enterprise.Customs.EU.Business.Declaration.JobComInvoiceGroupHeader", "Enterprise.Customs.EU.Business.Declaration.JobComInvoiceGroupHeader", typeDecider.GetTypeForBinding().FullName);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AssertEquals("Type for DE must be Enterprise.Customs.DE.Business.Declaration.JobComInvoiceGroupHeader", ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceGroupHeader>().FullName, typeDecider.GetTypeForBinding().FullName);
			}
		}
	}
}
