using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>))]
	sealed class ArrivalCusTransportMeansCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetCollectionRelationshipsForBill()
		{
			var collection = (ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>)Collection;
			var arrivalCusTransportMeans1 = Factory.New<ArrivalCusTransportMeans>();
			collection.Add(arrivalCusTransportMeans1);
			var arrivalCusTransportMeans2 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("arrivalCusTransportMeans1.BK_ParentID", collection.Master.PK, arrivalCusTransportMeans1.TPM_ParentID);
				AssertEquals("arrivalCusTransportMeans1.BK_ParentTableCode", CusInBondBillSchema.Constants.Prefix, arrivalCusTransportMeans1.TPM_ParentTableCode);
				AssertEquals("arrivalCusTransportMeans2.BK_ParentID", collection.Master.PK, arrivalCusTransportMeans2.TPM_ParentID);
				AssertEquals("arrivalCusTransportMeans2.BK_ParentTableCode", CusInBondBillSchema.Constants.Prefix, arrivalCusTransportMeans2.TPM_ParentTableCode);
			});
		}

		public void TestSetCollectionRelationshipsForArrivalMovementHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var collection = new ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(header.ArrivalMovementHeader);

			var arrivalCusTransportMeans = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("arrivalCusTransportMeans.BK_ParentID", header.ArrivalMovementHeader.PK, arrivalCusTransportMeans.TPM_ParentID);
				AssertEquals("arrivalCusTransportMeans.BK_ParentTableCode", CusInBondMoveHeaderSchema.Constants.Prefix, arrivalCusTransportMeans.TPM_ParentTableCode);
			});
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = (ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>)GetCollectionToTest();
			var arrivalCusTransportMeans1 = collection.AddNew();
			var arrivalCusTransportMeans2 = collection.AddNew();
			var arrivalCusTransportMeans3 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("arrivalCusTransportMeans1.BK_SequenceNumber", (ZShort)1, arrivalCusTransportMeans1.TPM_SequenceNumber);
				AssertEquals("arrivalCusTransportMeans2.BK_SequenceNumber", (ZShort)2, arrivalCusTransportMeans2.TPM_SequenceNumber);
				AssertEquals("arrivalCusTransportMeans3.BK_SequenceNumber", (ZShort)3, arrivalCusTransportMeans3.TPM_SequenceNumber);

				arrivalCusTransportMeans2.Delete();
				AssertEquals("arrivalCusTransportMeans1.BK_SequenceNumber after arrivalCusTransportMeans2 is deleted", (ZShort)1, arrivalCusTransportMeans1.TPM_SequenceNumber);
				AssertEquals("arrivalCusTransportMeans3.BK_SequenceNumber after arrivalCusTransportMeans2 is deleted", (ZShort)2, arrivalCusTransportMeans3.TPM_SequenceNumber);

				var arrivalCusTransportMeans4 = collection.AddNew();
				AssertEquals("arrivalCusTransportMeans4.BK_SequenceNumber", (ZShort)3, arrivalCusTransportMeans4.TPM_SequenceNumber);
			});
		}

		public void TestNoResultQueryForDepartureBill()
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = departureHeader.Bills.AddNew();

			nctsBill.DepartureTransportInfos.AddNew();
			nctsBill.DepartureTransportInfos.AddNew();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(0, nctsBill.ArrivalTransportInfos.Count);

				var reloadedBill = new BusinessObjectFactory().Load<NctsBill>(nctsBill.PK);

				AssertEquals(0, reloadedBill.ArrivalTransportInfos.Count);
				AssertEquals(2, reloadedBill.DepartureTransportInfos.Count);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBill = header.Bills.AddNew();
			return new ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(nctsBill);
		}
	}
}
