using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingBaseLookupsTest : TransactionHeaderLookupsTest
	{
		public void TestApprovalStatusList()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var lookup = new InvoicingBaseLookups(invoice);
			AssertEquals("Invoice without request. ApprovalStatusList.Count", 0, lookup.ApprovalStatusList.Count);

			invoice = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APInvoice>(TestObjectCreator.Creditor1, 100);
			lookup = new InvoicingBaseLookups(invoice);
			AssertEquals("Invoice with request. ApprovalStatusList.Count", 5, lookup.ApprovalStatusList.Count);
			int i = 0;
			AssertEquals("Invoice with request. ApprovalStatusList.Count", Constants.GenApprovalRequestApprovalStatus.Requested, lookup.ApprovalStatusList[i++].Code);
			AssertEquals("Invoice with request. ApprovalStatusList.Count", Constants.GenApprovalRequestApprovalStatus.Cancelled, lookup.ApprovalStatusList[i++].Code);
			AssertEquals("Invoice with request. ApprovalStatusList.Count", Constants.GenApprovalRequestApprovalStatus.Rejected, lookup.ApprovalStatusList[i++].Code);
			AssertEquals("Invoice with request. ApprovalStatusList.Count", Constants.GenApprovalRequestApprovalStatus.Approved, lookup.ApprovalStatusList[i++].Code);
			AssertEquals("Invoice with request. ApprovalStatusList.Count", Constants.GenApprovalRequestApprovalStatus.Posted, lookup.ApprovalStatusList[i++].Code);
			AssertEquals("All elements are tested", i, lookup.ApprovalStatusList.Count);
		}

		public void TestAH_Calc_AmendStatusCodeList()
		{
			var expectedOptions = new[] {
				("01","Mistakes or correction of entries or the tax rate is incorrectly applied"),
				("02","Supply amount change"),
				("03","The goods supplied have been returned"),
				("04","Disengagement of a contract"),
				("05","Post-opening of domestic L/C"),
				("06","Double issuance by mistake"),
			};

			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, krBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var lookup = new InvoicingBaseLookups(GetTestingInvoice());
				var lookupOptions = lookup.AmendStatusCodeList
					.Cast<CodeDescriptionPair>()
					.Select(x => ((string)x.Code, (string)x.Description))
					.ToArray();
				AssertContainsExactElementsInAnyOrder(expectedOptions, lookupOptions);
			}

			var auBranch = TestObjectCreator.CreateBranchWithCompany("AU");
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var lookup = new InvoicingBaseLookups(GetTestingInvoice());
				AssertEquals(0, lookup.AmendStatusCodeList.Count);
			}
		}

		public void TestReversalStatusCodeList_IReversalStatusCodeConfigurationProvider_UsesGenericImplementation()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("01", "Code from registry");
			var lookup = new InvoicingBaseLookups(GetTestingInvoice());

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedList))
			{
				var lookupOptions = lookup.ReversalStatusCodeList;
				AssertContainsExactElementsInAnyOrder(expectedList, lookupOptions);
			}
		}

		public void TestReversalStatusCodeList_IReversalStatusCodeConfigurationProvider_IsNotImplemented_And_GenericRegistryIsEmpty()
		{
			var expectedEmptyList = new CodeDescriptionPairList();
			var lookup = new InvoicingBaseLookups(GetTestingInvoice());

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedEmptyList))
			{
				AssertEquals(0, lookup.ReversalStatusCodeList.Count);
			}
		}

		public void TestReversalStatusCodeList_IReversalStatusCodeConfigurationProvider_IsImplemented()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("02", "Mock reversal code One");
			expectedList.AddPair("03", "Mock reversal code Two");

			var lookup = new InvoicingBaseLookups(GetTestingInvoice());
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("FFF");
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeLookup()).Returns(expectedList);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			var lookupOptions = lookup.ReversalStatusCodeList;
			AssertContainsExactElementsInAnyOrder(expectedList, lookupOptions);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetReversalStatusCodeLookup(), Times.Once);
		}

		public void TestReversalStatusCodeList_IReversalStatusCodeConfigurationProvider_IsImplemented_And_ReturnsEmptyList()
		{
			var expectedEmptyList = new CodeDescriptionPairList();

			var lookup = new InvoicingBaseLookups(GetTestingInvoice());
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("FFF");
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeLookup()).Returns(expectedEmptyList);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			var lookupOptions = lookup.ReversalStatusCodeList;
			AssertEquals(0, lookupOptions.Count);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetReversalStatusCodeLookup(), Times.Once);
		}

		public void TestReversalStatusCodeList_GetCountryFactory_MustBeCalledWith_TransactionContryCode()
		{
			var expectedCountryCode = "MX";
			var invoice = GetTestingInvoice();
			invoice.Company.GC_RN_NKCountryCode = expectedCountryCode;

			AssertNotEquals("Precondition, current login country and invoice's country must be diffrent", invoice.Company.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>()));
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			var lookup = new InvoicingBaseLookups(invoice);
			var lookupOptions = lookup.ReversalStatusCodeList;

			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(expectedCountryCode), Times.Once);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		InvoicingBase GetTestingInvoice()
		{
			return TestObjectCreator.CreateInvoice(typeof(ARInvoice));
		}
	}
}
