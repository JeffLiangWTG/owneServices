using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	sealed class BillingTimeConverterTest : TestCase
	{
		public void TestConvertUtcToTimeInBillingTimeZone()
		{
			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			productRegistrationKey.CurrentBillingTimeZoneUtcOffsetForTest = 11.0d;
			productRegistrationKey.NextBillingTimeZoneUtcOffsetForTest = 10.0d;
			productRegistrationKey.NextUtcOffsetEffectiveTimeUtcForTest = new DateTime(2016, 4, 2, 16, 0, 0);

			var converter = new BillingTimeConverter();
			AssertEquals(ZDateTime.Empty, converter.ConvertUtcToTimeInBillingTimeZone(ZDateTime.Invalid));
			AssertEquals(new ZDateTime(2016, 1, 29, 13, 12, 0), converter.ConvertUtcToTimeInBillingTimeZone(new ZDateTime(2016, 1, 29, 2, 12, 0)));
			AssertEquals(new ZDateTime(2016, 4, 3, 2, 59, 59), converter.ConvertUtcToTimeInBillingTimeZone(new ZDateTime(2016, 4, 2, 15, 59, 59)));
			AssertEquals(new ZDateTime(2016, 4, 3, 2, 0, 0), converter.ConvertUtcToTimeInBillingTimeZone(new ZDateTime(2016, 4, 2, 16, 0, 0)));
			AssertEquals(new ZDateTime(2016, 5, 1, 0, 0, 0), converter.ConvertUtcToTimeInBillingTimeZone(new ZDateTime(2016, 4, 30, 14, 0, 0)));
		}

		public void TestConvertTimeInBillingTimeZoneToUtc()
		{
			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			productRegistrationKey.CurrentBillingTimeZoneUtcOffsetForTest = 11.0d;
			productRegistrationKey.NextBillingTimeZoneUtcOffsetForTest = 10.0d;
			productRegistrationKey.NextUtcOffsetEffectiveTimeUtcForTest = new DateTime(2016, 4, 2, 16, 0, 0);

			var converter = new BillingTimeConverter();
			AssertEquals(ZDateTime.Empty, converter.ConvertTimeInBillingTimeZoneToUtc(ZDateTime.Invalid));
			AssertEquals(new ZDateTime(2016, 1, 29, 2, 12, 0), converter.ConvertTimeInBillingTimeZoneToUtc(new ZDateTime(2016, 1, 29, 13, 12, 0)));
			AssertEquals(new ZDateTime(2016, 4, 2, 16, 59, 59), converter.ConvertTimeInBillingTimeZoneToUtc(new ZDateTime(2016, 4, 3, 2, 59, 59)));
			AssertEquals(new ZDateTime(2016, 4, 2, 17, 0, 0), converter.ConvertTimeInBillingTimeZoneToUtc(new ZDateTime(2016, 4, 3, 3, 0, 0)));
			AssertEquals(new ZDateTime(2016, 4, 30, 14, 0, 0), converter.ConvertTimeInBillingTimeZoneToUtc(new ZDateTime(2016, 5, 1, 0, 0, 0)));
		}
	}
}
