using System;

namespace Enterprise.Customs.CN.Business.Testing
{
	class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration() => EntryCreationStrategyTest.CreateDeclarationWithInstruction(Factory, Common.Shared.SharedJobMessageTypeList.Codes.Import, DecTypeList.Codes.CustomsEntry).JobDeclaration;
	}
}
