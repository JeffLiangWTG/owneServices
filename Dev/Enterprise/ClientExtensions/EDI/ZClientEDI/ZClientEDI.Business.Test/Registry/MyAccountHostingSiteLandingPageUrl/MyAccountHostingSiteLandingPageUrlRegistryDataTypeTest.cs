using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Testing
{
	[TestedType(typeof(MyAccountHostingSiteLandingPageUrlRegistryDataType))]
	public class MyAccountHostingSiteLandingPageUrlRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MyAccountHostingSiteLandingPageUrlRegistryDataType>
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new MyAccountHostingSiteLandingPageUrlCollection();
			collection.AddNew("ARC", "www.myaccount.com/#Architecture");
			collection.AddNew("CUS", "www.myaccount.com/#Customs");
			var collection2 = new MyAccountHostingSiteLandingPageUrlCollection();
			collection2.AddNew("GEO", "www.myaccount.com/#GEOCompliance");
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, new MyAccountHostingSiteLandingPageUrlRegistryDataType().Serialise(collection)), new ValidSampleAndBinaryValueInDB(collection2, new MyAccountHostingSiteLandingPageUrlRegistryDataType().Serialise(collection2)) };
		}

		protected override MyAccountHostingSiteLandingPageUrlRegistryDataType GetNewDataType()
		{
			return new MyAccountHostingSiteLandingPageUrlRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "MyAccountHostingSiteLandingPageUrlRegistryItemEditor";
			}
		}
	}
}
