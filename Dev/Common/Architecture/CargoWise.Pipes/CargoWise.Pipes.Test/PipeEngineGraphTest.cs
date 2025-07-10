using System;

namespace CargoWise.Pipes.Test
{
	class PipeEngineGraphTest : PipeEngineTestCase
	{
		public void TestExecute()
		{
			var source = Engine.AddSync(() => "Cats");
			var pipe = new Pipe<string>(PipeType.Synchronous, new Func<string, string>(s => s + " are Fat."), source);

			Engine.AddPipe(pipe);

			using (var resultSet = Engine.ExecuteAll(AsyncStrategy, MockDispatcher))
			{
				MockDispatcher.DispatchAll();

				AssertEquals("Cats are Fat.", resultSet.GetResult(pipe));
			}
		}

		public void TestEndPipe()
		{
			var source = Engine.AddSync(() => "Cats");
			var x = "Vehicle";
			Engine.AddPipeEnd(new Action<string>(s => x = s), source);

			Engine.ExecuteAll(AsyncStrategy, MockDispatcher);
			MockDispatcher.DispatchAll();

			AssertEquals("Cats", x);
		}

		public void TestMultiInput()
		{
			var source = Engine.AddSync(() => "Cats");
			var pipe1 = Engine.AddPipe<string>(PipeType.Synchronous, new Func<string, string>(s => s + " are Fat."), source);
			var pipe2 = Engine.AddPipe<string>(PipeType.Synchronous, new Func<string, string>(s => s + " are Stupid."), source);
			var pipe3 = Engine.AddPipe<string>(PipeType.Synchronous, new Func<string, string>(s => s + " are Ugly."), source);

			var merger = new Func<string, string, string, string>((s1, s2, s3) => string.Join(Environment.NewLine, new[] { "All of the good people of the world have come together to decide that:", s1, s2, s3 }));
			var aggrPipe = Engine.AddPipe<string>(PipeType.Synchronous, merger, pipe1, pipe2, pipe3);

			using (var resultSet = Engine.ExecuteAll(AsyncStrategy, MockDispatcher))
			{
				MockDispatcher.DispatchAll();

				var message = @"All of the good people of the world have come together to decide that:
Cats are Fat.
Cats are Stupid.
Cats are Ugly.";
				AssertEquals(message, resultSet.GetResult(aggrPipe));
			}
		}
	}
}
