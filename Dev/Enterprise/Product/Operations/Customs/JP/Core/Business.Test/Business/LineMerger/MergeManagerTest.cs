using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business.Testing
{
	using Enterprise.Customs.Business.Testing;
	using Enterprise.Customs.JP.Business;
	using Enterprise.Customs.JP.Common;

	public class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public override void TestShouldCheckExistenceOfInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mergeManager = declaration.MergeManager;
			AssertEquals(true, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoices(mergeManager));

			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.ECR };
			declaration.SetCurrentMessageSendingContext(messageSendingContext);
			declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(false, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoices(mergeManager));
		}

		public override void TestShouldCheckExistenceOfInvoiceLineForAllInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mergeManager = declaration.MergeManager;
			AssertEquals(true, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoiceLineForAllInvoices(mergeManager));

			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.ECR };
			declaration.SetCurrentMessageSendingContext(messageSendingContext);
			declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(false, MergeManagerTestHelper.GetShouldCheckExistenceOfInvoiceLineForAllInvoices(mergeManager));
		}

		public override void TestRequiresMerge()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			var mergeManager = declaration.MergeManager;
			AssertEquals(false, mergeManager.RequiresMerge);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals(true, mergeManager.RequiresMerge);
		}

		public override void TestSupportsAutoMerge()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			var mergeManager = declaration.MergeManager;
			AssertEquals(false, mergeManager.SupportsAutoMerge);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals(true, mergeManager.SupportsAutoMerge);
		}

		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}
	}
}
