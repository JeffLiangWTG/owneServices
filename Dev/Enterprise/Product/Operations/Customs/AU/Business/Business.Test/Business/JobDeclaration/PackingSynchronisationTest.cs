using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PackingSynchronisationTest : Customs.Business.Testing.PackingSynchronisationTest
	{
		public void TestDefaultTotalNoOfPacksToPackages_FRM()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertDefaultedTotalNoOfPacksToPackages(declaration, true);
		}

		public void TestDefaultTotalNoOfPacksToPackages_FRM_OTH()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Other;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertDefaultedTotalNoOfPacksToPackages(declaration, false);
		}

		public void TestDefaultTotalNoOfPacksToPackages_EXW()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertDefaultedTotalNoOfPacksToPackages(declaration, false);
		}

		public void TestDefaultTotalNoOfPacksToPackages_SAC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertDefaultedTotalNoOfPacksToPackages(declaration, true);
		}

		public void AssertDefaultedTotalNoOfPacksToPackages(BaseJobDeclaration declaration, bool expectPacks)
		{
			AssertEquals("No packing groups yet", 0, declaration.PackingGroups.Count);
			AssertEquals("No House Bills yet", 0, declaration.Bills.Count);
			declaration.JE_TotalNoOfPacks = 100;
			AssertEquals("No package should have been created", 0, declaration.PackingGroups.Count);
			declaration.JE_HouseBill = "HB1";
			Factory.Save();

			if (expectPacks)
			{
				AssertEquals("One package should have been created", 1, declaration.PackingGroups.Count);
				var packGroup = declaration.PackingGroups[0];
				AssertEquals(100, packGroup.TotalPackageCount());
				AssertEquals("A house bill is created to have PackGroup linked", declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill).First(), packGroup.Bill);
			}
			else
			{
				AssertEquals("No packing groups created", 0, declaration.PackingGroups.Count);
			}
		}

		public void TestDefaultTotalNoOfPacksToPackages_MasterOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 100;
			Factory.Save();

			AssertEquals("One package should exist", 1, declaration.PackingGroups.Count);
			var packGroup = declaration.PackingGroups[0];
			AssertEquals("PackGroup linked to Master", declaration.Bills.FindByBillType(BillTypeList.Codes.MasterBill).First(), packGroup.Bill);
			AssertEquals(100, packGroup.TotalPackageCount());
		}

		public void TestDefaultTotalNoOfPacksToPackages_HouseExistsFirst()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPacks = 100;
			Factory.Save();

			AssertEquals("One package should exist", 1, declaration.PackingGroups.Count);
			var packGroup = declaration.PackingGroups[0];
			AssertEquals("PackGroup linked to House", declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill).First(), packGroup.Bill);
			AssertEquals(100, packGroup.TotalPackageCount());
		}

		public void TestDefaultTotalNoOfPacksToPackages_ChangePackCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPacks = 100;
			Factory.Save();

			declaration.JE_TotalNoOfPacks = 5;
			Factory.Save();

			AssertEquals("One package should exist", 1, declaration.PackingGroups.Count);
			var packGroup = declaration.PackingGroups[0];
			AssertEquals("PackGroup linked to House", declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill).First(), packGroup.Bill);
			AssertEquals("Once defaulted the count does not change", 100, packGroup.TotalPackageCount());
		}

		public void TestDefaultTotalNoOfPacksToPackages_ChangePackCount_SAC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";
			declaration.JE_TotalNoOfPacks = 100;
			Factory.Save();

			declaration.JE_TotalNoOfPacks = 5;
			Factory.Save();

			AssertEquals("One package should exist", 1, declaration.PackingGroups.Count);
			var packGroup = declaration.PackingGroups[0];
			AssertEquals("PackGroup linked to House", declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill).First(), packGroup.Bill);
			AssertEquals("SAC needs to match TotalNoOfPacks since it can't be altered in the UI", 5, packGroup.TotalPackageCount());
		}

		protected override BaseJobDeclaration GetDeclarationPackingRelevant()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			return result;
		}
	}
}
