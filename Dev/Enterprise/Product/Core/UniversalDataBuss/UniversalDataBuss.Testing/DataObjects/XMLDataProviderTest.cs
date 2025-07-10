using System;
using CargoWise.Application;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	public class XMLDataProviderTest : TestCase
	{
		public void TestTimestamp()
		{
			var xmlDataProvider = ObjectFactory.Get<IXMLDataProvider>();

			xmlDataProvider.SetUseDefaultValueForTest(true);
			AssertEquals(XMLDataProviderConstant.DefaultTimestamp, xmlDataProvider.Timestamp);

			xmlDataProvider.SetUseDefaultValueForTest(false);
			AssertEquals(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), xmlDataProvider.Timestamp);
		}

		public void TestIgnoreTimestamp()
		{
			var xmlDataProvider = ObjectFactory.Get<IXMLDataProvider>();

			xmlDataProvider.SetIgnoreTimestampForTest(true);
			Assert(xmlDataProvider.IgnoreTimestamp);

			xmlDataProvider.SetIgnoreTimestampForTest(false);
			Assert(!xmlDataProvider.IgnoreTimestamp);
		}
	}
}
