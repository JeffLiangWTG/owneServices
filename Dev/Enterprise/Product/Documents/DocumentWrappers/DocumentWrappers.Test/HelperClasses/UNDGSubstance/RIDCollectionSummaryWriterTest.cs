using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class RIDCollectionSummaryWriterTest : TestCaseWithFactory
	{
		#region RID Summary

		public void TestGetSummaryDoNotReturnSummaryForNonRID()
		{
			var adnUNDGs = new List<UNDGSubstanceWrapper>();

			var adnSubstance = Factory.New<UNDGSubstanceADN>();
			adnSubstance.ADN_UNNO = "777";
			Factory.Save();

			var undg = CreateUNDGSubstanceWrapper("777", "2", "ADN", 1000, "KG");
			adnUNDGs.Add(undg);

			var writer = new RIDCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(adnUNDGs);

			AssertEquals("Should not have RID info", ZString.Empty, summary);
		}

		public void TestGetSummaryDoNotReturnSummaryForNonClass1()
		{
			var ridUNDIGs = new List<UNDGSubstanceWrapper>();

			CreateUNDGSubstanceRIDInFactory("777", "");
			var undg = CreateUNDGSubstanceWrapper("777", "2", "RID", 1000, "KG");
			ridUNDIGs.Add(undg);

			var writer = new RIDCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(ridUNDIGs);

			AssertEquals("Should not have RID info", ZString.Empty, summary);
		}

		public void TestGetSummary()
		{
			var ridUNDIGs = new List<UNDGSubstanceWrapper>();

			CreateUNDGSubstanceRIDInFactory("777", "MP1");
			var undg0 = CreateUNDGSubstanceWrapper("777", "1", "RID", 1000, "KG");
			ridUNDIGs.Add(undg0);

			CreateUNDGSubstanceRIDInFactory("007", "MP2");
			var undg1 = CreateUNDGSubstanceWrapper("007", "1", "RID", 1000, "LB");
			ridUNDIGs.Add(undg1);

			ZString expectedSummary = "Total Net Weight: 1453.592 KG" + System.Environment.NewLine + "GOODS ON UN NOS: 777, 007";

			var writer = new RIDCollectionSummaryWriter() as IUNDGSubstanceCollectionSummaryWriter;
			var summary = writer.GetSummary(ridUNDIGs);

			AssertEquals("Should have RID Class 1 and mixed packing provisions info", expectedSummary, summary);
		}

		UNDGSubstanceWrapper CreateUNDGSubstanceWrapper(string uNNO, string imoClass, string standard, int weight = 0, string unit = "KG")
		{
			var dgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, uNNO, standard: standard).First();
			dgSubstance.DG_Class = imoClass;
			var dgItem = Factory.New<UNDGDataItem>();
			dgItem.DI_DGWeight = weight;
			dgItem.DI_UnitOfWeight = unit;
			dgItem.LinkDefault(dgSubstance);
			var undg = new UNDGSubstanceWrapper(dgItem, Factory);
			return undg;
		}

		void CreateUNDGSubstanceRIDInFactory(string uNNO, string mixedPackingProvision)
		{
			var ridSubstance = Factory.New<UNDGSubstanceRID>();
			ridSubstance.RID_MixedPackProv = mixedPackingProvision;
			ridSubstance.RID_UNNO = uNNO;
			Factory.Save();
		}

		#endregion
	}
}
