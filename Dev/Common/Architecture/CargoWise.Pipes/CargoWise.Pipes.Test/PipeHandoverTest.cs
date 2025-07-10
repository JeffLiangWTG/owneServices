using System.Threading;

namespace CargoWise.Pipes.Test
{
	class PipeHandoverTest : PipeEngineTestCase
	{
		public void TestHandover()
		{
			var box = new Box<int>(0);
			var handover = Engine.AddSync(() => box).AddHandover(b => b.Value++, b => b.Value--);

			var result = -1;
			Engine.AddPipeEnd(b => result = b.Value, handover);

			Engine.ExecuteAll(AsyncStrategy, MockDispatcher);
			MockDispatcher.DispatchAll();

			AssertEquals("After execution the box should be in the released state.", 0, box.Value);
			AssertEquals("During execution the box should be in the claimed state.", 1, result);
		}

		public void TestHandover_Locking()
		{
			using (var mutex = new Mutex(true))
			{
				var negativeResult = false;
				var positiveResult = false;

				var box = new Box<int>(0);

				var pipe = Engine.AddSync(() => box);

				var handover1 = pipe.AddHandover(b => b.Value += 100, b => b.Value -= 100);
				var handover2 = pipe.AddHandover(b => b.Value -= 100, b => b.Value += 100);

				Engine.AddAsync(b =>
				{
					positiveResult = true;
					mutex.WaitOne(10000);
					return b.Value;
				}, handover1);

				Engine.AddAsync(b =>
				{
					negativeResult = true;
					mutex.WaitOne(10000);
					return b.Value;
				}, handover2);

				using (var resultSet = Engine.ExecuteAll(AsyncStrategy, MockDispatcher))
				{
					MockDispatcher.DispatchAll();

					while (!positiveResult && !negativeResult)
					{
					}

					var message = @"The parallel disabling handover should prevent these actions from running at the same time
And the currently running action should be stalled at a mutex.";
					AssertNotEquals(message, positiveResult, negativeResult);

					mutex.ReleaseMutex();

					resultSet.AwaitAll();

					AssertEquals(true, positiveResult);
					AssertEquals(true, negativeResult);
				}
			}
		}

		#region Implementation

		class Box<T>
		{
			public Box(T value)
			{
				Value = value;
			}

			public T Value { get; set; }
		}

		#endregion
	}
}
