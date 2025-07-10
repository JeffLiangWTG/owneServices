using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Pipes.Test
{
	class PipeSafetyAnalyzerTest : PipeEngineTestCase
	{
		#region Empty Engine

		public void TestEmptyEngine()
		{
			AssertSafety(PipeSafetyLevel.None);
		}

		#endregion

		#region Pipes with no input

		public void TestUnconnectedSyncNode_SafeOutput()
		{
			Engine.AddSync(() => new Safe());
			AssertSafety("All pipes can have safe output", PipeSafetyLevel.Safe);
		}

		public void TestUnconnectedSyncNode_UnsafeOutput()
		{
			Engine.AddSync(() => new Unsafe());
			AssertSafety("Synchronous nodes can have unsafe output because there are no cross thread implications.", PipeSafetyLevel.Safe);
		}

		public void TestUnconnectedAsyncNode_SafeOutput()
		{
			Engine.AddAsync(() => new Safe());
			AssertSafety("All pipes can have safe output", PipeSafetyLevel.Safe);
		}

		public void TestUnconnectedAsyncNode_UnsafeOutput()
		{
			Engine.AddAsync(() => new Unsafe());
			AssertSafety(PipeSafetyLevel.Safe);
		}

		#endregion

		#region Pipes with Unsafe Input

		public void TestSyncNodeToSyncNode_UnsafeInput()
		{
			var inputPipe = Engine.AddSync(() => new Unsafe());
			Engine.AddSync(u => u, inputPipe);

			AssertSafety("Passing around unsafe values is always allowed on the main thread.", PipeSafetyLevel.Safe);
		}

		public void TestSyncNodeToAsyncNode_UnsafeInput()
		{
			var inputPipe = Engine.AddSync(() => new Unsafe());
			Engine.AddAsync(u => u, inputPipe);

			AssertSafety(PipeSafetyLevel.Unsafe, "[Asynchronous Pipe -> Unsafe] has mutable input from [Synchronous Pipe -> Unsafe].\r\n");
		}

		public void TestAsyncNodeToSyncNode_UnsafeInput()
		{
			var inputPipe = Engine.AddAsync(() => new Unsafe());
			Engine.AddSync(u => u, inputPipe);

			AssertSafety("It is permissable for Async pipes to generate mutable data used in Sync pipes", PipeSafetyLevel.Safe);
		}

		public void TestAsyncNodeToAsyncNode_UnsafeInput()
		{
			var inputPipe = Engine.AddAsync(() => new Unsafe());
			Engine.AddAsync(u => u, inputPipe);

			AssertSafety(PipeSafetyLevel.Unsafe, "[Asynchronous Pipe -> Unsafe] has mutable input from [Asynchronous Pipe -> Unsafe].\r\n");
		}

		#endregion

		#region Pipes with Safe Input

		public void TestSyncNodeToSyncNode_SafeInput()
		{
			var inputPipe = Engine.AddSync(() => new Safe());
			Engine.AddSync(u => u, inputPipe);

			AssertSafety(PipeSafetyLevel.Safe);
		}

		public void TestSyncNodeToAsyncNode_SafeInput()
		{
			var inputPipe = Engine.AddSync(() => new Safe());
			Engine.AddAsync(u => u, inputPipe);

			AssertSafety(PipeSafetyLevel.Safe);
		}

		public void TestAsyncNodeToSyncNode_SafeInput()
		{
			var inputPipe = Engine.AddAsync(() => new Safe());
			Engine.AddSync(u => u, inputPipe);

			AssertSafety(PipeSafetyLevel.Safe);
		}

		public void TestAsyncNodeToAsyncNode_SafeInput()
		{
			var inputPipe = Engine.AddAsync(() => new Safe());
			Engine.AddAsync(u => u, inputPipe);

			AssertSafety(PipeSafetyLevel.Safe);
		}

		#endregion

		#region Thread Safe Input

		public void TestThreadSafeInput()
		{
			var inputPipe = Engine.AddAsync(() => new ThreadSafe());
			Engine.AddAsync(u => u, inputPipe);

			AssertSafety(PipeSafetyLevel.Safe);
		}

		#endregion

		#region Handovers Make the Unsafe Safe

		public void TestHandoverAsyncToAsync()
		{
			var inputPipe = Engine.AddAsync(() => new Unsafe()).AddHandover(_ => { }, _ => { });
			Engine.AddAsync(u => u, inputPipe);

			AssertSafety("Using a handover makes unsafe things unknown.", PipeSafetyLevel.Unknown, "The safety of the handover from [Asynchronous Pipe -> Unsafe] to [Asynchronous Pipe -> Unsafe] is unverifiable.\r\n");
		}

		#endregion

		#region Implementation

		void AssertSafety(PipeSafetyLevel expectedLevel, string expectedDescription = null)
		{
			AssertSafety("Assert Engine Safety level", expectedLevel, expectedDescription);
		}

		void AssertSafety(string message, PipeSafetyLevel expectedLevel, string expectedDescription = null)
		{
			var safety = Engine.FindSafetyLevel();

			CombineAssertions(message, () =>
			{
				AssertEquals("Expected level", expectedLevel, safety.Level);
				AssertEquals("Expected description", expectedDescription, safety.Description);
			});
		}

		class Unsafe
		{
			public int EditableInt { get; set; }
		}

		[Immutable]
		class Safe
		{
			public int Shh { get; }
		}

		[ThreadSafe]
		class ThreadSafe
		{
			public int Bleh { get; }
		}

		#endregion
	}
}
