using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	public class ConsignmentUpdatorTest : TestCaseWithFactory
	{
		#region TestUpdate
		#region TestUpdate_Normal
		public void TestUpdate_Normal()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				MasterBill.CM_RL_NKDischargePort = Consignment.Destination;
				CusHAWB houseBill = MasterBill.ChildBills.AddNew();
				Updator.Update(houseBill);
				AssertHouseBill(houseBill, Consignment.HouseBill, Consignment.Origin, Consignment.Destination, Consignment.Weight, Consignment.GoodsValue, Consignment.GoodsCurrency, ConsignmentNote.NoteText, Consignment.PackageCount, "PO", "STD");
				AssertConsignee(houseBill, Consignment.ConsigneeContactName, Consignment.ConsigneeName, Consignment.ConsigneeAddress1, Consignment.ConsigneeAddress2, Consignment.ConsigneeCity, Consignment.ConsigneeState, Consignment.ConsigneePostCode, Consignment.ConsigneeCountry, Consignment.ConsigneeContactPhone);
				AssertDelivery(houseBill, Consignment.DeliveryContactName, Consignment.DeliveryName, Consignment.DeliveryAddress1, Consignment.DeliveryAddress2, Consignment.DeliveryCity, Consignment.DeliveryState, Consignment.DeliveryPostCode, Consignment.DeliveryContactPhone);
				AssertConsignor(houseBill, Consignment.ConsignorContactName, Consignment.ConsignorName, Consignment.ConsignorAddress1, Consignment.ConsignorAddress2, Consignment.ConsignorCity, Consignment.ConsignorState, Consignment.ConsignorPostCode, Consignment.ConsignorCountry, Consignment.ConsignorContactPhone);
				AssertPickup(houseBill, Consignment.PickupContactName, Consignment.PickupName, Consignment.PickupAddress1, Consignment.PickupAddress2, Consignment.PickupCity, Consignment.PickupState, Consignment.PickupPostCode, Consignment.PickupContactPhone);
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		#endregion
		#region TestUpdate_DefaultStringIsUsedForEmptyFields
		public void TestUpdate_DefaultStringIsUsedForEmptyFields()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				Consignment.ConsignorName = "";
				Consignment.ConsignorAddress1 = "";
				Consignment.ConsignorAddress2 = "";
				Consignment.ConsignorContactPhone = "";
				Consignment.PickupName = "";
				Consignment.PickupAddress1 = "";
				Consignment.PickupAddress2 = "";
				Consignment.PickupContactPhone = "";
				Consignment.ConsigneeName = "";
				Consignment.ConsigneeAddress1 = "";
				Consignment.ConsigneeAddress2 = "";
				Consignment.ConsigneeContactPhone = "";
				Consignment.DeliveryName = "";
				Consignment.DeliveryAddress1 = "";
				Consignment.DeliveryAddress2 = "";
				Consignment.DeliveryContactPhone = "";
				ConsignmentNote.Description1 = "";
				ConsignmentNote.Description2 = "";
				ConsignmentNote.Description3 = "";
				Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				MasterBill.CM_RL_NKDischargePort = Consignment.Destination;
				CusHAWB houseBill = MasterBill.ChildBills.AddNew();
				Updator.Update(houseBill);
				AssertHouseBill(houseBill, Consignment.HouseBill, Consignment.Origin, Consignment.Destination, Consignment.Weight, Consignment.GoodsValue, Consignment.GoodsCurrency, ConsignmentUpdator.DefaultEmptyGoodsDescriptionText, Consignment.PackageCount, "PO", "STD");
				AssertConsignor(houseBill, Consignment.ConsignorContactName, ConsignmentUpdator.NotSuppliedString, ConsignmentUpdator.NotSuppliedString, Consignment.ConsignorCity, Consignment.ConsignorState, Consignment.ConsignorPostCode, Consignment.ConsignorCountry, Consignment.ConsignorPhone);
				JobDocAddress pickupAddress = Factory.LoadTop1<JobDocAddress>(CusHawbQuery(houseBill.PK, DocAddressTypes.Codes.SupplierPickupDeliveryAddress));
				AssertNotNull("pickup address should be null, as address line 1 and address line 2 are not blank", pickupAddress);
				AssertConsignee(houseBill, Consignment.ConsigneeContactName, ConsignmentUpdator.NotSuppliedString, ConsignmentUpdator.NotSuppliedString, Consignment.ConsigneeCity, Consignment.ConsigneeState, Consignment.ConsigneePostCode, Consignment.ConsigneeCountry, Consignment.ConsigneePhone);
				JobDocAddress deliveryAddress = Factory.LoadTop1<JobDocAddress>(CusHawbQuery(houseBill.PK, DocAddressTypes.Codes.SupplierPickupDeliveryAddress));
				AssertNotNull("delivery address should be null, as address line 1 and address line 2 are both not blank", deliveryAddress);
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		#endregion
		#region TestUpdate_Address2IsUsedIfAddress1IsEmpty
		public void TestUpdate_Address2IsUsedIfAddress1IsEmpty()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				Consignment.ConsignorAddress1 = "";
				Consignment.ConsignorAddress2 = "CONSIGNOR ADD 2";
				Consignment.PickupAddress1 = "";
				Consignment.PickupAddress2 = "CONSIGNOR ADD 2";
				Consignment.ConsigneeAddress1 = "";
				Consignment.ConsigneeAddress2 = "CONSIGNEE ADD 2";
				Consignment.DeliveryAddress1 = "";
				Consignment.DeliveryAddress2 = "DELIVERY ADD 2";
				Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				MasterBill.CM_RL_NKDischargePort = Consignment.Destination;
				CusHAWB houseBill = MasterBill.ChildBills.AddNew();
				Updator.Update(houseBill);
				AssertHouseBill(houseBill, Consignment.HouseBill, Consignment.Origin, Consignment.Destination, Consignment.Weight, Consignment.GoodsValue, Consignment.GoodsCurrency, ConsignmentNote.NoteText, Consignment.PackageCount, "PO", "STD");
				AssertConsignor(houseBill, Consignment.ConsignorContactName, Consignment.ConsignorName, Consignment.ConsignorAddress2, Consignment.ConsignorCity, Consignment.ConsignorState, Consignment.ConsignorPostCode, Consignment.ConsignorCountry, Consignment.ConsignorContactPhone);
				AssertPickup(houseBill, Consignment.PickupContactName, Consignment.PickupName, Consignment.PickupAddress2, "", Consignment.PickupCity, Consignment.PickupState, Consignment.PickupPostCode, Consignment.PickupContactPhone);
				AssertConsignee(houseBill, Consignment.ConsigneeContactName, Consignment.ConsigneeName, Consignment.ConsigneeAddress2, Consignment.ConsigneeCity, Consignment.ConsigneeState, Consignment.ConsigneePostCode, Consignment.ConsigneeCountry, Consignment.ConsigneeContactPhone);
				AssertDelivery(houseBill, Consignment.DeliveryContactName, Consignment.DeliveryName, Consignment.DeliveryAddress2, "", Consignment.DeliveryCity, Consignment.DeliveryState, Consignment.DeliveryPostCode, Consignment.DeliveryContactPhone);
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		public void TestUpdate_Addr1AndAddr2AreUsedIfBothAreNotEmpty()
		{
			Consignment.ConsignorAddress1 = "CONSIGNOR ADD 1";
			Consignment.ConsignorAddress2 = "CONSIGNOR ADD 2";
			Consignment.PickupAddress1 = "PICKUP ADD 1";
			Consignment.PickupAddress2 = "PICKUP ADD 2";
			Consignment.ConsigneeAddress1 = "CONSIGNEE ADD 1";
			Consignment.ConsigneeAddress2 = "CONSIGNEE ADD 2";
			Consignment.DeliveryAddress1 = "DELIVERY ADD 1";
			Consignment.DeliveryAddress2 = "DELIVERY ADD 2";
			Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			MasterBill.CM_RL_NKDischargePort = Consignment.Destination;
			CusHAWB houseBill = MasterBill.ChildBills.AddNew();
			Updator.Update(houseBill);
			AssertHouseBill(houseBill, Consignment.HouseBill, Consignment.Origin, Consignment.Destination, Consignment.Weight, Consignment.GoodsValue, Consignment.GoodsCurrency, ConsignmentNote.NoteText, Consignment.PackageCount, "PO", "STD");
			AssertConsignor(houseBill, Consignment.ConsignorContactName, Consignment.ConsignorName, Consignment.ConsignorAddress1, Consignment.ConsignorAddress2, Consignment.ConsignorCity, Consignment.ConsignorState, Consignment.ConsignorPostCode, Consignment.ConsignorCountry, Consignment.ConsignorContactPhone);
			AssertPickup(houseBill, Consignment.PickupContactName, Consignment.PickupName, Consignment.PickupAddress1, Consignment.PickupAddress2, Consignment.PickupCity, Consignment.PickupState, Consignment.PickupPostCode, Consignment.PickupContactPhone);
			AssertConsignee(houseBill, Consignment.ConsigneeContactName, Consignment.ConsigneeName, Consignment.ConsigneeAddress1, Consignment.ConsigneeAddress2, Consignment.ConsigneeCity, Consignment.ConsigneeState, Consignment.ConsigneePostCode, Consignment.ConsigneeCountry, Consignment.ConsigneeContactPhone);
			AssertDelivery(houseBill, Consignment.DeliveryContactName, Consignment.DeliveryName, Consignment.DeliveryAddress1, Consignment.DeliveryAddress2, Consignment.DeliveryCity, Consignment.DeliveryState, Consignment.DeliveryPostCode, Consignment.DeliveryContactPhone);
		}

		#endregion
		#region TestUpdate_TermsOfPayment
		public void TestUpdate_TermsOfPayment()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				MasterBill.CM_RL_NKDischargePort = Consignment.Destination;
				CusHAWB houseBill = MasterBill.ChildBills.AddNew();
				Consignment.TermsOfPayment = "S";
				Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				Updator.Update(houseBill);
				AssertEqualsIgnoreTrailingSpaces("Payment Terms", "PO", houseBill.CS_FreightPrepaidCollect);
				Consignment.TermsOfPayment = "R";
				Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				Updator.Update(houseBill);
				AssertEqualsIgnoreTrailingSpaces("Payment Terms", "CC", houseBill.CS_FreightPrepaidCollect);
				Consignment.TermsOfPayment = "Jafd d";
				Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				Updator.Update(houseBill);
				AssertEqualsIgnoreTrailingSpaces("Payment Terms", "", houseBill.CS_FreightPrepaidCollect);
				Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				Updator.Update(houseBill);
				AssertEqualsIgnoreTrailingSpaces("Payment Terms", "", houseBill.CS_FreightPrepaidCollect);
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		#endregion
		#region TestAssertUpdate_ShipmentType
		public void TestAssertUpdate_ShipmentType()
		{
			MasterBill.CM_RL_NKDischargePort = Consignment.Destination;
			CusHAWB houseBill = MasterBill.ChildBills.AddNew();
			Consignment.DocumentIndicator = "D";
			Updator.Update(houseBill);
			AssertEqualsIgnoreTrailingSpaces("Shipment Type", "DOC", houseBill.CS_ShipmentType);
			Consignment.DocumentIndicator = "N";
			Updator.Update(houseBill);
			AssertEqualsIgnoreTrailingSpaces("Shipment Type", "STD", houseBill.CS_ShipmentType);
			Consignment.DocumentIndicator = "";
			Updator.Update(houseBill);
			AssertEqualsIgnoreTrailingSpaces("Shipment Type", "STD", houseBill.CS_ShipmentType);
		}

		#endregion
		#region TestAssertUpdate_UseCompanyPhoneIfContactIsEmpty
		public void TestAssertUpdate_UseCompanyPhoneIfContactIsEmpty()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				Consignment.ConsigneeAddress2 = "";
				Consignment.ConsignorAddress2 = "";
				Consignment.ConsignorPhone = "CONSN PHONE";
				Consignment.ConsignorContactPhone = "";
				Consignment.PickupPhone = "PICKUP PHONE";
				Consignment.PickupContactPhone = "";
				Consignment.ConsigneePhone = "CGNEE PHONE";
				Consignment.ConsigneeContactPhone = "";
				Consignment.DeliveryPhone = "DLV PHONE";
				Consignment.DeliveryContactPhone = "";
				Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				MasterBill.CM_RL_NKDischargePort = Consignment.Destination;
				CusHAWB houseBill = MasterBill.ChildBills.AddNew();
				Updator.Update(houseBill);
				AssertHouseBill(houseBill, Consignment.HouseBill, Consignment.Origin, Consignment.Destination, Consignment.Weight, Consignment.GoodsValue, Consignment.GoodsCurrency, ConsignmentNote.NoteText, Consignment.PackageCount, "PO", "STD");
				AssertConsignee(houseBill, Consignment.ConsigneeContactName, Consignment.ConsigneeName, Consignment.ConsigneeAddress1, Consignment.ConsigneeCity, Consignment.ConsigneeState, Consignment.ConsigneePostCode, Consignment.ConsigneeCountry, Consignment.ConsigneePhone);
				AssertDelivery(houseBill, Consignment.DeliveryContactName, Consignment.DeliveryName, Consignment.DeliveryAddress1, Consignment.DeliveryAddress2, Consignment.DeliveryCity, Consignment.DeliveryState, Consignment.DeliveryPostCode, Consignment.DeliveryPhone);
				AssertConsignor(houseBill, Consignment.ConsignorContactName, Consignment.ConsignorName, Consignment.ConsignorAddress1, Consignment.ConsignorCity, Consignment.ConsignorState, Consignment.ConsignorPostCode, Consignment.ConsignorCountry, Consignment.ConsignorPhone);
				AssertPickup(houseBill, Consignment.PickupContactName, Consignment.PickupName, Consignment.PickupAddress1, Consignment.PickupAddress2, Consignment.PickupCity, Consignment.PickupState, Consignment.PickupPostCode, Consignment.PickupPhone);
				Consignment.ConsignorPhone = "CONSN PHONE";
				Consignment.ConsignorContactPhone = "CONSN PHONE1";
				Consignment.PickupPhone = "PICKUP PHONE";
				Consignment.PickupContactPhone = "PICKUP PHONE 1";
				Consignment.ConsigneePhone = "CGNEE PHONE";
				Consignment.ConsigneeContactPhone = "CGNEE PHONE 1";
				Consignment.DeliveryPhone = "DLV PHONE";
				Consignment.DeliveryContactPhone = "DLV PHONE 1";
				houseBill = MasterBill.ChildBills.AddNew();
				Updator.Update(houseBill);
				AssertHouseBill(houseBill, Consignment.HouseBill, Consignment.Origin, Consignment.Destination, Consignment.Weight, Consignment.GoodsValue, Consignment.GoodsCurrency, ConsignmentNote.NoteText, Consignment.PackageCount, "PO", "STD");
				AssertConsignee(houseBill, Consignment.ConsigneeContactName, Consignment.ConsigneeName, Consignment.ConsigneeAddress1, Consignment.ConsigneeCity, Consignment.ConsigneeState, Consignment.ConsigneePostCode, Consignment.ConsigneeCountry, Consignment.ConsigneeContactPhone);
				AssertDelivery(houseBill, Consignment.DeliveryContactName, Consignment.DeliveryName, Consignment.DeliveryAddress1, Consignment.DeliveryAddress2, Consignment.DeliveryCity, Consignment.DeliveryState, Consignment.DeliveryPostCode, Consignment.DeliveryContactPhone);
				AssertConsignor(houseBill, Consignment.ConsignorContactName, Consignment.ConsignorName, Consignment.ConsignorAddress1, Consignment.ConsignorCity, Consignment.ConsignorState, Consignment.ConsignorPostCode, Consignment.ConsignorCountry, Consignment.ConsignorContactPhone);
				AssertPickup(houseBill, Consignment.PickupContactName, Consignment.PickupName, Consignment.PickupAddress1, Consignment.PickupAddress2, Consignment.PickupCity, Consignment.PickupState, Consignment.PickupPostCode, Consignment.PickupContactPhone);
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		#endregion
		#region TestUpdate_SelfAssessedClearanceFlag
		public void TestUpdate_SelfAssessedClearanceFlag()
		{
			TaxOrFeeTestHelper.SetUp();
			MasterBill.CM_RL_NKDischargePort = "AUSYD";
			CusHAWB houseBill = MasterBill.ChildBills.AddNew();
			Consignment.GoodsCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			Consignment.GoodsValue = 10m;
			Updator.Update(houseBill);
			AssertEquals("CS_IsSelfAssessedClearance should be true", true, houseBill.CS_IsSelfAssessedClearance);
			var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);
			Consignment.GoodsValue = deminimus + 1;
			Updator.Update(houseBill);
			AssertEquals("CS_IsSelfAssessedClearance should be false as overthreshold", false, houseBill.CS_IsSelfAssessedClearance);
			Consignment.GoodsValue = 10m;
			houseBill.CS_IsMasterHouse = true;
			AssertEquals("CS_IsSelfAssessedClearance should be false as it is a Master House", false, houseBill.CS_IsSelfAssessedClearance);
			houseBill.CS_IsMasterHouse = false;
			Consignment.ConsigneeCountry = "US";
			Updator.Update(houseBill);
			AssertEquals("CS_IsSelfAssessedClearance should be false as it is a transhipment", false, houseBill.CS_IsSelfAssessedClearance);
		}

		#endregion
		#region TestUpdateOfPickupAndDeliveryDetailsTwice()
		public void TestUpdateOfPickupOrDeliveryDetailsTwice()
		{
			ZString oldSetting = Env.Registry.AUCustomsImportsMessagingMode;
			try
			{
				Env.Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				MasterBill.CM_RL_NKDischargePort = Consignment.Destination;
				CusHAWB houseBill = MasterBill.ChildBills.AddNew();
				Consignment.ConsigneeAddress2 = "";
				Consignment.ConsignorAddress2 = "";
				Updator.Update(houseBill);
				Updator.Update(houseBill);
				Factory.Save();
				AssertHouseBill(houseBill, Consignment.HouseBill, Consignment.Origin, Consignment.Destination, Consignment.Weight, Consignment.GoodsValue, Consignment.GoodsCurrency, ConsignmentNote.NoteText, Consignment.PackageCount, "PO", "STD");
				AssertConsignee(houseBill, Consignment.ConsigneeContactName, Consignment.ConsigneeName, Consignment.ConsigneeAddress1, Consignment.ConsigneeCity, Consignment.ConsigneeState, Consignment.ConsigneePostCode, Consignment.ConsigneeCountry, Consignment.ConsigneeContactPhone);
				AssertDelivery(houseBill, Consignment.DeliveryContactName, Consignment.DeliveryName, Consignment.DeliveryAddress1, Consignment.DeliveryAddress2, Consignment.DeliveryCity, Consignment.DeliveryState, Consignment.DeliveryPostCode, Consignment.DeliveryContactPhone);
				AssertConsignor(houseBill, Consignment.ConsignorContactName, Consignment.ConsignorName, Consignment.ConsignorAddress1, Consignment.ConsignorCity, Consignment.ConsignorState, Consignment.ConsignorPostCode, Consignment.ConsignorCountry, Consignment.ConsignorContactPhone);
				AssertPickup(houseBill, Consignment.PickupContactName, Consignment.PickupName, Consignment.PickupAddress1, Consignment.PickupAddress2, Consignment.PickupCity, Consignment.PickupState, Consignment.PickupPostCode, Consignment.PickupContactPhone);
			}
			finally
			{
				Env.Registry.AUCustomsImportsMessagingMode = oldSetting;
			}
		}

		#endregion
		#endregion
		#region TestUpdateQuantumOriginalValue
		[ExpectNoExceptions()]
		public void TestUpdateQuantumOriginalValue_NullHousebill()
		{
			Updator.UpdateQuantumOriginalValue(null, "TEST SECTOR", SydBranch.GB_Code);
		}

		public void TestUpdateQuantumOriginalValue()
		{
			MasterBill.CM_RL_NKDischargePort = Consignment.Destination;
			CusHAWB houseBill = MasterBill.ChildBills.AddNew();
			Updator.UpdateQuantumOriginalValue(houseBill, "TEST SECTOR", SydBranch.GB_Code);
			string expectedValue = ZString.Format("TEST SECTOR-{0}-{1}-{2}", Consignment.Origin.Right(3).PadRight(3), Consignment.Destination.Right(3).PadRight(3), SydBranch.GB_Code);
			StmNote[] sectorNotes = houseBill.Notes.FindByDescription(ConsignmentUpdator.OriginalQuantumSectorNoteDescription);
			AssertEquals("Housebill should have 1 Original Sector Note", 1, sectorNotes.Length);
			StmNote sectorNote1 = sectorNotes[0];
			AssertEqualsIgnoreTrailingSpaces("Original Quantum Sector Note", expectedValue, sectorNote1.ST_NoteDataAsText);
			StmNote sectorNote2 = houseBill.Notes.AddNew(true, ConsignmentUpdator.OriginalQuantumSectorNoteDescription, "New Note");
			Updator.UpdateQuantumOriginalValue(houseBill, "TEST NEW SECTOR", SydBranch.GB_Code);
			sectorNotes = houseBill.Notes.FindByDescription(ConsignmentUpdator.OriginalQuantumSectorNoteDescription);
			AssertEquals("Housebill should have 2 Original Sector Notes", 2, sectorNotes.Length);
			AssertCollectionContains("SectorNote1 should be in the list of Sector Notes", sectorNote1, sectorNotes);
			AssertCollectionContains("SectorNote2 should be in the list of Sector Notes", sectorNote2, sectorNotes);
			string newExpectedValue = expectedValue.Replace("TEST SECTOR", "TEST NEW SECTOR");
			if (newExpectedValue == sectorNote1.ST_NoteDataAsText)
			{
				AssertEqualsIgnoreTrailingSpaces("Sector2 Note should not have been changed", "New Note", sectorNote2.ST_NoteDataAsText);
			}
			else
			{
				AssertEqualsIgnoreTrailingSpaces("Sector1 Note should not have been changed", expectedValue, sectorNote1.ST_NoteDataAsText);
				AssertEqualsIgnoreTrailingSpaces("Original Quantum Sector2 Note", newExpectedValue, sectorNote2.ST_NoteDataAsText);
			}
		}

		#endregion
		#region TestGetStateCode
		public void TestGetStateCode()
		{
			AssertEquals("Non-Australian State", "TEST STATE", Updator.GetStateCode("BLAH COUNTRY", "TEST STATE"));
			AssertEquals("Australian State", "TEST STATE", Updator.GetStateCode(Constants.CountryCodes.Australia, "TEST STATE"));
			AssertEquals("NSW State", "NSW", Updator.GetStateCode(Constants.CountryCodes.Australia, " NEW. south .WALES. "));
			AssertEquals("VIC State", "VIC", Updator.GetStateCode(Constants.CountryCodes.Australia, " vict.ORIA. "));
			AssertEquals("QLD State", "QLD", Updator.GetStateCode(Constants.CountryCodes.Australia, " QUE.ensland. "));
			AssertEquals("WA State", "WA", Updator.GetStateCode(Constants.CountryCodes.Australia, " weste.RN AU.StraLIA. "));
			AssertEquals("SA State", "SA", Updator.GetStateCode(Constants.CountryCodes.Australia, " sou.TH aust.RALIA. "));
			AssertEquals("TAS State", "TAS", Updator.GetStateCode(Constants.CountryCodes.Australia, " TAsm.aNIA. "));
			AssertEquals("NT State", "NT", Updator.GetStateCode(Constants.CountryCodes.Australia, " NO.rtheRN TERri.toRY. "));
		}

		#endregion
		#region Implementation
		#region Assertions
		ZQuery CusHawbQuery(ZGuid housebillPK, ZString docAddressType)
		{
			ZQuery query = new ZQuery(JobDocAddressSchema.E2_ParentID, housebillPK);
			query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(CusHAWBSchema.Constants.TableName));
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, docAddressType);
			return query;
		}

		void AssertHouseBill(CusHAWB houseBill, ZString hAWB, ZString origin, ZString destination, ZDecimal weight, ZDecimal goodsValue, ZString currency, ZString goodsDescription, ZShort packageCount, ZString termsOfPayment, ZString shipmentType)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			AssertEqualsIgnoreTrailingSpaces("HouseBill No", hAWB, houseBill.CS_HAWB);
			AssertEqualsIgnoreTrailingSpaces("Origin", origin, houseBill.CS_RL_NKOrigin);
			AssertEqualsIgnoreTrailingSpaces("Destination", destination, houseBill.CS_RL_NKDestination);
			AssertEquals("Weight", weight, houseBill.CS_Weight);
			AssertEquals("Chargable Weight and Weight should be the same", houseBill.CS_Weight, houseBill.CS_ChargableWeight);
			AssertEqualsIgnoreTrailingSpaces("Weight Unit Quantity", Core.Constants.Weight.Kilograms, houseBill.CS_WeightUQ);
			AssertEquals("Goods Value", goodsValue, houseBill.CS_GoodsValue);
			AssertEqualsIgnoreTrailingSpaces("Currency", currency, houseBill.CS_RX_NKGoodsCurrency);
			AssertEqualsIgnoreTrailingSpaces("Goods Description", goodsDescription.Left(CusHAWBSchema.CS_GoodsDescription.MaxLength), houseBill.CS_GoodsDescription);
			AssertEquals("Manifested PackageCount", packageCount, houseBill.CS_PiecesManifested);
			AssertEqualsIgnoreTrailingSpaces("Payment Terms", termsOfPayment, houseBill.CS_FreightPrepaidCollect);
			AssertEqualsIgnoreTrailingSpaces("Shipment Type", shipmentType, houseBill.CS_ShipmentType);
		}

		void AssertConsignor(CusHAWB houseBill, ZString contactName, ZString name, ZString street, ZString city, ZString state, ZString post, ZString country, ZString phone)
		{
			AssertConsignor(houseBill, contactName, name, street, "", city, state, post, country, phone);
		}

		void AssertConsignor(CusHAWB houseBill, ZString contactName, ZString name, ZString street1, ZString street2, ZString city, ZString state, ZString post, ZString country, ZString phone)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			AssertEquals("CS_OH_Consignor", ZGuid.Empty, houseBill.CS_OH_Consignor);
			AssertEqualsIgnoreTrailingSpaces("Consignor Contact Name", contactName, houseBill.CS_ConsignorContactName);
			AssertEqualsIgnoreTrailingSpaces("Consignor Name", name, houseBill.CS_ConsignorName);
			AssertEqualsIgnoreTrailingSpaces("Consignor Street", street1, houseBill.CS_ConsignorStreet);
			AssertEqualsIgnoreTrailingSpaces("Consignor Street", street2, houseBill.CS_ConsignorStreet2);
			AssertEqualsIgnoreTrailingSpaces("Consignor City", city, houseBill.CS_ConsignorCity);
			AssertEqualsIgnoreTrailingSpaces("Consignor State", state.Left(CusHAWBSchema.CS_ConsigneeState.MaxLength), houseBill.CS_ConsignorState);
			AssertEqualsIgnoreTrailingSpaces("Consignor Post", post, houseBill.CS_ConsignorPostcode);
			AssertEqualsIgnoreTrailingSpaces("Consignor Country", country, houseBill.CS_RN_NKConsignorCountry);
			AssertEqualsIgnoreTrailingSpaces("Consignor Phone", phone, houseBill.CS_ConsignorPhone);
		}

		void AssertPickup(CusHAWB houseBill, ZString contactName, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString post, ZString phone)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			ZQuery pickUpFilter = CusHawbQuery(houseBill.PK, DocAddressTypes.Codes.SupplierPickupDeliveryAddress);
			JobDocAddress pickup = Factory.LoadTop1<JobDocAddress>(pickUpFilter);
			AssertNotNull("Pickup is not null", pickup);
			AssertEqualsIgnoreTrailingSpaces("Pickup Contact Name", contactName, pickup.E2_Contact);
			AssertEqualsIgnoreTrailingSpaces("Pickup Name", name, pickup.E2_CompanyName);
			AssertEqualsIgnoreTrailingSpaces("Pickup Address 1", address1, pickup.E2_Address1);
			AssertEqualsIgnoreTrailingSpaces("Pickup Address 2", address2, pickup.E2_Address2);
			AssertEqualsIgnoreTrailingSpaces("Pickup City", city, pickup.E2_City);
			AssertEqualsIgnoreTrailingSpaces("Pickup State", state.Left(JobDocAddressSchema.E2_State.MaxLength), pickup.E2_State);
			AssertEqualsIgnoreTrailingSpaces("Pickup Post", post, pickup.E2_Postcode);
			AssertEqualsIgnoreTrailingSpaces("Pickup Phone", phone, pickup.E2_Phone);
		}

		void AssertConsignee(CusHAWB houseBill, ZString contactName, ZString name, ZString street, ZString city, ZString state, ZString post, ZString country, ZString phone)
		{
			AssertConsignee(houseBill, contactName, name, street, "", city, state, post, country, phone);
		}

		void AssertConsignee(CusHAWB houseBill, ZString contactName, ZString name, ZString street, ZString street2, ZString city, ZString state, ZString post, ZString country, ZString phone)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			AssertEquals("CS_OH_Consignee", ZGuid.Empty, houseBill.CS_OH_Consignee);
			AssertEqualsIgnoreTrailingSpaces("Consignee Contact Name", contactName, houseBill.CS_ConsigneeContactName);
			AssertEqualsIgnoreTrailingSpaces("Consignee Name", name, houseBill.CS_ConsigneeName);
			AssertEqualsIgnoreTrailingSpaces("Consignee Street 1", street, houseBill.CS_ConsigneeStreet);
			AssertEqualsIgnoreTrailingSpaces("Consignee Street 2", street2, houseBill.CS_ConsigneeStreet2);
			AssertEqualsIgnoreTrailingSpaces("Consignee City", city, houseBill.CS_ConsigneeCity);
			AssertEqualsIgnoreTrailingSpaces("Consignee State", state.Left(CusHAWBSchema.CS_ConsigneeState.MaxLength), houseBill.CS_ConsigneeState);
			AssertEqualsIgnoreTrailingSpaces("Consignee Post", post, houseBill.CS_ConsigneePostcode);
			AssertEqualsIgnoreTrailingSpaces("Consignee Country", country, houseBill.CS_RN_NKConsigneeCountry);
			AssertEqualsIgnoreTrailingSpaces("Consignee Phone", phone, houseBill.CS_ConsigneePhone);
		}

		void AssertDelivery(CusHAWB houseBill, ZString contactName, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString post, ZString phone)
		{
			AssertNotNull("HouseBill is not null", houseBill);
			ZQuery deliveryFilter = new ZQuery(JobDocAddressSchema.E2_ParentID, houseBill.PK);
			deliveryFilter.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(CusHAWBSchema.Constants.TableName));
			deliveryFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.ImporterPickupDeliveryAddress);
			JobDocAddress delivery = Factory.LoadTop1<JobDocAddress>(deliveryFilter);
			AssertNotNull("Delivery address shouldn't be null", delivery);
			AssertEqualsIgnoreTrailingSpaces("Delivery Contact Name", contactName, delivery.E2_Contact);
			AssertEqualsIgnoreTrailingSpaces("Delivery Name", name, delivery.E2_CompanyName);
			AssertEqualsIgnoreTrailingSpaces("Delivery Address 1", address1, delivery.E2_Address1);
			AssertEqualsIgnoreTrailingSpaces("Delivery Address 2", address2, delivery.E2_Address2);
			AssertEqualsIgnoreTrailingSpaces("Delivery City", city, delivery.E2_City);
			AssertEqualsIgnoreTrailingSpaces("Delivery State", state.Left(JobDocAddress.Schema.E2_StateMaxLength), delivery.E2_State);
			AssertEqualsIgnoreTrailingSpaces("Delivery Post", post, delivery.E2_Postcode);
			AssertEqualsIgnoreTrailingSpaces("Delivery Phone", phone, delivery.E2_Phone);
		}

		void AssertEqualsIgnoreTrailingSpaces(string message, string expected, string actual)
		{
			AssertEquals(message, expected.TrimEnd(), actual.TrimEnd());
		}

		#endregion
		#region Buffer
		NotificationBuffer Buffer
		{
			get
			{
				if (fBuffer == null)
				{
					fBuffer = new NotificationBuffer();
				}

				return fBuffer;
			}
		}

		NotificationBuffer fBuffer;
		#endregion
		#region SydBranch
		protected GlbBranch SydBranch
		{
			get
			{
				if (fSydBranch == null)
				{
					fSydBranch = GetSydBranch();
				}

				return fSydBranch;
			}
		}

		GlbBranch fSydBranch;
		GlbBranch GetSydBranch()
		{
			string branchCode = "SYD";
			GlbBranch result = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
			if (result == null)
			{
				result = Factory.New<GlbBranch>();
				result.GB_Code = branchCode;
				Factory.Save();
			}

			return result;
		}

		#endregion ;
		protected override void SetUp()
		{
			base.SetUp();
			Consignment = new ConsignmentRecord(ConsignmentDetailLine);
			ConsignmentNote = new ConsignmentNoteRecord(ConsignmentNoteLine);
			ConsignmentNotes = new ConsignmentNoteRecordCollection();
			ConsignmentNotes.Add(ConsignmentNote);
			Updator = new ConsignmentUpdator(Consignment, ConsignmentNotes, Buffer);
			MasterBill = Factory.New<CusMAWB>();
		}

		ConsignmentRecord Consignment;
		ConsignmentNoteRecord ConsignmentNote;
		ConsignmentNoteRecordCollection ConsignmentNotes;
		ConsignmentUpdator Updator;
		CusMAWB MasterBill;
		const string ConsignmentDetailLine = "03940432180 SINWHR20908767HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             SG 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     WHERE IS THIS STATE            AU 2113     0288792000  0288792011  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SA                             AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "NS1233233234.34AUD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
		const string ConsignmentNoteLine = "04940432180 01123456789012345USED CLOTHES TOILET BAG                                                                                                                                                                                                                   SINWHR                                                                                                                                                                                                                            .";
		#endregion
	}
}
