using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CADCorrectionMessageSendingActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCARMChangeReasonCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeType("CCRC", "Canada Change Reason Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CCRC", "010", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CCRC", "100", yesterday, tomorrow);

			Factory.Save();

			var list = action.Lookups.CARMChangeReasonCodeList as BusinessObjectCollection;
			list.Load();

			AssertEquals(2, list.Count);
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "010"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "100"));
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
		}

		public void TestCARMAppealsProgramCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeType("CAPC", "Canada Appeals Program Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CAPC", "1", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Canada, "CAPC", "2", yesterday, tomorrow);

			Factory.Save();

			var list = action.Lookups.CARMAppealsProgramCodeList;

			AssertEquals(2, list.Count);
			AssertType<CodeDescriptionPairList>(list);
			Assert(list.ContainsCode("1"));
			Assert(list.ContainsCode("2"));
		}

		CADCorrectionMessageSendingAction action;
		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_CommoditySequence = 3;
			entryLine.CL_GoodsShipmentSequence = 4;
			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			action = wrapper.SendingActions.AddNew();
			action.CSI_ParentTableCode = CusEntryLineSchema.Constants.Prefix;
		}
	}
}
