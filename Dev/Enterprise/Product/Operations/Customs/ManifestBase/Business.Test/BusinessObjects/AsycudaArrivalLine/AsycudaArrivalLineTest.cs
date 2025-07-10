using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaArrivalLine))]
	class AsycudaArrivalLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClusterKey()
		{
			var arrivalLine = Factory.New<AsycudaArrivalLine>();
			var arrivalHeader = Factory.New<AsycudaArrivalHeader>();
			arrivalLine.ATL_ATH = arrivalHeader.PK;
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			arrivalHeader.ATH_AMA_ManifestHeader = manifestHeader.PK;
			AssertEquals(0, arrivalLine.ATL_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated OnSaving for manifestHeader.", 1, manifestHeader.AMA_ClusterKey);
			AssertEquals("ClusterKey generated OnSaving and same as manifestHeader.", 1, arrivalLine.ATL_ClusterKey);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "X";
			header.AMA_ClusterKey = 1;
			header.AMA_RN_NKCountry = "SB";
			header.AMA_Nature = "ABC";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SBHIR";

			var outtturnHeader = factory.New<AsycudaArrivalHeader>();
			outtturnHeader.ATH_AMA_ManifestHeader = header.PK;
			outtturnHeader.ATH_ETAAtDischargePort = DateTime.Today;
			var line = factory.New<AsycudaArrivalLine>();
			line.ATL_ATH = outtturnHeader.PK;
			line.ATL_ABL_AsycudaBill = bill.PK;

			return line;
		}
	}
}
