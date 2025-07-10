using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Business.Testing;

[TestedType(typeof(AsycudaBillCollectionSS))]
public class AsycudaBillCollectionSSTest : BusinessObjectCollectionTestCase
{
	protected override Type GetExpectedCollectionType() => typeof(AsycudaBillCollectionSS);

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
		return header.Bills;
	}
}
