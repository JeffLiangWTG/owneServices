using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	[TestedType(typeof(ErrorLogStatusFilter))]
	public class ErrorLogStatusFilterTest : ModuleTextFilterTest
	{
		public void TestPreffixAndSuffixText()
		{
			ErrorLogStatusFilter errorFilter = new ErrorLogStatusFilter("Mock ErrorStatusFilter", delegate(ZString value)
			{
				return new ZQuery();
			}, new CodeDescriptionPairList());
			errorFilter.Property = ErrorLogStatusFilter.CodeConstants.Fixed;
			AssertEquals("Property should be 'FIX'", "FIX", errorFilter.Property);
			AssertEquals("Prefix should be 'In the last'", "In the last", errorFilter.PrefixText);
			AssertEquals("Suffix should be 'days'", "days", errorFilter.SuffixText);
		}

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new ErrorLogStatusFilter("moo", _ => new ZQuery(), new CodeDescriptionPairList());
		}
	}
}
