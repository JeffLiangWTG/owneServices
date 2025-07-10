using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Common.ZA.Testing
{
	class ZAMessageStatusListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsError()
		{
			var list = new ZAMessageStatusList();
			list.RemoveCode(ZAMessageStatusList.Codes.Error);
			foreach (ICodeDescription pair in list)
			{
				NUnit.Framework.Assert.That(ZAMessageStatusList.IsError(pair.Code), Is.EqualTo(false), pair.Code);
			}
			NUnit.Framework.Assert.That(ZAMessageStatusList.IsError(ZAMessageStatusList.Codes.Error), Is.EqualTo(true), ZAMessageStatusList.Codes.Error);
		}

		[ExpectNoExceptions]
		public void TestIsAwaiting()
		{
			var list = new ZAMessageStatusList();
			list.RemoveCode(ZAMessageStatusList.Codes.AwaitingResponse);
			foreach (ICodeDescription pair in list)
			{
				NUnit.Framework.Assert.That(ZAMessageStatusList.IsAwaiting(pair.Code), Is.EqualTo(false), pair.Code);
			}
			NUnit.Framework.Assert.That(ZAMessageStatusList.IsAwaiting(ZAMessageStatusList.Codes.AwaitingResponse), Is.EqualTo(true), ZAMessageStatusList.Codes.AwaitingResponse);
		}

		[ExpectNoExceptions]
		public void TestIsAcknowledged()
		{
			var list = new ZAMessageStatusList();
			list.RemoveCode(ZAMessageStatusList.Codes.Acknowledged);
			foreach (ICodeDescription pair in list)
			{
				NUnit.Framework.Assert.That(ZAMessageStatusList.IsAcknowledged(pair.Code), Is.EqualTo(false), pair.Code);
			}
			NUnit.Framework.Assert.That(ZAMessageStatusList.IsAcknowledged(ZAMessageStatusList.Codes.Acknowledged), Is.EqualTo(true), ZAMessageStatusList.Codes.Acknowledged);
		}
	}
}
