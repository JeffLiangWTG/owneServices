using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class UNDGSubstanceWrapperHelperTest : TestCaseWithFactory
	{
		public void TestGetUNDGPackagesSummary()
		{
			var ridUNDGs = new List<UNDGSubstanceWrapper>();

			var ridSubstance = Factory.New<UNDGSubstanceRID>();
			ridSubstance.RID_MixedPackProv = "MP2";
			ridSubstance.RID_UNNO = "000";
			Factory.Save();
			var dgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "000", standard: "RID").First();
			dgSubstance.DG_Class = "1";
			var dgItem = Factory.New<UNDGDataItem>();
			dgItem.DI_DGWeight = 1000;
			dgItem.DI_UnitOfWeight = "KG";
			dgItem.LinkDefault(dgSubstance);
			var undg = new UNDGSubstanceWrapper(dgItem, Factory);
			ridUNDGs.Add(undg);

			ZString expectedSummary = "Total Net Weight: 1000 KG" + System.Environment.NewLine + "GOODS ON UN NOS: 000";

			var helper = new UNDGSubstanceWrapperHelper();
			var summary = helper.GetUNDGPackagesSummary(ridUNDGs);

			AssertEquals("Should have UNDG Substance info", expectedSummary, summary);
		}
	}
}
