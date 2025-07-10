using System.Threading;
using CargoWise.Async;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	#region Test Case

	[TestedType(typeof(DummyValidatedAsyncOperator))]
	sealed class DummyValidatedAsyncOperatorTest : ValidatedAsyncOperatorTestCase<DummyValidatedAsyncOperator, IntegerWrapper, IntegerWrapper>
	{
		protected override IValidatedAsyncOperator GetNewOperator()
		{
			return new DummyValidatedAsyncOperator();
		}

		protected override IntegerWrapper GetExpectedInitialSnapshot()
		{
			return new IntegerWrapper(1);
		}

		protected override IntegerWrapper GetExpectedResultSnapshot()
		{
			return new IntegerWrapper(2);
		}

		protected override IntegerWrapper GetNewInitialReal()
		{
			return new IntegerWrapper(1);
		}

		protected override IntegerWrapper GetRealWithIncompatibleChanges()
		{
			return new IntegerWrapper(0);
		}

		protected override IntegerWrapper GetExpectedReal()
		{
			return new IntegerWrapper(2);
		}
	}

	#endregion

	#region Dummy Implementation

	class IntegerWrapper
	{
		public IntegerWrapper(int value)
		{
			Value = value;
		}

		public int Value { get; set; }

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var otherInt = obj as IntegerWrapper;
			if (otherInt != null)
			{
				return otherInt.Value == Value;
			}
			else
			{
				return base.Equals(obj);
			}
		}
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Testing tests")]
	class DummyValidatedAsyncOperator : ValidatedAsyncOperator<IntegerWrapper, IntegerWrapper>
	{
		protected override IntegerWrapper GetSnapshot(IntegerWrapper source)
		{
			return source;
		}

		protected override IntegerWrapper TransformSnapshot(IntegerWrapper initialSnapshot, CancellationTokenSource cancellationTokenSource)
		{
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Token.ThrowIfCancellationRequested();
			}

			return new IntegerWrapper(initialSnapshot.Value + 1);
		}

		protected override bool AssumptionsOfInitialSnapShotValid(IntegerWrapper real, IntegerWrapper initialSnapshot)
		{
			return real.Equals(initialSnapshot);
		}

		protected override void MapSnapshot(IntegerWrapper newReal, IntegerWrapper newSnapShot)
		{
			newReal.Value = newSnapShot.Value;
		}
	}

	#endregion
}
