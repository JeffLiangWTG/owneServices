using System;
using CargoWise.Data.Diagnostics;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class QueryStackTraceRecorderTest : TestCase
	{
		public void TestCreateObject()
		{
			IQueryStackTraceRecorder recorder1 = QueryStackTraceRecorder.Instance;
			IQueryStackTraceRecorder recorder2 = QueryStackTraceRecorder.Instance;
			AssertEquals("Object reference is the same", true, Object.ReferenceEquals(recorder1, recorder2));
		}
	}
}
