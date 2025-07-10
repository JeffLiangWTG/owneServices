using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class HVLVArchivalFileFormatsTest : TestCase
	{
		public void TestHVLVArchivalFileFormatsCodesAndDescriptions()
		{
			var hvlvArchivalFileFormats = new HVLVArchivalFileFormats();
			CombineAssertions(() =>
			{
				AssertEquals("There should only be 2 options", 2, hvlvArchivalFileFormats.Count);
				Assert("Codes should contain CSV", hvlvArchivalFileFormats.ContainsCode(Constants.FileFormats.CSV));
				Assert("Codes should contain XML", hvlvArchivalFileFormats.ContainsCode(Constants.FileFormats.XML));
				AssertEquals("Description for CSV", "Comma Separated Values", hvlvArchivalFileFormats.GetDescriptionFromCode(Constants.FileFormats.CSV));
				AssertEquals("Description for XML", "Extensible Markup Language", hvlvArchivalFileFormats.GetDescriptionFromCode(Constants.FileFormats.XML));
			});
		}
	}
}
