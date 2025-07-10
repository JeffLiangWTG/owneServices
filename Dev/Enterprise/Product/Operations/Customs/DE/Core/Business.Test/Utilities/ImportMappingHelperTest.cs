using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportMappingHelperTest : TestCaseWithFactory
	{
		public void TestMapDeclarantTypeToMessaging()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ImportMappingHelper.MapDeclarantTypeToMessaging(RepresentationTypeList.Codes._1Self), "0");
				AssertEquals(ImportMappingHelper.MapDeclarantTypeToMessaging(RepresentationTypeList.Codes._2Direct), "1");
				AssertEquals(ImportMappingHelper.MapDeclarantTypeToMessaging(RepresentationTypeList.Codes._3Indirect), "2");
			});
		}

		public void TestMapDeclarantTypeToMessagingWithoutIndirect()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ImportMappingHelper.MapDeclarantTypeToMessagingWithoutIndirect(RepresentationTypeList.Codes._1Self), "0");
				AssertEquals(ImportMappingHelper.MapDeclarantTypeToMessagingWithoutIndirect(RepresentationTypeList.Codes._2Direct), "1");
				AssertEquals(ImportMappingHelper.MapDeclarantTypeToMessagingWithoutIndirect(RepresentationTypeList.Codes._3Indirect), ZString.Empty);
			});
		}

		public void TestGetDestinationFederalState_DE()
		{
			var expectedMappingDictionary = new Dictionary<string, string>()
			{
				{ "BW", "08" },
				{ "BY", "09" },
				{ "BE", "11" },
				{ "BB", "12" },
				{ "HB", "04" },
				{ "HH", "02" },
				{ "HE", "06" },
				{ "MV", "13" },
				{ "NI", "03" },
				{ "NW", "05" },
				{ "RP", "07" },
				{ "SL", "10" },
				{ "SN", "14" },
				{ "ST", "15" },
				{ "SH", "01" },
				{ "TH", "16" },
				{ "XX", string.Empty }
			};

			var declaration = Factory.New<JobDeclaration>();
			var loader = new RefUNLOCO.Loader(Factory);
			var berlin = loader.Load("DEBER");
			var state = Factory.New<RefCountryStates>();
			CombineAssertions(() =>
			{
				AssertEquals("No RefUNLOCO", ZString.Empty, ImportMappingHelper.GetDestinationFederalState(declaration, Core.Constants.CountryCodes.Germany));

				declaration.JE_RL_NKFinalDestination = "DEBER";
				berlin.RL_RW = ZGuid.Empty;
				AssertEquals("No RefCountryStates", ZString.Empty, ImportMappingHelper.GetDestinationFederalState(declaration, Core.Constants.CountryCodes.Germany));

				berlin.RL_RW = state.PK;
				foreach (var expectedItem in expectedMappingDictionary)
				{
					var stateCode = expectedItem.Key;
					state.RW_Code = stateCode;
					AssertEquals($"RW_Code = {stateCode}", expectedItem.Value, ImportMappingHelper.GetDestinationFederalState(declaration, Core.Constants.CountryCodes.Germany));
				}
			});
		}

		public void TestGetDestinationFederalState_NotDE()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Empty", "25", ImportMappingHelper.GetDestinationFederalState(declaration, ZString.Empty));
				AssertEquals("Not empty or DE", "25", ImportMappingHelper.GetDestinationFederalState(declaration, Core.Constants.CountryCodes.Australia));
			});
		}
	}
}
