using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaBillForwarderSubDealNumberManagerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When asycudaBill is null", () => new AsycudaBillForwarderSubDealNumberManager(null));
		}

		public void TestIL1CreateForSeaWithManifestNumberAndCMP()
		{
			var factory = Factory;
			var (orgAddressShippingAgent, orgAddressDeclarant) = CreatePartners();
			var headerA = CreateNewHeader("headerA", orgAddressShippingAgent, orgAddressDeclarant);
			headerA.AMA_TransportMode = "ROA";
			var billA = headerA.Bills.AddNew();
			billA.ABL_SequenceNumber = 1;
			billA.OnSaving();

			AssertEquals("IL1 bill Transport Document created only for SEA manifest", 0, billA.TransportDocuments.Count(x => x.CSI_Code == "IL1"));

			headerA.AMA_ManifestNumber = ZString.Empty;
			headerA.AMA_TransportMode = "SEA";
			billA.OnSaving();
			AssertEquals("IL1 bill Transport Document created only for header with ManifestNumber", 0, billA.TransportDocuments.Count(x => x.CSI_Code == "IL1"));

			headerA.AMA_ManifestNumber = "123456";

			AssertEquals("IL1 bill Transport Document created, when sea, ManifestNumber is not empty, declarant valid and shipping agent valid", 1, billA.TransportDocuments.Count(x => x.CSI_Code == "IL1"));
			var forwarderSubDealNumber = billA.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("There should be a Transport Document record with IL1 code and reference consist of 'CCC' and 'CMP' codes value and a counter based on the bill Sequence Number", "I123456A01", forwarderSubDealNumber);

			headerA.AMA_ManifestNumber = ZString.Empty;
			AssertEquals("Clearing ManifestNumber clear IL1 TransportDocuments", 0, billA.TransportDocuments.Count(x => x.CSI_Code == "IL1"));

			headerA.AMA_ManifestNumber = "123456";

			AssertEquals("Restoring Manifest Number creates IL1", 1, billA.TransportDocuments.Count(x => x.CSI_Code == "IL1"));

			orgAddressDeclarant.Header.CustomsCodes.RemoveAndDeleteAll();
			factory.Save();
			billA.OnSaving();
			AssertEquals("Remove the Sub Deal Number removes IL1", 0, billA.TransportDocuments.Count(x => x.CSI_Code == "IL1"));
		}

		public void TestIL1CreateForCMPWith3Digits()
		{
			var factory = Factory;
			var (orgAddressShippingAgent, orgAddressDeclarant) = CreatePartners();
			var headerA = CreateNewHeader("headerA", orgAddressShippingAgent, orgAddressDeclarant);
			var billA = headerA.Bills.AddNew();
			billA.ABL_SequenceNumber = 1;

			orgAddressDeclarant.Header.CustomsCodes.RemoveAndDeleteAll();
			orgAddressDeclarant.Header.CustomsCodes.AddNew("CMP", "4567", "IL");
			billA.OnSaving();
			AssertEquals("When declarant or shipping agent code length greater than 3", 0, billA.TransportDocuments.Count(x => x.CSI_Code == "IL1"));

			orgAddressDeclarant.Header.CustomsCodes.RemoveAndDeleteAll();
			orgAddressDeclarant.Header.CustomsCodes.AddNew("CMP", "457", "IL");
			factory.Save();
			var forwarderSubDealNumber = billA.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("When declarant or shipping agent code length equals 3", "I123457A01", forwarderSubDealNumber);
		}

		public void TestIL1IsNotOverriden()
		{
			var factory = Factory;
			var (orgAddressShippingAgent, orgAddressDeclarant) = CreatePartners();
			var header = CreateNewHeader("headerA", orgAddressShippingAgent, orgAddressDeclarant);
			var bill1 = header.Bills.AddNew();
			bill1.ABL_SequenceNumber = 1;
			factory.Save();
			var forwarderSubDealNumber = bill1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("IL1 equals expected value", "I123456A01", forwarderSubDealNumber);

			bill1.ABL_SequenceNumber = 2;
			bill1.OnSaving();
			forwarderSubDealNumber = bill1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("IL1 keeps the old value", "I123456A01", forwarderSubDealNumber);

			var transportDocument = bill1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1");
			transportDocument.CSI_ReferenceNumberUserInterface = "UI025123A01";
			bill1.OnSaving();
			var il1 = bill1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1");
			AssertNotNull("IL1 still exists", il1);
			AssertEquals("IL1 remains with value entered by user", "UI025123A01", il1.CSI_ReferenceNumber);
		}

		public void TestIL1PicksFirstSmallesAvailableSuffix()
		{
			var factory = Factory;
			var (orgAddressShippingAgent, orgAddressDeclarant) = CreatePartners();
			var header = CreateNewHeader("headerA", orgAddressShippingAgent, orgAddressDeclarant);
			var bill1 = header.Bills.AddNew();
			bill1.ABL_SequenceNumber = 1;
			factory.Save();
			var forwarderSubDealNumber1 = bill1.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("IL1 equals expected value", "I123456A01", forwarderSubDealNumber1);

			var bill2 = header.Bills.AddNew();
			bill2.ABL_SequenceNumber = 2;
			factory.Save();
			var forwarderSubDealNumber2 = bill2.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("IL1 equals expected value", "I123456A02", forwarderSubDealNumber2);

			var bill3 = header.Bills.AddNew();
			bill3.ABL_SequenceNumber = 3;
			factory.Save();
			var forwarderSubDealNumber3 = bill3.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("IL1 equals expected value", "I123456A03", forwarderSubDealNumber3);

			header.Bills.RemoveAndDelete(bill2);

			var bill4 = header.Bills.AddNew();
			bill4.ABL_SequenceNumber = 4;
			factory.Save();
			var forwarderSubDealNumber4 = bill4.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("IL1 equals expected value", "I123456A02", forwarderSubDealNumber4);
		}

		public void TestIL1JumpsLetterAfter99()
		{
			var factory = Factory;
			var (orgAddressShippingAgent, orgAddressDeclarant) = CreatePartners();
			var header = CreateNewHeader("headerA", orgAddressShippingAgent, orgAddressDeclarant);

			for (ZShort i = 1; i <= 99; i++)
			{
				var billA = header.Bills.AddNew();
				billA.ABL_SequenceNumber = i;
				factory.Save();
				var forwarderSubDealNumberA = billA.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
				AssertEquals($"Unique suffix should be A{i:00}", $"I123456A{i:00}", forwarderSubDealNumberA);
			}

			var billB = header.Bills.AddNew();
			billB.ABL_SequenceNumber = 100;
			factory.Save();
			var forwarderSubDealNumberB = billB.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("Unique suffix should be B01", "I123456B01", forwarderSubDealNumberB);
		}

		public void TestIL1TakesLowestAvailableSuffixForNewManifest()
		{
			var factory = Factory;
			var (orgAddressShippingAgent, orgAddressDeclarant) = CreatePartners();
			var headerA = CreateNewHeader("header1", orgAddressShippingAgent, orgAddressDeclarant);
			var billA = headerA.Bills.AddNew();
			billA.ABL_SequenceNumber = 1;
			factory.Save();

			for (var i = 2; i <= 5; i++)
			{
				var header = CreateNewHeader($"header{i}", orgAddressShippingAgent, orgAddressDeclarant);
				var bill = header.Bills.AddNew();
				bill.ABL_SequenceNumber = 1;
				factory.Save();
				var forwarderSubDealNumber = bill.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
				AssertEquals($"next Unique Suffix should be A, sequence {i:00}", $"I123456A{i:00}", forwarderSubDealNumber);
			}
		}

		public void TestIL1TakesSuffixBecameAvailableForNewManifest()
		{
			var factory = Factory;
			var (orgAddressShippingAgent, orgAddressDeclarant) = CreatePartners();
			var headerA = CreateNewHeader("headerA", orgAddressShippingAgent, orgAddressDeclarant);
			var billA = headerA.Bills.AddNew();
			billA.ABL_SequenceNumber = 1;
			factory.Save();

			var headerB = CreateNewHeader($"headerB", orgAddressShippingAgent, orgAddressDeclarant);
			var billB = headerB.Bills.AddNew();
			billB.ABL_SequenceNumber = 1;
			factory.Save();
			var forwarderSubDealNumberB = billB.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals($"Prerequisite: next Unique Suffix should be A, sequence 02", $"I123456A02", forwarderSubDealNumberB);

			headerA.AMA_ManifestNumber = ZString.Empty;
			factory.Save();
			var il1A = billA.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1");
			AssertNull("Prerequisite: IL1 should be removed when manifest number is cleared", il1A);

			var headerC = CreateNewHeader($"headerC", orgAddressShippingAgent, orgAddressDeclarant);
			var billC = headerC.Bills.AddNew();
			billC.ABL_SequenceNumber = 1;
			factory.Save();
			var forwarderSubDealNumberC = billC.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals($"Letter A became available, Unique Suffix should be A, sequence 01", $"I123456A01", forwarderSubDealNumberC);
		}

		public void TestIL1TakesDifferentLetterWhenNextOneTakenByAnotherManifest()
		{
			var factory = Factory;
			var (orgAddressShippingAgent, orgAddressDeclarant) = CreatePartners();
			var headerA = CreateNewHeader("headerA", orgAddressShippingAgent, orgAddressDeclarant);
			var billA = headerA.Bills.AddNew();
			billA.ABL_SequenceNumber = 1;
			factory.Save();
			var forwarderSubDealNumberA = billA.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("Prereq: Bill A IL1 should be I123456A01", "I123456A01", forwarderSubDealNumberA);

			var headerB = CreateNewHeader("headerB", orgAddressShippingAgent, orgAddressDeclarant);
			var billB01 = headerB.Bills.AddNew();
			billB01.ABL_SequenceNumber = 1;
			factory.Save();
			var forwarderSubDealNumberB01 = billB01.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("Prereq: Bill B IL1 should be I123456A02", "I123456A02", forwarderSubDealNumberB01);

			var billC = headerA.Bills.AddNew();
			billC.ABL_SequenceNumber = 1;
			factory.Save();
			var forwarderSubDealNumber = billC.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("Since A01 and A02 already taken, next should be A03", "I123456A03", forwarderSubDealNumber);

			var billB02 = headerB.Bills.AddNew();
			billB02.ABL_SequenceNumber = 2;
			factory.Save();
			var forwarderSubDealNumberB02 = billB02.TransportDocuments.FirstOrDefault(x => x.CSI_Code == "IL1").CSI_ReferenceNumber;
			AssertEquals("Since A01, A02 and A03 already taken, next should be A04", "I123456A04", forwarderSubDealNumberB02);
		}

		AsycudaManifestHeader CreateNewHeader(string jobReference, OrgAddress orgAddressShippingAgent, OrgAddress orgAddressDeclarant)
		{
			var factory = Factory;
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "IL";
			header.AMA_JobReference = jobReference;
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestNumber = "123456";
			header.AMA_OA_ShippingAgent = orgAddressShippingAgent.PK;
			header.AMA_OA_Declarant = orgAddressDeclarant.PK;

			return header;
		}

		(OrgAddress, OrgAddress) CreatePartners()
		{
			var factory = Factory;
			var orgHeaderShippingAgent = factory.NewWithValidTestData<OrgHeader>();
			var orgAddressShippingAgent = orgHeaderShippingAgent.MainAddress;
			orgHeaderShippingAgent.OH_Code = "SA";
			orgHeaderShippingAgent.CustomsCodes.AddNew("CCC", "123", "IL");

			var orgHeaderDeclarant = factory.NewWithValidTestData<OrgHeader>();
			var orgAddressDeclarant = orgHeaderDeclarant.MainAddress;
			orgHeaderDeclarant.OH_Code = "DEC";
			orgHeaderDeclarant.CustomsCodes.AddNew("CMP", "456", "IL");
			return (orgAddressShippingAgent, orgAddressDeclarant);
		}
	}
}
