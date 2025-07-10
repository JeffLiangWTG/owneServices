using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class AddInfoCollectionExtensionsTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestAddAddInfo_IAddInfoCollectionParent()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertNull("PreCondtion", shipment.AddInfoCollection);
			shipment.AddAddInfo("GREETING", ZString.Empty);
			AssertNull("Don't create when empty value", shipment.AddInfoCollection);
			shipment.AddAddInfo("GREETING", ZString.Empty, false);
			AssertNull("Don't create when empty value", shipment.AddInfoCollection);
			shipment.AddAddInfo("GREETING", ZString.Empty, true);
			var addInfoCollection = shipment.AddInfoCollection;
			AssertEquals("One added", 1, addInfoCollection.Count);
			AssertAddInfo(addInfoCollection[0], "GREETING", ZString.Empty);
		}

		public void TestAddAddInfo_AddInfoList()
		{
			var addInfoCollection = new List<AddInfo>();
			addInfoCollection.AddAddInfo("GREETING", ZString.Empty);
			AssertEquals("Don't create when empty value", 0, addInfoCollection.Count);
			addInfoCollection.AddAddInfo("GREETING", ZString.Empty, false);
			AssertEquals("Don't create when empty value", 0, addInfoCollection.Count);
			addInfoCollection.AddAddInfo("GREETING", ZString.Empty, true);
			AssertEquals("One added", 1, addInfoCollection.Count);
			AssertAddInfo(addInfoCollection[0], "GREETING", ZString.Empty);
		}

		void AssertAddInfo(AddInfo addInfo, ZString key, ZString value)
		{
			AssertEquals("addInfo.Key", key, addInfo.Key);
			AssertEquals("addInfo.Value", value, addInfo.Value);
		}

		public void TestGetZStringValue()
		{
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZStringValue("HELLO");
			AssertEquals(false, value.HasValue);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZStringValue("HELLO");
			AssertEquals(false, value.HasValue);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZStringValue("HELLO");
			AssertEquals(false, value.HasValue);
			addInfo.Value = "HI";
			value = addInfos.GetZStringValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals("HI", value.Value);
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "BYE" });
			value = addInfos.GetZStringValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals("HI", value.Value);
			addInfos.Remove(addInfo);
			value = addInfos.GetZStringValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals("BYE", value.Value);
		}

		public void TestGetZDecimalValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZDecimalValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDecimalValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZDecimalValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDecimalValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZDecimalValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDecimalValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			value = addInfos.GetZDecimalValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDecimalValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
