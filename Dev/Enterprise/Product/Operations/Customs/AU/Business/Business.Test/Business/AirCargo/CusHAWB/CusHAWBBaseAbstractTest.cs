using System;
using System.Data;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusHAWBBaseAbstractTest : Customs.Business.Testing.CusHAWBTest
	{
		public void TestITransitWarehouseSyncEventParent()
		{
			var houseBill = GetHouseBillWithMaster();
			houseBill.CS_CustomsStatus = "CLR";

			var eventParent = (ITransitWarehouseSyncEventParent)houseBill;
			CombineAssertions(() =>
			{
				AssertEquals("CustomsStatus", "CLR", eventParent.CustomsStatus);
				AssertSame("Factory", houseBill.Factory, eventParent.Factory);
			});
		}

		public void TestStrictBizPropertyInfos()
		{
			var houseAwb = GetHouseBillWithMaster();
			var infos = new[] { houseAwb.CS_CMInfo, houseAwb.CS_MsgStatusInfo };

			CombineAssertions(() =>
			{
				var format = "The concurrency policy of {0} should default to ConcurrencyPolicy.Default.";

				foreach (var info in infos)
				{
					var message = string.Format(format, info.Name);

					AssertEquals(message, ConcurrencyPolicy.Default.AllowMerge, info.ConcurrencyPolicy.AllowMerge);
					AssertEquals(message, ConcurrencyPolicy.Default.CollisionCheck, info.ConcurrencyPolicy.CollisionCheck);
				}
			});

			Factory.Save();

			CombineAssertions(() =>
			{
				var format = "The concurrency policy should is ConcurrencyPolicy.Default as the house bill just saved when it is not InDatabase.";

				foreach (var info in infos)
				{
					var message = string.Format(format, info.Name);

					AssertEquals(message, ConcurrencyPolicy.Default.AllowMerge, info.ConcurrencyPolicy.AllowMerge);
					AssertEquals(message, ConcurrencyPolicy.Default.CollisionCheck, info.ConcurrencyPolicy.CollisionCheck);
				}
			});

			houseAwb.CS_GoodsDescription = "TEST FOR SAVE";
			Factory.Save();

			CombineAssertions(() =>
			{
				var format = "The concurrency policy should is ConcurrencyPolicy.Strict as the house bill just saved when it is InDatabase.";

				foreach (var info in infos)
				{
					var message = string.Format(format, info.Name);

					AssertEquals(message, ConcurrencyPolicy.Strict.AllowMerge, info.ConcurrencyPolicy.AllowMerge);
					AssertEquals(message, ConcurrencyPolicy.Strict.CollisionCheck, info.ConcurrencyPolicy.CollisionCheck);
				}
			});
		}

		public void TestConcurrencyPolicy_CS_CM()
		{
			var houseAwb = GetHouseBillWithMaster();

			Factory.Save();

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;

			var houseAwbInNewFactory = (CusHAWBBase)newFactory.Load(houseAwb.GetType(), houseAwb.PK);

			Assert("Precodition", houseAwb.CS_CM.IsValid);
			Assert("Precodition", houseAwbInNewFactory.CS_CM.IsValid);

			houseAwbInNewFactory.CS_GoodsDescription = "TEST FOR SAVE";
			houseAwbInNewFactory.CS_CM = ZGuid.Empty;

			newFactory.Save();

			Assert("Precodition", houseAwb.CS_CM.IsValid);
			Assert("Precodition", !houseAwbInNewFactory.CS_CM.IsValid);

			houseAwb.CS_ConsignorPhone = "15000000000";
			AssertExceptionThrown<ZSaveConcurrencyException>("The value of CS_CM is different from the database.", () => Factory.Save());
		}

		public void TestConcurrencyPolicy_CS_MsgStatus()
		{
			var houseAwb = GetHouseBillWithMaster();
			houseAwb.CS_MsgStatus = string.Empty;

			Factory.Save();

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;

			var houseAwbInNewFactory = (CusHAWBBase)newFactory.Load(houseAwb.GetType(), houseAwb.PK);

			Assert("Precodition", houseAwb.CS_MsgStatus.IsEmpty);
			Assert("Precodition", houseAwbInNewFactory.CS_MsgStatus.IsEmpty);

			houseAwbInNewFactory.CS_GoodsDescription = "TEST FOR SAVE";
			houseAwbInNewFactory.CS_MsgStatus = CMRBaseStatuses.Codes.AmendmentAccepted;

			newFactory.Save();

			Assert("Precodition", houseAwb.CS_MsgStatus.IsEmpty);
			Assert("Precodition", !houseAwbInNewFactory.CS_MsgStatus.IsEmpty);

			houseAwb.CS_ConsignorPhone = "15000000000";
			AssertExceptionThrown<ZSaveConcurrencyException>("The value of CS_MsgStatus is different from the database.", () => Factory.Save());
		}

		public void TestErrorWhenHawbAlreadyCreatedOnSaving()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;

				var consol = factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_MasterBillNum = "08111111111";
				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HB001";
				var mawb = factory.New<CusMAWB>();
				mawb.CM_MAWB = "08111111111";
				mawb.CM_JK = consol.PK;
				factory.Save();

				var bill1 = factory.New<CusHAWB>();
				bill1.CS_HAWB = "Bill1";
				bill1.CS_JS = shipment.PK;
				bill1.CS_CM = mawb.PK;

				CusHAWB bill2 = null;

				factory.Saving += (BusinessObjectFactory f) =>
				{
					var factory2 = new BusinessObjectFactory();
					factory2.RefreshEnabled = false;

					bill2 = factory2.New<CusHAWB>();
					bill2.CS_HAWB = "Bill2";
					bill2.CS_JS = shipment.PK;
					bill2.CS_CM = mawb.PK;
					AssertNoExceptionThrown(() => factory2.Save());
				};

				var odysseyException = AssertExceptionThrown<ZArchitecture.Environment.OdysseyException>(() => factory.Save());

				bill2.Reload();
				var expectedErrorMessage = $@"A House Bill for this Shipment has already been created (On saving).
This Hawb PK: {bill1.PK}
HAWB: 'Bill1'
MAWB: '08111111111' ({bill1.MAWB.PK})
Shipment: '{bill1.Shipment.JobNumber}' ({bill1.Shipment.PK})
Created: '{bill1.CS_SystemCreateTimeUtc.ToString("u")}' by '{bill1.CS_SystemCreateUser}'
Other Hawb PK: {bill2.PK}
HAWB: 'Bill2'
MAWB: '08111111111' ({bill2.MAWB.PK})
Shipment: '{bill2.Shipment.JobNumber}' ({bill2.Shipment.PK})
Created: '{bill2.CS_SystemCreateTimeUtc.ToString("u")}' by '{bill2.CS_SystemCreateUser}'";
				AssertMultilineASCIIEquals(expectedErrorMessage, odysseyException.Message);
			}
		}

		public void TestIsMasterBillChangedFromEmpty()
		{
			var houseAwb = GetHouseBillWithMaster();

			Factory.Save();

			var newFactory = NewFactory();
			var houseAwbInNewFactory = (CusHAWBBase)newFactory.Load(houseAwb.GetType(), houseAwb.PK);

			Assert("Precodition", !houseAwbInNewFactory.CS_CMInfo.OriginalValue.IsEmpty);
			Assert("Precodition", houseAwbInNewFactory.CS_CM.IsValid);

			Assert("Should be false as the CS_CM is valid.", !houseAwbInNewFactory.IsMasterBillChangedFromEmpty);

			houseAwbInNewFactory.CS_CM = ZGuid.Empty;
			Assert("Should be false as the original value of CS_CM is not empty.", !houseAwbInNewFactory.IsMasterBillChangedFromEmpty);

			newFactory.Save();

			Assert("Precodition", houseAwbInNewFactory.CS_CMInfo.OriginalValue.IsEmpty);
			Assert("Precodition", !houseAwbInNewFactory.CS_CM.IsValid);

			Assert("Should be false as the CS_CM is not valid.", !houseAwbInNewFactory.IsMasterBillChangedFromEmpty);

			houseAwbInNewFactory.CS_CM = newFactory.NewWithValidTestData<CusMAWB>().PK;
			Assert("Should be true as the CS_CM is changed from an empty value to a valid value.", houseAwbInNewFactory.IsMasterBillChangedFromEmpty);

			newFactory.Save();

			Assert("Precodition", !houseAwbInNewFactory.CS_CMInfo.OriginalValue.IsEmpty);
			Assert("Should be false as the original value of CS_CM is not empty.", !houseAwbInNewFactory.IsMasterBillChangedFromEmpty);
		}

		public void TestCS_GoodsDescription()
		{
			var hawb = Factory.NewWithValidTestData<CusHAWB>();
			hawb.CS_GoodsDescription = "A";
			AssertEquals("A", hawb.CS_GoodsDescription);
			AssertEquals("A", ((INeedRow)hawb).Row[CusHAWBSchema.Constants.CS_GoodsDescription]);
			AssertEquals(0, hawb.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).Length);
			hawb.CS_GoodsDescription = "B";
			AssertEquals("B", ((INeedRow)hawb).Row[CusHAWBSchema.Constants.CS_GoodsDescription]);
			AssertEquals("B", hawb.CS_GoodsDescription);
			Factory.Save();
			AssertEquals(0, hawb.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).Length);
		}

		public void TestGetCS_GoodsDescriptionNoException()
		{
			var hawb = Factory.New<CusHAWB>();
			AssertNoExceptionThrown("No Null Reference Exception thrown", () => hawb.CS_GoodsDescription = "~HASH~222");
			AssertEquals("~HASH~222", hawb.CS_GoodsDescription);

			hawb.CS_GoodsDescription = "A111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111101234567890";
			AssertEquals("A111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111101234567890", hawb.CS_GoodsDescription);

			Assert(!hawb.Notes.HasNotes);
		}

		[TestDate(2014, 02, 01, 01, 01, 01)]
		public void TestSettingConRef()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var mawb1 = Factory.New<CusMAWB>();
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "H1";
			var hawb2 = mawb1.ChildBills.AddNew();
			hawb2.CS_HAWB = "H2";
			hawb2.CS_fPartShipConsignmentReference = "XXX2012120112345678";
			Factory.Save();
			AssertEquals("New con ref allocated", "EDI2014020100000001", hawb1.CS_fPartShipConsignmentReference);
			AssertEquals("Already set", "XXX2012120112345678", hawb2.CS_fPartShipConsignmentReference);

			var mawb2 = Factory.New<CusMAWB>();
			var hawb3 = mawb2.ChildBills.AddNew();
			hawb3.CS_HAWB = "H1";
			var hawb4 = mawb2.ChildBills.AddNew();
			hawb4.CS_HAWB = "H2";
			var hawb5 = mawb2.ChildBills.AddNew();
			hawb5.CS_HAWB = "H5";
			hawb5.CS_fPartShipConsignmentReference = "GARBAGE";
			Factory.Save();
			AssertEquals("Same as h1", "EDI2014020100000001", hawb3.CS_fPartShipConsignmentReference);
			AssertEquals("New con ref as existing is old", "EDI2014020100000002", hawb4.CS_fPartShipConsignmentReference);
			AssertEquals("GARBAGE", hawb5.CS_fPartShipConsignmentReference);

			hawb4.CS_fPartShipConsignmentReference = "";
			hawb2.CS_fPartShipConsignmentReference = "XXX2014010112345678";
			var hawb6 = mawb2.ChildBills.AddNew();
			hawb6.CS_HAWB = "H2";
			var hawb7 = mawb2.ChildBills.AddNew();
			hawb7.CS_HAWB = "H5";
			Factory.Save();
			AssertEquals("Sames as h2 as now is not old", "XXX2014010112345678", hawb6.CS_fPartShipConsignmentReference);
			AssertEquals("New allocated as existing is garbage, and no exception thrown", "EDI2014020100000003", hawb7.CS_fPartShipConsignmentReference);
		}

		[TestDate(2013, 02, 01, 01, 01, 01)]
		public void TestSaveFailureRevertsConRefObtainedFromNumberFountain()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var mawb1 = Factory.New<CusMAWB>();
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "H1";
			bool saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				saveFailed = true;
			}
			Assert(!saveFailed);
			AssertEquals("New con ref allocated", "EDI2013020100000001", hawb1.CS_fPartShipConsignmentReference);
			var hawb2 = mawb1.ChildBills.AddNew();
			hawb2.CS_HAWB = "H2";
			hawb2.ForceExceptionAfterConRefAllocated = true;
			saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				saveFailed = true;
			}
			Assert(saveFailed);
			AssertEquals("Con ref should NOT allocated", ZString.Empty, hawb2.CS_fPartShipConsignmentReference);
		}

		public void TestCS_fPartShipConsignmentReference()
		{
			Assert(HAWB.CS_fPartShipConsignmentReference.IsEmpty);
			HAWB.CS_fPartShipConsignmentReference = "XYZ";
			AssertEquals("XYZ", HAWB.CS_fPartShipConsignmentReference);
		}

		public void TestLoadFromMessageReference()
		{
			CusMAWB masterBill = Factory.New<CusMAWB>();
			CusHAWB hAWB = masterBill.ChildBills.AddNew();
			hAWB.CS_MessageReference = "12345";
			AssertNotNull(CusHAWBBase.Load(Factory, "12345"));
			AssertNotNull(CusHAWBBase.Load(Factory, "12345/1"));
			AssertEquals(typeof(CusHAWB), CusHAWBBase.Load(Factory, "12345/1").GetType());

			CTOCusMAWB masterBill2 = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB2 = masterBill2.ChildBills.AddNew();
			hAWB2.CS_MessageReference = "12346";
			AssertNotNull(CusHAWBBase.Load(Factory, "12346"));
			AssertNotNull(CusHAWBBase.Load(Factory, "12346/1"));
			AssertEquals(typeof(CTOCusHAWB), CusHAWBBase.Load(Factory, "12346/1").GetType());
		}

		public void TestPopulateResponsiblePartyFromConsigneeABNorCCID()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "Cuckoo Sqkr";
			var address = consignee.Addresses.AddNewMainAddress();
			consignee.PrimaryRegistrationNumber.Number = "123456789";
			HAWB.CS_OA_ConsigneeAddress = address.PK;

			var cusCode = consignee.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			cusCode.OK_CustomsRegNo = "987654321";
			cusCode.OK_OA_PremisesAddress = address.PK;
			Factory.Save();

			HAWB.PopulateResponsiblePartyFromConsigneeABNorCCID();
			AssertEquals("123456789", HAWB.CS_ResponsiblePartyID);
			consignee.PrimaryRegistrationNumber.Number = "";
			Factory.Save();
			HAWB.PopulateResponsiblePartyFromConsigneeABNorCCID();
			AssertEquals("987654321", HAWB.CS_ResponsiblePartyID);

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			HAWB.PopulateResponsiblePartyFromConsigneeABNorCCID();
			AssertEquals(ZString.Empty, HAWB.CS_ResponsiblePartyID);
		}

		public void TestCS_ConsigneeIdentifierAndCS_ConsigneeBusinessNumberReadOnlyWhenUnmatched()
		{
			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			HAWB.CS_OA_ConsigneeAddress = unmatchedOrg.MainAddress.PK;
			Assert(!HAWB.CS_ConsigneeIdentifierInfo.ReadOnly);
			Assert(!HAWB.CS_ConsigneeBusinessNumberInfo.ReadOnly);

			var org = Factory.New<OrgHeader>();
			HAWB.CS_OA_ConsigneeAddress = org.MainAddress.PK;
			Assert(HAWB.CS_ConsigneeIdentifierInfo.ReadOnly);
			Assert(HAWB.CS_ConsigneeBusinessNumberInfo.ReadOnly);
		}

		public void TestCS_ConsignorIdentifierAndCS_VendorIdentifierReadOnlyWhenUnmatched()
		{
			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			HAWB.CS_OA_ConsignorAddress = unmatchedOrg.MainAddress.PK;
			Assert(!HAWB.CS_ConsignorIdentifierInfo.ReadOnly);
			Assert(!HAWB.CS_VendorIdentifierInfo.ReadOnly);

			var org = Factory.New<OrgHeader>();
			HAWB.CS_OA_ConsignorAddress = org.MainAddress.PK;
			Assert(HAWB.CS_ConsignorIdentifierInfo.ReadOnly);
			Assert(HAWB.CS_VendorIdentifierInfo.ReadOnly);
		}

		public void TestImporterABNAndImporterIdentifier()
		{
			var consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_Code = "Test 1";
			consignee1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
			consignee1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");

			HAWB.CS_OA_ConsigneeAddress = consignee1.MainAddress.PK;
			AssertEquals("", HAWB.CS_ConsigneeBusinessNumber);
			AssertEquals("12345678901", HAWB.CS_ConsigneeIdentifier);

			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "Test 2";
			consignee2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");

			HAWB.CS_OA_ConsigneeAddress = consignee2.MainAddress.PK;
			AssertEquals("12345678901/123", HAWB.CS_ConsigneeBusinessNumber);
			AssertEquals("", HAWB.CS_ConsigneeIdentifier);

			var consignee3 = Factory.New<OrgHeader>();
			consignee3.OH_Code = "Test 3";
			consignee3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");

			HAWB.CS_OA_ConsigneeAddress = consignee3.MainAddress.PK;
			AssertEquals("", HAWB.CS_ConsigneeBusinessNumber);
			AssertEquals("12345678901", HAWB.CS_ConsigneeIdentifier);

			var consignee4 = Factory.New<OrgHeader>();
			consignee4.OH_Code = "Test 4";
			consignee4.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");

			HAWB.CS_OA_ConsigneeAddress = consignee4.MainAddress.PK;
			AssertEquals("12345678901", HAWB.CS_ConsigneeBusinessNumber);
			AssertEquals("", HAWB.CS_ConsigneeIdentifier);
		}

		public void TestUnderbondStatusOnHAWB()
		{
			Assert("Should default to empty", HAWB.UnderbondStatus.IsEmpty);
			CusUnderbond underbond = HAWB.Underbonds.AddNew();
			HAWB.AllUnderbonds.Load();
			underbond.C4_ParentID = HAWB.PK;
			underbond.UnderbondStatus.Code = "WTO";
			Factory.Save();
			AssertEquals("Awaiting Response to Original", HAWB.UnderbondStatus);
			CusUnderbond underbond2 = HAWB.Underbonds.AddNew();
			HAWB.AllUnderbonds.Load();
			underbond2.C4_ParentID = HAWB.PK;
			HAWB.Underbonds.Add(underbond2);
			HAWB.AllUnderbonds.Load();
			Factory.Save();
			AssertEquals("More than one underbond", HAWB.UnderbondStatus);
		}

		public void TestICurrencyConverterDataProvider()
		{
			AssertNotNull("PreCondition:MAWB is not null", HAWB.MAWB);
			HAWB.MAWB.CM_DepartureDate = new ZDateTime(2005, 1, 1);

			AssertEquals("HAWB.DateOfValuation", new ZDateTime(2005, 1, 1), ((ICurrencyConverterDataProvider)HAWB).DateOfValuation);
			AssertEquals("HAWB.RateType", ZArchitecture.Core.ExchangeRateType.Customs, ((ICurrencyConverterDataProvider)HAWB).RateType);
			AssertEquals("HAWB.MaximumDaysToFallback", 7, ((ICurrencyConverterDataProvider)HAWB).MaximumDaysToFallback);
		}

		public void TestCargoStatus()
		{
			HAWB.CMRCargoStatus.Code = CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn;

			AssertEquals(CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn, HAWB.CMRCargoStatus.Code);
			AssertEquals(CMRConsolidatedCargoStatuses.Descriptions.WithdrawnCargoReportHadBeenWithdrawn, HAWB.CMRCargoStatus.Description);
		}

		public void TestIOutturnableLineCargoStatus()
		{
			HAWB.CMRCargoStatus.Code = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals(CMRConsolidatedCargoStatuses.Descriptions.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, ((IOutturnableLine)HAWB).CargoStatus);
		}

		public void TestIOutturnableLinePackagesManifested()
		{
			HAWB.CS_PiecesManifested = 6;
			AssertEquals(6, ((IOutturnableLine)HAWB).PackagesManifested);
		}

		public void TestMessageStatus()
		{
			HAWB.CMRMessageStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			AssertEquals(CMRBaseStatuses.Codes.OriginalAccepted, HAWB.CMRMessageStatus.Code);
			AssertEquals(CMRBaseStatuses.Descriptions.OriginalAccepted, HAWB.CMRMessageStatus.Description);
		}

		public void TestIsConsolidationIsTickedWhenSubStatusSet()
		{
			HAWB.MAWB.CM_MAWB = "61855037172";
			HAWB.CS_HAWB = "CuckooSqueaker";
			HAWB.CS_CustomsStatus = "";
			AssertEquals(false, HAWB.CS_IsMasterHouse);
			HAWB.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed;
			AssertEquals(true, HAWB.CS_IsMasterHouse);
		}

		public void TestCanSendWithoutDelay()
		{
			Assert("Should be delayed because No CARSTS were found", !((ICusUnderbondDependentCollectionParent)HAWB).CanSendWithoutDelay);
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			HAWB.Messages.Add(message);
			Assert("Should not be delayed due to carst.", ((ICusUnderbondDependentCollectionParent)HAWB).CanSendWithoutDelay);
		}

		public void TestIsConsolidationIsTickedWhenSubStatusSetEndToEnd()
		{
			HAWB.MAWB.CM_MAWB = "12571269214";
			HAWB.CS_HAWB = "74617885";
			HAWB.CS_MessageReference = "A00019471";
			HAWB.CS_CustomsStatus = "";
			AssertEquals(false, HAWB.CS_IsMasterHouse);
			Factory.Save();

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+430B 588B 3D1F:1+8'
DTM+9:20051125083935802473:ZZZ'
DTM+132:20051125:102'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+015++6+BA::3'
LOC+12+AUSYD::6'
NAD+MR+FFM969M::95'
NAD+UD+32003890328::95'
RFF+ABO:A00019471/SYD1::1'
RFF+MWB:12571269214'
RFF+HWB:74617885'
UNT+14+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();
			AssertEquals(true, HAWB.Messages.Contains(message));
			AssertEquals(CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed, HAWB.CMRCargoStatus.Code);
			AssertEquals("Status being set to SUB should have ticked IsConsolidation", true, HAWB.CS_IsMasterHouse);
		}

		public void TestHAWBCannotBeDeletedWhenAccepted()
		{
			HAWB.MAWB.CM_MAWB = "61855037172";
			HAWB.CS_HAWB = "CuckooSqueaker";
			HAWB.CS_MessageReference = "S00001663";
			HAWB.CS_MsgStatus = "";
			var outMessage = Factory.New<CMRAIRCRMessage>();
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outMessage.EM_Status = EDIMessage.Status.Sent;
			outMessage.EM_MessageNum = "1";
			outMessage.EM_MessageText = "UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001663/1:1+9'RFF+PQ:CC'RFF+HWB:CuckooSqueaker'RFF+MWB:61855037172'NAD+CN+++ZF AUSTRALIA PTY LTD+LOCKED BAG 6305+BLACKTOWN++2147+AU'NAD+CZ+++LEMFORDER+INTERNATIONAL AG & CO. KG.:BORGWARDSTRASSE 16+BREMEN++28279+DE'NAD+VW+66015286036::95'TDT+20+221++6+SQ::3'LOC+8+AUSYD::6'LOC+76+DEFRA::6'LOC+12+AUSYD::6'LOC+91+DEFRA::6'DTM+178:20041216:102'CNI+1'RFF+UCN:S00001663'MOA+96:NDV'GID+1'PAC+1'FTX+AAA+++AUTOMOTIVE SPARE PART'MEA+AAE+G+KG:144.00'UNT+22+<<MSGNO PLACEHOLDER>>'";
			outMessage.EM_LinkUniqueID = HAWB.PK;
			outMessage.EM_LinkTable = "CusHAWB";
			HAWB.Messages.Add(outMessage);

			var inMessage = Factory.New<CMRAIRCRRMessage>();
			inMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			inMessage.EM_MessageType = CMRMessage.CMRMessageTypes.AIRCR;
			inMessage.EM_MessageSubType = "CLR";
			inMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::AIRCRR+4H63 AJ0D HDGE:001+11'
NAD+MR+AAA374M:110:95'
RFF+ACW:AIRCR'
RFF+AFM:9'
RFF+ABO:S00001663/1::001'
DTM+310:20041216012712:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			inMessage.EM_LinkUniqueID = HAWB.PK;
			inMessage.EM_LinkTable = "CusHAWB";
			HAWB.Messages.Add(inMessage);
			Factory.Save();

			Assert("Should be non deletable", !HAWB.CanDelete);
			AssertEquals("Wrong 'NotAbleToDelete' Message", CusHAWBBase.HAWBCannotBeDeleted, HAWB.ReasonForNotAbleToDelete);

			inMessage.DeleteFromTest();
			outMessage.DeleteFromTest();
			HAWB.Messages.RemoveAndDeleteAllFromTest();

			HAWB.CS_MsgStatus = "";
			Assert("Should be deletable", HAWB.CanDelete);
			HAWB.Delete();
			AssertEquals(true, HAWB.IsDeleted);
		}

		public void TestIsForAir()
		{
			HAWB.CS_HAWB = "324235";
			ICusUnderbondUnionCollectionParent decider = HAWB;
			AssertNotNull(decider);
			AssertEquals(true, decider.IsForAirCargo);
		}

		public void TestCalculator()
		{
			AssertNotNull("Calculator", HAWB.Calculator);
			AssertEquals("type", typeof(CusHAWBStatusCalculator), HAWB.Calculator.GetType());
		}

		public void TestShipmentCalculator()
		{
			AssertNotNull("Calculator", HAWB.MessageStatusCalculator);
			AssertEquals("type", typeof(CusHAWBMessageStatusCalculator), HAWB.MessageStatusCalculator.GetType());
		}

		public void TestConsigneeAddressAsASingleLine()
		{
			HAWB.CS_ConsigneeStreet = "26 Myrtle";
			HAWB.CS_ConsigneeStreet2 = "Street";
			HAWB.CS_ConsigneeCity = "Prospect";
			HAWB.CS_ConsigneeState = "NSW";
			HAWB.CS_ConsigneePostcode = "2149";

			AssertEquals("26 Myrtle Street Prospect NSW 2149", HAWB.ConsigneeAddressAsASingleLine);

			HAWB.CS_ConsigneeStreet2 = "";

			AssertEquals("26 Myrtle Prospect NSW 2149", HAWB.ConsigneeAddressAsASingleLine);
		}

		public void TestWarehouseLocationCaption()
		{
			AssertEquals("Warehouse Location:", HAWB.WarehouseLocationCaption);
		}

		public void TestChargableWeightCaption()
		{
			AssertEquals("Chargeable Weight:", HAWB.ChargableWeightCaption);
		}

		public void TestMakeReadonlyOnLoadedIfResponseIsPending()
		{
			AssertEquals("Not readonly yet", false, HAWB.ReadOnly);
			HAWB.CS_IsResponsePending = true;
			HAWB.OnLoaded();

			AssertEquals("ReadOnly", true, HAWB.ReadOnly);
		}

		public void TestDelete()
		{
			HAWB.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Assert("Should be non deletable", !HAWB.CanDelete);
			AssertEquals("Wrong 'NotAbleToDelete' Message", CusHAWBBase.HAWBCannotBeDeleted, HAWB.ReasonForNotAbleToDelete);
			HAWB.CS_MsgStatus = CMRBaseStatuses.Codes.NotSent;
			Assert("Should be deletable", HAWB.CanDelete);
			HAWB.Delete();
			Assert("Should be deleted", HAWB.IsDeleted);
		}

		public void TestIsDocuments()
		{
			HAWB.IsDocuments = true;
			AssertEquals("CS_ShipmentType", "DOC", HAWB.CS_ShipmentType);
			HAWB.IsDocuments = false;
			AssertEquals("CS_ShipmentType", "STD", HAWB.CS_ShipmentType);
			HAWB.CS_ShipmentType = "DOC";
			AssertEquals("IsDocuments", true, HAWB.IsDocuments);
		}

		public void TestCS_CMReadonly()
		{
			AssertEquals("CS_CM is readonly", true, HAWB.CS_CMInfo.ReadOnly);
		}

		public void TestMAWB()
		{
			Assert("MAWB should exist if there is a reference to MAWB", HAWB.CS_CM.IsEmpty || HAWB.MAWB != null);
		}

		public void TestSetMessageReferenceOnSaving()
		{
			AssertEquals("Precondition:Not in database", false, HAWB.IsInDatabase);
			AssertEquals("Precondition:Message Reference number", true, HAWB.CS_MessageReference.IsEmpty);

			Factory.Save();
			AssertEquals("Message reference number is set", false, HAWB.CS_MessageReference.IsEmpty);
		}

		public void TestIsPrealertHeldByUsersReadOnly()
		{
			AssertEquals("HAWB is not prealerted yet", false, HAWB.CS_IsPrealerted);
			AssertEquals("HAWB's IsPrealertHeldByUsers not readonly yet", false, HAWB.CS_IsPrealertHeldByUserInfo.ReadOnly);

			HAWB.CS_IsPrealerted = true;
			AssertEquals("HAWB's IsPrealertHeldByUsers not readonly yet", true, HAWB.CS_IsPrealertHeldByUserInfo.ReadOnly);
		}

		public void TestAddAHoldPrealertLogWhenSetBeforeInDatabase()
		{
			AssertEquals("Not in database", false, HAWB.IsInDatabase);
			HAWB.CS_IsPrealertHeldByUser = true;
			Factory.Save();
			AssertEquals("Log should have been added", 1, HAWB.PrealertHeldByUsers.Count);
		}

		public void TestAddAHoldPrealertLogOnSaving()
		{
			AssertEquals("No Prealert log at start", 0, HAWB.PrealertHeldByUsers.Count);
			Factory.Save();
			AssertEquals("No Prealert log is added", 0, HAWB.PrealertHeldByUsers.Count);

			HAWB.CS_IsPrealertHeldByUser = true;
			Factory.Save();
			AssertEquals("a Prealert log is added", 1, HAWB.PrealertHeldByUsers.Count);
			AssertEquals("Log reference", Constants.HoldPrealertReference.IsPrealertHeld, HAWB.PrealertHeldByUsers[0].SL_Reference);

			HAWB.CS_IsPrealertHeldByUser = false;
			HAWB.CS_IsPrealertHeldByUser = true;
			Factory.Save();
			AssertEquals("Nothing should have been added as there is no change between Save", 1, HAWB.PrealertHeldByUsers.Count);

			HAWB.CS_IsPrealertHeldByUser = false;
			Factory.Save();
			AssertEquals("a Prealert log is added", 2, HAWB.PrealertHeldByUsers.Count);
			bool isThereReleasedLog = false;
			foreach (StmALog log in HAWB.PrealertHeldByUsers)
			{
				if (log.SL_Reference == Constants.HoldPrealertReference.IsPrealertHeldReleased)
				{
					isThereReleasedLog = true;
				}
			}
			AssertEquals("Log reference", true, isThereReleasedLog);
		}

		public void TestPrealertHeldByUsersLogs()
		{
			AssertEquals("HoldPrealert", Events.HoldAwaiting.Code, HAWB.PrealertHeldByUsers.NominatedEvent.Code);
			AssertEquals("HoldPrealert", Events.HoldAwaiting.Description, HAWB.PrealertHeldByUsers.NominatedEvent.Description);
		}

		public void TestUnderbonds()
		{
			ICusUnderbondDependentCollectionParent hAWBUnder = HAWB;
			AssertNotNull(hAWBUnder.Underbonds);
			Assert(HAWB.IsRegisteredEditableChildObject(hAWBUnder.Underbonds));
			hAWBUnder.Underbonds.AddNew();
			AssertEquals(1, hAWBUnder.Underbonds.Count);
		}

		public void TestDetailsNotNull()
		{
			AssertNotNull(((ICusUnderbondDependentCollectionParent)HAWB).Details);
		}

		public void TestMessages()
		{
			AssertNotNull("Messages", HAWB.Messages);
		}

		public void TestStatusNeedsRecalculation()
		{
			AssertEquals("StatusNeedsRecalculation", false, ((Customs.Business.IStatusNeedsRecalculationProvider)HAWB).StatusNeedsRecalculation);
			HAWB.Messages.AddNew().HasChanges = true;
			AssertEquals("StatusNeedsRecalculation", true, ((Customs.Business.IStatusNeedsRecalculationProvider)HAWB).StatusNeedsRecalculation);
		}

		public void TestStatusNeedsRecalculation_MessagesShouldNotBeLoadedJustForThisPurpose()
		{
			FieldInfo fMessagesField = typeof(Customs.Business.CusHAWB).GetField("fMessages", BindingFlags.NonPublic | BindingFlags.Instance);
			fMessagesField.SetValue(HAWB, null);
			AssertEquals("Should not require calculation as the messages have not been loaded into the collection", false, ((Customs.Business.IStatusNeedsRecalculationProvider)HAWB).StatusNeedsRecalculation);
			AssertNull("Should not be loaded", fMessagesField.GetValue(HAWB));
		}

		public void TestOutturnableLines()
		{
			AssertEquals("Length", 1, ((ICusUnderbondDependentCollectionParent)HAWB).OutturnableLines.Length);
			AssertEquals("OutturnableLines[0]", HAWB, ((ICusUnderbondDependentCollectionParent)HAWB).OutturnableLines[0]);
		}

		public void TestTypeDecider()
		{
			AssertEquals("TypeDeciderType", typeof(CusHAWBBaseTypeDecider), CusHAWBBase.TypeDecider.GetType());
		}

		public void TestMessageReferenceReadonly()
		{
			AssertEquals("MessageReferenceReadonly", true, HAWB.CS_MessageReferenceInfo.ReadOnly);
		}

		public void TestPartShipmentsAreNotReadonlyForCMR()
		{
			AssertEquals("PartShips.ReadOnly", false, HAWB.PartShips.ReadOnly);
		}

		public void TestPartShipmentsAreRegisteredEditableForCMR()
		{
			AssertEquals("IsRegisteredEditableChildObject", true, HAWB.IsRegisteredEditableChildObject(HAWB.PartShips));
		}

		public void TestLongPartyName()
		{
			var party = Factory.New<OrgHeader>();
			party.OH_FullName = "12345678901234567890123456789012345678901234567890";

			HAWB.CS_OA_ConsigneeAddress = party.MainAddress.PK;
			AssertEquals("Full name is trimmed", party.OH_FullName.SubstringSafe(0, CusHAWBSchema.CS_ConsigneeName.MaxLength), HAWB.CS_ConsigneeName);

			HAWB.CS_OA_ConsignorAddress = party.MainAddress.PK;
			AssertEquals("Full name is trimmed", party.OH_FullName.SubstringSafe(0, CusHAWBSchema.CS_ConsignorName.MaxLength), HAWB.CS_ConsignorName);
		}

		public void TestOrgHeaderChangeUpdateWithLoadedHAWB()
		{
			var party = Factory.LoadTop1<OrgHeader>(new ZQuery());

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusHAWBBase houseBill = GetHAWBToTest(factory2);
			houseBill.CS_OA_ConsigneeAddress = party.MainAddress.PK;
			factory2.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			CusHAWBBase loadedHAWB = (CusHAWBBase)factory3.Load(houseBill.GetType(), houseBill.PK);

			party.OH_FullName = "SomethingDifferent";
			Factory.Save();
			AssertEquals("Party name change should be reflected in HouseBill", "SomethingDifferent", loadedHAWB.CS_ConsigneeName);
		}

		public void TestChangeInOrganisationReflectedInHouseBill()
		{
			var party = Factory.LoadTop1<OrgHeader>(new ZQuery());

			HAWB.CS_OA_ConsigneeAddress = party.MainAddress.PK;

			party.OH_FullName = "SomethingDifferent";
			Factory.Save();

			AssertEquals("Party name change should be reflected", "SomethingDifferent", HAWB.CS_ConsigneeName);
		}

		public void TestOrgHeaderAddressChangeUpdate()
		{
			OrgHeader party = OrgHeader.LoadFromCode(Factory, "ABABEU");
			AssertNotNull("PreCondition: this party exists", party);

			var partyAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, party.PK));
			AssertNotNull("PreCondition: this party address exists", partyAddress);

			partyAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusHAWBBase houseBill = GetHAWBToTest(factory2);
			houseBill.CS_OA_ConsignorAddress = partyAddress.PK;

			partyAddress.OA_Address1 = "Something Different";
			Factory.Save();

			AssertEquals("Address1", partyAddress.OA_Address1, ((Customs.Business.OrgAddressDecider)houseBill.ConsignorAddressForPortOfOrigin).Address1);

			AssertEquals("Consignor Address should have updated", partyAddress.OA_Address1, houseBill.CS_ConsignorStreet);
			AssertEquals("Consignor Address2 should have updated", partyAddress.OA_Address2, houseBill.CS_ConsignorStreet2);
		}

		public void TestConsignorStreetDefaultedOnLoaded()
		{
			HAWB.CS_ConsignorStreet = "1 STREET ST";
			HAWB.CS_ConsignorStreet2 = "STREET 2";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusHAWBBase loadedHouse = (CusHAWBBase)newFactory.Load(HAWB.GetType(), HAWB.PK);
			AssertEquals("Consignor Street", HAWB.CS_ConsignorStreet, loadedHouse.CS_ConsignorStreet);
			AssertEquals("Consignor Street2", HAWB.CS_ConsignorStreet2, loadedHouse.CS_ConsignorStreet2);

			HAWB.CS_OA_ConsignorAddress = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;
			Factory.Save();

			newFactory = new BusinessObjectFactory();
			loadedHouse = (CusHAWBBase)newFactory.Load(HAWB.GetType(), HAWB.PK);
			AssertEquals("Consignor Street", HAWB.ConsignorAddressForPortOfOrigin.Address1, loadedHouse.CS_ConsignorStreet);
			AssertEquals("Consignor Street2", HAWB.ConsignorAddressForPortOfOrigin.Address2, loadedHouse.CS_ConsignorStreet2);
		}

		public void TestVendorIdentifierAndConsignorIdentifier()
		{
			AssertEquals("", HAWB.CS_ConsignorIdentifier);
			AssertEquals("", HAWB.CS_VendorIdentifier);

			var consignor1 = Factory.New<OrgHeader>();
			consignor1.OH_Code = "Test 1";
			HAWB.CS_OA_ConsignorAddress = consignor1.MainAddress.PK;
			AssertEquals("", HAWB.CS_ConsignorIdentifier);
			AssertEquals("", HAWB.CS_VendorIdentifier);

			var consignor2 = Factory.New<OrgHeader>();
			consignor2.OH_Code = "Test 2";
			consignor2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");
			consignor2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123");
			HAWB.CS_OA_ConsignorAddress = consignor2.MainAddress.PK;
			AssertEquals("12345678901", HAWB.CS_ConsignorIdentifier);
			AssertEquals("123", HAWB.CS_VendorIdentifier);
		}

		public void TestIsMAWBEditableChild()
		{
			var consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			HAWB.MAWB.CM_JK = consol.PK;
			HAWB.CS_JS = shipment.PK;

			AssertEquals("Business does not initiate Registration", false, HAWB.IsRegisteredEditableChildObject(HAWB.MAWB));
			AssertEquals("Business does not initiate Registration", false, HAWB.MAWB.IsRegisteredEditableChildObject(HAWB));

			AssertEquals("Registered", true, HAWB.MAWB.IsRegisteredEditableChildObject(HAWB.MAWB.ChildBills));
		}

		public void TestCanDeleteFromICusHAWBCollection()
		{
			AssertEquals(true, HAWB.CanDeleteFromICusHAWBCollection);
		}

		public void TestTranshipmentNumberReadOnly()
		{
			AssertEquals(true, HAWB.CS_TranshipmentEntryNumInfo.ReadOnly);
		}

		public void TestConsignorContactList()
		{
			var testOrg = Factory.New<OrgHeader>();
			OrgContact contact = testOrg.Contacts.AddNew();
			contact.OC_ContactName = "Jim Beam";

			var testOrg2 = Factory.New<OrgHeader>();
			OrgContact contact2 = testOrg2.Contacts.AddNew();
			contact2.OC_ContactName = "Jack Daniels";

			HAWB.CS_OA_ConsignorAddress = testOrg.MainAddress.PK;
			OrgContactDependentCollection collection = HAWB.Lookups.ConsignorContactList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("Jim Beam", collection[0].OC_ContactName);

			HAWB.CS_OA_ConsignorAddress = testOrg2.MainAddress.PK;
			collection = HAWB.Lookups.ConsignorContactList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("Jack Daniels", collection[0].OC_ContactName);
		}

		public void TestConsigneeContactList()
		{
			var testOrg = Factory.New<OrgHeader>();
			OrgContact contact = testOrg.Contacts.AddNew();
			contact.OC_ContactName = "Luke Duke";

			var testOrg2 = Factory.New<OrgHeader>();
			OrgContact contact2 = testOrg2.Contacts.AddNew();
			contact2.OC_ContactName = "Bo Duke";

			HAWB.CS_OA_ConsigneeAddress = testOrg.MainAddress.PK;
			OrgContactDependentCollection collection = HAWB.Lookups.ConsigneeContactList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("Luke Duke", collection[0].OC_ContactName);

			HAWB.CS_OA_ConsigneeAddress = testOrg2.MainAddress.PK;
			collection = HAWB.Lookups.ConsigneeContactList;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals("Bo Duke", collection[0].OC_ContactName);
		}

		public void TestAggregatedCoLoadMaster()
		{
			HAWB.CS_MasterHouseBill = "";
			HAWB.MAWB.CM_MasterHouseBill = "ABC";
			AssertEquals("AggregatedCoLoadMaster", "ABC", HAWB.AggregatedCoLoadMaster);

			HAWB.CS_MasterHouseBill = "DEF";
			AssertEquals("AggregatedCoLoadMaster", "DEF", HAWB.AggregatedCoLoadMaster);
		}

		public virtual void TestCS_PaymentTypeCaption()
		{
			AssertEquals("Method of Payment:", HAWB.CS_PaymentTypeCaption);
		}

		public void TestCS_PaymentTypeCaptionInfo()
		{
			AssertEquals(20, HAWB.CS_PaymentTypeCaptionInfo.MaxLength);
		}

		public void TestCS_FreightPrepaidCollectForBinding()
		{
			AssertEquals("Default should be " + HAWB.CS_FreightPrepaidCollectInfo.Name, HAWB.CS_FreightPrepaidCollectInfo, ((ZWrappedPropertyInfo)HAWB.CS_FreightPrepaidCollectForBindingInfo).InnerInfo);

			HAWB.CS_FreightPrepaidCollectForBinding = "A";
			AssertEquals("A", HAWB.CS_FreightPrepaidCollect);

			HAWB.CS_FreightPrepaidCollect = "B";
			AssertEquals("B", HAWB.CS_FreightPrepaidCollectForBinding);
		}

		public void TestCS_ShipmentTypeForBinding()
		{
			AssertEquals("Default should be " + HAWB.CS_ShipmentTypeInfo.Name, HAWB.CS_ShipmentTypeInfo, ((ZWrappedPropertyInfo)HAWB.CS_ShipmentTypeForBindingInfo).InnerInfo);

			HAWB.CS_ShipmentTypeForBinding = "X";
			AssertEquals("X", HAWB.CS_ShipmentType);

			HAWB.CS_ShipmentType = "Y";
			AssertEquals("Y", HAWB.CS_ShipmentTypeForBinding);
		}

		public void TestIsAutoLogged()
		{
			AssertEquals("CusHAWB should be auto-logged", true, HAWB.IsAutoLogged);
		}

		public void TestGoodsValueInLocalCurrency()
		{
			HAWB.MAWB.CM_DepartureDate = ZDateTime.Today;
			RefCurrency aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			HAWB.CS_RX_NKGoodsCurrency = aUDCurrency.RX_Code;
			HAWB.CS_GoodsValue = 100m;
			HAWB.CS_RX_NKGoodsCurrency = JobDeclaration.LocalCurrencyConstantCode;

			AssertEquals("GoodsValueInLocalCurrency", 100m, HAWB.GoodsValueInLocalCurrency);

			RefCountry country = Factory.New<RefCountry>();
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "BAS";
			country.RN_Code = "BS";
			country.RN_RX_NKLocalCurrency = currency.RX_Code;
			RefExchangeRate exchangeRate = country.LocalCurrency.ExchangeRates.AddNew();
			exchangeRate.RE_StartDate = ZDateTime.BrettsBirthday;
			exchangeRate.RE_ExpiryDate = ZDateTime.MaxSmallDateTime;
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_SellRate = 1.5m;
			HAWB.CS_RX_NKGoodsCurrency = currency.RX_Code;
			AssertEquals("GoodsValueInLocalCurrency", 66.67m, HAWB.GoodsValueInLocalCurrency);
		}

		public void TestCanBeDeleted()
		{
			AssertEquals(true, HAWB.CanDelete);

			HAWB.CS_MsgStatus = ZString.Empty;
			AssertEquals(true, HAWB.CanDelete);

			HAWB.CS_MsgStatus = CMRBaseStatuses.Codes.NotSent;
			AssertEquals(true, HAWB.CanDelete);

			HAWB.CS_MsgStatus = CMRBaseStatuses.Codes.AmendmentRejected;
			AssertEquals(false, HAWB.CanDelete);
		}

		[ExpectNoExceptions]
		public void TestAllowDeleteTwice()
		{
			HAWB.Delete();
			HAWB.Delete();
		}

		public void TestShortDescription()
		{
			AssertEquals(ZString.Empty, HAWB.ShortDescription);
			HAWB.CS_HAWB = "CuckooSqueaker";
			AssertEquals("HAWB: CuckooSqueaker", HAWB.ShortDescription);
		}

		public void TestIDocumentSupportable()
		{
			IDocumentSupportable documentSupportable = (IDocumentSupportable)GetNewBusinessObject();
			AssertEquals("DocumentSupporter of correct type", typeof(CusHAWBDocumentSupporter), documentSupportable.DocumentSupporter.GetType());
		}

		public void TestLoadFromQuery()
		{
			var cmrCusMAWB = Factory.New<CusMAWB>();
			cmrCusMAWB.CM_ApplicationCode = "CMR";
			cmrCusMAWB.CM_MAWB = "MB234";
			var cmrCusHAWB = Factory.New<CusHAWB>();
			cmrCusHAWB.CS_CM = cmrCusMAWB.PK;
			cmrCusHAWB.CS_HAWB = "HB343";
			var nzeCusMAWB = Factory.New<CusMAWB>();
			nzeCusMAWB.CM_ApplicationCode = "NZE";
			nzeCusMAWB.CM_MAWB = "MB234";
			var nzeCusHAWB = Factory.New<CusHAWB>();
			nzeCusHAWB.CS_CM = nzeCusMAWB.PK;
			nzeCusHAWB.CS_HAWB = "HB343";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertEquals("Precondition: nzeCusHAWB should not inherit from CusHAWBBase", false, typeof(CusHAWBBase).IsAssignableFrom(newFactory.Load<Customs.Business.CusHAWB>(nzeCusHAWB.PK).GetType()));
			var query = new ZQuery(CusHAWBSchema.CS_HAWB, "HB343");
			for (int i = 0; i < 5; i++)
			{
				CusHAWBBase hawb = null;
				AssertNoExceptionThrown(delegate
				{
					hawb = CusHAWBBase.LoadFromQuery(query, new BusinessObjectFactory());
				});
				AssertNotNull(hawb);
				AssertEquals(cmrCusHAWB.PK, hawb.PK);
			}
		}

		public void TestLoadAllFromQuery()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			CusMAWB cusMAWB1 = factory2.New<CusMAWB>();
			cusMAWB1.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusHAWBBaseTestHelper cusHAWB1 = factory2.New<CusHAWBBaseTestHelper>();
			cusHAWB1.CS_CM = cusMAWB1.PK;
			cusHAWB1.CS_HAWB = "HAWB1";

			CusMAWB cusMAWB2 = factory2.New<CusMAWB>();
			cusMAWB2.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			cusMAWB2.CM_ArrivalDate = ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value).AddDays(-1);
			CusHAWBBaseTestHelper cusHAWB2 = factory2.New<CusHAWBBaseTestHelper>();
			cusHAWB2.CS_CM = cusMAWB2.PK;
			cusHAWB2.CS_HAWB = "HAWB2";

			CusHAWBBaseTestHelper cusHAWB3 = factory2.New<CusHAWBBaseTestHelper>();
			cusHAWB3.CS_HAWB = "HAWB3";

			CusMAWB cusMAWB4 = factory2.New<CusMAWB>();
			cusMAWB4.CM_ApplicationCode = "";
			cusMAWB4.CM_ArrivalDate = ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value).AddDays(1);
			CusHAWBBaseTestHelper cusHAWB4 = factory2.New<CusHAWBBaseTestHelper>();
			cusHAWB4.CS_CM = cusMAWB4.PK;
			cusHAWB4.CS_HAWB = "HAWB4";

			factory2.Save();

			var cusHAWB = (CusHAWB)CusHAWBBase.LoadFromQuery(new ZQuery(CusHAWBSchema.CS_HAWB, "HAWB000"), Factory);
			AssertNull(cusHAWB);
			cusHAWB = (CusHAWB)CusHAWBBase.LoadFromQuery(new ZQuery(CusHAWBSchema.CS_HAWB, "HAWB1"), Factory);
			AssertEquals("HAWB1", cusHAWB.CS_HAWB);
			cusHAWB = (CusHAWB)CusHAWBBase.LoadFromQuery(new ZQuery(CusHAWBSchema.CS_HAWB, "HAWB3"), Factory);
			AssertEquals("HAWB3", cusHAWB.CS_HAWB);

			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWBBase));
			CusHAWBBase[] cusHAWBs = CusHAWBBase.LoadAllFromQuery(dbOnlyQuery, Factory);
			AssertEquals(2, cusHAWBs.Length);
			bool hbl1Found = false;
			bool hbl2Found = false;
			bool hbl3Found = false;
			bool hbl4Found = false;
			foreach (CusHAWB cusHAWBinArray in cusHAWBs)
			{
				if (cusHAWBinArray.CS_HAWB == "HAWB1")
				{
					hbl1Found = true;
				}
				else if (cusHAWBinArray.CS_HAWB == "HAWB2")
				{
					hbl2Found = true;
				}
				else if (cusHAWBinArray.CS_HAWB == "HAWB3")
				{
					hbl3Found = true;
				}
				else if (cusHAWBinArray.CS_HAWB == "HAWB4")
				{
					hbl4Found = true;
				}
			}
			Assert("Should contain HAWB1", hbl1Found);
			Assert("Should contain HAWB2", hbl2Found);
			Assert("Should not contain HAWB3", !hbl3Found);
			Assert("Should not contain HAWB4", !hbl4Found);

			dbOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWBBase));
			cusHAWBs = CusHAWBBase.LoadAllFromQuery(dbOnlyQuery, Factory, loadRecentOnly: true);
			AssertEquals(1, cusHAWBs.Length);
			hbl1Found = false;
			hbl2Found = false;
			hbl3Found = false;
			hbl4Found = false;
			foreach (CusHAWB cusHAWBinArray in cusHAWBs)
			{
				if (cusHAWBinArray.CS_HAWB == "HAWB1")
				{
					hbl1Found = true;
				}
				else if (cusHAWBinArray.CS_HAWB == "HAWB2")
				{
					hbl2Found = true;
				}
				else if (cusHAWBinArray.CS_HAWB == "HAWB3")
				{
					hbl3Found = true;
				}
				else if (cusHAWBinArray.CS_HAWB == "HAWB4")
				{
					hbl4Found = true;
				}
			}
			Assert("Should contain HAWB1", hbl1Found);
			Assert("Should not contain HAWB2", !hbl2Found);
			Assert("Should not contain HAWB3", !hbl3Found);
			Assert("Should not contain HAWB4", !hbl4Found);
		}

		public void TestDefaultingVendorIdentifierForConsignor()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;
			org.OH_IsConsignor = true;

			hawb.CS_OA_ConsignorAddress = org.MainAddress.PK;

			Factory.Save();

			AssertEquals("If ABN and ARN don't exist, Vendor Identifier should be empty", ZString.Empty, hawb.CS_VendorIdentifier);

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;
			org.OH_IsConsignor = true;

			var abn = org.CustomsCodes.AddNew();
			abn.OK_CodeType = "ABN";
			abn.OK_CustomsRegNo = "9876543210";

			hawb.CS_OA_ConsignorAddress = org.MainAddress.PK;

			Factory.Save();

			AssertEquals("If ABN exists but not ARN, ABN should be the Vendor Identifier", abn.OK_CustomsRegNo, hawb.CS_VendorIdentifier);

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;
			org.OH_IsConsignor = true;

			var arn = org.CustomsCodes.AddNew();
			arn.OK_CodeType = "ARN";
			arn.OK_CustomsRegNo = "123456789012";
			hawb.CS_VendorIdentifier = ZString.Empty;

			hawb.CS_OA_ConsignorAddress = org.MainAddress.PK;

			Factory.Save();

			AssertEquals("If ABN and ARN exist, ARN should be the Vendor Identifier", arn.OK_CustomsRegNo, hawb.CS_VendorIdentifier);
		}

		public void TestSetConsigneeDetails()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_FullName = "TEST CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "1addr";
			consignee.MainAddress.OA_Address2 = "2addr";
			consignee.MainAddress.OA_City = "barcity";
			consignee.MainAddress.OA_Phone = "333";
			consignee.MainAddress.OA_Fax = "555";
			consignee.MainAddress.OA_PostCode = "6768";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			consignee.MainAddress.OA_State = "HEH";
			consignee.OH_IsConsignee = true;

			var deliveryAddress = consignee.Addresses.AddNew(OrgAddressType.Delivery, false);
			deliveryAddress.OA_Address1 = "Delivery 1addr";
			deliveryAddress.OA_Address2 = "Delivery 2addr";
			deliveryAddress.OA_City = "Delivery barcity";
			deliveryAddress.OA_Phone = "444";
			deliveryAddress.OA_Fax = "666";
			deliveryAddress.OA_PostCode = "8687";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			deliveryAddress.OA_State = "ACT";

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_OA_ConsigneeAddress = deliveryAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(deliveryAddress.PK, hawb.CS_OA_ConsigneeAddress);
				AssertEquals("TEST CONSIGNEE", hawb.CS_ConsigneeName);
				AssertEquals("Delivery 1addr", hawb.CS_ConsigneeStreet);
				AssertEquals("Delivery 2addr", hawb.CS_ConsigneeStreet2);
				AssertEquals("Delivery barcity", hawb.CS_ConsigneeCity);
				AssertEquals("ACT", hawb.CS_ConsigneeState);
				AssertEquals("8687", hawb.CS_ConsigneePostcode);
				AssertEquals("444", hawb.CS_ConsigneePhone);
				AssertEquals("AU", hawb.CS_RN_NKConsigneeCountry);
			});

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = consignee.MainAddress.PK;
			hawb.SetConsigneeDetails(Customs.Business.JobDocAddressWrapper.New(docAddress));
			CombineAssertions(() =>
			{
				AssertEquals(consignee.MainAddress.PK, hawb.CS_OA_ConsigneeAddress);
				AssertEquals("TEST CONSIGNEE", hawb.CS_ConsigneeName);
				AssertEquals("1addr", hawb.CS_ConsigneeStreet);
				AssertEquals("2addr", hawb.CS_ConsigneeStreet2);
				AssertEquals("barcity", hawb.CS_ConsigneeCity);
				AssertEquals("HEH", hawb.CS_ConsigneeState);
				AssertEquals("6768", hawb.CS_ConsigneePostcode);
				AssertEquals("333", hawb.CS_ConsigneePhone);
				AssertEquals("US", hawb.CS_RN_NKConsigneeCountry);
			});
		}

		public void TestSetConsignorDetails()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_FullName = "TEST CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "1addr";
			consignor.MainAddress.OA_Address2 = "2addr";
			consignor.MainAddress.OA_City = "barcity";
			consignor.MainAddress.OA_Phone = "333";
			consignor.MainAddress.OA_Fax = "555";
			consignor.MainAddress.OA_PostCode = "6768";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			consignor.MainAddress.OA_State = "HEH";
			consignor.OH_IsConsignor = true;

			var deliveryAddress = consignor.Addresses.AddNew(OrgAddressType.Pickup, false);
			deliveryAddress.OA_Address1 = "Pickup 1addr";
			deliveryAddress.OA_Address2 = "Pickup 2addr";
			deliveryAddress.OA_City = "Pickup barcity";
			deliveryAddress.OA_Phone = "444";
			deliveryAddress.OA_Fax = "666";
			deliveryAddress.OA_PostCode = "8687";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			deliveryAddress.OA_State = "ACT";

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_OA_ConsignorAddress = deliveryAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(deliveryAddress.PK, hawb.CS_OA_ConsignorAddress);
				AssertEquals("TEST CONSIGNOR", hawb.CS_ConsignorName);
				AssertEquals("Pickup 1addr", hawb.CS_ConsignorStreet);
				AssertEquals("Pickup 2addr", hawb.CS_ConsignorStreet2);
				AssertEquals("Pickup barcity", hawb.CS_ConsignorCity);
				AssertEquals("ACT", hawb.CS_ConsignorState);
				AssertEquals("8687", hawb.CS_ConsignorPostcode);
				AssertEquals("444", hawb.CS_ConsignorPhone);
				AssertEquals("AU", hawb.CS_RN_NKConsignorCountry);
			});

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = consignor.MainAddress.PK;
			hawb.SetConsignorDetails(Customs.Business.JobDocAddressWrapper.New(docAddress));
			CombineAssertions(() =>
			{
				AssertEquals(consignor.MainAddress.PK, hawb.CS_OA_ConsignorAddress);
				AssertEquals("TEST CONSIGNOR", hawb.CS_ConsignorName);
				AssertEquals("1addr", hawb.CS_ConsignorStreet);
				AssertEquals("2addr", hawb.CS_ConsignorStreet2);
				AssertEquals("barcity", hawb.CS_ConsignorCity);
				AssertEquals("HEH", hawb.CS_ConsignorState);
				AssertEquals("6768", hawb.CS_ConsignorPostcode);
				AssertEquals("333", hawb.CS_ConsignorPhone);
				AssertEquals("US", hawb.CS_RN_NKConsignorCountry);
			});
		}

		public void TestSyncOHWithOAFields()
		{
			var consignor = Factory.New<OrgHeader>();
			var consignorAddress = consignor.Addresses.AddNew(OrgAddressType.Pickup, false);
			var consignee = Factory.New<OrgHeader>();
			var consigneeAddress = consignee.Addresses.AddNew(OrgAddressType.Delivery, false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();

			AssertEquals("Pre-condition: CS_OH_Consignor", ZGuid.Empty, hawb.CS_OH_Consignor);
			AssertEquals("Pre-condition: CS_OH_Consignee", ZGuid.Empty, hawb.CS_OH_Consignee);
			hawb.CS_OA_ConsignorAddress = consignorAddress.PK;
			hawb.CS_OA_ConsigneeAddress = consigneeAddress.PK;
			AssertEquals("CS_OH_Consignor is populated based on CS_OA_ConsignorAddress", consignor.PK, hawb.CS_OH_Consignor);
			AssertEquals("CS_OH_Consignee is populated based on CS_OA_ConsigneeAddress", consignee.PK, hawb.CS_OH_Consignee);
		}

		public void TestReportWhenLoadAsAWrongType()
		{
			var gbMawb = Factory.New<CusMAWB>();
			gbMawb.CM_MAWB = "61898391193";
			gbMawb.CM_ApplicationCode = "CUK";
			var gbHawb1 = gbMawb.ChildBills.AddNew();
			gbHawb1.CS_HAWB = "8888388";
			gbHawb1.CS_ApplicationCode = "CUK";
			var gbHawb2 = Factory.New<CusHAWB>();
			gbHawb2.CS_HAWB = "8888399";
			gbHawb2.CS_ApplicationCode = "CUK";
			Factory.Save();

			var query = new ZQuery(CusHAWBSchema.CS_HAWB, "8888388");
			new BusinessObjectFactory().LoadTop1<CusHAWB>(query);
			AssertEquals("Attempting to load a non-AU CusHAWB as a AU CusHAWB, CS_ApplicationCode: CUK, CM_ApplicationCode: CUK", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			query = new ZQuery(CusHAWBSchema.CS_HAWB, "8888399");
			new BusinessObjectFactory().LoadTop1<CusHAWB>(query);
			AssertEquals("Attempting to load a non-AU CusHAWB as a AU CusHAWB, CS_ApplicationCode: CUK, CM_ApplicationCode: None", ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		public void TestOverrideAddressDataForConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "BIG BUSINESS INC.";
			consignee.MainAddress.OA_Address1 = "LEVEL 48 BIG BUILDING";
			consignee.MainAddress.OA_Address2 = "1 MARTIN PLACE";
			consignee.MainAddress.OA_City = "SYDNEY";
			consignee.MainAddress.OA_Phone = "95002200";
			consignee.MainAddress.OA_PostCode = "2000";
			consignee.MainAddress.OA_State = "NSW";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var consigneeWareHouseAddress = consignee.Addresses.AddNew();
			consigneeWareHouseAddress.OA_Address1 = "Building C";
			consigneeWareHouseAddress.OA_Address2 = "1450 Industrial Lane";
			consigneeWareHouseAddress.OA_City = "BOTANY";
			consigneeWareHouseAddress.OA_Phone = "90703000";
			consigneeWareHouseAddress.OA_PostCode = "2045";
			consigneeWareHouseAddress.OA_State = "NSW";
			consigneeWareHouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			HAWB.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(consignee, HAWB.Consignee);
				AssertEquals(consignee.MainAddress.CompanyName, HAWB.CS_ConsigneeName);
				AssertEquals(consignee.MainAddress.OA_Address1, HAWB.CS_ConsigneeStreet);
				AssertEquals(consignee.MainAddress.OA_Address2, HAWB.CS_ConsigneeStreet2);
				AssertEquals(consignee.MainAddress.OA_City, HAWB.CS_ConsigneeCity);
				AssertEquals(consignee.MainAddress.OA_Phone, HAWB.CS_ConsigneePhone);
				AssertEquals(consignee.MainAddress.OA_PostCode, HAWB.CS_ConsigneePostcode);
				AssertEquals(consignee.MainAddress.OA_State, HAWB.CS_ConsigneeState);
				AssertEquals("AU", HAWB.CS_RN_NKConsigneeCountry);
			});

			HAWB.CS_OA_ConsigneeAddress = consigneeWareHouseAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("When the consignee address is overriden with another of the organisations addresses, the Consignee should still be the same", consignee, HAWB.Consignee);
				AssertEquals(consigneeWareHouseAddress.CompanyName, HAWB.CS_ConsigneeName);
				AssertEquals(consigneeWareHouseAddress.OA_Address1, HAWB.CS_ConsigneeStreet);
				AssertEquals(consigneeWareHouseAddress.OA_Address2, HAWB.CS_ConsigneeStreet2);
				AssertEquals(consigneeWareHouseAddress.OA_City, HAWB.CS_ConsigneeCity);
				AssertEquals(consigneeWareHouseAddress.OA_Phone, HAWB.CS_ConsigneePhone);
				AssertEquals(consigneeWareHouseAddress.OA_PostCode, HAWB.CS_ConsigneePostcode);
				AssertEquals(consigneeWareHouseAddress.OA_State, HAWB.CS_ConsigneeState);
				AssertEquals("AU", HAWB.CS_RN_NKConsigneeCountry);
			});
		}

		public void TestReadOnlyForConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "BIG BUSINESS INC.";
			consignee.MainAddress.OA_Address1 = "LEVEL 48 BIG BUILDING";

			HAWB.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(true, HAWB.CS_ConsigneeNameInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsigneeStreetInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsigneeStreet2Info.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsigneeCityInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsigneePhoneInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsigneePostcodeInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsigneeStateInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_RN_NKConsigneeCountryInfo.ReadOnly);
			});

			HAWB.CS_OA_ConsigneeAddress = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals(false, HAWB.CS_ConsigneeNameInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsigneeStreetInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsigneeStreet2Info.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsigneeCityInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsigneePhoneInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsigneePostcodeInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsigneeStateInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_RN_NKConsigneeCountryInfo.ReadOnly);
			});
		}

		public void TestOverrideAddressDataForConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "BIG BUSINESS INC.";
			consignor.MainAddress.OA_Address1 = "LEVEL 48 BIG BUILDING";
			consignor.MainAddress.OA_Address2 = "1 MARTIN PLACE";
			consignor.MainAddress.OA_City = "SYDNEY";
			consignor.MainAddress.OA_Phone = "95002200";
			consignor.MainAddress.OA_PostCode = "2000";
			consignor.MainAddress.OA_State = "NSW";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var consignorWareHouseAddress = consignor.Addresses.AddNew();
			consignorWareHouseAddress.OA_Address1 = "Building C";
			consignorWareHouseAddress.OA_Address2 = "1450 Industrial Lane";
			consignorWareHouseAddress.OA_City = "BOTANY";
			consignorWareHouseAddress.OA_Phone = "90703000";
			consignorWareHouseAddress.OA_PostCode = "2045";
			consignorWareHouseAddress.OA_State = "NSW";
			consignorWareHouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			HAWB.CS_OA_ConsignorAddress = consignor.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(consignor, HAWB.Consignor);
				AssertEquals(consignor.MainAddress.CompanyName, HAWB.CS_ConsignorName);
				AssertEquals(consignor.MainAddress.OA_Address1, HAWB.CS_ConsignorStreet);
				AssertEquals(consignor.MainAddress.OA_Address2, HAWB.CS_ConsignorStreet2);
				AssertEquals(consignor.MainAddress.OA_City, HAWB.CS_ConsignorCity);
				AssertEquals(consignor.MainAddress.OA_Phone, HAWB.CS_ConsignorPhone);
				AssertEquals(consignor.MainAddress.OA_PostCode, HAWB.CS_ConsignorPostcode);
				AssertEquals(consignor.MainAddress.OA_State, HAWB.CS_ConsignorState);
				AssertEquals("AU", HAWB.CS_RN_NKConsignorCountry);
			});

			HAWB.CS_OA_ConsignorAddress = consignorWareHouseAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("When the Consignor address is overriden with another of the organisations addresses, the Consignor should still be the same", consignor, HAWB.Consignor);
				AssertEquals(consignorWareHouseAddress.CompanyName, HAWB.CS_ConsignorName);
				AssertEquals(consignorWareHouseAddress.OA_Address1, HAWB.CS_ConsignorStreet);
				AssertEquals(consignorWareHouseAddress.OA_Address2, HAWB.CS_ConsignorStreet2);
				AssertEquals(consignorWareHouseAddress.OA_City, HAWB.CS_ConsignorCity);
				AssertEquals(consignorWareHouseAddress.OA_Phone, HAWB.CS_ConsignorPhone);
				AssertEquals(consignorWareHouseAddress.OA_PostCode, HAWB.CS_ConsignorPostcode);
				AssertEquals(consignorWareHouseAddress.OA_State, HAWB.CS_ConsignorState);
				AssertEquals("AU", HAWB.CS_RN_NKConsignorCountry);
			});
		}

		public void TestReadOnlyForConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "BIG BUSINESS INC.";
			consignor.MainAddress.OA_Address1 = "LEVEL 48 BIG BUILDING";

			HAWB.CS_OA_ConsignorAddress = consignor.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(true, HAWB.CS_ConsignorNameInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsignorStreetInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsignorStreet2Info.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsignorCityInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsignorPhoneInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsignorPostcodeInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_ConsignorStateInfo.ReadOnly);
				AssertEquals(true, HAWB.CS_RN_NKConsignorCountryInfo.ReadOnly);
			});

			HAWB.CS_OA_ConsignorAddress = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals(false, HAWB.CS_ConsignorNameInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsignorStreetInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsignorStreet2Info.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsignorCityInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsignorPhoneInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsignorPostcodeInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_ConsignorStateInfo.ReadOnly);
				AssertEquals(false, HAWB.CS_RN_NKConsignorCountryInfo.ReadOnly);
			});
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.AUCusHAWB);

		protected abstract CusHAWBBase GetHAWBToTest(BusinessObjectFactory factory);

		protected CusHAWBBase GetHAWBToTest() => GetHAWBToTest(Factory);

		CusHAWBBase hawb;
		CusHAWBBase HAWB => hawb ?? (hawb = GetHAWBToTest());

		CusHAWBBase GetHouseBillWithMaster()
		{
			var masterAwb = Factory.NewWithValidTestData<CusMAWB>();
			var typeOfBill = HAWB.GetType();

			var houseAwb = (CusHAWBBase)Factory.New(typeOfBill);
			houseAwb.FillWithValidTestData();

			masterAwb.ChildBills.Add(houseAwb);

			return houseAwb;
		}

		sealed class CusHAWBBaseTestHelper : CusHAWB
		{
			public CusHAWBBaseTestHelper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}
}
