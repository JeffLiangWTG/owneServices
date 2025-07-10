using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestsSubclassesOf(typeof(ITempStoragePremisesLayoutProvider))]
	public abstract class TempStoragePremisesLayoutProviderAbstractTest<TLayoutProvider> : TestCaseWithFactory where TLayoutProvider : ITempStoragePremisesLayoutProvider, new()
	{
		public void TestGetTempStoragePremisesDetailsLayout()
		{
			AssertType(
				"GetTempStoragePremisesDetailsLayout() should return object with correct subclass of ITempStoragePremisesLayoutProvider",
				ExpectedGetTempStoragePremisesDetailsLayoutType,
				new TLayoutProvider().GetTempStoragePremisesDetailsLayout()
			);
		}

		protected abstract Type ExpectedGetTempStoragePremisesDetailsLayoutType { get; }
	}
}
