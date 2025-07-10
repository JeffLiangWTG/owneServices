using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExportJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			AssertSame("Cached", Factory.GetCachedValue<IELogicalStatusList>(), lookups.MessageStatusList);
		}

		public void TestEntryStatusList()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "AMR, CAR, CAN, COX, CON, DRJ, EXJ, EXP, MRN, MLT, PEN, PRE, PRD, REF, REJ, EXR, REL, REQ", lookups.EntryStatusList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<AESEntryStatusList>(), lookups.EntryStatusList);
			});
		}

		public void TestGoodsOrigin()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "Country");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "CN", "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "IE", "IE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var list = (CodeDescriptionPairList)lookups.GoodsOrigin;

			AssertSame("Cached", list, declaration.Lookups.GoodsOrigin);
			AssertEquals("Code List", "AU, CN, IE", list.CodesAsString);
		}

		public void TestGoodsDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "Country");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "AU", "AU Name", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "CN", "CN Name", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "IE", "IE Name", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			var list = (CodeDescriptionPairList)lookups.GoodsDestination;

			AssertSame("Cached", list, declaration.Lookups.GoodsDestination);
			AssertEquals("Code List", "AU, CN, IE", list.CodesAsString);

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			list = (CodeDescriptionPairList)lookups.GoodsDestination;
			AssertEquals("Contain IE", true, list.ContainsCode("IE"));
			AssertEquals("Contain DE", true, list.ContainsCode("DE"));
			AssertEquals("Not Contain AU", false, list.ContainsCode("AU"));
			AssertEquals("Not Contain CN", false, list.ContainsCode("CN"));
			AssertSame("Cached", list, declaration.Lookups.GoodsDestination);
		}

		public void TestLocationQualifierList()
		{
			var locationQualifierList = lookups.LocationQualifierList;
			CombineAssertions(() =>
			{
				AssertEquals("List should have 1 items.", 1, locationQualifierList.Count);
				AssertEquals("List should have item U", true, locationQualifierList.ContainsCode("U"));

				var newDeclaration = Factory.New<JobDeclaration>();
				newDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertSame("List should have been cached.", locationQualifierList, newDeclaration.Lookups.LocationQualifierList);
			});
		}

		public void TestMessageSubTypeList()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "CO, EX", lookups.MessageSubTypeList.CodesAsString);
				AssertSame("Cached", lookups.MessageTypeList, lookups.MessageTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			lookups = (ExportJobDeclarationLookups)declaration.Lookups;
		}
		JobDeclaration declaration;
		ExportJobDeclarationLookups lookups;
	}
}
