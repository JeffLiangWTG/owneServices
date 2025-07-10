using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public override void TestRequiresMerge()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			var mergeManager = new MergeManager(declaration);
			AssertEquals(false, mergeManager.RequiresMerge);

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			mergeManager = new MergeManager(declaration);
			AssertEquals(true, mergeManager.RequiresMerge);
		}

		public override void TestSupportsAutoMerge()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			var mergeManager = new MergeManager(declaration);
			AssertEquals(false, mergeManager.SupportsAutoMerge);

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			mergeManager = new MergeManager(declaration);
			AssertEquals(true, mergeManager.SupportsAutoMerge);
		}
		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}
	}
}
