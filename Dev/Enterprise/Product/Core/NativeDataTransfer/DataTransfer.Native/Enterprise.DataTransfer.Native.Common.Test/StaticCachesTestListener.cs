using System;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.DB;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common
{
	class StaticCachesTestListener : BaseTestListener
	{
		public override void AfterEachTest(DateTime endTime)
		{
			base.AfterEachTest(endTime);
			GlobalDefinition.ResetStaticCacheForTesting();
			EntitySetDefinitionCache.ResetStaticCacheForTesting();
			TableBuilder.ResetStaticCacheForTesting();
		}
	}
}
