using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(INManifestTypes))]
sealed class INManifestTypesTest : TestCaseWithFactory
{
	public void TestCGM()
	{
		var cgmManifest = new INManifestTypes().CGM;
		CombineAssertions(() =>
		{
			AssertEquals("Code", "CGM", cgmManifest.Code);
			AssertEquals("Description", "Consol General Manifest", cgmManifest.Description);
			AssertContainsExactElementsInAnyOrder("ApplicableTransportModes", new[] { Core.Constants.TransportModes.Air }, cgmManifest.ApplicableTransportModes);
			AssertContainsExactElementsInAnyOrder("ApplicableManifestStyles", new[] { ApplicationCodeTypeList.Codes.Consolidator }, cgmManifest.ApplicableManifestStyles);
			AssertEquals("MessageLevel", MessageLevel.Manifest, cgmManifest.MessageLevel);
			AssertEquals("Manifest Natures", "IMP", cgmManifest.ManifestNatures.CodesAsString);
		});
	}

	public void TestIGM()
	{
		var igmManifest = new INManifestTypes().IGM;
		CombineAssertions(() =>
		{
			AssertEquals("Code", "IGM", igmManifest.Code);
			AssertEquals("Description", "Import General Manifest", igmManifest.Description);
			AssertContainsExactElementsInAnyOrder("ApplicableTransportModes", new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea }, igmManifest.ApplicableTransportModes);
			AssertContainsExactElementsInAnyOrder("ApplicableManifestStyles", new[] { ApplicationCodeTypeList.Codes.ShippingLine }, igmManifest.ApplicableManifestStyles);
			AssertEquals("MessageLevel", MessageLevel.Manifest, igmManifest.MessageLevel);
		});
	}

	public void TestAll()
	{
		var allManifests = new INManifestTypes().All;
		AssertContainsExactElementsInAnyOrder("All", new[] { INManifestTypes.Codes.CGM, INManifestTypes.Codes.IGM }, allManifests.Select(x => x.Code));
	}
}
