using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using WTG.TestHelpers.Xml;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.ArgentinaOrgCusCodeInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	public class AuthRequestBuilderTest : TestCaseWithFactory
	{
		public void Test_CuitRepresentada()
		{
			var expectedEmptyXml = @"<authRequest>
  <token></token>
  <sign></sign>
  <cuitRepresentada>{cuitRepresentada}</cuitRepresentada>
</authRequest>";

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();

			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetTransactionInfoHelper()).Returns(transactionInfoHelperMock.Object);

			var expectedCuit = "20999999992";

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.BranchAddress, It.IsAny<ZString>(), It.IsAny<ZString>())).Returns("AAAA2099999999-2");
				AssertTestAuthRequestBuilder(expectedCuit);
				transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(It.Is<OrganizationAddress>(target => target == transactionInfo.BranchAddress), CountryCodes.Argentina, OrgCusCodes.CUIT), Times.Once);

				transactionInfoHelperMock.Reset();

				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.BranchAddress, It.IsAny<ZString>(), It.IsAny<ZString>())).Returns("20999999992");
				AssertTestAuthRequestBuilder(expectedCuit);
				transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(It.Is<OrganizationAddress>(target => target == transactionInfo.BranchAddress), CountryCodes.Argentina, OrgCusCodes.CUIT), Times.Once);

				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.BranchAddress, It.IsAny<ZString>(), It.IsAny<ZString>())).Returns("");
				AssertTestAuthRequestBuilder("");

				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.BranchAddress, It.IsAny<ZString>(), It.IsAny<ZString>())).Returns((string)null);
				AssertTestAuthRequestBuilder("");
			}

			void AssertTestAuthRequestBuilder(string expectedValue)
			{
				var authRequestBuilder = new AuthRequestBuilder() as IAuthRequestBuilder;
				var authRequestXml = authRequestBuilder.BuildXML(transactionInfo).ToString();
				XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{cuitRepresentada}", expectedValue), authRequestXml);
			}
		}
	}
}
