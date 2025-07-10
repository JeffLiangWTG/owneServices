using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class MultiFactorTestCase<T>
		where T : BusinessObject
	{
		public MultiFactorTestCase(Func<T> setUp, bool shouldConfirmNecessity = true)
		{
			dataSetUp = setUp;
			this.shouldConfirmNecessity = shouldConfirmNecessity;
		}

		public void SetUpCondition(Preq<T> preq)
		{
			satisfactoryPreq = preq;
		}

		public void SetUpProcessAction(Action<T> betweenAction)
		{
			this.betweenAction = betweenAction;
		}

		public void RunAssertion(Action<T> assertExpectation, Action<T> assertFailure)
		{
			if (shouldConfirmNecessity && assertFailure is null)
			{
				throw new InvalidOperationException("Failure assertion must be passed if shouldConfirmNecessity is set to true.");
			}

			foreach (var satisfactoryAction in satisfactoryPreq.GetSatisfactoryActions())
			{
				var target = dataSetUp();
				satisfactoryAction(target);
				betweenAction?.Invoke(target);
				assertExpectation(target);

				ConfigurationPreq<T>.Dispose();
				tearDown?.Invoke(target);
			}

			var anyoneSatisfactoryAction = satisfactoryPreq.GetSatisfactoryActions().First();
			foreach (var failingAction in satisfactoryPreq.GetFailingActions())
			{
				var target = dataSetUp();
				anyoneSatisfactoryAction(target);
				failingAction(target);
				betweenAction?.Invoke(target);
				assertFailure(target);

				ConfigurationPreq<T>.Dispose();
				tearDown?.Invoke(target);
			}
		}

		public void SetUpTearDown(Action<T> tearDown)
		{
			this.tearDown = tearDown;
		}

		readonly Func<T> dataSetUp;
		Preq<T> satisfactoryPreq;
		Action<T> betweenAction;
		Action<T> tearDown;
		readonly bool shouldConfirmNecessity;
	}

	public struct FieldPreqValue
	{
		public FieldPreqValue(string stringValue) : this((ZString)stringValue as IZType) { }

		public FieldPreqValue(bool boolValue) : this((ZBool)boolValue as IZType) { }

		public FieldPreqValue(int intValue) : this((ZInt)intValue as IZType) { }

		public FieldPreqValue(decimal decimalValue) : this((ZDecimal)decimalValue as IZType) { }

		public FieldPreqValue(DateTime dateTimeValue) : this((ZDateTime)dateTimeValue) { }

		FieldPreqValue(IZType validValue) : this(new[] { validValue }) { }

		FieldPreqValue(IEnumerable<IZType> validValues)
		{
			Values = validValues;
		}
		public IEnumerable<IZType> Values;

		#region Convertion

		public static implicit operator FieldPreqValue(string value) => new FieldPreqValue(value);

		public static implicit operator FieldPreqValue(bool value) => new FieldPreqValue(value);

		public static implicit operator FieldPreqValue(int value) => new FieldPreqValue(value);

		public static implicit operator FieldPreqValue(decimal value) => new FieldPreqValue(value);

		public static implicit operator FieldPreqValue(DateTime value) => new FieldPreqValue(value);

		public static implicit operator FieldPreqValue(ZString value) => new FieldPreqValue(value as IZType);

		public static implicit operator FieldPreqValue(ZBool value) => new FieldPreqValue(value as IZType);

		public static implicit operator FieldPreqValue(ZInt value) => new FieldPreqValue(value as IZType);

		public static implicit operator FieldPreqValue(ZDecimal value) => new FieldPreqValue(value as IZType);

		public static implicit operator FieldPreqValue(ZDateTime value) => new FieldPreqValue(value);

		#endregion

		#region Operator Overloads

		public static bool operator true(FieldPreqValue v) => false;

		public static bool operator false(FieldPreqValue v) => false;

		public static FieldPreqValue operator |(FieldPreqValue value1, FieldPreqValue value2) => new FieldPreqValue(value1.Values.Union(value2.Values));

		#endregion

	}
}
