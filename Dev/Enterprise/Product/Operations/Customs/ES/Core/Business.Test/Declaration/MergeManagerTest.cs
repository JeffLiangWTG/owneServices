using System;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class MergeManagerTest : EU.Business.Declaration.Testing.MergeManagerTest
	{
		protected override Type GetLineMergerType() => typeof(LineMerger);
	}
}
