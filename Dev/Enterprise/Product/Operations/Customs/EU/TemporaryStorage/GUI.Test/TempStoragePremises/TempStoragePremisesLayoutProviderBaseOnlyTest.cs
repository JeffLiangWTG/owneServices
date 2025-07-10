using System;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStoragePremisesLayoutProvider))]
	sealed class TempStoragePremisesLayoutProviderBaseOnlyTest : TempStoragePremisesLayoutProviderAbstractTest<TempStoragePremisesLayoutProvider>
	{
		protected override Type ExpectedGetTempStoragePremisesDetailsLayoutType => typeof(TempStoragePremisesDetailsLayout);
	}
}
