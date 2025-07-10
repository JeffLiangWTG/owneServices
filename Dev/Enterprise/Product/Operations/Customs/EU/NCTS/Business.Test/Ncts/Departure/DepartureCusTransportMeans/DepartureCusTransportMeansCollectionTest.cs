using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(DepartureCusTransportMeansCollection<DepartureCusTransportMeans>))]
	sealed class DepartureCusTransportMeansCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetCollectionRelationships()
		{
			var collection = (IDepartureCusTransportMeansCollection<DepartureCusTransportMeans>)Collection;
			var departureCusTransportMeans1 = Factory.New<DepartureCusTransportMeans>();
			collection.Add(departureCusTransportMeans1);
			var departureCusTransportMeans2 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("departureCusTransportMeans1.BK_ParentID", collection.Master.PK, departureCusTransportMeans1.TPM_ParentID);
				AssertEquals("departureCusTransportMeans1.BK_ParentTableCode", CusInBondBillSchema.Constants.Prefix, departureCusTransportMeans1.TPM_ParentTableCode);
				AssertEquals("departureCusTransportMeans2.BK_ParentID", collection.Master.PK, departureCusTransportMeans2.TPM_ParentID);
				AssertEquals("departureCusTransportMeans2.BK_ParentTableCode", CusInBondBillSchema.Constants.Prefix, departureCusTransportMeans2.TPM_ParentTableCode);
			});
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = (IDepartureCusTransportMeansCollection<DepartureCusTransportMeans>)Collection;
			var departureCusTransportMeans1 = collection.AddNew();
			var departureCusTransportMeans2 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("departureCusTransportMeans1.BK_SequenceNumber", (ZShort)1, departureCusTransportMeans1.TPM_SequenceNumber);
				AssertEquals("departureCusTransportMeans2.BK_SequenceNumber", (ZShort)2, departureCusTransportMeans2.TPM_SequenceNumber);

				departureCusTransportMeans2.Delete();
				var departureCusTransportMeans3 = collection.AddNew();
				AssertEquals("departureCusTransportMeans3.BK_SequenceNumber", (ZShort)2, departureCusTransportMeans3.TPM_SequenceNumber);
			});
		}

		public void TestMaxCountValidationEnable()
		{
			Enumerable.Range(0, 998).ForEach(_ => Collection.AddNew());
			var item999 = Collection.AddNew();
			var item1000 = Collection.AddNew();

			CombineAssertions(() =>
			{
				AssertNoRowErrorContaining(item999, "allowed a maximum");
				AssertHasRowErrorContaining(item1000, "allowed a maximum");
			});
		}

		public void TestMaxCountValidationWithMessageErrorEnable()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureHeader = header.MovementHeader;
			departureHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
			var collection = departureHeader.AdditionalTransportAtBorderList;

			Enumerable.Range(0, 7).ForEach(_ => collection.AddNew());
			var item8 = collection.AddNew();
			var item9 = collection.AddNew();

			CombineAssertions(() =>
			{
				AssertNoRowMessageErrorContaining(item8, "[R0789-2]");
				AssertHasRowMessageErrorContaining(item9, "[R0789-2]");
			});
		}

		public void TestAllowNewCore()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureHeader = header.MovementHeader;
			departureHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
			var collection = departureHeader.AdditionalTransportAtBorderList;

			Enumerable.Range(0, 7).ForEach(_ => collection.AddNew());

			CombineAssertions(() =>
			{
				AssertEquals("Has 7 items", true, collection.AllowNew);

				collection.AddNew();

				AssertEquals("Has 8 items", false, collection.AllowNew);
			});
		}

		public void TestNoResultQueryForArrivalBill()
		{
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = arrivalHeader.Bills.AddNew();
			nctsBill.ArrivalTransportInfos.AddNew();
			nctsBill.ArrivalTransportInfos.AddNew();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(expected: 0, nctsBill.DepartureTransportInfos.Count);
				var reloadedBill = new BusinessObjectFactory().Load<NctsBill>(nctsBill.PK);
				AssertEquals(expected: 0, reloadedBill.DepartureTransportInfos.Count);
				AssertEquals(expected: 2, reloadedBill.ArrivalTransportInfos.Count);
			});
		}

		public void TestParentTableForNctsBill()
		{
			var collection = (IDepartureCusTransportMeansCollection<DepartureCusTransportMeans>)Collection;
			var transport = collection.AddNew();
			AssertEquals(CusInBondBillSchema.Constants.Prefix, transport.TPM_ParentTableCode);
		}

		public void TestMaxCount_BillParent()
		{
			AssertEquals("MaxCount", 999, Collection.MaxCount);
		}

		public void TestMaxCount_DepartureMovementHeaderParent()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureHeader = header.MovementHeader;
			departureHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
			var collection = new DepartureCusTransportMeansCollection<DepartureCusTransportMeans>(departureHeader);

			AssertEquals(8, collection.MaxCount);
		}

		public void TestOnRemoved()
		{
			var nctsBill = Factory.New<NctsBill>();
			var collection = (DepartureCusTransportMeansCollection<DepartureCusTransportMeans>)nctsBill.DepartureTransportInfos;
			CombineAssertions(() =>
			{
				var item1 = collection.AddNew();
				var item2 = collection.AddNew();
				var item3 = collection.AddNew();
				var item4 = collection.AddNew();
				var item5 = collection.AddNew();
				var item6 = collection.AddNew();

				AssertEquals("Sequence Nr. 1", (short)1, item1.TPM_SequenceNumber);
				AssertEquals("Sequence Nr. 2", (short)2, item2.TPM_SequenceNumber);
				AssertEquals("Sequence Nr. 3", (short)3, item3.TPM_SequenceNumber);
				AssertEquals("Sequence Nr. 4", (short)4, item4.TPM_SequenceNumber);
				AssertEquals("Sequence Nr. 5", (short)5, item5.TPM_SequenceNumber);
				AssertEquals("Sequence Nr. 6", (short)6, item6.TPM_SequenceNumber);

				collection.Remove(item1);
				AssertEquals("Removing item1, Sequence Nr. 2 does not change", (short)2, item2.TPM_SequenceNumber);
				AssertEquals("Removing item1, Sequence Nr. 3 does not change", (short)3, item3.TPM_SequenceNumber);
				AssertEquals("Removing item1, Sequence Nr. 4 does not change", (short)4, item4.TPM_SequenceNumber);
				AssertEquals("Removing item1, Sequence Nr. 5 does not change", (short)5, item5.TPM_SequenceNumber);
				AssertEquals("Removing item1, Sequence Nr. 6 does not change", (short)6, item6.TPM_SequenceNumber);

				collection.Remove(item3);
				AssertEquals("Removing item3, Sequence Nr. 2 does not change", (short)2, item2.TPM_SequenceNumber);
				AssertEquals("Removing item3, Sequence Nr. 4 changes to 3", (short)3, item4.TPM_SequenceNumber);
				AssertEquals("Removing item3, Sequence Nr. 5 changes to 4", (short)4, item5.TPM_SequenceNumber);
				AssertEquals("Removing item3, Sequence Nr. 6 changes to 5", (short)5, item6.TPM_SequenceNumber);

				collection.Remove(item5);
				AssertEquals("Removing item5, Sequence Nr. 2 does not change", (short)2, item2.TPM_SequenceNumber);
				AssertEquals("Removing item5, Sequence Nr. 3 does not change", (short)3, item4.TPM_SequenceNumber);
				AssertEquals("Removing item5, Sequence Nr. 5 changes to 4", (short)4, item6.TPM_SequenceNumber);

				collection.Remove(item2);
				AssertEquals("Removing item2, Sequence Nr. 3 does not change", (short)3, item4.TPM_SequenceNumber);
				AssertEquals("Removing Nr. 2, Sequence Nr. 4 does not change", (short)4, item6.TPM_SequenceNumber);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBill = header.Bills.AddNew();
			return new DepartureCusTransportMeansCollection<DepartureCusTransportMeans>(nctsBill);
		}
	}
}
