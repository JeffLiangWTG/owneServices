using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(LocationOfGoodsFromAuthorisationDefaulterConfiguration))]
	public abstract class LocationOfGoodsFromAuthorisationDefaulterConfigurationAbstractTest<L> : TestCaseWithFactory
		where L : LocationOfGoodsFromAuthorisationDefaulterConfiguration
	{
		public void TestDerivedClassIsSealed()
		{
			if (typeof(L) == typeof(LocationOfGoodsFromAuthorisationDefaulterConfiguration))
			{
				Assert("Doesn’t need to be sealed", condition: true);
			}
			else
			{
				Assert("Derived class should be sealed", typeof(L).IsSealed);
			}
		}

		public virtual void TestIsDefaultingEnabled() => AssertEquals(nameof(configuration.IsDefaultingEnabled), expected: true, configuration.IsDefaultingEnabled);

		public virtual void TestQualifierCode() => AssertEquals(nameof(configuration.QualifierCode), expected: CusGoodsLocationQualifierList.Codes.AuthorizationNumber, configuration.QualifierCode);

		public virtual void TestTypeCode() => AssertEquals(nameof(configuration.TypeCode), expected: CusGoodsLocationTypeList.Codes.AuthorizedPlace, configuration.TypeCode);

		protected override void SetUp()
		{
			base.SetUp();
			configuration = Activator.CreateInstance<L>();
		}
		protected L configuration;
	}

	[TestedType(typeof(LocationOfGoodsFromAuthorisationDefaulterConfiguration))]
	sealed class LocationOfGoodsFromAuthorisationDefaulterConfigurationBaseOnlyTest : LocationOfGoodsFromAuthorisationDefaulterConfigurationAbstractTest<LocationOfGoodsFromAuthorisationDefaulterConfiguration>
	{
	}
}
