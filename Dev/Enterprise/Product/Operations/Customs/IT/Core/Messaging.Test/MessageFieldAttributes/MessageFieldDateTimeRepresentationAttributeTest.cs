using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes.Testing;

sealed class MessageFieldDateTimeRepresentationAttributeTest : TestCase
{
	sealed class MessageFieldDateYYYYMMDDRepresentationAttributeAttributeTest : TestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				var attribute = new MessageFieldDateYYYYMMDDRepresentationAttribute();
				AssertEquals("Length should be correctly set via constructor.", 8, attribute.Length);
				AssertEquals("Format Data should be correctly set via constructor.", "yyyyMMdd", attribute.FormatDate);
				AssertEquals("IsFixedLength should be correctly set via constructor.", true, attribute.IsFixedLength);
			});
		}

		public void TestSerialize()
		{
			var attribute = new MessageFieldDateYYYYMMDDRepresentationAttribute();

			CombineAssertions("SerializeValue method", () =>
			{
				AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => attribute.SerializeValue(null));
				AssertExceptionThrown<InvalidCastException>("Input value must be ZDate or ZDateTime", () => attribute.SerializeValue(new ZString("aaa")));
				AssertEquals("Return value", ZDate.Today.ToString("yyyyMMdd"), attribute.SerializeValue(ZDate.Today));
				AssertEquals("Return value", ZDateTime.Today.ToString("yyyyMMdd"), attribute.SerializeValue(ZDateTime.Today));
			});
		}
	}

	sealed class MessageFieldDateDDMMYYYYRepresentationAttributeAttributeTest : TestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				var attribute = new MessageFieldDateDDMMYYYYRepresentationAttribute();
				AssertEquals("Length should be correctly set via constructor.", 8, attribute.Length);
				AssertEquals("Format Data should be correctly set via constructor.", "ddMMyyyy", attribute.FormatDate);
				AssertEquals("IsFixedLength should be correctly set via constructor.", true, attribute.IsFixedLength);
			});
		}

		public void TestSerialize()
		{
			var attribute = new MessageFieldDateDDMMYYYYRepresentationAttribute();

			CombineAssertions("SerializeValue method", () =>
			{
				AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => attribute.SerializeValue(null));
				AssertExceptionThrown<InvalidCastException>("Input value must be ZDate or ZDateTime", () => attribute.SerializeValue(new ZString("aaa")));
				AssertEquals("SerializeValue method return value", ZDate.Today.ToString("ddMMyyyy"), attribute.SerializeValue(ZDate.Today));
				AssertEquals("SerializeValue method return value", ZDateTime.Today.ToString("ddMMyyyy"), attribute.SerializeValue(ZDateTime.Today));
			});
		}
	}

	sealed class MessageFieldDateHHMMSSRepresentationAttributeAttributeTest : TestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				var attribute = new MessageFieldDateHHMMSSRepresentationAttribute();
				AssertEquals("Length should be correctly set via constructor.", 6, attribute.Length);
				AssertEquals("Format Data should be correctly set via constructor.", "HHmmss", attribute.FormatDate);
				AssertEquals("IsFixedLength should be correctly set via constructor.", true, attribute.IsFixedLength);
			});
		}

		public void TestSerialize()
		{
			var attribute = new MessageFieldDateHHMMSSRepresentationAttribute();

			CombineAssertions("SerializeValue method", () =>
			{
				AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => attribute.SerializeValue(null));
				AssertExceptionThrown<InvalidCastException>("Input value must be ZDate or ZDateTime", () => attribute.SerializeValue(new ZString("aaa")));
				AssertEquals("SerializeValue method return value", ZDate.Today.ToString("HHmmss"), attribute.SerializeValue(ZDate.Today));
				AssertEquals("SerializeValue method return value", ZDateTime.Today.ToString("HHmmss"), attribute.SerializeValue(ZDateTime.Today));
			});
		}
	}

	sealed class MessageFieldDateDDMMYYRepresentationAttributeAttributeTest : TestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				var attribute = new MessageFieldDateDDMMYYRepresentationAttribute();
				AssertEquals("Length should be correctly set via constructor.", 6, attribute.Length);
				AssertEquals("Format Data should be correctly set via constructor.", "ddMMyy", attribute.FormatDate);
				AssertEquals("IsFixedLength should be correctly set via constructor.", true, attribute.IsFixedLength);
			});
		}

		public void TestSerialize()
		{
			var attribute = new MessageFieldDateDDMMYYRepresentationAttribute();

			CombineAssertions("SerializeValue method", () =>
			{
				AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => attribute.SerializeValue(null));
				AssertExceptionThrown<InvalidCastException>("Input value must be ZDate or ZDateTime", () => attribute.SerializeValue(new ZString("aaa")));
				AssertEquals("SerializeValue method return value", ZDate.Today.ToString("ddMMyy"), attribute.SerializeValue(ZDate.Today));
				AssertEquals("SerializeValue method return value", ZDateTime.Today.ToString("ddMMyy"), attribute.SerializeValue(ZDateTime.Today));
			});
		}
	}

	sealed class MessageFieldDateYYYYMMDDHHMMRepresentationAttributeTest : TestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				var attribute = new MessageFieldDateYYYYMMDDHHMMRepresentationAttribute();
				AssertEquals("Length should be correctly set via constructor.", 12, attribute.Length);
				AssertEquals("Format Data should be correctly set via constructor.", "yyyyMMddHHmm", attribute.FormatDate);
				AssertEquals("IsFixedLength should be correctly set via constructor.", true, attribute.IsFixedLength);
			});
		}

		public void TestSerialize()
		{
			var attribute = new MessageFieldDateYYYYMMDDHHMMRepresentationAttribute();

			CombineAssertions("SerializeValue method", () =>
			{
				AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => attribute.SerializeValue(null));
				AssertExceptionThrown<InvalidCastException>("Input value must be ZDate or ZDateTime", () => attribute.SerializeValue(new ZString("aaa")));
				AssertEquals("SerializeValue method return value", ZDate.Today.ToString("yyyyMMddHHmm"), attribute.SerializeValue(ZDate.Today));
				AssertEquals("SerializeValue method return value", ZDateTime.Today.ToString("yyyyMMddHHmm"), attribute.SerializeValue(ZDateTime.Today));
			});
		}
	}
}
