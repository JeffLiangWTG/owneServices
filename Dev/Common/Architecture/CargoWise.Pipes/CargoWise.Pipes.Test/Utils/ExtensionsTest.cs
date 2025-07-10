using CargoWise.Async;
using NUnit.Framework;

namespace CargoWise.Pipes.Test
{
	class ExtensionsTest : PipeEngineTestCase
	{
		public void TestAddSync()
		{
			var pipe1 = Engine.AddSync(() => 1);
			var pipe2 = Engine.AddSync(one => one + 1, pipe1);
			var pipe3 = Engine.AddSync((one, two) => one + two + 1, pipe1, pipe2);
			var pipe4 = Engine.AddSync((one, two, three) => one + two + three + 1, pipe1, pipe2, pipe3);

			using (var resultSet = Engine.ExecuteAll(MockDispatcher, MockDispatcher))
			{
				MockDispatcher.DispatchAll();
				AssertEquals(8, resultSet.GetResult(pipe4));
			}
		}

		[RequiresSTA]
		public void TestAddAsync()
		{
			var pipe1 = Engine.AddAsync(() => 1);
			var pipe2 = Engine.AddAsync(one => one + 1, pipe1);
			var pipe3 = Engine.AddAsync((one, two) => one + two + 1, pipe1, pipe2);
			var pipe4 = Engine.AddAsync((one, two, three) => one + two + three + 1, pipe1, pipe2, pipe3);

			using (var resultSet = Engine.ExecuteAll(MockDispatcher, MockDispatcher))
			{
				MockDispatcher.DispatchAll();
				AssertEquals(8, resultSet.GetResult(pipe4));
			}
		}

		public void TestAddEnd()
		{
			var pipe1 = Engine.AddSync(() => 1);
			var pipe2 = Engine.AddSync(() => 2);
			var pipe3 = Engine.AddSync(() => 3);

			var end1 = 0;
			var end2 = 0;
			var end3 = 0;

			Engine.AddPipeEnd(one => end1 = one * 10, pipe1);
			Engine.AddPipeEnd((one, two) => end2 = (one + two) * 10, pipe1, pipe2);
			Engine.AddPipeEnd((one, two, three) => end3 = (one + two + three) * 10, pipe1, pipe2, pipe3);

			using (var resultSet = Engine.ExecuteAll(MockDispatcher, MockDispatcher))
			{
				MockDispatcher.DispatchAll();
				AssertEquals(10, end1);
				AssertEquals(30, end2);
				AssertEquals(60, end3);
			}
		}

		protected override void TearDown()
		{
			try
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
			finally
			{
				base.TearDown();
			}
		}
	}
}
