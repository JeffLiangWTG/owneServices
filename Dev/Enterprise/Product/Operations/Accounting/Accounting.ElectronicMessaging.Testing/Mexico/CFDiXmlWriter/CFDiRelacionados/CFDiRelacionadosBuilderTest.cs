using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class CFDiRelacionadosBuilderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCFDiRelacionadosBuilder_UsesGetGovernmentNumber_WithCorrectOriginalReferenceAuthorizationDetailCollection()
		{
			var (eInvoicinigDependencyMock, transactionInfoHelperMock) = CreateDependencyMocks();
			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			transactionInfo.OriginalReference = null;
			Assert_UsesGetOrgRefGovernmentNumber_WithCorrectOriginalReference(null);

			var orgRef = new OriginalReference(DefaultDataObjectWriterStrategy.TestInstance);
			orgRef.SetAuthorizationDetailCollection(() => new List<AuthorizationDetails>());
			transactionInfo.OriginalReference = orgRef;
			Assert_UsesGetOrgRefGovernmentNumber_WithCorrectOriginalReference(orgRef.AuthorizationDetailCollection);

			var authorizationDetails = new List<AuthorizationDetails>()
			{
				new AuthorizationDetails() { Purpose = new CodeDescriptionPair4Char() { Code = "XXX" }, GovernmentNumber = "1234" },
			};
			orgRef.SetAuthorizationDetailCollection(() => authorizationDetails);
			Assert_UsesGetOrgRefGovernmentNumber_WithCorrectOriginalReference(orgRef.AuthorizationDetailCollection);

			void Assert_UsesGetOrgRefGovernmentNumber_WithCorrectOriginalReference(List<AuthorizationDetails> authorizationDetailCollection)
			{
				var builder = new CFDiRelacionadosBuilder() as ICFDiRelacionadosBuilder;
				var relacionado = builder.BuildRelacionadosInfo(transactionInfo);

				transactionInfoHelperMock.Verify(x => x.GetGovernmentNumber(authorizationDetailCollection));
			}
		}

		public void TestRelacionado_WhenUUID_IsEmpty()
		{
			var (eInvoicinigDependencyMock, transactionInfoHelperMock) = CreateDependencyMocks();
			transactionInfoHelperMock.Setup(x => x.GetGovernmentNumber(It.IsAny<List<AuthorizationDetails>>())).Returns(string.Empty);
			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var builder = new CFDiRelacionadosBuilder() as ICFDiRelacionadosBuilder;
			var relacionado = builder.BuildRelacionadosInfo(transaction);

			AssertNull("Relacionado should be null", relacionado);
		}

		public void TestRelacionado_WhenUUID_IsNotEmpty()
		{
			var governmentAllocatedNumber = "123456";

			var (eInvoicinigDependencyMock, transactionInfoHelperMock) = CreateDependencyMocks();
			transactionInfoHelperMock.Setup(x => x.GetGovernmentNumber(It.IsAny<List<AuthorizationDetails>>())).Returns(governmentAllocatedNumber);
			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var builder = new CFDiRelacionadosBuilder() as ICFDiRelacionadosBuilder;
			var relacionado =  builder.BuildRelacionadosInfo(transaction);

			AssertEquals("UUID Should be populated with goverment allocation number", governmentAllocatedNumber, relacionado[0].CfdiRelacionado[0].UUID);
		}

		public void TestRelacionadoTipoRelacion()
		{
			var (eInvoicinigDependencyMock, transactionInfoHelperMock) = CreateDependencyMocks();
			transactionInfoHelperMock.Setup(x => x.GetGovernmentNumber(It.IsAny<List<AuthorizationDetails>>())).Returns("123456");
			ObjectFactory.Substitute(eInvoicinigDependencyMock.Object);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			AssertTipoRelacion(MexicoComplianceInfo.ComplianceSubTypeCodes.TCR, c_TipoRelacion.Item01);
			AssertTipoRelacion(MexicoComplianceInfo.ComplianceSubTypeCodes.TDR, c_TipoRelacion.Item02);

			void AssertTipoRelacion(string complianceSubType, c_TipoRelacion expectedTipoRelacion)
			{
				transaction.ComplianceSubType = complianceSubType;

				var builder = new CFDiRelacionadosBuilder() as ICFDiRelacionadosBuilder;
				var relacionado = builder.BuildRelacionadosInfo(transaction);

				AssertEquals(expectedTipoRelacion, relacionado[0].TipoRelacion);
			}
		}

		(Mock<IEInvoicingDependencyFactory>, Mock<ITransactionInfoHelper>) CreateDependencyMocks()
		{
			var eInvoicinigDependencyMock = new Mock<IEInvoicingDependencyFactory>();
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();

			eInvoicinigDependencyMock.Setup(x => x.GetTransactionInfoHelper()).Returns(transactionInfoHelperMock.Object);

			return (eInvoicinigDependencyMock, transactionInfoHelperMock);
		}
	}
}
