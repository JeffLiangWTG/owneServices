using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeader))]
	public class CusSeaManOBLHeaderTest : BaseCusSeaManOBLHeaderTest
	{
		public void TestReadOnlyWhenWaiting()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals("by default", false, header.ReadOnly);
			AssertEquals("collection by default", false, header.Details.ReadOnly);

			header.CargoReportStatus.Code = "WTO";
			AssertEquals("read-only when waiting", true, header.ReadOnly);
			AssertEquals("collection read-only when waiting", true, header.Details.ReadOnly);

			header.CargoReportStatus.Code = "ACO";
			AssertEquals("not read-only when not waiting", false, header.ReadOnly);
			AssertEquals("collection not read-only when not waiting", false, header.Details.ReadOnly);

			header.ReadOnly = true;
			AssertEquals("read-only when set to be read-only", true, header.ReadOnly);
			AssertEquals("collection remains in its original state", false, header.Details.ReadOnly);
		}

		public void TestBO_RL_NKDischargePort()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals("precondition discharge empty", ZString.Empty, header.BO_RL_NKDischargePort);
			AssertEquals("precondition destination empty", ZString.Empty, header.BO_RL_NKDestinationPort);

			header.BO_RL_NKDischargePort = "AUSYD";
			AssertEquals("destination defaults to discharge port", "AUSYD", header.BO_RL_NKDestinationPort);

			header.BO_RL_NKDischargePort = "NZAKL";
			AssertEquals("destination NOT changed to discharge port", "AUSYD", header.BO_RL_NKDestinationPort);
		}

		public void TestBO_RL_NKLoadPort()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals("precondition load empty", ZString.Empty, header.BO_RL_NKLoadPort);
			AssertEquals("precondition origin empty", ZString.Empty, header.BO_RL_NKOriginPort);

			header.BO_RL_NKLoadPort = "NZAKL";
			AssertEquals("origin defaults to load port", "NZAKL", header.BO_RL_NKOriginPort);

			header.BO_RL_NKLoadPort = "AUSYD";
			AssertEquals("origin NOT changed to loading port", "NZAKL", header.BO_RL_NKOriginPort);
		}

		public void TestBO_RL_NKOriginPort()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals("precondition origin empty", ZString.Empty, header.BO_RL_NKOriginPort);
			AssertEquals("precondition Goods Origin Country/Region is empty", ZString.Empty, header.BO_RN_NKGoodsCountryOfOrigin);

			header.BO_RL_NKOriginPort = "NZAKL";
			AssertEquals("origin should be NZAKL", "NZAKL", header.BO_RL_NKOriginPort);
			AssertEquals("goods origin should default to NZ", "NZ", header.BO_RN_NKGoodsCountryOfOrigin);

			header.BO_RL_NKOriginPort = "AUSYD";
			AssertEquals("origin should be AUSYD", "AUSYD", header.BO_RL_NKOriginPort);
			AssertEquals("goods origin should be AU", "AU", header.BO_RN_NKGoodsCountryOfOrigin);
		}

		public void TestDetails()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals(typeof(CusSeaManOBLDetailCollection), header.Details.GetType());
		}

		public void TestLookups()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals(typeof(CusSeaManOBLHeaderLookups), header.Lookups.GetType());
		}

		public void TestValidation()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals("Expected overridden class", typeof(CusSeaManOBLHeaderValidation), header.Validation.GetType());
		}

		public void TestCargoReportStatus()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();

			header.CargoReportStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			AssertEquals(CMRBaseStatuses.Codes.OriginalAccepted, header.CargoReportStatus.Code);
			AssertEquals(CMRBaseStatuses.Descriptions.OriginalAccepted, header.CargoReportStatus.Description);
		}

		public void TestCalculator()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertNotNull("Calculator", header.Calculator);
			AssertEquals("type", typeof(CusSeaManOBLHeaderStatusCalculator), header.Calculator.GetType());
		}

		public void TestCargoReportStatusStartsAsNotSent()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();
			AssertEquals("Status Code", CMRBaseStatuses.Codes.NotSent, oceanBill.CargoReportStatus.Code);
		}

		public void TestICMRMessageRespondeeDetails()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();
			transportHeader.BT_VoyageNum = "12345";
			transportHeader.BT_VesselName = "ADMIRALENGRACHT";
			oceanBill.BO_OceanBill = "12345";
			AssertEquals("Details", "Vessel: ADMIRALENGRACHT\r\nVoyage: 12345\r\nOcean Bill: 12345\r\n", ((ICMRMessageRespondee)oceanBill).Details);
		}

		public void TestICMRMessageRespondeeShortDescription()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();
			transportHeader.BT_VoyageNum = "12345";
			transportHeader.BT_VesselName = "ADMIRALENGRACHT";
			oceanBill.BO_OceanBill = "12345";
			AssertEquals("ShortDescription", "Voyage: 12345 Ocean Bill: 12345", ((ICMRMessageRespondee)oceanBill).ShortDescription);
		}

		public void TestLoadFromICusSeaManOBLHeaderInfoProvider()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();
			transportHeader.BT_VoyageNum = "12345";
			transportHeader.BT_VesselName = "ADMIRALENGRACHT";
			oceanBill.BO_OceanBill = "12345";
			TestHelperCusSeaManOBLHeaderInfoProvider info = new TestHelperCusSeaManOBLHeaderInfoProvider();
			info.VoyageNumber = "12344";
			info.OceanBillNumber = "12345";
			info.LloydsNumber = "8811924";
			AssertEquals("Result", null, CusSeaManOBLHeader.Load(Factory, info));
			info.VoyageNumber = "12345";
			AssertEquals("Result", oceanBill, CusSeaManOBLHeader.Load(Factory, info));
			info.LloydsNumber = "8811925";
			AssertEquals("Result", null, CusSeaManOBLHeader.Load(Factory, info));
		}

		public void TestDefaultValues()
		{
			Env.Registry.ConsolPaymentTerm = Enterprise.Core.Constants.PaymentType.Prepaid;
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals("default cargo code", CMRImportCargoCodes.Codes.Import, header.BO_HeaderCargoType);
			AssertEquals("Default Payment Terms", CMRMethodsOfPayment.Codes.PrepaidOnly, header.BO_PaymentMethod);

			Env.Registry.ConsolPaymentTerm = Enterprise.Core.Constants.PaymentType.Collect;
			header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals("Payment Terms", CMRMethodsOfPayment.Codes.Collect, header.BO_PaymentMethod);
		}

		public void TestCanBeDeleted()
		{
			CusSeaManOBLHeader oceanBill = Factory.New<CusSeaManOBLHeader>();

			oceanBill.BO_MessageStatus = CMRBaseStatuses.Codes.WithdrawalAccepted;
			Assert(oceanBill.CanDelete);

			oceanBill.BO_MessageStatus = CMRBaseStatuses.Codes.WithdrawalRejected;
			AssertEquals(false, oceanBill.CanDelete);

			oceanBill.BO_MessageStatus = CMRBaseStatuses.Codes.OriginalRejected;
			Assert(oceanBill.CanDelete);

			oceanBill.BO_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(false, oceanBill.CanDelete);

			oceanBill.BO_MessageStatus = CMRBaseStatuses.Codes.NotSent;
			Assert(oceanBill.CanDelete);

			oceanBill.BO_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
			AssertEquals(false, oceanBill.CanDelete);

			oceanBill.BO_MessageStatus = ZString.Empty;
			Assert(oceanBill.CanDelete);

			oceanBill.BO_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals(false, oceanBill.CanDelete);

			oceanBill.BO_MessageStatus = "YYY";
			AssertEquals(false, new CMRBaseStatuses().ContainsCode(oceanBill.BO_MessageStatus));
			Assert(oceanBill.CanDelete);

			oceanBill.BO_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals(false, oceanBill.CanDelete);
		}

		#region TestHelperCusSeaManOBLHeaderInfoProvider

		class TestHelperCusSeaManOBLHeaderInfoProvider : ICusSeaManOBLHeaderInfoProvider
		{
			ZString fLloydsNumber;
			public ZString LloydsNumber
			{
				get
				{
					return fLloydsNumber;
				}
				set
				{
					fLloydsNumber = value;
				}
			}

			ZString fVoyageNumber;
			public ZString VoyageNumber
			{
				get
				{
					return fVoyageNumber;
				}
				set
				{
					fVoyageNumber = value;
				}
			}

			ZString fOceanBillNumber;
			public ZString OceanBillNumber
			{
				get
				{
					return fOceanBillNumber;
				}
				set
				{
					fOceanBillNumber = value;
				}
			}
		}

		#endregion

		#region Consignee / Consignor

		public void TestConsigneeReadOnly()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();
			AssertConsigneeReadOnly(oceanBill, new ReadOnlyAsserter("by default", false));

			OrgHeader org = GetNewOrg();
			oceanBill.BO_OH_Consignee = org.PK;
			AssertConsigneeReadOnly(oceanBill, new ReadOnlyAsserter("valid consignee set", true));

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusSeaManOBLHeader loadedOceanBill = factory2.Load<CusSeaManOBLHeader>(oceanBill.PK);
			AssertConsigneeReadOnly(loadedOceanBill, new ReadOnlyAsserter("valid consignee set and loaded", true));

			oceanBill.BO_OH_Consignee = ZGuid.Empty;
			AssertConsigneeReadOnly(oceanBill, new ReadOnlyAsserter("consignee reverted to empty", false));
		}

		public void TestConsignorReadOnly()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();
			AssertConsignorReadOnly(oceanBill, new ReadOnlyAsserter("by default", false));

			OrgHeader org = GetNewOrg();
			oceanBill.BO_OH_Consignor = org.PK;
			AssertConsignorReadOnly(oceanBill, new ReadOnlyAsserter("valid consignee set", true));

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusSeaManOBLHeader loadedOceanBill = factory2.Load<CusSeaManOBLHeader>(oceanBill.PK);
			AssertConsignorReadOnly(loadedOceanBill, new ReadOnlyAsserter("valid consignee set and loaded", true));

			oceanBill.BO_OH_Consignor = ZGuid.Empty;
			AssertConsignorReadOnly(oceanBill, new ReadOnlyAsserter("consignee reverted to empty", false));
		}

		public void TestConsigneeProxy()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();

			oceanBill.BO_ConsigneeName = "Yeah";
			oceanBill.BO_ConsigneeAddress1 = "Yeah";
			oceanBill.BO_ConsigneeAddress2 = "Yeah";
			oceanBill.BO_ConsigneeCity = "Yeah";
			oceanBill.BO_ConsigneePostCode = "Yeah";
			oceanBill.BO_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.Australia;

			OrgHeader org = GetNewOrg();
			oceanBill.BO_OH_Consignee = org.PK;

			AssertEquals("BO_ConsigneeName is proxied", "Blah", oceanBill.BO_ConsigneeName);
			AssertEquals("BO_ConsigneeAddress1 is proxied", "Bleh", oceanBill.BO_ConsigneeAddress1);
			AssertEquals("BO_ConsigneeAddress2 is proxied", "Bloh", oceanBill.BO_ConsigneeAddress2);
			AssertEquals("BO_ConsigneeCity is proxied", "Blap", oceanBill.BO_ConsigneeCity);
			AssertEquals("BO_ConsigneePostCode is proxied", "1234", oceanBill.BO_ConsigneePostCode);
			AssertEquals("BO_RN_NKConsigneeCountryCode is proxied", Core.Constants.CountryCodes.NewZealand, oceanBill.BO_RN_NKConsigneeCountryCode);

			oceanBill.BO_OH_Consignee = ZGuid.Empty;

			AssertEquals("BO_ConsigneeName reverts", "Yeah", oceanBill.BO_ConsigneeName);
			AssertEquals("BO_ConsigneeAddress1 reverts", "Yeah", oceanBill.BO_ConsigneeAddress1);
			AssertEquals("BO_ConsigneeAddress2 reverts", "Yeah", oceanBill.BO_ConsigneeAddress2);
			AssertEquals("BO_ConsigneeCity reverts", "Yeah", oceanBill.BO_ConsigneeCity);
			AssertEquals("BO_ConsigneePostCode reverts", "Yeah", oceanBill.BO_ConsigneePostCode);
			AssertEquals("BO_RN_NKConsigneeCountryCode reverts", Core.Constants.CountryCodes.Australia, oceanBill.BO_RN_NKConsigneeCountryCode);
		}

		public void TestConsigneeProxyAfterLoad()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();

			OrgHeader org = GetNewOrg();
			oceanBill.BO_OH_Consignee = org.PK;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusSeaManOBLHeader loadedOceanBill = factory2.Load<CusSeaManOBLHeader>(oceanBill.PK);

			AssertEquals("BO_ConsigneeName is proxied after load", "Blah", loadedOceanBill.BO_ConsigneeName);
			AssertEquals("BO_ConsigneeAddress1 is proxied after load", "Bleh", loadedOceanBill.BO_ConsigneeAddress1);
			AssertEquals("BO_ConsigneeAddress2 is proxied after load", "Bloh", loadedOceanBill.BO_ConsigneeAddress2);
			AssertEquals("BO_ConsigneeCity is proxied after load", "Blap", loadedOceanBill.BO_ConsigneeCity);
			AssertEquals("BO_ConsigneePostCode is proxied after load", "1234", loadedOceanBill.BO_ConsigneePostCode);
			AssertEquals("BO_RN_NKConsigneeCountryCode reverts", Core.Constants.CountryCodes.NewZealand, loadedOceanBill.BO_RN_NKConsigneeCountryCode);
		}

		public void TestConsignorProxy()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();

			oceanBill.BO_ConsignorName = "Yeah";
			oceanBill.BO_ConsignorAddress1 = "Yeah";
			oceanBill.BO_ConsignorAddress2 = "Yeah";
			oceanBill.BO_ConsignorCity = "Yeah";
			oceanBill.BO_ConsignorPostCode = "Yeah";
			oceanBill.BO_RN_NKConsignorCountryCode = Core.Constants.CountryCodes.Australia;

			OrgHeader org = GetNewOrg();
			oceanBill.BO_OH_Consignor = org.PK;

			AssertEquals("BO_ConsignorName is proxied", "Blah", oceanBill.BO_ConsignorName);
			AssertEquals("BO_ConsignorAddress1 is proxied", "Bleh", oceanBill.BO_ConsignorAddress1);
			AssertEquals("BO_ConsignorAddress2 is proxied", "Bloh", oceanBill.BO_ConsignorAddress2);
			AssertEquals("BO_ConsignorCity is proxied", "Blap", oceanBill.BO_ConsignorCity);
			AssertEquals("BO_ConsignorPostCode is proxied", "1234", oceanBill.BO_ConsignorPostCode);
			AssertEquals("BO_RN_NKConsignorCountryCode is proxied", Core.Constants.CountryCodes.NewZealand, oceanBill.BO_RN_NKConsignorCountryCode);

			oceanBill.BO_OH_Consignor = ZGuid.Empty;

			AssertEquals("BO_ConsignorName reverts", "Yeah", oceanBill.BO_ConsignorName);
			AssertEquals("BO_ConsignorAddress1 reverts", "Yeah", oceanBill.BO_ConsignorAddress1);
			AssertEquals("BO_ConsignorAddress2 reverts", "Yeah", oceanBill.BO_ConsignorAddress2);
			AssertEquals("BO_ConsignorCity reverts", "Yeah", oceanBill.BO_ConsignorCity);
			AssertEquals("BO_ConsignorPostCode reverts", "Yeah", oceanBill.BO_ConsignorPostCode);
			AssertEquals("BO_RN_NKConsignorCountryCode reverts", Core.Constants.CountryCodes.Australia, oceanBill.BO_RN_NKConsignorCountryCode);
		}

		public void TestConsignorProxyAfterLoad()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oceanBill = transportHeader.OceanBills.AddNew();

			OrgHeader org = GetNewOrg();
			oceanBill.BO_OH_Consignor = org.PK;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusSeaManOBLHeader loadedOceanBill = factory2.Load<CusSeaManOBLHeader>(oceanBill.PK);

			AssertEquals("BO_ConsignorName is proxied after load", "Blah", loadedOceanBill.BO_ConsignorName);
			AssertEquals("BO_ConsignorAddress1 is proxied after load", "Bleh", loadedOceanBill.BO_ConsignorAddress1);
			AssertEquals("BO_ConsignorAddress2 is proxied after load", "Bloh", loadedOceanBill.BO_ConsignorAddress2);
			AssertEquals("BO_ConsignorCity is proxied after load", "Blap", loadedOceanBill.BO_ConsignorCity);
			AssertEquals("BO_ConsignorPostCode is proxied after load", "1234", loadedOceanBill.BO_ConsignorPostCode);
			AssertEquals("BO_RN_NKConsignorCountryCode reverts", Core.Constants.CountryCodes.NewZealand, loadedOceanBill.BO_RN_NKConsignorCountryCode);
		}

		#endregion

		#region Implementation

		OrgHeader GetNewOrg()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;
			org.OH_IsConsignor = true;
			org.OH_FullName = "Blah";
			org.MainAddress.OA_Address1 = "Bleh";
			org.MainAddress.OA_Address2 = "Bloh";
			org.MainAddress.OA_City = "Blap";
			org.MainAddress.OA_PostCode = "1234";

			RefUNLOCO port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			org.OH_RL_NKClosestPort = port.RL_Code;

			return org;
		}

		void AssertConsigneeReadOnly(CusSeaManOBLHeader oceanBill, ReadOnlyAsserter asserter)
		{
			asserter.Assert(oceanBill.BO_ConsigneeAddress1Info);
			asserter.Assert(oceanBill.BO_ConsigneeCityInfo);
			asserter.Assert(oceanBill.BO_ConsigneeNameInfo);
			asserter.Assert(oceanBill.BO_ConsigneePostCodeInfo);
			asserter.Assert(oceanBill.BO_RN_NKConsigneeCountryCodeInfo);
		}

		void AssertConsignorReadOnly(CusSeaManOBLHeader oceanBill, ReadOnlyAsserter asserter)
		{
			asserter.Assert(oceanBill.BO_ConsignorAddress1Info);
			asserter.Assert(oceanBill.BO_ConsignorCityInfo);
			asserter.Assert(oceanBill.BO_ConsignorNameInfo);
			asserter.Assert(oceanBill.BO_ConsignorPostCodeInfo);
			asserter.Assert(oceanBill.BO_RN_NKConsignorCountryCodeInfo);
		}

		class ReadOnlyAsserter
		{
			public ReadOnlyAsserter(ZString message, ZBool isReadOnly)
			{
				this.message = message;
				this.isReadOnly = isReadOnly;
			}

			public void Assert(ZPropertyInfo info)
			{
				AssertEquals(info.HumanReadableName + (isReadOnly ? " not" : "") + " read only when " + message, isReadOnly, info.ReadOnly);
			}

			readonly ZString message;
			readonly ZBool isReadOnly;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(CusSeaManOBLHeader));
		}

		#endregion
	}
}
