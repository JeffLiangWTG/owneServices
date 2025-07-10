using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class EnumStringConverterTest : TestCase
	{
		enum TestEnum
		{
			aaa, bbb, ccc
		}

		public void TestConvert()
		{
			var notification = new Notification(new NotificationType("n1", 0, false, "aaa"), "moo");
			AssertEquals(TestEnum.aaa, EnumStringConverter.ConvertStringToEnumEntry(notification, TestEnum.ccc));

			notification = new Notification(new NotificationType("n1", 0, false, "bbb"), "moo");
			AssertEquals(TestEnum.bbb, EnumStringConverter.ConvertStringToEnumEntry(notification, TestEnum.ccc));

			notification = new Notification(new NotificationType("n1", 0, false, "zzz"), "moo");
			AssertEquals(TestEnum.ccc, EnumStringConverter.ConvertStringToEnumEntry(notification, TestEnum.ccc));
		}
	}
}
