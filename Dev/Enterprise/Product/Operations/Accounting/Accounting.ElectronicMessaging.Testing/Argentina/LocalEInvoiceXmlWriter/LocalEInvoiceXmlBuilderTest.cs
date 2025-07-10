using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	public class LocalEInvoiceXmlBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<LocalEInvoiceXmlBuilder>(new ArgentinaEInvoiceXmlWriter().LocalEInvoiceXmlBuilder_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestFEDetRequestBuildXml()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch(),
			};

			var builderMock = new Mock<IFEDetRequestBuilder>();
			var mockAccComplianceMock = new Mock<IComplianceSequenceRetriever>();
			var transactionInfoHelperMock = new Mock<ITransactionInfoHelper>();

			var localEInvioceBuilder = new LocalEInvoiceXmlBuilder();
			localEInvioceBuilder.SubstituteFeDetRequestBuilder_ForTestOnly(builderMock.Object);
			localEInvioceBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(mockAccComplianceMock.Object);
			localEInvioceBuilder.SubstituteTransactionInfoHelper_ForTestOnly(transactionInfoHelperMock.Object);

			var builder = (IEInvoiceXmlBuilder)localEInvioceBuilder;
			builder.BuildXml(transactionInfo, accBatch);
			builderMock.Verify(x => x.BuildFEDetRequestInfo(transactionInfo, "http://ar.gov.afip.dif.FEV1/", accBatch.Factory, It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Once);
			transactionInfoHelperMock.Verify(x => x.GetRegistrationCode(It.IsAny<OrganizationAddress>(), It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestFECabRequestBuildXml()
		{
			var accBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = "BUE" },
				TransactionType = TransactionType.CRD,
				Department = new Department() { Code = "BUE" }
			};

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			transaction.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			transaction.Branch.Code = branch.GB_Code;
			transaction.Department.Code = department.GE_Code;

			var originalAccComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			originalAccComplianceSequence.XD_GC_Company = branch.GB_GC;
			originalAccComplianceSequence.XD_GB_BranchOwner = branch.PK;
			originalAccComplianceSequence.XD_GE_Department = department.PK;
			originalAccComplianceSequence.XD_SequenceClass = "TXA";
			originalAccComplianceSequence.XD_Prefix = "00233";
			Factory.Save();

			var fexBuilder = new LocalEInvoiceXmlBuilder();
			var complianceSequenceMock = new Mock<IComplianceSequenceRetriever>();

			complianceSequenceMock.Setup(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceMock.Object);

			var builder = fexBuilder as IEInvoiceXmlBuilder;
			var cfe = builder.BuildXml(transaction, accBatch);
			complianceSequenceMock.Verify(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, department.PK, It.IsAny<ZDateTime>()), Times.Once);

			transaction.Branch = null;
			originalAccComplianceSequence.XD_GC_Company = ZGuid.Empty;
			originalAccComplianceSequence.XD_GB_BranchOwner = ZGuid.Empty;

			complianceSequenceMock.Setup(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, ZGuid.Empty, ZGuid.Empty, department.PK, It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceMock.Object);

			cfe = builder.BuildXml(transaction, accBatch);
			complianceSequenceMock.Verify(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, ZGuid.Empty, ZGuid.Empty, department.PK, It.IsAny<ZDateTime>()), Times.Once);

			transaction.Department = null;
			transaction.Branch = new Branch() { Code = branch.GB_Code };
			originalAccComplianceSequence.XD_GC_Company = branch.GB_GC;
			originalAccComplianceSequence.XD_GB_BranchOwner = branch.PK;
			originalAccComplianceSequence.XD_GE_Department = ZGuid.Empty;

			complianceSequenceMock.Setup(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, ZGuid.Empty, It.IsAny<ZDateTime>())).Returns(originalAccComplianceSequence);
			fexBuilder.SubstituteComplianceSequenceRetriever_ForTestOnly(complianceSequenceMock.Object);

			cfe = builder.BuildXml(transaction, accBatch);
			complianceSequenceMock.Verify(x => x.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.Value, branch.GB_GC, branch.PK, ZGuid.Empty, It.IsAny<ZDateTime>()), Times.Once);
		}
	}
}
