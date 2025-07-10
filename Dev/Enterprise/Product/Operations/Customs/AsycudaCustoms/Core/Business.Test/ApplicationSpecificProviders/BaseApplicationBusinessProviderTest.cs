using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestsSubclassesOf(typeof(BaseApplicationBusinessProvider))]
	public abstract class BaseApplicationBusinessProviderTest : TestCaseWithFactory
	{
		public virtual void TestTariffType()
		{
			foreach (var country in CountriesInGroup)
			{
				var provider = BaseApplicationBusinessProvider.GetApplicationBusinessProvider(Factory, country);
				AssertEquals(ExpectedTariffType, provider.UniversalTariffType);
				AssertEquals(ExpectedDataSource, provider.UniversalRefDataSource);
			}
		}

		public virtual void TestGetApplicationBusinessProvider()
		{
			var provider = BaseApplicationBusinessProvider.GetApplicationBusinessProvider(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertType(ApplicationBusinessProviderType, provider);
		}

		protected abstract Type ApplicationBusinessProviderType { get; }

		public virtual void TestGetApplicationBusinessProviderCache()
		{
			var provider1 = BaseApplicationBusinessProvider.GetApplicationBusinessProvider(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var provider2 = BaseApplicationBusinessProvider.GetApplicationBusinessProvider(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			Assert("GetApplicationBusinessProvider should be cached", object.ReferenceEquals(provider1, provider2));
		}

		protected abstract ZString ExpectedTariffType { get; }

		protected abstract IEnumerable<ZString> CountriesInGroup { get; }

		protected abstract ZString ExpectedDataSource { get; }
	}
}
