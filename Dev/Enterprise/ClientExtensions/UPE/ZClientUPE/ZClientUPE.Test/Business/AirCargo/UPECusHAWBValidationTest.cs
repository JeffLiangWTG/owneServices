using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class UPECusHAWBValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateBillingTerms()
		{
			CusHAWB.BillingTerms = "";
			AssertMandatoryValidationError(CusHAWB.BillingTermsInfo, true);
			CusHAWB.BillingTerms = "XXX";
			AssertListValidationInvalidCodeError(CusHAWB.BillingTermsInfo, true);
			CusHAWB.BillingTerms = BillingTermsCodeDescriptionPairList.Codes.CostAndFreight;
			AssertListValidationInvalidCodeError(CusHAWB.BillingTermsInfo, false);
		}

		public void TestValidateShipmentType()
		{
			CusHAWB.ShipmentType = "";
			AssertMandatoryValidationError(CusHAWB.ShipmentTypeInfo, true);
			CusHAWB.ShipmentType = "XXX";
			AssertListValidationInvalidCodeError(CusHAWB.ShipmentTypeInfo, true);
			CusHAWB.ShipmentType = ShipmentTypeCodeDescriptionPairList.Codes.Documents;
			AssertListValidationInvalidCodeError(CusHAWB.ShipmentTypeInfo, false);
		}

		public void TestValidateDutyType()
		{
			CusHAWB.DutyType = "";
			AssertMandatoryValidationError(CusHAWB.DutyTypeInfo, true);
			CusHAWB.DutyType = "XXX";
			AssertListValidationInvalidCodeError(CusHAWB.DutyTypeInfo, true);
			CusHAWB.DutyType = DutyTypeCodeDescriptionPairList.Codes.NonDutiable;
			AssertListValidationInvalidCodeError(CusHAWB.DutyTypeInfo, false);
		}

		#region Profiling and Screening
		#region Stop Phrases
		public void TestValidateCS_GoodsDescription_ForBothUPSAndCustomsStopPhrases()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, CustomsStopPhrase);
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = TestUPSStopPhrases;
			TaxOrFeeTestHelper.SetDeminimus(Factory, 100m);
			CusHAWB.CS_GoodsValue = 100;
			CusHAWB.CS_GoodsDescription = "MEH " + CustomsStopPhrase + " HELLoo-ooooooooo;LAUGH ";
			AssertHasWarning("Should have a UPS Stop Words warning", CusHAWB.CS_GoodsDescriptionInfo, "Customs Stop Words found: " + CustomsStopPhrase + "\r\nUPS Stop Words found: MEH, LAUGH, HELLOO");
			CusHAWB.CS_GoodsValue = 101;
			AssertNoWarnings("Should not have a warning if over the customs screen-free threshold", CusHAWB.CS_GoodsDescriptionInfo);
		}

		public void TestValidateCS_GoodsDescription_ForCustomsStopPhrases()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, CustomsStopPhrase);
			TaxOrFeeTestHelper.SetDeminimus(Factory, 100m);
			CusHAWB.CS_GoodsValue = 50;
			CusHAWB.CS_GoodsDescription = "Customs stop word is " + CustomsStopPhrase;
			AssertHasWarning("Should have a Customs Stop Words warning", CusHAWB.CS_GoodsDescriptionInfo, "Customs Stop Words found: " + CustomsStopPhrase);
			CusHAWB.CS_GoodsValue = 101;
			AssertNoWarnings("Should not have a warning if over the customs screen-free threshold", CusHAWB.CS_GoodsDescriptionInfo);
		}

		public void TestValidateCS_GoodsDescription_ForUPSStopPhrases()
		{
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = TestUPSStopPhrases;
			TestValidatePropertyForStopPhrase(CusHAWB.CS_GoodsDescriptionInfo, true);
		}

		public void TestValidateCS_ConsignorName_ForStopPhrases()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsignorName = TestUPSStopPhrases;
			TestValidatePropertyForStopPhrase(CusHAWB.CS_ConsignorNameInfo, false);
		}

		public void TestValidateCS_ConsignorStreet_ForStopPhrases()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsignorAddress = TestUPSStopPhrases;
			TestValidatePropertyForStopPhrase(CusHAWB.CS_ConsignorStreetInfo, false);
		}

		public void TestValidateCS_ConsignorStreet2_ForStopPhrases()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsignorAddress = TestUPSStopPhrases;
			TestValidatePropertyForStopPhrase(CusHAWB.CS_ConsignorStreet2Info, false);
		}

		public void TestValidateLevel1RecordConsignorAccountNumInfo_ForStopPhrases()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsignorAccountNum = TestUPSStopPhrases;
			TestValidatePropertyForStopPhrase(CusHAWB.Level1RecordConsignorAccountNumInfo, false);
		}

		public void TestValidateCS_ConsigneeName_ForStopPhrases()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsigneeName = TestUPSStopPhrases;
			TestValidatePropertyForStopPhrase(CusHAWB.CS_ConsigneeNameInfo, false);
		}

		public void TestValidateCS_ConsigneeStreet_ForStopPhrases()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAddress = TestUPSStopPhrases;
			TestValidatePropertyForStopPhrase(CusHAWB.CS_ConsigneeStreetInfo, false);
		}

		public void TestValidateCS_ConsigneeStreet2_ForStopPhrases()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAddress = TestUPSStopPhrases;
			TestValidatePropertyForStopPhrase(CusHAWB.CS_ConsigneeStreet2Info, false);
		}

		public void TestValidateLevel1RecordConsigneeAccountNumInfo_ForStopPhrases()
		{
			UPEDataRegistry.Instance.StopPhrasesForConsigneeAccountNum = TestUPSStopPhrases;
			TestValidatePropertyForStopPhrase(CusHAWB.Level1RecordConsigneeAccountNumInfo, false);
		}

		void TestValidatePropertyForStopPhrase(ZPropertyInfo propertyToCheck, bool expectNoWarningIfOverScreenFreeThreshold)
		{
			AssertNoWarnings("No warning initially for the test", propertyToCheck);
			propertyToCheck.Value = new ZString("HELLOO LAUGH");
			AssertHasWarning("Should have a warning now that a stop phrase has been set", propertyToCheck, "UPS Stop Words found: LAUGH, HELLOO");
			if (expectNoWarningIfOverScreenFreeThreshold)
			{
				TaxOrFeeTestHelper.SetDeminimus(Factory, 100m);
				CusHAWB.CS_GoodsValue = 150;
				propertyToCheck.Value = new ZString("HELLOO LAUGH");
				AssertNoWarnings("Expected no warning if over the screen free threshold", propertyToCheck);
			}
		}

		readonly string[] TestUPSStopPhrases = new string[] { "MEH", "LAUGH", "HELLOO" };
		#endregion
		#region Goods Value
		public void TestValidateCS_GoodsValue()
		{
			UPEDataRegistry.Instance.NonDocumentScreeningValueRangesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2-5");
			CusHAWB.CS_GoodsValue = 1;
			AssertNoWarnings("No warnings initially for the test", CusHAWB.CS_GoodsValueInfo);
			CusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.Letter;
			CusHAWB.CS_GoodsValue = 3;
			AssertNoWarnings("No warnings when shipment type is NOT 'NonDocuments'", CusHAWB.CS_GoodsValueInfo);
			CusHAWB.CS_ShipmentTypeForBinding = ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments;
			CusHAWB.CS_GoodsValue = 3;
			AssertHasWarning(CusHAWB.CS_GoodsValueInfo, "Goods value identified for UPS screening");
		}

		#endregion
		#endregion
		#region Test Classes
		class TestUPECusHAWB : UPECusHAWB
		{
			public TestUPECusHAWB(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new ZString Level1RecordConsignorAccountNum
			{
				get
				{
					return base.Level1RecordConsignorAccountNum;
				}

				set
				{
					Level1Record._300000 = new Test300000Line(value);
					UPECusHAWBValidation.ValidateLevel1RecordConsignorAccountNum();
				}
			}

			public new ZString Level1RecordConsigneeAccountNum
			{
				get
				{
					return base.Level1RecordConsigneeAccountNum;
				}

				set
				{
					Level1Record._400000 = new Test400000Line(value);
					UPECusHAWBValidation.ValidateLevel1RecordConsigneeAccountNum();
				}
			}
		}

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
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			TaxOrFeeTestHelper.SetUp();
		}

		TestUPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.NewWithValidTestData<TestUPECusHAWB>();
					fCusHAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;
					fCusHAWB.Level1Record = new Level1Record();
				}

				return fCusHAWB;
			}
		}

		TestUPECusHAWB fCusHAWB;
		string CustomsStopPhrase => "Trigger";
		#endregion
	}
}
