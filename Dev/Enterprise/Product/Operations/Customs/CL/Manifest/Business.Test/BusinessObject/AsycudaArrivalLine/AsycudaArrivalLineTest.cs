using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaArrivalLine))]
	class AsycudaArrivalLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "X";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "UYMVD";

			var outtturnHeader = factory.NewWithValidTestData<AsycudaArrivalHeader>();
			outtturnHeader.ATH_AMA_ManifestHeader = header.PK;
			outtturnHeader.ATH_ETAAtDischargePort = DateTime.Today;
			var line = outtturnHeader.ArrivalDetails.AddNew();
			line.ATL_ABL_AsycudaBill = bill.PK;

			return line;
		}
	}
}
