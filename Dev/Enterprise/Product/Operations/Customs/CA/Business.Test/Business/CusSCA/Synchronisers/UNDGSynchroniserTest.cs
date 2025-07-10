using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class UNDGSynchroniserTest : SynchroniserTestCase
	{
		public void TestUNDGSynchroniser()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subsA = UNDGSubstanceLoader.LoadSubstances(Factory, "A", "", "IMO").First();
			var source = Factory.New<UNDGDataItem>();
			var destination = Factory.New<UNDGDataItem>();
			var synchroniser = new UNDGSynchroniser(destination, source);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			source.LinkDefault(subs);
			source.DI_DG = subsA.PK;
			source.DI_DGFlashPoint = 1m;
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			source.DI_OC_DGContact = contact1.PK;
			AssertEquals("A", destination.UNDGSubstance.DG_Code);
			AssertEquals(1m, destination.DI_DGFlashPoint);
			AssertEquals(contact1.PK, destination.DI_OC_DGContact);
			AssertEquals(subsA.PK, destination.DI_DG);
			AssertNotEquals("1", destination.SubstanceCode);
		}
	}
}
