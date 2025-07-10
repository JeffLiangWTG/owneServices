using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeFactoryFormatDateTimeTest : TestCase
	{
		public void TestFormatDateTime()
		{
			DateTime aDateTime = new DateTime(2003, 03, 02, 13, 45, 55);

			string stringDate = EnvProxy.Instance.Time.FormatDate(aDateTime);
			string stringDateTime = EnvProxy.Instance.Time.FormatDateTime(aDateTime);
			string stringDateTimeWithSeconds = EnvProxy.Instance.Time.FormatDateTimeWithSeconds(aDateTime);
			string stringTime = EnvProxy.Instance.Time.FormatTime(aDateTime);
			string stringTimeWithSeconds = EnvProxy.Instance.Time.FormatTimeWithSeconds(aDateTime);
			string sqlStringDate = SqlFormatInfo.ToSqlDateString(aDateTime);
			string sqlStringDateTime = SqlFormatInfo.ToSqlDateTimeString(aDateTime);

			AssertEquals("Format should be dd-MMM-yy", "02-Mar-03", stringDate);
			AssertEquals("Format should be dd-MMM-yy HH:mm", "02-Mar-03 13:45", stringDateTime);
			AssertEquals("Format should be dd-MMM-yy HH:mm:ss", "02-Mar-03 13:45:55", stringDateTimeWithSeconds);
			AssertEquals("Format should be HH:mm", "13:45", stringTime);
			AssertEquals("Format should be HH:mm:ss", "13:45:55", stringTimeWithSeconds);
			AssertEquals("Format should be yyyy-MM-dd", "2003-03-02", sqlStringDate);
			AssertEquals("Format should be yyyy-MM-dd HH:mm:ss", "2003-03-02 13:45:55.000", sqlStringDateTime);
		}
	}
}
