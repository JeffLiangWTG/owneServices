using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;
using WTG.TestHelpers.Xml;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class CFDiCancellationBuilderTest : TestCaseWithFactory
	{
		public void Test_RFCEmisor()
		{
			var expectedEmptyXml = @"<Cancelacion>
  <RFCEmisor>{RFCEmisor}</RFCEmisor>
  <RFCReceptor></RFCReceptor>
  <Total>0</Total>
  <UUID></UUID>
  <Certificado></Certificado>
  <ClavePrivada></ClavePrivada>
  <Motivo></Motivo>
</Cancelacion>";

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			(var mexicoDependencyMock, _, _, _) = CreateMexicoDependencyMocks();
			var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, transactionInfoHelperMock);

			var expectedCodeRFC = "RFCEmisor030201001";

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.BranchAddress, It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(expectedCodeRFC);
				AssertTestCFDiCancellationBuilder(expectedCodeRFC);
				transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(transactionInfo.BranchAddress, CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.RFC), Times.Once);

				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.BranchAddress, It.IsAny<ZString>(), It.IsAny<ZString>())).Returns("");
				AssertTestCFDiCancellationBuilder("");

				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.BranchAddress, It.IsAny<ZString>(), It.IsAny<ZString>())).Returns((string)null);
				AssertTestCFDiCancellationBuilder("");
			}

			void AssertTestCFDiCancellationBuilder(string expectedValue)
			{
				var cancellationBuilder = new CFDiCancellationBuilder() as ICFDiCancellationBuilder;
				var cancellationXml = cancellationBuilder.BuildXml(transactionInfo, accBatch).ToString();
				XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{RFCEmisor}", expectedValue), cancellationXml);
			}
		}

		public void Test_RFCReceptor()
		{
			var expectedEmptyXml = @"<Cancelacion>
  <RFCEmisor></RFCEmisor>
  <RFCReceptor>{RFCReceptor}</RFCReceptor>
  <Total>0</Total>
  <UUID></UUID>
  <Certificado></Certificado>
  <ClavePrivada></ClavePrivada>
  <Motivo></Motivo>
</Cancelacion>";

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			(var mexicoDependencyMock, _, _, _) = CreateMexicoDependencyMocks();
			var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, transactionInfoHelperMock);

			var expectedCodeRFC = "RFCReceptor030201001";
			var expectedCodeRFG = "RFGReceptor010203001";

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.OrganizationAddress, It.IsAny<ZString>(), MexicoOrgCusCodeInfo.OrgCusCodes.RFG)).Returns(expectedCodeRFG);
				AssertTestCFDiCancellationBuilder(expectedCodeRFG);
				transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.RFG), Times.Once);

				transactionInfoHelperMock.Reset();
				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.OrganizationAddress, It.IsAny<ZString>(), MexicoOrgCusCodeInfo.OrgCusCodes.RFC)).Returns(expectedCodeRFC);
				AssertTestCFDiCancellationBuilder(expectedCodeRFC);
				transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(transactionInfo.OrganizationAddress, CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.RFC), Times.Once);

				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.OrganizationAddress, It.IsAny<ZString>(), It.IsAny<ZString>())).Returns("");
				AssertTestCFDiCancellationBuilder("");

				transactionInfoHelperMock.Setup(x => x.GetRegistrationCode(transactionInfo.OrganizationAddress, It.IsAny<ZString>(), It.IsAny<ZString>())).Returns((string)null);
				AssertTestCFDiCancellationBuilder("");
			}

			void AssertTestCFDiCancellationBuilder(string expectedValue)
			{
				var cancellationBuilder = new CFDiCancellationBuilder() as ICFDiCancellationBuilder;
				var cancellationXml = cancellationBuilder.BuildXml(transactionInfo, accBatch).ToString();
				XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{RFCReceptor}", expectedValue), cancellationXml);
			}
		}

		public void Test_TotalAmount_Abs()
		{
			var expectedEmptyXml = @"<Cancelacion>
  <RFCEmisor></RFCEmisor>
  <RFCReceptor></RFCReceptor>
  <Total>{Total}</Total>
  <UUID></UUID>
  <Certificado></Certificado>
  <ClavePrivada></ClavePrivada>
  <Motivo></Motivo>
</Cancelacion>";

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			(var mexicoDependencyMock, _, _, _) = CreateMexicoDependencyMocks();
			var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, transactionInfoHelperMock);

			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			var cancellationBuilder = new CFDiCancellationBuilder() as ICFDiCancellationBuilder;
			AssertTestCFDiCancellationBuilder("0");

			transactionInfo.OSTotal = -100;
			AssertTestCFDiCancellationBuilder("100");

			transactionInfo.OSTotal = 200;
			AssertTestCFDiCancellationBuilder("200");

			void AssertTestCFDiCancellationBuilder(string expectedValue)
			{
				var cancellationXml = cancellationBuilder.BuildXml(transactionInfo, accBatch).ToString();
				XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{Total}", expectedValue), cancellationXml);
			}
		}

		public void Test_TotalAmount_Format()
		{
			var expectedEmptyXml = @"<Cancelacion>
  <RFCEmisor></RFCEmisor>
  <RFCReceptor></RFCReceptor>
  <Total>{Total}</Total>
  <UUID></UUID>
  <Certificado></Certificado>
  <ClavePrivada></ClavePrivada>
  <Motivo></Motivo>
</Cancelacion>";

			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.OSTotal = 100;

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			(var mexicoDependencyMock, _, _, _) = CreateMexicoDependencyMocks();
			var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, transactionInfoHelperMock);

			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			var cancellationBuilder = new CFDiCancellationBuilder() as ICFDiCancellationBuilder;

			transactionInfoHelperMock.Setup(x => x.GetOSCurrencyDecimals(It.IsAny<BusinessObjectFactory>(), It.IsAny<TransactionInfo>())).Returns(0);
			AssertTestCFDiCancellationBuilder("100");

			transactionInfoHelperMock.Setup(x => x.GetOSCurrencyDecimals(It.IsAny<BusinessObjectFactory>(), It.IsAny<TransactionInfo>())).Returns(1);
			AssertTestCFDiCancellationBuilder("100.0");

			transactionInfoHelperMock.Setup(x => x.GetOSCurrencyDecimals(It.IsAny<BusinessObjectFactory>(), It.IsAny<TransactionInfo>())).Returns(2);
			AssertTestCFDiCancellationBuilder("100.00");

			transactionInfoHelperMock.Setup(x => x.GetOSCurrencyDecimals(It.IsAny<BusinessObjectFactory>(), It.IsAny<TransactionInfo>())).Returns(3);
			AssertTestCFDiCancellationBuilder("100.000");

			transactionInfoHelperMock.Verify(x => x.GetOSCurrencyDecimals(accBatch.Factory, transactionInfo), Times.Exactly(4));

			void AssertTestCFDiCancellationBuilder(string expectedValue)
			{
				var cancellationXml = cancellationBuilder.BuildXml(transactionInfo, accBatch).ToString();
				XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{Total}", expectedValue), cancellationXml);
			}
		}

		#region UUID

		[ExpectNoExceptions]
		public void TestCFDiCancellationBuilder_UsesGetGovernmentNumber_WithCorrectAuthorizationDetailCollection()
		{
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();

			(var mexicoDependencyMock, _, _, _) = CreateMexicoDependencyMocks();
			var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, transactionInfoHelperMock);
			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();

			transaction.OriginalReference = null;
			Assert_UsesGetOrgRefGovernmentNumber_WithCorrectOriginalReference(transaction, null);

			var orgRef = new OriginalReference(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OriginalReference = orgRef;
			transaction.OriginalReference.SetAuthorizationDetailCollection(() => null);
			Assert_UsesGetOrgRefGovernmentNumber_WithCorrectOriginalReference(transaction, orgRef.AuthorizationDetailCollection);

			var authorizationDetails = new List<AuthorizationDetails>()
			{
				new AuthorizationDetails() { Purpose = new CodeDescriptionPair4Char() { Code = "XXX" }, GovernmentNumber = "1234" },
			};
			transaction.OriginalReference.SetAuthorizationDetailCollection(() => authorizationDetails);
			Assert_UsesGetOrgRefGovernmentNumber_WithCorrectOriginalReference(transaction, orgRef.AuthorizationDetailCollection);

			void Assert_UsesGetOrgRefGovernmentNumber_WithCorrectOriginalReference(TransactionInfo transaction, List<AuthorizationDetails> authorizationDetails)
			{
				var cancellationBuilder = new CFDiCancellationBuilder() as ICFDiCancellationBuilder;
				var cancellationXml = cancellationBuilder.BuildXml(transaction, accBatch).ToString();

				transactionInfoHelperMock.Verify(x => x.GetGovernmentNumber(authorizationDetails));
			}
		}

		public void TestCFDiCancellationBuilder_UUID()
		{
			var expectedEmptyXml = @"<Cancelacion>
  <RFCEmisor></RFCEmisor>
  <RFCReceptor></RFCReceptor>
  <Total>0</Total>
  <UUID>{UUID}</UUID>
  <Certificado></Certificado>
  <ClavePrivada></ClavePrivada>
  <Motivo></Motivo>
</Cancelacion>";

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();

			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();
			(var mexicoDependencyMock, _, _, _) = CreateMexicoDependencyMocks();

			var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, transactionInfoHelperMock);
			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			AssertTestCFDiCancellationBuilder_UUID(string.Empty);
			AssertTestCFDiCancellationBuilder_UUID("A10718");

			void AssertTestCFDiCancellationBuilder_UUID(string expectedValue)
			{
				transactionInfoHelperMock.Setup(x => x.GetGovernmentNumber(It.IsAny<List<AuthorizationDetails>>())).Returns(expectedValue);

				var cancellationBuilder = new CFDiCancellationBuilder() as ICFDiCancellationBuilder;
				var cancellationXml = cancellationBuilder.BuildXml(transaction, accBatch).ToString();
				XmlComparison.CompareAndAssertXml(expectedEmptyXml.Replace("{UUID}", expectedValue), cancellationXml);
			}
		}

		#endregion

		public void TestCFDiCancellationBuilderCertificado_ReturnsCertificateAndPrivateKey_WhenValidCertificate()
		{
			var expectedEmptyXml = @"<Cancelacion>
  <RFCEmisor></RFCEmisor>
  <RFCReceptor></RFCReceptor>
  <Total>0</Total>
  <UUID></UUID>
  <Certificado>{Certificate}</Certificado>
  <ClavePrivada>{PrivateKey}</ClavePrivada>
  <Motivo></Motivo>
</Cancelacion>";

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var credential_ForTestOnly = Factory.New<GlbCompanyEInvoicingCertificateCredential>();
			var certCode = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);
			var data_ForTestOnly = (Certificate: CertificateHelperTest.CertificateEncoded_ForTestOnly, Password: CertificateHelperTest.CertificatePrivateKey_ForTestOnly);

			(var mexicoDependencyMock, var companyCredential, var certificateHelperMock, _) = CreateMexicoDependencyMocks();
			companyCredential.Setup(x => x.GetCompanyCredential(transactionInfo)).Returns(credential_ForTestOnly);
			certificateHelperMock.Setup(x => x.GetCredentialDataForCancellation(credential_ForTestOnly)).Returns(data_ForTestOnly);
			var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, new Mock<ITransactionInfoHelper>());

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				AssertTestCFDiCancellationBuilder(data_ForTestOnly, transactionInfo, expectedEmptyXml);
				certificateHelperMock.Verify(x => x.GetCredentialDataForCancellation(credential_ForTestOnly), Times.Once);
			}
		}

		public void TestCFDiCancellationBuilderCertificado_ReturnsEmpty_WhenCredentialDataAreEmpty()
		{
			var expectedEmptyXml = @"<Cancelacion>
  <RFCEmisor></RFCEmisor>
  <RFCReceptor></RFCReceptor>
  <Total>0</Total>
  <UUID></UUID>
  <Certificado>{Certificate}</Certificado>
  <ClavePrivada>{PrivateKey}</ClavePrivada>
  <Motivo></Motivo>
</Cancelacion>";

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var credential_ForTestOnly = Factory.New<GlbCompanyEInvoicingCertificateCredential>();
			var data_ForTestOnly = (Certificate: string.Empty, Password: string.Empty);

			(var mexicoDependencyMock, var companyCredential, var certificateHelperMock, _) = CreateMexicoDependencyMocks();
			companyCredential.Setup(x => x.GetCompanyCredential(transactionInfo)).Returns(credential_ForTestOnly);
			certificateHelperMock.Setup(x => x.GetCredentialDataForCancellation(credential_ForTestOnly)).Returns(data_ForTestOnly);
			var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, new Mock<ITransactionInfoHelper>());

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				AssertTestCFDiCancellationBuilder(data_ForTestOnly, transactionInfo, expectedEmptyXml);
				certificateHelperMock.Verify(x => x.GetCredentialDataForCancellation(credential_ForTestOnly), Times.Once);
			}
		}

		public void TestCFDiCancellationBuilderCertificado_GetCredentialDataRunOnce_WhenGetCredentialDataReceivesNullParam()
		{
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			(var mexicoDependencyMock, _, var certificateHelperMock, _) = CreateMexicoDependencyMocks();
			var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, new Mock<ITransactionInfoHelper>());

			using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
			{
				var cancellationBuilder = new CFDiCancellationBuilder() as ICFDiCancellationBuilder;
				var cancellationXml = cancellationBuilder.BuildXml(transactionInfo, Factory.NewWithValidTestData<AccEInvoicingBatch>()).ToString();

				certificateHelperMock.Verify(x => x.GetCredentialDataForCancellation(null), Times.Once);
				Assert("This is not an empty test", true);
			}
		}

		public void TestCFDiCancellationBuilder_Motivo()
		{
			AssertCFDiCancellationBuilder_Motivo(ZString.Empty);
			AssertCFDiCancellationBuilder_Motivo("SomeReason");

			void AssertCFDiCancellationBuilder_Motivo(string expectedReason)
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();

				(var mexicoDependencyMock, _, _, var helperMock) = CreateMexicoDependencyMocks();
				helperMock.Setup(x => x.GetReasonOfCancellation(It.IsAny<ZGuid>())).Returns(expectedReason);
				var eInvoicinigDependencyMock = CreateDependencyFactoryMocks(mexicoDependencyMock, new Mock<ITransactionInfoHelper>());

				using (ObjectFactory.Substitute(eInvoicinigDependencyMock.Object))
				{
					var cancellationBuilder = new CFDiCancellationBuilder() as ICFDiCancellationBuilder;
					var xml = cancellationBuilder.BuildXml(transactionInfo, batch).ToString();

					AssertContains($"<Motivo>{expectedReason}</Motivo>", xml);
					helperMock.Verify(x => x.GetReasonOfCancellation(batch.PK), Times.Once);
				}
			}
		}

		void AssertTestCFDiCancellationBuilder((string Certificate, string PrivateKey) expectedTupleData, TransactionInfo transactionInfo, string expectedEmptyXml)
		{
			var cancellationBuilder = new CFDiCancellationBuilder() as ICFDiCancellationBuilder;
			var cancellationXml = cancellationBuilder.BuildXml(transactionInfo, Factory.NewWithValidTestData<AccEInvoicingBatch>()).ToString();

			var expectedXml = expectedEmptyXml.Replace("{Certificate}", expectedTupleData.Certificate).Replace("{PrivateKey}", expectedTupleData.PrivateKey);
			XmlComparison.CompareAndAssertXml(expectedXml, cancellationXml);
		}

		(Mock<IMexicoEInvoicingDependencyFactory>, Mock<ICompanyCredential>, Mock<ICertificateHelper>, Mock<IEInvoiceHelper>) CreateMexicoDependencyMocks()
		{
			var companyCredentialMock = new Mock<ICompanyCredential>();
			var certificateHelperMock = new Mock<ICertificateHelper>();
			var eInvoiceHelperMock = new Mock<IEInvoiceHelper>();

			var mexicoDependencyMock = new Mock<IMexicoEInvoicingDependencyFactory>();
			mexicoDependencyMock.Setup(x => x.GetCompanyCredential()).Returns(companyCredentialMock.Object);
			mexicoDependencyMock.Setup(x => x.GetCertificateHelper()).Returns(certificateHelperMock.Object);
			mexicoDependencyMock.Setup(x => x.GetEInvoiceHelper()).Returns(eInvoiceHelperMock.Object);

			return (mexicoDependencyMock, companyCredentialMock, certificateHelperMock, eInvoiceHelperMock);
		}

		Mock<IEInvoicingDependencyFactory> CreateDependencyFactoryMocks(Mock<IMexicoEInvoicingDependencyFactory> mexicoDependencyMock, Mock<ITransactionInfoHelper> transactionInfoHelperMock)
		{
			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			eInvoicinigDependencyMock.Setup(x => x.GetMexicoEInvoicingDependencyFactory()).Returns(mexicoDependencyMock.Object);
			eInvoicinigDependencyMock.Setup(x => x.GetTransactionInfoHelper()).Returns(transactionInfoHelperMock.Object);

			return eInvoicinigDependencyMock;
		}
	}
}
