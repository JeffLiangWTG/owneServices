using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CN.Module.Testing
{
	class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestBuyer()
		{
			AssertEquals("Buyer of correct type", typeof(OrgHeaderCollection), lookups.Buyer.GetType());
		}

		public void TestManufacturer()
		{
			AssertEquals("Manufacturer of correct type", typeof(OrgHeaderCollection), lookups.Manufacturer.GetType());
		}

		public void TestEntryInstructionCPCList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CN", "A", "10", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("CN", "A", "11", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("US", "H", "60", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				AssertEquals("No Procedure for ZA", 0, lookups.EntryInstructionCPCList.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				AssertEquals("Getting List for CN", 2, lookups.EntryInstructionCPCList.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				AssertEquals("Getting List for US", 1, lookups.EntryInstructionCPCList.Count);
			}

			new UniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure("CN", "B", "80", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				AssertEquals("Getting List for CN", 2, lookups.EntryInstructionCPCList.Count);
				filterBizObj.Factory.ClearCachedValue<CodeDescriptionPairList>("CN_RefCusProcedures_ProcedureCode");
				AssertEquals("Clear Cache and Get New List for CN", 3, lookups.EntryInstructionCPCList.Count);
			}
		}

		public void TestMessageSubTypeListList()
		{
			using (CNCustomsDataRegistry.Instance.CNBTHFunctionActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var list = lookups.MessageSubTypeList();
				AssertEquals(3, list.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "CUS", "REC", "BTH" }, lookups.MessageSubTypeList().GetAllCodes());
			}

			using (CNCustomsDataRegistry.Instance.CNBTHFunctionActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), false))
			{
				var list = lookups.MessageSubTypeList();
				AssertEquals(2, list.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "CUS", "REC" }, lookups.MessageSubTypeList().GetAllCodes());
			}
		}

		[TestDate(2019, 12, 26)]
		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList("CN", Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CNJ", "Nanjing Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 30));
			helper.CreateNewOrGetExistingCusCodeList("CN", Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CSH", "Shanghai Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 25));
			helper.CreateNewOrGetExistingCusCodeList("US", Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CNY", "New York Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 30));
			Factory.Save();
			var dec = new JobDeclarationFilterBusinessObject();
			var offices = dec.Lookups.CustomsOfficeList;
			offices.Load();
			AssertEquals(1, offices.Count);
			AssertEquals("Customs Office must be 'CNJ'", "CNJ", offices[0].ZZD_Code);
		}

		public void TestContainerModeList()
		{
			var list = lookups.ContainerModeList;
			AssertEquals("CNT, BBK, BLK, LQD", list.CodesAsString);
		}

		public void TestTransportTypeList()
		{
			var list = lookups.TransportTypeList;
			AssertEquals("AIR, SEA, MAI, ROA, RAI, FIX, PHC", list.CodesAsString);
		}

		public void TestMessageTypeList()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			CombineAssertions(() =>
			{
				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
				{
					var filterBizObj = new JobDeclarationFilterBusinessObject();
					var lookups = new JobDeclarationFilterLookups(filterBizObj);
					var list = lookups.MessageTypeList;
					AssertEquals("Built-In", "EXP, IMP", list.CodesAsString);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
				{
					var filterBizObj = new JobDeclarationFilterBusinessObject();
					var lookups = new JobDeclarationFilterLookups(filterBizObj);
					var list = lookups.MessageTypeList;
					AssertEquals("Interfaced", "DRW, EXP, EXW, IMP, MSC, REF", list.CodesAsString);
				}
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
