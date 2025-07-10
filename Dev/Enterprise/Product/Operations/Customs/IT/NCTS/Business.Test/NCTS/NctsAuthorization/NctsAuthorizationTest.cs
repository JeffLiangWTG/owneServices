using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsAuthorization))]
sealed class NctsAuthorizationTest : CusReferenceAbstractTest<NctsAuthorization>
{
	public void TestDefaultCFR_Type()
	{
		var authorization = Factory.New<NctsAuthorization>();
		AssertEquals("CFR_Type", "NCT", authorization.CFR_Type);
	}

	public void TestDefaultCFR_Code()
	{
		var authorization = Factory.New<NctsAuthorization>();
		AssertEquals("CFR_Code", "AUT", authorization.CFR_Code);
	}

	public void TestSetInvalidCFR_TypeThrowsException()
	{
		var authorization = Factory.New<NctsAuthorization>();
		AssertExceptionThrown<InvalidOperationException>("Empty", () => authorization.CFR_Type = ZString.Empty);
		AssertExceptionThrown<InvalidOperationException>("Invalid", () => authorization.CFR_Type = "XXX");
		AssertNoExceptionThrown("Valid", () => authorization.CFR_Type = "NCT");
	}

	public void TestSetInvalidCFR_CodeThrowsException()
	{
		var authorization = Factory.New<NctsAuthorization>();
		AssertExceptionThrown<InvalidOperationException>("Empty", () => authorization.CFR_Code = ZString.Empty);
		AssertExceptionThrown<InvalidOperationException>("Invalid", () => authorization.CFR_Code = "XXX");
		AssertNoExceptionThrown("Valid", () => authorization.CFR_Code = "AUT");
	}

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name == NctsAuthorization.Schema.CFR_Type)
		{
			AssertEquals("NCT", (ZString)info.Value);
		}
		else if (info.Name == NctsAuthorization.Schema.CFR_Code)
		{
			AssertEquals("AUT", (ZString)info.Value);
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
			if (!result.ContainsKey(NctsAuthorization.Schema.CFR_Type))
			{
				result.Add(NctsAuthorization.Schema.CFR_Type, new ZString("NCT"));
			}
			if (!result.ContainsKey(NctsAuthorization.Schema.CFR_Code))
			{
				result.Add(NctsAuthorization.Schema.CFR_Code, new ZString("AUT"));
			}

			return result;
		}
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewAuthorizationForTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewAuthorizationForTest(factory);

	NctsAuthorization GetNewAuthorizationForTest(BusinessObjectFactory factory)
	{
		var authorization = factory.New<NctsAuthorization>();
		var nctsHeader = factory.New<NctsHeader>();
		authorization.CFR_ParentTableCode = nctsHeader.TablePrefix;
		authorization.CFR_ParentID = nctsHeader.PK;
		authorization.CFR_Reference = "X";
		return authorization;
	}
}
