using System;
using CargoWise.Common;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	[Immutable]
	public sealed class NUnitAssertionProvider : IAssertionProvider
	{
		public static NUnitAssertionProvider Instance { get; } = new NUnitAssertionProvider();

		NUnitAssertionProvider()
		{
		}

		Assertion<TObject, TState> IAssertionProvider.BuildAssertion<TObject, TState, TValue>(AssertionType assertionType, string assertionMessage, GetValueForAssertionDelegate<TObject, TState, TValue> getActualValue, TValue expectedValue)
		{
			switch (assertionType)
			{
				case AssertionType.Equals: return (dataObject, stateForAssertions) => Assertion.AssertEquals(assertionMessage, expectedValue, getActualValue(dataObject, stateForAssertions));
				case AssertionType.NotEquals: return (dataObject, stateForAssertions) => Assertion.AssertNotEquals(assertionMessage, expectedValue, getActualValue(dataObject, stateForAssertions));
				case AssertionType.IsNull: return (dataObject, stateForAssertions) => Assertion.AssertNull(assertionMessage, getActualValue(dataObject, stateForAssertions));
				case AssertionType.NotNull: return (dataObject, stateForAssertions) => Assertion.AssertNotNull(assertionMessage, getActualValue(dataObject, stateForAssertions));

				default: throw new NotSupportedException($"Assertion type {assertionType} is not supported for {nameof(NUnitAssertionProvider)}.");
			}
		}

		void IAssertionProvider.FailAssertionRun(string errorMessage)
		{
			Assertion.Fail(errorMessage);
		}

		void IAssertionProvider.RunAllAssertions<TObject, TState>(string runAllAssertionsMessage, TObject objectToAssert, TState stateForAssertions, Assertion<TObject, TState> assertions)
		{
			AssertionWithHtml.CombineAssertions(runAllAssertionsMessage, () => ((IAssertionProvider)this).RunNestedAssertions(objectToAssert, stateForAssertions, assertions));
		}

		void IAssertionProvider.RunNestedAssertions<TObject, TState>(TObject objectToAssert, TState stateForAssertions, Assertion<TObject, TState> assertions)
		{
			assertions(objectToAssert, stateForAssertions);
		}

		IDisposable IAssertionProvider.CreateVerificationTracker() => new MakeSureToVerifyAssertions();

		sealed class MakeSureToVerifyAssertions : DisposableObject
		{
		}
	}
}
