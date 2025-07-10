using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(EDIMessageCollectionNonDependent))]
	class EDIMessageCollectionNonDependentTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => Factory.NewWithValidTestData<AsycudaManifestHeader>().Messages;

		protected override Type GetExpectedCollectionType() => typeof(EDIMessageCollectionNonDependent);
	}
}
