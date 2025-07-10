using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.CountryCompliance.UruguayComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	public class ReferenciaBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<ReferenciaBuilder>(new CFEBuilder().ReferenciaBuilder_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestEFacBuildXml()
		{
			var cFEHelperMock = new Mock<ICFEHelper>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ComplianceSubType = ComplianceSubTypeCodes.TCR,
				OriginalReference = new OriginalReference() { OriginalTransactionComplianceSubType = ComplianceSubTypeCodes.TXI, OriginalTransactionReference = "AB00000005" }
			};

			var localReferenciaBuilder = new ReferenciaBuilder();

			localReferenciaBuilder.SubstituteCFEHelper_ForTestOnly(cFEHelperMock.Object);

			var builder = (IReferenciaBuilder)localReferenciaBuilder;
			builder.BuildReferenciaInfo(transactionInfo);

			cFEHelperMock.Verify(x => x.GetInvoiceSerieAndNumber(It.IsAny<ZString>()), Times.Once);
		}

		public void TestReferenciaBuilder_NullData()
		{
			var builder = new ReferenciaBuilder() as IReferenciaBuilder;
			var referenciaBuilder = builder.BuildReferenciaInfo(null);

			AssertNull(referenciaBuilder);

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

			AssertNull(referenciaBuilder);
		}

		public void TestNotIsCreditOrDebitNote()
		{
			var subTypes = new[]
			{
				ComplianceSubTypeCodes.TXI,
				ComplianceSubTypeCodes.TKT,
				ComplianceSubTypeCodes.YXI,
				ComplianceSubTypeCodes.YKT,
				ComplianceSubTypeCodes.XCL,
			};

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			foreach (var subType in subTypes)
			{
				transactionInfo.ComplianceSubType = subType;

				var builder = new ReferenciaBuilder() as IReferenciaBuilder;
				var referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

				AssertNull(subType, referenciaBuilder);
			}
		}

		public void TestIsCreditOrDebitNote()
		{
			var subTypes = new[]
			{
				ComplianceSubTypeCodes.TCR,
				ComplianceSubTypeCodes.TCD,
				ComplianceSubTypeCodes.YCD,
				ComplianceSubTypeCodes.YCR,
				ComplianceSubTypeCodes.YKD,
				ComplianceSubTypeCodes.YKR,
				ComplianceSubTypeCodes.TKD,
				ComplianceSubTypeCodes.TKC,
			};
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			foreach (var subType in subTypes)
			{
				transactionInfo.ComplianceSubType = subType;

				var builder = new ReferenciaBuilder() as IReferenciaBuilder;
				var referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

				AssertNotNull(referenciaBuilder);
				AssertEquals(1, referenciaBuilder.Length);
				AssertEquals("1", referenciaBuilder[0].NroLinRef);
			}
		}

		public void TestReferenciaBuilder_TpoDocRef()
		{
			var subTypes = new[]
			{
				new { SubType = ComplianceSubTypeCodes.TXI, ExpectedTpoDocRef = CFEType.Item111 },
				new { SubType = ComplianceSubTypeCodes.TCR, ExpectedTpoDocRef = CFEType.Item112 },
				new { SubType = ComplianceSubTypeCodes.TCD, ExpectedTpoDocRef = CFEType.Item113 },
				new { SubType = ComplianceSubTypeCodes.TKT, ExpectedTpoDocRef = CFEType.Item101 },
				new { SubType = ComplianceSubTypeCodes.TKC, ExpectedTpoDocRef = CFEType.Item102 },
				new { SubType = ComplianceSubTypeCodes.TKD, ExpectedTpoDocRef = CFEType.Item103 },
				new { SubType = ComplianceSubTypeCodes.YXI, ExpectedTpoDocRef = CFEType.Item211 },
				new { SubType = ComplianceSubTypeCodes.YCR, ExpectedTpoDocRef = CFEType.Item212 },
				new { SubType = ComplianceSubTypeCodes.YCD, ExpectedTpoDocRef = CFEType.Item213 },
				new { SubType = ComplianceSubTypeCodes.YKT, ExpectedTpoDocRef = CFEType.Item201 },
				new { SubType = ComplianceSubTypeCodes.YKR, ExpectedTpoDocRef = CFEType.Item202 },
				new { SubType = ComplianceSubTypeCodes.YKD, ExpectedTpoDocRef = CFEType.Item203 }
			};

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ComplianceSubType = ComplianceSubTypeCodes.TCR,
				OriginalReference = new OriginalReference()
			};

			var builder = new ReferenciaBuilder() as IReferenciaBuilder;
			ReferenciaReferencia[] referenciaBuilder = null;

			foreach (var subType in subTypes)
			{
				transactionInfo.OriginalReference.OriginalTransactionComplianceSubType = subType.SubType;
				referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

				AssertEquals(subType.ExpectedTpoDocRef, referenciaBuilder[0].TpoDocRef);
				Assert(referenciaBuilder[0].TpoDocRefSpecified);
			}

			transactionInfo.OriginalReference.OriginalTransactionComplianceSubType = null;
			transactionInfo.OriginalReference.OriginalTransactionNumber = "112AB014785";
			referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

			AssertEquals(CFEType.Item112, referenciaBuilder[0].TpoDocRef);
			Assert(referenciaBuilder[0].TpoDocRefSpecified);

			transactionInfo.OriginalReference.OriginalTransactionNumber = "678AB014785";
			referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

			Assert(!referenciaBuilder[0].TpoDocRefSpecified);

			transactionInfo.OriginalReference.OriginalTransactionNumber = null;
			referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

			Assert(!referenciaBuilder[0].TpoDocRefSpecified);
		}

		public void TestReferenciaBuilder_NroCFERef_And_Serie()
		{
			var builder = new ReferenciaBuilder() as IReferenciaBuilder;
			ReferenciaReferencia[] referenciaBuilder;

			var transactionNumbers = new List<(string transactionNumber, string expectedSerie, string expectedNumber)>() {
				("101TR10000004", "TR", "10000004"),
				("TR10000004", null, null),
				("", null, null)
			};

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ComplianceSubType = ComplianceSubTypeCodes.TCR,
				OriginalReference = new OriginalReference()
				{
					OriginalTransactionComplianceSubType = ComplianceSubTypeCodes.TCR,
				}
			};

			transactionInfo.OriginalReference.OriginalTransactionReference = "AB00000005";
			referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

			AssertEquals("AB", referenciaBuilder[0].Serie);
			AssertEquals("00000005", referenciaBuilder[0].NroCFERef);

			transactionInfo.OriginalReference.OriginalTransactionReference = null;
			referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

			AssertNull(referenciaBuilder[0].Serie);
			AssertNull(referenciaBuilder[0].NroCFERef);

			transactionInfo.OriginalReference.OriginalTransactionComplianceSubType = null;

			foreach (var numbers in transactionNumbers)
			{
				transactionInfo.OriginalReference.OriginalTransactionNumber = numbers.transactionNumber;
				referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

				AssertEquals(numbers.expectedSerie, referenciaBuilder[0].Serie);
				AssertEquals(numbers.expectedNumber, referenciaBuilder[0].NroCFERef);
			}

			transactionInfo.OriginalReference.OriginalTransactionNumber = null;
			referenciaBuilder = builder.BuildReferenciaInfo(transactionInfo);

			AssertNull(referenciaBuilder[0].NroCFERef);
		}
	}
}
