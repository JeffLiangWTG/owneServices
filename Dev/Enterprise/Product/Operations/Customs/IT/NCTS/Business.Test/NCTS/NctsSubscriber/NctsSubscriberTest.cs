using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsSubscriber))]
sealed class NctsSubscriberTest : EnterpriseBusinessObjectTestCase
{
	public void TestNctsHeader()
	{
		var subscriber = Factory.New<NctsSubscriber>();
		subscriber.CP_BH_Header = ZGuid.Empty;
		AssertNull("Empty CP_BH_Header", subscriber.NctsHeader);

		var nctsHeader = Factory.New<NctsHeader>();
		subscriber.CP_BH_Header = nctsHeader.PK;
		AssertSame("Valid CP_BH_Header", nctsHeader, subscriber.NctsHeader);

		subscriber.CP_BH_Header = ZGuid.Invalid;
		AssertNull("Invalid CP_BH_Header", subscriber.NctsHeader);
	}

	public void TestDefaultCP_Type()
	{
		var subscriber = Factory.New<NctsSubscriber>();
		AssertEquals("CP_Type", "SBR", subscriber.CP_Type);
	}

	public void TestSetInvalidCP_TypeThrowsException()
	{
		var subscriber = Factory.New<NctsSubscriber>();
		AssertExceptionThrown<InvalidOperationException>("Empty", () => subscriber.CP_Type = ZString.Empty);
		AssertExceptionThrown<InvalidOperationException>("Invalid", () => subscriber.CP_Type = "XXX");
		AssertNoExceptionThrown("Valid", () => subscriber.CP_Type = "SBR");
	}

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name == NctsSubscriber.Schema.CP_Type)
		{
			AssertEquals("SBR", (ZString)info.Value);
		}
		else
		{
			base.TestBizObjectField(info);
		}
	}

	protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
	{
		get
		{
			var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
			if (!result.ContainsKey(NctsSubscriber.Schema.CP_Type))
			{
				result.Add(NctsSubscriber.Schema.CP_Type, new ZString("SBR"));
			}
			return result;
		}
	}
}
