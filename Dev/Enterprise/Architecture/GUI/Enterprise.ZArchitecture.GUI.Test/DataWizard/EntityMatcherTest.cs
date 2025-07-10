using System.Reflection;
using CargoWise.Application;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class EntityMatcherTest : TestCaseWithFactory
	{
		public void TestGetMatchingBusinessObject_NavigationPropertyIsInvalid()
		{
			var ex = AssertExceptionThrown<EntityMatchingException>(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IGlbBranch", new[] { ("SomeProperty.RN_Code", "US") }));
			AssertContains("IGlbBranch.SomeProperty", ex.Message);
		}

		public void TestGetMatchingBusinessObject_PropertyIsNotNavigationProperty()
		{
			var ex = AssertExceptionThrown<EntityMatchingException>(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IGlbBranch", new[] { ("GB_Fax.RN_Code", "US") }));
			AssertContains("IGlbBranch.GB_Fax", ex.Message);
		}

		public void TestGetMatchingBusinessObject_WhenInvalidConversion()
		{
			var shipment = Factory.New<ICommonShipment>();
			((BusinessObject)shipment)[JobShipmentSchema.JS_GoodsValue] = "123";
			Factory.Save();
			AssertSame(shipment, matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IJobShipment", new[] { ("JS_GoodsValue", "123") }));
		}

		public void TestGetMatchingBusinessObject_WhenFormatConversionError_ThrowException()
		{
			AssertExceptionThrown<EntityMatchingException>(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IJobShipment", new[] { ("JS_GoodsValue", "ABC") }));
		}

		public void TestGetMatchingBusinessObject_WhenZTypeValueException_ThrowException()
		{
			AssertExceptionThrown<EntityMatchingException>(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IJobShipment", new[] { ("JS_IsValid", "086 58070434") }));
		}

		public void TestGetMatchingBusinessObject_ExceptionWhenInvalidColumn()
		{
			AssertExceptionThrown<EntityMatchingException>(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IRefCountry", new[] { ("AA_Code", "US") }));
		}

		public void TestGetMatchingBusinessObject_DoesNotMatchByCodeAndDesc()
		{
			AssertEquals(
				"Could not find related IRefCountry.",
				AssertExceptionThrown<EntityMatchingException>(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IRefCountry", new[] { ("RN_Code", "US"), ("RN_Desc", "Australia") })).Message
			);
		}

		public void TestGetMatchingBusinessObject_DoesNotMatch_NoExceptionIfRelationIsCanCreate()
		{
			AssertNoExceptionThrown(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IRefCountry", new[] { ("RN_Code", "US"), ("RN_Desc", "Australia") }, true));
		}

		public void TestGetMatchingBusinessObject_MultipleMatches()
		{
			AssertEquals(
				"Found multiple related IRefUNLOCO.",
				AssertExceptionThrown<EntityMatchingException>(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IRefUNLOCO", new[] { ("RL_PortName", "Aberdeen") })).Message
			);
		}

		public void TestGetMatchingBusinessObject_AccChargeCode_WhenManyMatchesExist_ButOnlyOneForCurrentCompany_PickTheCurrentCompany()
		{
			var currentCompany = Env.CurrentCompany;
			var currentCompanyChargeCode = CreateChargeCode(currentCompany.PK, "CCC");
			var otherCompany = (Environment.ICompany)Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			CreateChargeCode(otherCompany.PK, "CCC");

			Factory.Save();

			var match = matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IAccChargeCode", new[] { ("AC_Code", "CCC") }).PK;
			AssertEquals("Should find the curent company charge code only", currentCompanyChargeCode.PK, match);
		}

		public void TestGetMatchingBusinessObject_AccChargeCode__WhenManyMatchesExist_ButNoneForCurrentCompany_PickTheGlobalChargeCode()
		{
			// Making a global charge code makes a local one for each company. Need to find and rename
			// the one for the current company so that it isnt found anymore

			var globalCompanyChargeCode = CreateChargeCode(ZGuid.Empty, "CAT");
			Factory.Save();

			var queryForCat = new ZQuery(AccChargeCodeSchema.AC_Code, "CAT");
			var queryForCurrentCompany = new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
			var currentCompanyChargeCode = Factory.LoadTop1<IAccChargeCode>(new ZQuery(queryForCat, queryForCurrentCompany));
			currentCompanyChargeCode.AC_Code = "DOG";
			Factory.Save();

			var match = matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IAccChargeCode", new[] { ("AC_Code", "CAT") }).PK;
			AssertEquals("Should find the global charge code as a match", globalCompanyChargeCode.PK, match);
		}

		public void TestGetMatchingBusinessObject_AccChargeCode__WhenManyMatchesExist_IncludingGlobalAndLocal_PickTheLocalChargeCode()
		{
			// Making a global charge code makes a local one for each company.
			var globalCompanyChargeCode = CreateChargeCode(ZGuid.Empty, "CAT");
			Factory.Save();

			var queryForCat = new ZQuery(AccChargeCodeSchema.AC_Code, "CAT");
			var queryForCurrentCompany = new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
			var currentCompanyChargeCode = Factory.LoadTop1<IAccChargeCode>(new ZQuery(queryForCat, queryForCurrentCompany));

			var match = matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IAccChargeCode", new[] { ("AC_Code", "CAT") }).PK;
			AssertEquals("Should find the local charge code as a match", currentCompanyChargeCode.PK, match);
		}

		BusinessObject CreateChargeCode(ZGuid companyPK, ZString chargeCode)
		{
			var accountBizo = (BusinessObject)Factory.New<IAccGLHeader>();
			accountBizo.FillWithValidTestData();

			var chargeCodeBizo = (BusinessObject)Factory.New<IAccChargeCode>();
			chargeCodeBizo.FillWithValidTestData();

			chargeCodeBizo[AccChargeCodeSchema.AC_GC] = companyPK;
			chargeCodeBizo[AccChargeCodeSchema.AC_Code] = chargeCode;
			chargeCodeBizo[AccChargeCodeSchema.AC_Desc] = "Some chargecode";
			chargeCodeBizo[AccChargeCodeSchema.AC_ChargeType] = "MRG";
			chargeCodeBizo[AccChargeCodeSchema.AC_MarginPercentage] = "100";
			chargeCodeBizo[AccChargeCodeSchema.AC_RateCalculator] = "FLT";
			chargeCodeBizo[AccChargeCodeSchema.AC_ChargeGroup] = "FRT";
			chargeCodeBizo[AccChargeCodeSchema.AC_ChargeSubGroup] = "";
			chargeCodeBizo[AccChargeCodeSchema.AC_ShowOnQuotation] = false;
			chargeCodeBizo[AccChargeCodeSchema.AC_SuppressOnQuoteIfZero] = false;
			chargeCodeBizo[AccChargeCodeSchema.AC_AG_AccrualAccount] = accountBizo.PK;
			chargeCodeBizo[AccChargeCodeSchema.AC_AG_CostAccount] = accountBizo.PK;
			chargeCodeBizo[AccChargeCodeSchema.AC_AG_RevenueAccount] = accountBizo.PK;
			chargeCodeBizo[AccChargeCodeSchema.AC_AG_WIPAccount] = accountBizo.PK;

			return chargeCodeBizo;
		}

		public void TestGetMatchingOrgAddress_MultipleMatches()
		{
			var org = Factory.New<IOrgHeader>();
			org.OH_Code = "Test OH_Code";
			var mainAddress = org.MainAddress;

			var address1 = Factory.New<IOrgAddress>();
			address1.OA_OH = org.PK;
			address1.OA_Address1 = "Test Address #1";

			Factory.Save();

			AssertEquals(mainAddress.PK, matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IOrgAddress", new[] { ("OrgHeader.OH_Code", "Test OH_Code") }).PK);
		}

		public void TestGetMatchingBusinessObject_SingleMatchWithExtraCondition()
		{
			var result = matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IRefUNLOCO", new[] { ("RL_PortName", "Aberdeen"), ("RL_RN_NKCountryCode", "GB") });
			AssertEquals("GBABD", CodePropertyAttribute.CodeFromBusinessObject(result));
		}

		public void TestGetMatchingBusinessObject_MatchByCode()
		{
			var result = matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IRefCountry", new[] { ("RN_Code", "US") });
			AssertEquals("US", CodePropertyAttribute.CodeFromBusinessObject(result));
		}

		public void TestGetMatchingBusinessObject_MatchByDesc()
		{
			var result = matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IRefCountry", new[] { ("RN_Desc", "United States") });
			AssertEquals("US", CodePropertyAttribute.CodeFromBusinessObject(result));
		}

		public void TestGetMatchingBusinessObject_MixedPath()
		{
			var nettingType = Assembly.Load("Enterprise.Accounting.Netting").GetType("Enterprise.Accounting.Netting.NettingOrganisation");

			var nett = Factory.NewWithValidTestData(nettingType);
			var org = Factory.LoadTop1<IOrgHeader>(new ZQuery());
			nett["NSO_OH_Organisation"] = org.PK;
			nett["NSO_NettingType"] = "GRS";

			Factory.Save();

			AssertSame(nett, matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "INettingOrganisation", new[]
			{
				("Organisation.OH_Code", org.OH_Code.ToString()),
				("NSO_NettingType", "GRS")
			}));
		}

		public void TestGetMatchingBusinessObject_MixedPathDoesNotMatch()
		{
			var nettingType = Assembly.Load("Enterprise.Accounting.Netting").GetType("Enterprise.Accounting.Netting.NettingOrganisation");

			var nett = Factory.NewWithValidTestData(nettingType);
			var org = Factory.LoadTop1<IOrgHeader>(new ZQuery());
			nett["NSO_OH_Organisation"] = org.PK;
			nett["NSO_NettingType"] = "GRS";

			Factory.Save();

			AssertExceptionThrown<EntityMatchingException>(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "INettingOrganisation", new[]
			{
				("Organisation.OH_Code", org.OH_Code.ToString()),
				("NSO_NettingType", "AAA")
			}));
		}

		public void TestGetMatchingBusinessObject_MatchComplexPath()
		{
			var nettingType = Assembly.Load("Enterprise.Accounting.Netting").GetType("Enterprise.Accounting.Netting.NettingOrganisation");

			var nett = Factory.NewWithValidTestData(nettingType);
			var org = Factory.LoadTop1<IOrgHeader>(new ZQuery());
			nett["NSO_OH_Organisation"] = org.PK;

			Factory.Save();

			AssertSame(nett, matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "INettingOrganisation", new[]
			{
				("Organisation.OH_Code", org.OH_Code.ToString()),
				("Organisation.OH_FullName", org.OH_FullName.ToString())
			}));
		}

		public void TestGetMatchingBusinessObject_WhenOverflowException_ThrowException()
		{
			AssertExceptionThrown<EntityMatchingException>(() => matcher.GetMatchingBusinessObject(new EntityMatcherContext(Factory, mappingDataModel), "IJobShipment", new[] { ("JS_GoodsValue", "1111111111111111111111111111111111111111111111111111111111111111111111111111") }));
		}

		readonly EntityMatcher matcher = new EntityMatcher();

		readonly MappingDataModel mappingDataModel = new MappingDataModel(new MappingDataDefinition[]
		{
			new MappingDataDefinition("IGlbBranch", "GB"),
			new MappingDataDefinition("IOrgHeader", "OH"),
			new MappingDataDefinition("IOrgAddress", "OA").AddRelation("OrgHeader", "IOrgHeader", "OA_OH"),
			new MappingDataDefinition("IHVLVItem", "GB"),
			new MappingDataDefinition("IJobShipment", "JS"),
			new MappingDataDefinition("IRefCountry", "RN"),
			new MappingDataDefinition("IRefUNLOCO", "RL"),
			new MappingDataDefinition("IAccChargeCode", "AC").AddRelation("GlbCompany", "IGlbCompany", "AC_GC"),
			new MappingDataDefinition("INettingOrganisation", "NSO").AddRelation("Organisation", "IOrgHeader", "NSO_OH_Organisation"),
		});
	}
}
