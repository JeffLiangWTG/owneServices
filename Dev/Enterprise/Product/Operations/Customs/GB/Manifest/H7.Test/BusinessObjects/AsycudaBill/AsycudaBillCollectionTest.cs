using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	sealed class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAdditionalInfoIsCreatedAndDefaulted()
		{
			SetupRegionName();
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_RL_NKPortOfLoading = "GBLON";
			header.AMA_RL_NKPortOfDischarge = "GBBEL";
			var bill1 = header.Bills.AddNew();

			AssertEquals(1, bill1.AdditionalInfos.Count);
			AssertEquals("One AdditionalInfo row will be added and CSI_Code defaulted to NIDOM if in GB and RegionName isn't NORTHERN IRELAND", "NIDOM", bill1.AdditionalInfos.FirstOrDefault().CSI_Code);

			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "GBBEL";
			var bill2 = header.Bills.AddNew();

			AssertEquals(1, bill2.AdditionalInfos.Count);
			AssertEquals("One AdditionalInfo row will be added and CSI_Code defaulted to NIIMP if not in GB", "NIIMP", bill2.AdditionalInfos.FirstOrDefault().CSI_Code);

			header.AMA_RL_NKPortOfLoading = "GBBEL";
			var bill3 = header.Bills.AddNew();

			AssertEquals("No AdditionalInfo row will be added if in GB and RegionName is NORTHERN IRELAND", 0, bill3.AdditionalInfos.Count);

			header.AMA_RL_NKPortOfLoading = "GBLON";
			var bill4 = header.Bills.AddNew();
			bill4.AdditionalInfos.AddNew();
			bill4.AdditionalInfos.AddNew();

			AssertEquals(3, bill4.AdditionalInfos.Count);
			AssertEquals("1st row's CSI_Code will be defaulted to NIDOM", "NIDOM", bill4.AdditionalInfos.ElementAt(0).CSI_Code);
			AssertEquals("2nd row's CSI_Code will not be defaulted", ZString.Empty, bill4.AdditionalInfos.ElementAt(1).CSI_Code);
			AssertEquals("3rd row's CSI_Code will not be defaulted", ZString.Empty, bill4.AdditionalInfos.ElementAt(2).CSI_Code);

			bill4.AdditionalInfos.ElementAt(0).CSI_Code = "NIXXX";
			bill4.AdditionalInfos.ElementAt(1).CSI_Code = "NIYYY";
			bill4.AdditionalInfos.ElementAt(2).CSI_Code = "NIZZZ";
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var billReloaded = factory.Load<AsycudaBill>(bill4.PK);

			AssertEquals("No record will be added if AdditionalInfo collection is not empty when reloading", 3, billReloaded.AdditionalInfos.Count);
			AssertEquals("No record will be defaulted if AdditionalInfo collection is not empty when reloading", "NIXXX", billReloaded.AdditionalInfos.ElementAt(0).CSI_Code);
			AssertEquals("No record will be defaulted if AdditionalInfo collection is not empty when reloading", "NIYYY", billReloaded.AdditionalInfos.ElementAt(1).CSI_Code);
			AssertEquals("No record will be defaulted if AdditionalInfo collection is not empty when reloading", "NIZZZ", billReloaded.AdditionalInfos.ElementAt(2).CSI_Code);
		}

		public void TestAdditionalInfoIsCreatedAndDefaulted_DoNotApplyWhenDischargeIsNotInNorthenIreland()
		{
			SetupRegionName();
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_RL_NKPortOfLoading = "GBLON";
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			var bill1 = header.Bills.AddNew();

			Assert("No AdditionalInfo row will be added if Discharge is not in Northen Ireland", !bill1.AdditionalInfos.Any());

			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "GBLON";
			var bill2 = header.Bills.AddNew();

			Assert("No AdditionalInfo row will be added if Discharge is not in Northen Ireland", !bill2.AdditionalInfos.Any());
		}

		void SetupRegionName()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "XXX";
				refCountryStates.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = refCountryStates.PK;
			}

			var london = new RefUNLOCO.Loader(Factory).Load("GBLON");
			if (london.CountryStates == null || string.Compare(london.CountryStates.RW_RegionName, "ENGLAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "YYY";
				refCountryStates.RW_RegionName = "ENGLAND";
				london.RL_RW = refCountryStates.PK;
			}
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaBillCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
			return asycudaManifestHeader.Bills as AsycudaBillCollection;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaBill.Schema.ABL_SellerRegNoType };
		}
	}
}
