using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;

namespace Enterprise.Client.UPE.Business.CMR.Testing
{
	public class UPEScreeningTest : TestCaseWithFactory
	{
		#region UPS Stop Phrases
		public void TestUPSStopPhrasesFoundInConsignorName()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsignorName = TestUPSStopPhrases;
			TestStopPhrasesProperty("UPSStopPhrasesFoundInConsignorName", CusHAWB.CS_ConsignorNameInfo, "UPSStopPhrasesFoundInConsignorName");
		}

		public void TestUPSStopPhrasesFoundInConsignorStreet()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsignorAddress = TestUPSStopPhrases;
			TestStopPhrasesProperty("UPSStopPhrasesFoundInConsignorStreet", CusHAWB.CS_ConsignorStreetInfo, "UPSStopPhrasesFoundInConsignorStreet");
		}

		public void TestUPSStopPhrasesFoundInConsignorStreet2()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsignorAddress = TestUPSStopPhrases;
			TestStopPhrasesProperty("UPSStopPhrasesFoundInConsignorStreet2", CusHAWB.CS_ConsignorStreet2Info, "UPSStopPhrasesFoundInConsignorStreet2");
		}

		public void TestUPSStopPhrasesFoundInConsigneeName()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsigneeName = TestUPSStopPhrases;
			TestStopPhrasesProperty("UPSStopPhrasesFoundInConsigneeName", CusHAWB.CS_ConsigneeNameInfo, "UPSStopPhrasesFoundInConsigneeName");
		}

		public void TestUPSStopPhrasesFoundInConsigneeStreet()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAddress = TestUPSStopPhrases;
			TestStopPhrasesProperty("UPSStopPhrasesFoundInConsigneeStreet", CusHAWB.CS_ConsigneeStreetInfo, "UPSStopPhrasesFoundInConsigneeStreet");
		}

		public void TestUPSStopPhrasesFoundInConsigneeStreet2()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAddress = TestUPSStopPhrases;
			TestStopPhrasesProperty("UPSStopPhrasesFoundInConsigneeStreet2", CusHAWB.CS_ConsigneeStreet2Info, "UPSStopPhrasesFoundInConsigneeStreet2");
		}

		public void TestUPSStopPhrasesFoundInGoodsDescription()
		{
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = TestUPSStopPhrases;
			SACDecider baseDecider = new SACDecider(Factory, 200, "FREDOS");
			AssertEquals("Stop phrase should not be found in base SACDecider for the test", true, baseDecider.IsValidForSAC);
			TestStopPhrasesProperty("UPSStopPhrasesFoundInGoodsDescription", CusHAWB.CS_GoodsDescriptionInfo, "UPSStopPhrasesFoundInGoodsDescription");
		}

		public void TestQuarantineStopPhrasesFoundInGoodsDescription()
		{
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = TestUPSStopPhrases;
			TestStopPhrasesProperty("QuarantineStopPhrasesFoundInGoodsDescription", CusHAWB.CS_GoodsDescriptionInfo, "QuarantineStopPhrasesFoundInGoodsDescription");
		}

		public void TestGoodsDescriptionStopPhraseWarning()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Nuclear");
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = TestUPSStopPhrases;
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "fruit", "bugs" };
			CusHAWB.CS_GoodsDescription = "No stop words";
			AssertEquals("Without stop words", "", Screening.GoodsDescriptionStopPhraseWarning);
			CusHAWB.CS_GoodsDescription = "MMs Nuclear fruit";
			AssertEquals("With customs, UPS and Quanratine stop words", @"Customs Stop Words found: Nuclear
UPS Stop Words found: MMs
Quarantine Stop Words found: fruit", Screening.GoodsDescriptionStopPhraseWarning);
		}

		#endregion
		#region Identifying Shipments
		public void TestIdentifiedReason_Profiling()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Drug", "Biological");
			CusHAWB.CS_GoodsDescription = "A Biological Drug";
			AssertEquals("IdentifiedReasion for Profiling", "PRF:Biological,Drug", Screening.IdentifiedReason);
		}

		public void TestIdentifiedReason_Screening()
		{
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "1" };
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAccountNum = new string[] { "2" };
			UPEDataRegistry.Instance.StopPhrasesForConsignorAccountNum = new string[] { "3" };
			UPEDataRegistry.Instance.StopPhrasesForConsigneeName = new string[] { "4" };
			UPEDataRegistry.Instance.StopPhrasesForConsignorName = new string[] { "5" };
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAddress = new string[] { "6", "7" };
			UPEDataRegistry.Instance.StopPhrasesForConsignorAddress = new string[] { "8", "9" };
			UPEDataRegistry.Instance.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1-20");
			AssertEquals("Not identified for screening initially", false, Screening.IsIdentifiedForScreening);
			CusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			CusHAWB.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<UPEOrgHeader>().MainAddress.PK;
			CusHAWB.CS_GoodsValue = 10.55m;
			CusHAWB.CS_GoodsDescription = "1";
			CusHAWB.Level1Record = new Level1Record();
			CusHAWB.Level1Record._400000 = new Test400000Line("2");
			CusHAWB.Level1Record._300000 = new Test300000Line("3");
			CusHAWB.Consignee.OH_FullName = "4";
			CusHAWB.Consignor.OH_FullName = "5";
			CusHAWB.Consignee.MainAddress.OA_Address1 = "6";
			CusHAWB.Consignee.MainAddress.OA_Address2 = "7";
			CusHAWB.Consignor.MainAddress.OA_Address1 = "8";
			CusHAWB.Consignor.MainAddress.OA_Address2 = "9";
			AssertEquals("Shipment identified by screening", true, Screening.IsIdentifiedForScreening);
			AssertEquals("IdentifiedReason for all stop phrases", "SCR:1,2,3,4,5,6,7,8,9", Screening.IdentifiedReason);
		}

		public void TestIdentifiedReason_Quarantine()
		{
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "Fruit", "Fly" };
			CusHAWB.CS_GoodsDescription = "Small Fruit Fly";
			AssertEquals("IdentifiedReasion for Quarantine", "QUA:Fruit,Fly", Screening.IdentifiedReason);
		}

		public void TestIdentifiedReason_ANY()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Drug");
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "word" };
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "Fruit" };
			CusHAWB.CS_GoodsDescription = "word fruit drug";
			AssertEquals("IdentifiedReasion for Quarantine", "ANY:Drug,word,Fruit", Screening.IdentifiedReason);
		}

		public void TestIdentifiedReason_WhenMaxLengthExceeded()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Biological");
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "Word1", "Word2", "Word3", "" };
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "Fruit", "Fly", "Bug", "Word4" };
			UPEDataRegistry.Instance.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1-20");
			AssertEquals(false, Screening.IsIdentifiedForScreening);
			CusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			CusHAWB.CS_GoodsDescription = "Biological Word1 Fruit Word4 Bug";
			CusHAWB.CS_GoodsValue = 10.55m;
			AssertEquals(true, Screening.IsIdentifiedForScreening);
			AssertEquals("IdentifiedReason with 'etc' to when not enough space to fit all stop words", "ANY:Biological,Word1,Fruit,Bug,etc", Screening.IdentifiedReason);
		}

		public void TestIsGoodsValueIdentified()
		{
			UPEDataRegistry.Instance.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1-2; 5.01-5.99");
			CusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			CusHAWB.CS_GoodsValue = 0.99m;
			AssertEquals("Outside of bounds", false, Screening.IsGoodsValueIdentified);
			CusHAWB.CS_GoodsValue = 1.00m;
			AssertEquals("Lowest value in 1st range", true, Screening.IsGoodsValueIdentified);
			CusHAWB.CS_GoodsValue = 1.00m;
			AssertEquals("Lowest value in 1st range", true, Screening.IsGoodsValueIdentified);
			CusHAWB.CS_GoodsValue = 2.00m;
			AssertEquals("Highest value in 1st range", true, Screening.IsGoodsValueIdentified);
			CusHAWB.CS_GoodsValue = 2.01m;
			AssertEquals("Outside of bounds", false, Screening.IsGoodsValueIdentified);
			CusHAWB.CS_GoodsValue = 5.00m;
			AssertEquals("Outside of bounds", false, Screening.IsGoodsValueIdentified);
			CusHAWB.CS_GoodsValue = 5.01m;
			AssertEquals("Lowest value in 2nd range", true, Screening.IsGoodsValueIdentified);
			CusHAWB.CS_GoodsValue = 5.99m;
			AssertEquals("Highest value in 2nd range", true, Screening.IsGoodsValueIdentified);
			CusHAWB.CS_GoodsValue = 6.00m;
			AssertEquals("Outside of bounds", false, Screening.IsGoodsValueIdentified);
		}

		public void TestIsGoodsValueIdentified_OnlyForNonDocumentShipments()
		{
			UPEDataRegistry.Instance.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1-2; 5.01-5.99");
			CusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
			CusHAWB.CS_GoodsValue = 1.5m;
			AssertEquals("Not identified when shipment type is NOT 'NonDocuments'", false, Screening.IsGoodsValueIdentified);
			CusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			CusHAWB.CS_GoodsValue = 1.5m;
			AssertEquals("Identified when shipment type is 'NonDocuments'", true, Screening.IsGoodsValueIdentified);
		}

		public void TestIsIdentifiedForScreening()
		{
			UPEDataRegistry.Instance.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "20.0 - 30.0");
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "UPS Stop" };
			CusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			CusHAWB.CS_GoodsValue = 10m;
			CusHAWB.CS_GoodsDescription = "UPS Stop";
			AssertEquals("Goods description with a UPS stop phrase", true, Screening.IsIdentifiedForScreening);
			CusHAWB.CS_GoodsValue = 10m;
			CusHAWB.CS_GoodsDescription = "Biological";
			AssertEquals("Goods description has a customs stop phrase only", false, Screening.IsIdentifiedForScreening);
			CusHAWB.CS_GoodsValue = 25m; // within the UPS screening goods value range
			CusHAWB.CS_GoodsDescription = "";
			AssertEquals("Goods value within screening range", true, Screening.IsIdentifiedForScreening);
			CusHAWB.CS_GoodsValue = 30.01m;
			CusHAWB.CS_GoodsDescription = "";
			AssertEquals("Goods value outside of screening range and description with no UPS stop phrases", false, Screening.IsIdentifiedForScreening);
		}

		public void TestIsIdentifiedForQuarantine()
		{
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = TestUPSStopPhrases;
			CusHAWB.CS_GoodsDescription = "no quarantine words";
			AssertEquals("should not be identified for quarantine", false, Screening.IsIdentifiedForQuarantine);
			CusHAWB.CS_GoodsDescription = "blah";
			AssertEquals("should be identified for quarantine", true, Screening.IsIdentifiedForQuarantine);
		}

		#endregion
		#region Test Classes
		class Test300000Line : _300000Line
		{
			public Test300000Line(ZString accountNumber) : base("")
			{
				this.fAccountNumber = accountNumber;
			}

			protected override string UnformattedAccountNumber
			{
				get
				{
					return fAccountNumber;
				}
			}

			readonly ZString fAccountNumber;
		}

		class Test400000Line : _400000Line
		{
			public Test400000Line(ZString accountNumber) : base("")
			{
				this.fAccountNumber = accountNumber;
			}

			protected override string UnformattedAccountNumber
			{
				get
				{
					return fAccountNumber;
				}
			}

			readonly ZString fAccountNumber;
		}

		#endregion
		#region Implementation
		readonly string[] TestUPSStopPhrases = new string[] { "MMs", "FREDOs", "ORANGE BICYCLE", "BLAH" };
		void TestStopPhrasesProperty(ZString message, ZPropertyInfo propertyToCheck, ZString stopPhrasesFoundPropertyName)
		{
			propertyToCheck.Value = (ZString)"Fredos Orange Bicycle";
			var stopPhrases = (IEnumerable<ZString>)Screening.GetType().InvokeMember(stopPhrasesFoundPropertyName, BindingFlags.GetProperty, null, Screening, null);
			AssertEquals(message, 2, stopPhrases.Count());
			AssertEquals(message, "FREDOs", stopPhrases.First());
			AssertEquals(message, "ORANGE BICYCLE", stopPhrases.Skip(1).First());
		}

		UPEScreening Screening
		{
			get
			{
				if (fUPESACDecider == null)
				{
					fUPESACDecider = new UPEScreening(CusHAWB);
				}

				return fUPESACDecider;
			}
		}

		UPEScreening fUPESACDecider;
		UPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.New<UPECusHAWB>();
					fCusHAWB.CS_GoodsValue = 200;
					fCusHAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;
				}

				return fCusHAWB;
			}
		}

		UPECusHAWB fCusHAWB;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 200m);
		}
		#endregion
	}
}