#if NETFRAMEWORK
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Input string was not in a correct format. (in ConvertFrom, value = 'HI')", logger.Logs);
#else
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
The input string 'HI' was not in a correct format. (in ConvertFrom, value = 'HI')", logger.Logs);
#endif
			logger.ClearLogs();
			addInfo.Value = "45";
			value = addInfos.GetZDecimalValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(45m, value.Value);
			value = addInfos.GetZDecimalValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(45m, value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "45.586";
			value = addInfos.GetZDecimalValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(45.586m, value.Value);
			value = addInfos.GetZDecimalValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(45.586m, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "86.45" });
			value = addInfos.GetZDecimalValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(45.586m, value.Value);
			value = addInfos.GetZDecimalValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(45.586m, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZDecimalValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(86.45m, value.Value);
			value = addInfos.GetZDecimalValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(86.45m, value.Value);
			AssertEquals("", logger.Logs);
		}

		public void TestGetZBoolValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZBoolValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZBoolValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZBoolValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZBoolValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZBoolValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZBoolValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			value = addInfos.GetZBoolValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZBoolValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Cannot initialise a CargoWise.Types.ZBool with <HI> (CargoWise.Types.ZString).", logger.Logs);
			logger.ClearLogs();
			addInfo.Value = "Y";
			value = addInfos.GetZBoolValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(ZBool.True, value.Value);
			value = addInfos.GetZBoolValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(ZBool.True, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "N" });
			value = addInfos.GetZBoolValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(ZBool.True, value.Value);
			value = addInfos.GetZBoolValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(ZBool.True, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZBoolValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(ZBool.False, value.Value);
			value = addInfos.GetZBoolValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(ZBool.False, value.Value);
			AssertEquals("", logger.Logs);
		}

		public void TestGetZDateTimeValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZDateTimeValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDateTimeValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZDateTimeValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDateTimeValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZDateTimeValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDateTimeValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			value = addInfos.GetZDateTimeValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDateTimeValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Cannot initialise a CargoWise.Types.ZDateTime with <HI> (CargoWise.Types.ZString).", logger.Logs);
			logger.ClearLogs();
			addInfo.Value = "2013-9-19";
			value = addInfos.GetZDateTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 9, 19), value.Value);
			value = addInfos.GetZDateTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 9, 19), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "2013-9-19 11:34:32";
			value = addInfos.GetZDateTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 9, 19, 11, 34, 32), value.Value);
			value = addInfos.GetZDateTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 9, 19, 11, 34, 32), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "2013-9-19 11:34:32.012";
			value = addInfos.GetZDateTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 9, 19, 11, 34, 32, 12), value.Value);
			value = addInfos.GetZDateTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 9, 19, 11, 34, 32, 12), value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "2013-11-25" });
			value = addInfos.GetZDateTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 9, 19, 11, 34, 32, 12), value.Value);
			value = addInfos.GetZDateTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 9, 19, 11, 34, 32, 12), value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZDateTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 11, 25), value.Value);
			value = addInfos.GetZDateTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTime(2013, 11, 25), value.Value);
			AssertEquals("", logger.Logs);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestGetZDateTimeOffsetValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Cannot initialise a CargoWise.Types.ZDateTimeOffset with <HI> (CargoWise.Types.ZString).", logger.Logs);
			logger.ClearLogs();
			addInfo.Value = "2013-9-19";
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 0, 0, 0, TimeSpan.FromHours(10)), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 0, 0, 0, TimeSpan.FromHours(10)), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "2013-9-19 11:34:32";
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, TimeSpan.FromHours(10)), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, TimeSpan.FromHours(10)), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "2013-9-19 11:34:32.012";
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.FromHours(10)), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.FromHours(10)), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "2013-9-19 11:34:32.012-10:00";
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.FromHours(-10)), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.FromHours(-10)), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "2013-9-19T11:34:32.012";
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.FromHours(10)), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.FromHours(10)), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "2013-9-19T11:34:32.012-10:00";
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.FromHours(-10)), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.FromHours(-10)), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "2013-9-19T11:34:32.012Z";
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.Zero), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.Zero), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "2013-9-19 11:34:32.012Z";
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.Zero), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.Zero), value.Value);
			AssertEquals("", logger.Logs);

			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "2013-11-25T10:10:10.123+08:00" });
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.Zero), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 9, 19, 11, 34, 32, 12, TimeSpan.Zero), value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZDateTimeOffsetValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 11, 25, 10, 10, 10, 123, TimeSpan.FromHours(8)), value.Value);
			value = addInfos.GetZDateTimeOffsetValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZDateTimeOffset(2013, 11, 25, 10, 10, 10, 123, TimeSpan.FromHours(8)), value.Value);
			AssertEquals("", logger.Logs);
		}

		public void TestGetZTimeValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZTimeValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZTimeValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZTimeValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZTimeValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZTimeValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZTimeValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			value = addInfos.GetZTimeValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZTimeValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Cannot initialise a CargoWise.Types.ZTime with <HI> (CargoWise.Types.ZString).", logger.Logs);
			logger.ClearLogs();
			addInfo.Value = "13:09";
			value = addInfos.GetZTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(13, 9), value.Value);
			value = addInfos.GetZTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(13, 9), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "11:34:32";
			value = addInfos.GetZTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(11, 34), value.Value);
			value = addInfos.GetZTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(11, 34), value.Value);
			AssertEquals("", logger.Logs);
			addInfo.Value = "11:34:32.012";
			value = addInfos.GetZTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(11, 34), value.Value);
			value = addInfos.GetZTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(11, 34), value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "13:11:25" });
			value = addInfos.GetZTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(11, 34), value.Value);
			value = addInfos.GetZTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(11, 34), value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZTimeValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(13, 11), value.Value);
			value = addInfos.GetZTimeValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZTime(13, 11), value.Value);
			AssertEquals("", logger.Logs);
		}

		public void TestGetZGeographyValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZGeographyValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZGeographyValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZGeographyValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZGeographyValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZGeographyValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZGeographyValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			value = addInfos.GetZGeographyValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZGeographyValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Cannot initialise a CargoWise.Types.ZGeography with <HI> (System.String).", logger.Logs);
			logger.ClearLogs();
			addInfo.Value = "-121 48";
			value = addInfos.GetZGeographyValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZGeography("-121 48"), value.Value);
			value = addInfos.GetZGeographyValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZGeography("-121 48"), value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "POINT (-121.2 48.9)" });
			value = addInfos.GetZGeographyValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZGeography("-121 48"), value.Value);
			value = addInfos.GetZGeographyValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZGeography("-121 48"), value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZGeographyValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZGeography("-121.2 48.9"), value.Value);
			value = addInfos.GetZGeographyValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(new ZGeography("-121.2 48.9"), value.Value);
			AssertEquals("", logger.Logs);
		}

		public void TestGetZIntValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZIntValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZIntValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZIntValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZIntValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZIntValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZIntValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			value = addInfos.GetZIntValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZIntValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
