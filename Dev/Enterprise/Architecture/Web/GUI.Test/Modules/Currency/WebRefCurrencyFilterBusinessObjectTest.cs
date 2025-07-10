using System;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.Currency.Testing
{
	[TestedType(typeof(WebRefCurrencyFilterBusinessObject))]
	sealed class WebRefCurrencyFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		public void TestDescTooLong()
		{
			var filter = (WebRefCurrencyFilterBusinessObject)GetNewBusinessObject();
			var expectedCode = new string('A', RefCurrencySchema.RX_Code.MaxLength);
			var expectedDesc = new string('A', RefCurrencySchema.RX_Desc.MaxLength);
			var tooLong = new string('A', Math.Max(RefCurrencySchema.RX_Desc.MaxLength, RefCurrencySchema.RX_Code.MaxLength) + 1);
			filter.RX_Desc = tooLong;

			var expectedFilter = $"({RefCurrencySchema.RX_Code.Name} = '{expectedCode}' or {RefCurrencySchema.RX_Desc.Name} = '{expectedDesc}') and {RefCurrencySchema.RX_IsActive.Name} = 1";

			AssertEquals(expectedFilter, filter.Filter.LiteralTextADO);
		}
	}
}
