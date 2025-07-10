using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[TestsSubclassesOf(typeof(ZMultilingual), ExcludePrivate = true)]
	public abstract class ZMultilingualTest : TestCase
	{
		public void TestIZTypeMembers()
		{
			AssertEquals(GetZMultilingualConcrete().GetUnresolvedValue().DataType, GetZMultilingualConcrete().DataType);
			AssertEquals(GetZMultilingualConcrete().GetUnresolvedValue().Default, GetZMultilingualConcrete().Default);
			AssertEquals(GetZMultilingualConcrete().GetUnresolvedValue().IsDefault, GetZMultilingualConcrete().IsDefault);
			AssertEquals(GetZMultilingualConcrete().GetUnresolvedValue().IsEmpty, GetZMultilingualConcrete().IsEmpty);
			AssertEquals(GetZMultilingualConcrete().GetUnresolvedValue().IsValid, GetZMultilingualConcrete().IsValid);
		}

		public void TestIZTypeInternalsMembers()
		{
			using (Res.UseMockData())
			{
				Assert(GetZMultilingualConcrete().GetLocalizedValue(Res.CurrentLanguage).Equals(GetZMultilingualConcrete().GetValueForLogicalDataLayer(false)));
			}
		}

		protected abstract ZMultilingual GetZMultilingualConcrete();
	}
}
