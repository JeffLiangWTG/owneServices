using System;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		protected override Type GetLineMergerType() => typeof(LineMerger);
	}
}
