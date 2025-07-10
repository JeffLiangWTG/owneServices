using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace CargoWise.ResourceStrings.Cache
{
	[TestedType(typeof(ModifiedMultilingualString))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Use source generated static methods", Justification = "The test of the ResourceString")]
	public class ModifiedMultilingualStringTest : ZMultilingualTest
	{
		protected override ZMultilingual GetZMultilingualConcrete()
		{
			return new ModifiedMultilingualString(
				delegate(string[] str)
				{
					return string.Join(",", str);
				},
				 ResString.GetMultilingualString("0", "Carthage has been eventually destroyed")
			);
		}
	}
}
