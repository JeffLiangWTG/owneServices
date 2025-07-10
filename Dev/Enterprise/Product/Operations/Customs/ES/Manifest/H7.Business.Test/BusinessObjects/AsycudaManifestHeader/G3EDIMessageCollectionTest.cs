using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(G3EDIMessageCollection))]
	class G3EDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => Factory.NewWithValidTestData<AsycudaManifestHeader>().Messages;

		protected override Type GetExpectedCollectionType() => typeof(G3EDIMessageCollection);
	}
}
