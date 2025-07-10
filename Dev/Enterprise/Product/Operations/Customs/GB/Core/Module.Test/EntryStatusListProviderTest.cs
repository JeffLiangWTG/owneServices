using System;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Module.Testing
{
	sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var codePairImportExport = ReplaceNotSendCode(new EntryStatusList());
				RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.UnitedKingdom, codePairImportExport.GetAllCodes(), new string[] { "ROK", "CEO" });
			}
		}

		public void TestEntryStatusListCore()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var provider = new EntryStatusListProvider();
				var entryStatusList = (CodeDescriptionPairList)provider.EntryStatusList(Factory, Core.Constants.CountryCodes.UnitedKingdom, ZString.Empty);
				AssertEquals("AWR, B, CA, CAN, CRQ, CB, CC, CLR, CR, CS, CT, CU, CW, CX, FBK, HLD, MLT, NOT, RH1, RH2, RH3, RH5, RH6, RT0, 0H, 0P, 0X, RT1, R15, 1F, 1H, 1P, 1, 1X, 1Y, 1Z, RT2, R25, 2F, 2H, 2P, 2, 2X, 2Y, 2Z, RT3, RT5, RT6, RTE, RTF, RTH, RTB, ERR, XA, XC, XO, XS, ACC, ALV, AMD, CLE, COR, CPI, CPR, CTL, DOC, EOG, EXT, GER, GPR, INC, INV, QRY, RCV, REJ, REQ, RES, ROG, TAX, SUB, ACK", entryStatusList.CodesAsString);
			}
		}
	}
}
