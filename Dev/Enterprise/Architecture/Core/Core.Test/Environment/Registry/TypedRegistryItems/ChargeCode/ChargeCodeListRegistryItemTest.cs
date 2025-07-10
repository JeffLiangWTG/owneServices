using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ChargeCodeListRegistryItem))]
	sealed class ChargeCodeListRegistryItemTest : ChargeCodeRegistryItemWrapperTestCase<string>
	{
		public void TestEditorInfo()
		{
			ChargeCodeListRegistryItem item1 = new ChargeCodeListRegistryItem("", null, null, null, "");
			ChargeCodeListRegistryItem item2 = new ChargeCodeListRegistryItem("", null, null, null, "", RegistryFindBoxFilter.FreightChargeCode);

			AssertEquals("Item1.EditorInfo.Filter", RegistryFindBoxFilter.None, ((AccChargeCodeListRegistryEditorInfo)item1.EditorInfo).Filter);
			AssertEquals("Item2.EditorInfo.Filter", RegistryFindBoxFilter.FreightChargeCode, ((AccChargeCodeListRegistryEditorInfo)item2.EditorInfo).Filter);
		}

		#region Implementation

		protected override Type ChargeCodeRegistryItemWrapperType
		{
			get { return typeof(ChargeCodeListRegistryItem); }
		}

		protected override bool IsEqual(Guid actualChargeCodePK, object obtainedDefaultValue)
		{
			return (actualChargeCodePK.ToString() == (string)obtainedDefaultValue);
		}

		#endregion
	}
}
