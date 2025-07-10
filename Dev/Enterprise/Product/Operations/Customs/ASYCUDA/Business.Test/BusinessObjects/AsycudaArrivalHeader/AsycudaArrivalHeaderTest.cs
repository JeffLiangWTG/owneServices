using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ASYCUDA.Business.Testing.AsycudaManifestHeaderBaseOnlyTest;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaArrivalHeader))]
	sealed class AsycudaArrivalHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_JobReference = "X";

			var arrHeader = header.ArrivalHeaders.AddNew();
			arrHeader.ATH_AMA_ManifestHeader = header.PK;
			arrHeader.ATH_VoyageFlightNo = "Flight1";

			var arrDetails = arrHeader.ArrivalDetails.AddNew();
			arrDetails.ATL_ABL_AsycudaBill = bill.PK;
			arrDetails.ATL_Quantity = 5;
			AssertEquals(5, arrHeader.ArrivalDetails[0].ATL_Quantity);
		}

		public void TestCanDelete()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_JobReference = "X";

			var arrHeader = header.ArrivalHeaders.AddNew();
			arrHeader.ATH_AMA_ManifestHeader = header.PK;
			arrHeader.ATH_VoyageFlightNo = "Flight1";

			var arrDetails = arrHeader.ArrivalDetails.AddNew();
			arrDetails.ATL_ABL_AsycudaBill = bill.PK;
			arrDetails.ATL_Quantity = 5;

			var arrHeader2 = header.ArrivalHeaders.AddNew();
			arrHeader2.ATH_AMA_ManifestHeader = header.PK;
			arrHeader2.ATH_VoyageFlightNo = "Flight2";

			var arrDetails2 = arrHeader.ArrivalDetails.AddNew();
			arrDetails2.ATL_ABL_AsycudaBill = bill.PK;
			arrDetails2.ATL_Quantity = 1;

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USAMA;
			message.EM_MessageNum = "M1234";
			message.EM_LinkTable = AsycudaArrivalHeader.Schema.TableName;
			message.EM_LinkUniqueID = arrHeader.PK;

			header.Messages.Add(message);

			AssertEquals(false, arrHeader.CanDelete);
			AssertEquals(true, arrHeader2.CanDelete);
		}

		public void TestClusterKey()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			Factory.Save();
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FL1";
			var clusterKey = header.AMA_ClusterKey;
			AssertNotEquals("Cluster key is not 0", 0, clusterKey);
			AssertEquals("Cluster key set with foreign key", arrivalHeader.ATH_ClusterKey, clusterKey);
			Factory.Save();
			AssertEquals("Cluster key does not change", arrivalHeader.ATH_ClusterKey, clusterKey);
		}

		public void TestCreateNewAsycudaTransferHeaderCollection()
		{
			var header = Factory.New<AsycudaArrivalHeader>();
			AssertEquals(typeof(ManifestBase.AsycudaTransferHeaderCollection<AsycudaTransferHeader>), header.TransferHeaders.GetType());
			AssertEquals(typeof(ManifestBase.AsycudaTransferHeaderCollection<AsycudaTransferHeader>), ((ManifestBase.AsycudaArrivalHeader)header).TransferHeaders.GetType());
		}

		public void TestGetNewValidation()
		{
			var header = Factory.New<AsycudaArrivalHeader>();
			AssertEquals(typeof(AsycudaArrivalHeaderValidation), header.Validation.GetType());
			AssertEquals(typeof(AsycudaArrivalHeaderValidation), ((ManifestBase.AsycudaArrivalHeader)header).Validation.GetType());
		}

		public void TestGetNewLookups()
		{
			var header = Factory.New<AsycudaArrivalHeader>();
			AssertEquals(typeof(AsycudaArrivalHeaderLookups), header.Lookups.GetType());
			AssertEquals(typeof(AsycudaArrivalHeaderLookups), ((ManifestBase.AsycudaArrivalHeader)header).Lookups.GetType());
		}

		public void TestATH_ArrivalSequence()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.Bills.AddNew();

			var arrHeader = header.ArrivalHeaders.AddNew();
			var arrHeader2 = header.ArrivalHeaders.AddNew();
			var arrHeader3 = header.ArrivalHeaders.AddNew();
			var arrHeader4 = header.ArrivalHeaders.AddNew();
			var arrHeader5 = header.ArrivalHeaders.AddNew();

			AssertEquals((ZShort)1, arrHeader.ATH_ArrivalSequence);
			AssertEquals((ZShort)2, arrHeader2.ATH_ArrivalSequence);
			AssertEquals((ZShort)3, arrHeader3.ATH_ArrivalSequence);
			AssertEquals((ZShort)4, arrHeader4.ATH_ArrivalSequence);
			AssertEquals((ZShort)5, arrHeader5.ATH_ArrivalSequence);
			arrHeader2.Delete();
			AssertEquals((ZShort)1, arrHeader.ATH_ArrivalSequence);
			AssertEquals((ZShort)2, arrHeader3.ATH_ArrivalSequence);
			AssertEquals((ZShort)3, arrHeader4.ATH_ArrivalSequence);
			AssertEquals((ZShort)4, arrHeader5.ATH_ArrivalSequence);
			var arrHeader6 = header.ArrivalHeaders.AddNew();

			AssertEquals((ZShort)5, arrHeader6.ATH_ArrivalSequence);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			header.AMA_JobReference = "X";
			header.AMA_E_ARV = ZDateTime.Today;
			header.AMA_RL_NKPortOfLoading = "SBHIR";
			header.AMA_RL_NKPortOfDischarge = "GBFXT";
			header.AMA_Nature = "ABC";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SBHIR";

			var outtturnHeader = factory.New<AsycudaArrivalHeader>();
			outtturnHeader.ATH_AMA_ManifestHeader = header.PK;
			outtturnHeader.ATH_ETAAtDischargePort = DateTime.Today;
			return outtturnHeader;
		}
	}
}
