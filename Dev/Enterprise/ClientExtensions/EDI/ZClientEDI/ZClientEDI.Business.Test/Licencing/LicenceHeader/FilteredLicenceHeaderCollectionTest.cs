using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(FilteredLicenceHeaderCollection))]
	public class FilteredLicenceHeaderCollectionTest : BusinessObjectCollectionViewTestCase<FilteredLicenceHeaderCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(FilteredLicenceHeaderCollection);
		}

		protected override FilteredLicenceHeaderCollection GetCollectionToTest()
		{
			ReleaseBuild build = Factory.NewWithValidTestData<ReleaseBuild>();
			return build.FilteredReleaseLicences;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<LicenceHeader>();
		}
	}
}
