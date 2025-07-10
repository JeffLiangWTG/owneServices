using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class RegistryHelperTest : TestCaseWithFactory
{
	[TestDate(2023, 08, 02)]
	public void TestGetValidCustomsRegistryForCompany()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		var nctsHeader2 = Factory.NewWithValidTestData<NctsHeader>();

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "ORG001";
		orgHeader.OH_FullName = "INTRISNV";

		nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;

		var orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.OH_Code = "ORG002";
		orgHeader2.OH_FullName = "Wisetech";

		nctsHeader2.DestinationTrader.OrganisationPK = orgHeader2.PK;

		var nctsHeader3 = Factory.NewWithValidTestData<NctsHeader>();
		var nctsHeader4 = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader3.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader4.BH_HeaderType = NctsMovementType.Codes.Departure;

		var orgHeader3 = Factory.New<OrgHeader>();
		orgHeader3.OH_Code = "ORG003";
		orgHeader3.OH_FullName = "Microsoft";

		nctsHeader3.MovementHeader.Representative.OrganisationPK = orgHeader3.PK;

		var orgHeader4 = Factory.New<OrgHeader>();
		orgHeader4.OH_Code = "ORG004";
		orgHeader4.OH_FullName = "Palo Alto Networks";

		nctsHeader4.Principal.OrganisationPK = orgHeader4.PK;

		var listOfValues = new CustomsRegistryCollection();

		var customsRegistryEntry1 = new CustomsRegistry();
		customsRegistryEntry1.Organization = orgHeader.PK;
		customsRegistryEntry1.DeclarationType = "TA";
		customsRegistryEntry1.StartingDate = new DateTime(2023, 07, 20);
		customsRegistryEntry1.StartingNo = 20;
		listOfValues.Add(customsRegistryEntry1);

		var customsRegistryEntry2 = new CustomsRegistry();
		customsRegistryEntry2.Organization = orgHeader.PK;
		customsRegistryEntry2.DeclarationType = "TA";
		customsRegistryEntry2.StartingDate = new DateTime(2023, 07, 20);
		customsRegistryEntry2.StartingNo = 1;
		listOfValues.Add(customsRegistryEntry2);

		var customsRegistryEntry3 = new CustomsRegistry();
		customsRegistryEntry3.Organization = orgHeader.PK;
		customsRegistryEntry3.DeclarationType = "TA";
		customsRegistryEntry3.StartingDate = new DateTime(2022, 01, 01);
		customsRegistryEntry3.StartingNo = 50;
		listOfValues.Add(customsRegistryEntry3);

		var customsRegistryEntry4 = new CustomsRegistry();
		customsRegistryEntry4.Organization = orgHeader.PK;
		customsRegistryEntry4.DeclarationType = "TA";
		customsRegistryEntry4.StartingDate = new DateTime(2024, 01, 01);
		customsRegistryEntry4.StartingNo = 100;
		listOfValues.Add(customsRegistryEntry4);

		var customsRegistryEntry5 = new CustomsRegistry();
		customsRegistryEntry5.Organization = orgHeader.PK;
		customsRegistryEntry5.DeclarationType = "H1";
		customsRegistryEntry5.StartingDate = new DateTime(2023, 07, 20);
		customsRegistryEntry5.StartingNo = 200;
		listOfValues.Add(customsRegistryEntry5);

		var customsRegistryEntry6 = new CustomsRegistry();
		customsRegistryEntry6.Organization = orgHeader2.PK;
		customsRegistryEntry6.DeclarationType = "TA";
		customsRegistryEntry6.StartingDate = new DateTime(2023, 07, 20);
		customsRegistryEntry6.StartingNo = 300;
		listOfValues.Add(customsRegistryEntry6);

		var customsRegistryEntry7 = new CustomsRegistry();
		customsRegistryEntry7.Organization = orgHeader3.PK;
		customsRegistryEntry7.DeclarationType = "TD";
		customsRegistryEntry7.StartingDate = new DateTime(2023, 07, 20);
		customsRegistryEntry7.StartingNo = 120;
		listOfValues.Add(customsRegistryEntry7);

		var customsRegistryEntry8 = new CustomsRegistry();
		customsRegistryEntry8.Organization = orgHeader3.PK;
		customsRegistryEntry8.DeclarationType = "TD";
		customsRegistryEntry8.StartingDate = new DateTime(2023, 07, 20);
		customsRegistryEntry8.StartingNo = 11;
		listOfValues.Add(customsRegistryEntry8);

		var customsRegistryEntry9 = new CustomsRegistry();
		customsRegistryEntry9.Organization = orgHeader3.PK;
		customsRegistryEntry9.DeclarationType = "TD";
		customsRegistryEntry9.StartingDate = new DateTime(2022, 01, 01);
		customsRegistryEntry9.StartingNo = 150;
		listOfValues.Add(customsRegistryEntry9);

		var customsRegistryEntry10 = new CustomsRegistry();
		customsRegistryEntry10.Organization = orgHeader3.PK;
		customsRegistryEntry10.DeclarationType = "TD";
		customsRegistryEntry10.StartingDate = new DateTime(2024, 01, 01);
		customsRegistryEntry10.StartingNo = 1100;
		listOfValues.Add(customsRegistryEntry10);

		var customsRegistryEntry11 = new CustomsRegistry();
		customsRegistryEntry11.Organization = orgHeader3.PK;
		customsRegistryEntry11.DeclarationType = "H1";
		customsRegistryEntry11.StartingDate = new DateTime(2023, 07, 20);
		customsRegistryEntry11.StartingNo = 1200;
		listOfValues.Add(customsRegistryEntry11);

		var customsRegistryEntry12 = new CustomsRegistry();
		customsRegistryEntry12.Organization = orgHeader4.PK;
		customsRegistryEntry12.DeclarationType = "TD";
		customsRegistryEntry12.StartingDate = new DateTime(2023, 07, 20);
		customsRegistryEntry12.StartingNo = 1300;
		listOfValues.Add(customsRegistryEntry12);

		using (BECustomsRegistry.Instance.CustomsRegistry.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, listOfValues))
		{
			var registryItem = RegistryHelper.GetValidCustomsRegistryForCompany(listOfValues, "TA", orgHeader.PK);
			var registryItem2 = RegistryHelper.GetValidCustomsRegistryForCompany(listOfValues, "TA", orgHeader2.PK);
			var registryItem3 = RegistryHelper.GetValidCustomsRegistryForCompany(listOfValues, "TD", orgHeader3.PK);
			var registryItem4 = RegistryHelper.GetValidCustomsRegistryForCompany(listOfValues, "TD", orgHeader4.PK);

			CombineAssertions(() =>
			{
				AssertEquals("The retrieved registry item for header with organisation ORG001 should be the one with starting number 20", 20, registryItem.StartingNo);
				AssertEquals("The retrieved registry item for header with organisation ORG002 should be the one with starting number 300", 300, registryItem2.StartingNo);
				AssertEquals("The retrieved registry item for header with organisation ORG003 should be the one with starting number 20", 120, registryItem3.StartingNo);
				AssertEquals("The retrieved registry item for header with organisation ORG004 should be the one with starting number 300", 1300, registryItem4.StartingNo);
			});
		}
	}
}
