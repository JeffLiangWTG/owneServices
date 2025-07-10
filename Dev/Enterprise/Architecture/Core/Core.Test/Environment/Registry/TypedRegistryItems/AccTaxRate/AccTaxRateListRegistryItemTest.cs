using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(AccTaxRateListRegistryItem))]
	sealed class AccTaxRateListRegistryItemTest : AccTaxRateRegistryItemWrapperTestCase<string>
	{
		public void TestEditorInfo()
		{
			AccTaxRateListRegistryItem item1 = new AccTaxRateListRegistryItem("", null, null, null, "");
			AccTaxRateListRegistryItem item2 = new AccTaxRateListRegistryItem("", null, null, null, "", RegistryFindBoxFilter.FreightChargeCode);

			AssertEquals("Item1.EditorInfo.Filter", RegistryFindBoxFilter.None, ((AccTaxRateListRegistryEditorInfo)item1.EditorInfo).Filter);
			AssertEquals("Item2.EditorInfo.Filter", RegistryFindBoxFilter.FreightChargeCode, ((AccTaxRateListRegistryEditorInfo)item2.EditorInfo).Filter);
		}

		#region Implementation

		protected override Type AccTaxRateRegistryItemWrapperType
		{
			get { return typeof(AccTaxRateListRegistryItem); }
		}

		protected override bool IsEqual(Guid actualChargeCodePK, object obtainedDefaultValue)
		{
			return (actualChargeCodePK.ToString() == (string)obtainedDefaultValue);
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new AccTaxRateListRegistryItem("", null, null, null, "");
		}

		#endregion
	}
}