#if NETFRAMEWORK
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Input string was not in a correct format. (in ConvertFrom, value = 'HI')", logger.Logs);
#else
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
The input string 'HI' was not in a correct format. (in ConvertFrom, value = 'HI')", logger.Logs);
#endif
			logger.ClearLogs();
			addInfo.Value = "58";
			value = addInfos.GetZIntValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(58, value.Value);
			value = addInfos.GetZIntValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(58, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "89" });
			value = addInfos.GetZIntValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(58, value.Value);
			value = addInfos.GetZIntValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(58, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZIntValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(89, value.Value);
			value = addInfos.GetZIntValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(89, value.Value);
			AssertEquals("", logger.Logs);
		}

		public void TestGetZLongValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZLongValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZLongValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZLongValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZLongValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZLongValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZLongValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZLongValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
#if NETFRAMEWORK
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Input string was not in a correct format. (in ConvertFrom, value = 'HI')", logger.Logs);
#else
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
The input string 'HI' was not in a correct format. (in ConvertFrom, value = 'HI')", logger.Logs);
#endif
			logger.ClearLogs();
			addInfo.Value = "58";
			value = addInfos.GetZLongValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals((ZLong)58, value.Value);
			value = addInfos.GetZLongValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals((ZLong)58, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "89" });
			value = addInfos.GetZLongValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals((ZLong)58, value.Value);
			value = addInfos.GetZLongValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals((ZLong)58, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZLongValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals((ZLong)89, value.Value);
			value = addInfos.GetZLongValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals((ZLong)89, value.Value);
			AssertEquals("", logger.Logs);
		}

		public void TestGetZShortValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZShortValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZShortValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZShortValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZShortValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZShortValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZShortValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZShortValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
#if NETFRAMEWORK
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Input string was not in a correct format. (in ConvertFrom, value = 'HI')", logger.Logs);
#else
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
The input string 'HI' was not in a correct format. (in ConvertFrom, value = 'HI')", logger.Logs);
#endif
			logger.ClearLogs();
			addInfo.Value = "58";
			value = addInfos.GetZShortValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals((ZShort)58, value.Value);
			value = addInfos.GetZShortValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals((ZShort)58, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = "89" });
			value = addInfos.GetZShortValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals((ZShort)58, value.Value);
			value = addInfos.GetZShortValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals((ZShort)58, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZShortValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals((ZShort)89, value.Value);
			value = addInfos.GetZShortValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals((ZShort)89, value.Value);
			AssertEquals("", logger.Logs);
		}

		public void TestGetZGuidValue()
		{
			var logger = new TestErrorLogger();
			List<AddInfo> addInfos = null;
			var value = addInfos.GetZGuidValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZGuidValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfos = new List<AddInfo>();
			value = addInfos.GetZGuidValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZGuidValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			var addInfo = new AddInfo() { Key = "HELLO" };
			addInfos.Add(addInfo);
			value = addInfos.GetZGuidValue("HELLO");
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZGuidValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
			AssertEquals("", logger.Logs);
			addInfo.Value = "HI";
			AssertEquals(false, value.HasValue);
			value = addInfos.GetZGuidValue("HELLO", logger);
			AssertEquals(false, value.HasValue);
#if NETFRAMEWORK
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx). (in ConvertFrom, value = 'HI')", logger.Logs);
#else
			AssertEquals(@"Warning - AddInfo Key (HELLO) has a invalid value (HI):
Unrecognized Guid format. (in ConvertFrom, value = 'HI')", logger.Logs);
#endif
			logger.ClearLogs();
			var guid = ZGuid.NewZGuid();
			addInfo.Value = guid.ToString();
			value = addInfos.GetZGuidValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(guid, value.Value);
			value = addInfos.GetZGuidValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(guid, value.Value);
			AssertEquals("", logger.Logs);
			var guid2 = ZGuid.NewZGuid();
			addInfos.Add(new AddInfo() { Key = "HELLO", Value = guid2.ToString() });
			value = addInfos.GetZGuidValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(guid, value.Value);
			value = addInfos.GetZGuidValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(guid, value.Value);
			AssertEquals("", logger.Logs);
			addInfos.Remove(addInfo);
			value = addInfos.GetZGuidValue("HELLO");
			AssertEquals(true, value.HasValue);
			AssertEquals(guid2, value.Value);
			value = addInfos.GetZGuidValue("HELLO", logger);
			AssertEquals(true, value.HasValue);
			AssertEquals(guid2, value.Value);
			AssertEquals("", logger.Logs);
		}
	}
}
