using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.CountryCompliance.UruguayComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	class CFEBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<CFEBuilder>(new CFEXmlWriter().CFEBuilder_ExposedForTestOnly);
		}

		[TestDate(2020, 7, 24)]
		public void TestBuildXml_eFactura()
		{
			var subTypes = new[] { ComplianceSubTypeCodes.TXI, ComplianceSubTypeCodes.TCR, ComplianceSubTypeCodes.TCD, ComplianceSubTypeCodes.YXI, ComplianceSubTypeCodes.YCR, ComplianceSubTypeCodes.YCD };
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			var itemDetFacts = Array.Empty<Item_Det_Fact>();
			var cfeBuilder = new CFEBuilder();

			foreach (var subType in subTypes)
			{
				transactionInfo.ComplianceSubType = subType;

				var encabezadoMock = new Mock<IEncabezadoBuilder>();
				cfeBuilder.SubstituteEncabezadoBuilder_ForTestOnly(encabezadoMock.Object);

				var itemsMock = new Mock<IItemsBuilder>();
				cfeBuilder.SubstituteItemsBuilder_ForTestOnly(itemsMock.Object);
				itemsMock.Setup(x => x.BuildItems(transactionInfo)).Returns(itemDetFacts);

				var referenciaMock = new Mock<IReferenciaBuilder>();
				cfeBuilder.SubstituteReferenciaBuilder_ForTestOnly(referenciaMock.Object);
				cfeBuilder.SubstituteFactory_ForTestOnly(Factory);

				var builder = cfeBuilder as ICFEBuilder;

				var cfe = builder.BuildCFEInfo(transactionInfo);
				encabezadoMock.Verify(x => x.BuildEFacEncabezadoInfo(transactionInfo, itemDetFacts, Factory), Times.Once);
				itemsMock.Verify(x => x.BuildItems(transactionInfo), Times.Once);
				referenciaMock.Verify(x => x.BuildReferenciaInfo(transactionInfo), Times.Once);

				AssertEquals(typeof(CFEDefTypeEFact), cfe.Item.GetType());

				var eFac = cfe.Item as CFEDefTypeEFact;
				AssertNull(nameof(CFEDefTypeEFact.TmstFirma), eFac.TmstFirma);
				AssertEquals(nameof(CFEDefType.version), "1.0", cfe.version);
			}
		}

		[TestDate(2020, 7, 24)]
		public void TestBuildXml_eTicket()
		{
			var subTypes = new[] { ComplianceSubTypeCodes.TKT, ComplianceSubTypeCodes.TKC, ComplianceSubTypeCodes.TKD, ComplianceSubTypeCodes.YKT, ComplianceSubTypeCodes.YKR, ComplianceSubTypeCodes.YKD };
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			var itemDetFacts = Array.Empty<Item_Det_Fact>();
			var cfeBuilder = new CFEBuilder();

			foreach (var subType in subTypes)
			{
				transactionInfo.ComplianceSubType = subType;

				var encabezadoMock = new Mock<IEncabezadoBuilder>();
				cfeBuilder.SubstituteEncabezadoBuilder_ForTestOnly(encabezadoMock.Object);

				var itemsMock = new Mock<IItemsBuilder>();
				cfeBuilder.SubstituteItemsBuilder_ForTestOnly(itemsMock.Object);
				itemsMock.Setup(x => x.BuildItems(transactionInfo)).Returns(itemDetFacts);

				var referenciaMock = new Mock<IReferenciaBuilder>();
				cfeBuilder.SubstituteReferenciaBuilder_ForTestOnly(referenciaMock.Object);
				cfeBuilder.SubstituteFactory_ForTestOnly(Factory);

				var builder = cfeBuilder as ICFEBuilder;
				var cfe = builder.BuildCFEInfo(transactionInfo);
				encabezadoMock.Verify(x => x.BuildETicketEncabezadoInfo(transactionInfo, itemDetFacts, Factory), Times.Once);
				itemsMock.Verify(x => x.BuildItems(transactionInfo), Times.Once);
				referenciaMock.Verify(x => x.BuildReferenciaInfo(transactionInfo), Times.Once);

				AssertEquals(typeof(CFEDefTypeETck), cfe.Item.GetType());

				var eTick = cfe.Item as CFEDefTypeETck;
				AssertNull(nameof(CFEDefTypeETck.TmstFirma), eTick.TmstFirma);
				AssertEquals(nameof(CFEDefType.version), "1.0", cfe.version);
			}
		}

		public void TestBuildXml_InvalidComplianceSubType()
		{
			var builder = new CFEBuilder() as ICFEBuilder;

			TransactionInfo transactionInfo = null;
			AssertTestBuildXml_InvalidComplianceSubType();

			transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertTestBuildXml_InvalidComplianceSubType();

			transactionInfo.ComplianceSubType = ComplianceSubTypeCodes.TKT;
			var cfe = builder.BuildCFEInfo(transactionInfo);
			AssertNotNull(nameof(CFEDefType), cfe.Item);

			transactionInfo.ComplianceSubType = "XXX";
			AssertTestBuildXml_InvalidComplianceSubType();

			void AssertTestBuildXml_InvalidComplianceSubType()
			{
				cfe = builder.BuildCFEInfo(transactionInfo);
				AssertNull(nameof(CFEDefType), cfe.Item);
			}
		}
	}
}
