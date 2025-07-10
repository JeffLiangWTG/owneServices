using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace CargoWise.ResourceStrings.Cache
{
	[TestedType(typeof(NoResString))]
	public class NoResStringTest : ZMultilingualTest
	{
		protected override ZMultilingual GetZMultilingualConcrete()
		{
			return (NoResString)"Don't mix vodka with champagne";
		}
	}
}
