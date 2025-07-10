using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Module.Testing
{
	sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestMessageStatusList()
		{
			var localPairList = new Common.Shared.EntryStatusList();
			var sharedPairList = new Common.Shared.MessageStatusList();

			var expectedCodes = localPairList.GetAllCodes().Union(sharedPairList.GetAllCodes().Where(x => !string.IsNullOrEmpty(x)));
			var actualPairList = lookups.MessageStatusList();
			AssertContainsExactElementsInAnyOrder(actualPairList.GetAllCodes(), expectedCodes);

			foreach (ICodeDescription pair in localPairList)
			{
				AssertContains(pair.Description, actualPairList.GetDescriptionFromCode(pair.Code));
			}
			foreach (ICodeDescription pair in sharedPairList)
			{
				if (!string.IsNullOrEmpty(pair.Code))
				{
					AssertContains(pair.Description, actualPairList.GetDescriptionFromCode(pair.Code));
				}
			}
			var commonCodes = localPairList.GetAllCodes().Intersect(sharedPairList.GetAllCodes());
			foreach (var code in commonCodes)
			{
				var localDescription = localPairList.GetDescriptionFromCode(code);
				var sharedDescription = sharedPairList.GetDescriptionFromCode(code);
				if (localDescription == sharedDescription)
				{
					AssertEquals(localDescription, actualPairList.GetDescriptionFromCode(code));
				}
			}
		}

		public void TestEntryStatusList()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("AWR, B, CA, CAN, CRQ, CB, CC, CLR, CR, CS, CT, CU, CW, CX, FBK, HLD, MLT, NOT, RH1, RH2, RH3, RH5, RH6, RT0, 0H, 0P, 0X, RT1, R15, 1F, 1H, 1P, 1, 1X, 1Y, 1Z, RT2, R25, 2F, 2H, 2P, 2, 2X, 2Y, 2Z, RT3, RT5, RT6, RTE, RTF, RTH, RTB, ERR, XA, XC, XO, XS, ACC, ALV, AMD, CLE, COR, CPI, CPR, CTL, DOC, EOG, EXT, GER, GPR, INC, INV, QRY, RCV, REJ, REQ, RES, ROG, TAX, SUB, ACK", lookups.EntryStatusList().CodesAsString);
			}
		}

		public void TestApplicationCodeList()
		{
			CombineAssertions(() =>
			{
				var applicationCodeList = lookups.ApplicationCodeList();
				AssertEquals("Codes", "CHF, CDS", applicationCodeList.CodesAsString);
				AssertSame("ApplicationCodeList is cached.", applicationCodeList, filterBizObj.Factory.GetCachedValue<Registry.Business.DeclarationApplicationCodeList>());
			});
		}

		public void TestEntrySubTypes()
		{
			CombineAssertions(() =>
			{
				var entrySubTypes = lookups.EntrySubTypes;
				AssertEquals("Codes", "J, K, Q, Z, A, D, C, F, Y, G, H, J, K, A, D, Z, F, C, Y", entrySubTypes.CodesAsString);
				AssertSame("Cached.", entrySubTypes, filterBizObj.Factory.GetCachedValue("EUJobDeclarationFilterLookupsEntrySubTypes", () => new CodeDescriptionPairList()));
			});
		}

		public void TestDeclarationTypeList()
		{
			CombineAssertions(() =>
			{
				var declarationTypeList = lookups.DeclarationTypeList;
				AssertEquals("Codes", "H1, H2, H3, H4, H5, I1, 21I, 21N, FS, H7, H8, 21B, B1, B2, B4, C1, 21E, CEN", declarationTypeList.CodesAsString);
				AssertSame("Cached.", declarationTypeList, filterBizObj.Factory.GetCachedValue("EUJobDeclarationFilterLookupsDeclarationTypeList", () => new CodeDescriptionPairList()));
			});
		}

		public void TestContainerModeList()
		{
			AssertEquals("LCL, FCL, LSE, ULD, BBK, BLK, LQD, ROR, LTL, FTL, OBC, UNA, CNT, NCT", lookups.ContainerModeList.CodesAsString);
		}

		public void TestTransportTypeList()
		{
			AssertEquals("AIR, FIX, IWT, MAI, OWN, RAI, ROA, ROR, SEA", lookups.TransportTypeList.CodesAsString);
		}

		public void TestGetDataGroupingCodesForSupportingDocumentList()
		{
			var result = (ZString[])lookups.GetType().GetProperty("GetDataGroupingCodesForSupportingDocumentList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(lookups);
			CombineAssertions(() =>
			{
				AssertEquals("Length", 1, result.Length);
				AssertContainsExactElementsInAnyOrder("Values", new ZString[] { Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services }, result);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new JobDeclarationFilterBusinessObject();
			lookups = new JobDeclarationFilterLookups(filterBizObj);
		}
		JobDeclarationFilterBusinessObject filterBizObj;
		JobDeclarationFilterLookups lookups;
	}
}
