using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(CusTempStorageJobHeaderConfiguration))]
	public abstract class CusTempStorageJobHeaderConfigurationAbstractTest<T> : TestCaseWithFactory
		where T : CusTempStorageJobHeaderConfiguration
	{
		protected virtual bool IsUCC6_Expected => false;

		protected virtual BusinessObject GetBusinessObjectForTest() => Factory.New<DummyBusinessObject>();

		[ExpectNoExceptions]
		public void TestIsUCC6()
		{
			NUnit.Framework.Assert.That(configuration.IsUCC6(GetBusinessObjectForTest()), NUnit.Framework.Is.EqualTo(IsUCC6_Expected).Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (T)Activator.CreateInstance(typeof(T));
		}
		protected T configuration;
	}

	[TestedType(typeof(CusTempStorageJobHeaderConfiguration))]
	sealed class CusTempStorageJobHeaderConfigurationBaseOnlyTest : CusTempStorageJobHeaderConfigurationAbstractTest<CusTempStorageJobHeaderConfiguration>
	{
	}
}
