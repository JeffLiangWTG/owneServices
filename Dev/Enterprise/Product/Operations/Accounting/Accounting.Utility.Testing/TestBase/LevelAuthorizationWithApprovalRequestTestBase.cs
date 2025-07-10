using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using NUnit.Framework;

namespace Enterprise.Accounting.Utility.Testing.TestBase
{
	[TestsSubclassesOf(typeof(LevelAuthorizationWithApprovalRequest<,,>), ExcludeClientDlls = true)]
	public abstract class LevelAuthorizationWithApprovalRequestTestBase<TransactionType, RequestType, DetailsType, GUIProviderType> : TestCaseWithFactory
		where TransactionType : ITransactionForApproval
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
		where GUIProviderType : class, IPostingTransactionApprovalGUIProvider
	{
		public abstract void TestIsSecondApproverApplicable();

		protected abstract bool PerformTransactionLevelAuthorization(GUIProviderType postingGUIProvider, TransactionType[] transactions,
			Tuple<
					Func<TransactionType, bool>,
					Func<TransactionType, bool>,
					Action<RequestType, TransactionType[]>,
					Func<RequestType, ITransactionApprovalHelper>
				> testOnlyOverrides);

		protected abstract TransactionType CreateTransactionHeader(ZString transactionNumber, GUIProviderType guiProvider);
		protected abstract void CreateTransactionLine(TransactionType transaction, Job job, decimal localAmount);
		protected abstract string GetExpectedMessageCaption();

		protected virtual bool IsSingleTransactionApprovalTest
		{
			get { return true; }
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
