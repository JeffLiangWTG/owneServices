using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN.Testing
{
	class CINMessageWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestMessageWrapperConstructor()
		{
			Activator.CreateInstance(typeof(CINHeaderWrapper), new object[] { null });
		}

		[TestDate(2011, 12, 13, 14, 15, 16, 17)]
		public void TestHeaderWrapperProperties()
		{
			var jobHeader = CreateTestJobHeader();
			var wrapper = new CINHeaderWrapper(jobHeader);
			CombineAssertions("Header Wrapper Properties", () =>
			{
				AssertEquals("Bills Count", 1, wrapper.Bills.Count());
				AssertEquals("CurrentLocation", "HOME", wrapper.CurrentLocation);
				AssertNull("CustomsDocuemnts", wrapper.CustomsDocuments);
				AssertEquals("CustomsReference", "ABC123", wrapper.CustomsReference);
				AssertEquals("CustomsStatus", "C1", wrapper.CustomsStatus);
				AssertEquals("MovementTime", ZDateTime.UtcNow, wrapper.MovementTime);
				AssertEquals("Location", "AWAY", wrapper.NewLocation);
				AssertEquals("MessageEnvelope.SchemaID", "750", wrapper.MessageEnvelope.SchemaID);
				AssertEquals("MessageEnvelope.SchemaVersion", "XML", wrapper.MessageEnvelope.SchemaVersion);
				AssertEquals("MessageEnvelope.PartnerId", ZString.Empty, wrapper.MessageEnvelope.PartnerId);
				AssertEquals("MessageEnvelope.TransactionId", "0000000001", wrapper.MessageEnvelope.TransactionId);
				AssertEquals("MessageEnvelope.NumSeq", (ZShort)0, wrapper.MessageEnvelope.NumSeq);
			});

			AssertNotNull(wrapper.Line);

			jobHeader.CusTempStorageDec.CusTempStorageLines[0].TSL_OwnerReferenceType = "HWB";
			wrapper = new CINHeaderWrapper(jobHeader);

			AssertNull(wrapper.Line);
		}

		public void TestLineWrapperProperties()
		{
			var jobHeader = CreateTestJobHeader();

			var line = jobHeader.CusTempStorageDec.CusTempStorageLines[0] as CusTempStorageLine;

			var wrapper = new CINLineWrapper(line);
			CombineAssertions("Line Wrapper Properties", () =>
			{
				AssertEquals(line.PK, wrapper.PK);
				AssertEquals("AWB", wrapper.Type);
				AssertEquals("UNITTEST", wrapper.ReferenceNumber);
				AssertEquals(25, wrapper.NoPieces);
				AssertEquals(25, wrapper.TotalNoPieces);
				AssertEquals(50.0m, wrapper.Mass);
				AssertEquals(50.0m, wrapper.TotalMass);
				AssertEquals("Original Goods", wrapper.DescriptionOfGoods);
				AssertEquals(true, wrapper.IsAirwayBill);
			});

			line.TSL_OwnerReferenceType = "HWB";
			AssertEquals(false, wrapper.IsAirwayBill);

			line.TSL_GrossWeightUQ = Core.Constants.Weight.Grams;
			line.TSL_GrossWeight = 1234.0m;

			AssertEquals(1.234m, wrapper.Mass);
		}

		CusTempStorageJobHeader CreateTestJobHeader()
		{
			var jobHeader = CusTempStorageJobHeader.New(Factory);
			jobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var dec = jobHeader.CusTempStorageDec;

			var line = dec.CusTempStorageLines[0];
			line.TSL_OwnerReferenceType = "AWB";
			line.TSL_OwnerReferenceNumber = "UNITTEST";
			line.TSL_LocationOfGoods = "HOME";
			line.TSL_DestinationPlace = "AWAY";
			line.TSL_GoodsDescription = "Original Goods";
			line.TSL_PackageQty = 25;
			line.TSL_GrossWeight = 50.0m;
			line.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			line.TSL_UnionStatus = "C1";
			line.TSL_ReferenceNumber = "ABC123";
			Factory.Save();

			return jobHeader;
		}
	}
}
