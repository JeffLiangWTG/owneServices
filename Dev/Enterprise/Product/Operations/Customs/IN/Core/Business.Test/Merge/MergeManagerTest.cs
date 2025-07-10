using System;

namespace Enterprise.Customs.IN.Business.Testing;

sealed class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
{
	protected override Type GetLineMergerType() => typeof(LineMerger);

	protected override Customs.Business.BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
}
