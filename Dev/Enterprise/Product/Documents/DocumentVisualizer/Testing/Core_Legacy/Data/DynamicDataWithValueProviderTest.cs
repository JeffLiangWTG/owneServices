using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DynamicDataWithValueProviderTest : TestCase
	{
		public void TestIsOverridden_WithConstant()
		{
			IDynamicData property = DynamicDataWithValueProvider.Create(() => new ZString(), new object().MakeDynamic());

			AssertEquals("new is not overridden", false, property.IsOverriddenIncludingChildren);

			property.SetValue(new ZString("aaa"));

			AssertEquals("value changed; should be overridden", true, property.IsOverriddenIncludingChildren);

			property.SetValue(new ZString());

			AssertEquals("set value to the original; should not be overridden", false, property.IsOverriddenIncludingChildren);
		}

		public void TestIsOverridden_WithValueProvider()
		{
			var data = new Data
			{
				Value = ""
			};

			IDynamicData property = DynamicDataWithValueProvider.Create(() => data.Value, new object().MakeDynamic());

			AssertEquals("new is not overridden", false, property.IsOverriddenIncludingChildren);

			data.Value = "aaa";

			AssertEquals("new is overridden when value changed", true, property.IsOverriddenIncludingChildren);

			property.SetValue(new ZString("bbb"));

			AssertEquals("value changed; should be overridden", true, property.IsOverriddenIncludingChildren);

			property.SetValue(new ZString("aaa"));
			property.AcceptChanges();

			AssertEquals("set value to the original; should not be overridden", false, property.IsOverriddenIncludingChildren);
		}

		#region Implementation

		sealed class Data
		{
			public ZString Value { get; set; }
		}

		#endregion
	}
}