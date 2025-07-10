using System;
using Enterprise.Integration.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ColumnLayoutCacheTestListener : BaseTestListener, IColumnLayoutCacheTestListener
	{
		public static readonly ColumnLayoutCacheTestListener Instance = new ColumnLayoutCacheTestListener();

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);
			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();
		}
	}
}
