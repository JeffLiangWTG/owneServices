using System;
using Microsoft.SqlServer.Server;

namespace Enterprise.SqlSecurity.Test.SqlClrAssembly
{
	[Serializable]
	[SqlUserDefinedAggregate(Format.Native)]
	public struct SqlClrAggregateTest
	{
		readonly int result;
		public void Init()
		{
		}

		public void Accumulate(int value)
		{
		}

		public int Terminate()
		{
			return result;
		}

		public void Merge(SqlClrAggregateTest other)
		{
		}
	}
}
