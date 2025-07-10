using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class AirCargoCusHAWBValidationAbstractTest : CusHAWBValidationAbstractTest
	{
		public void TestDuplicateKeys_InDatabase()
		{
			var hawb1 = (CusHAWB)GetNewHAWB();
			var mawb1 = hawb1.MAWB;
			mawb1.CM_MAWB = "08178451245";
			mawb1.CM_MasterHouseBill = "CoLoad1";
			hawb1.CS_HAWB = "H1";
			Factory.Save();  // For different CusMAWB, we only check if there is duplicates in database. 

			var hawb2 = (CusHAWB)GetNewHAWB();
			var mawb2 = hawb2.MAWB;
			mawb2.CM_MAWB = mawb1.CM_MAWB;
			hawb2.CS_HAWB = "H1";
			hawb2.Validation.ValidateAll();
			AssertEquals("Not duplicate yet", false, hawb2.CS_HAWBInfo.HasMessageErrors());

			hawb2.CS_MasterHouseBill = "CoLoad1";
			hawb2.Validation.ValidateAll();
			AssertHasWarningContaining("Duplicate, but warnings ", hawb2.CS_HAWBInfo, "These are the house bills that have duplicate combination of MAWB and HAWB");

			hawb1.CS_IsPrealerted = true;
			Factory.Save();

			hawb2.CS_MasterHouseBill = "CoLoad1";
			hawb2.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Duplicate and message errors", hawb2.CS_HAWBInfo, "These are the house bills that have duplicate combination of MAWB and HAWB");
		}

		public void TestDuplicateKeys_InMemory()
		{
			var hAWB1 = (CusHAWB)GetNewHAWB();
			var mAWB1 = hAWB1.MAWB;
			mAWB1.CM_MAWB = "08178451245";
			hAWB1.CS_MasterHouseBill = "CoLoad1";
			hAWB1.CS_HAWB = "H1";
			hAWB1.Validation.ValidateAll();
			AssertNoMessageErrors("Not duplicate yet", hAWB1.CS_HAWBInfo);
			AssertNoWarnings("Not duplicate yet", hAWB1.CS_HAWBInfo);

			var hAWB2 = mAWB1.ChildBills.AddNew();
			hAWB2.CS_HAWB = "H1";
			hAWB2.Validation.ValidateAll();
			AssertNoMessageErrors("Not duplicate yet", hAWB2.CS_HAWBInfo);
			AssertNoWarnings("Not duplicate yet", hAWB2.CS_HAWBInfo);

			hAWB2.CS_MasterHouseBill = "CoLoad1";
			hAWB2.Validation.ValidateAll();
			AssertHasWarningContaining("Duplicate, but warnings ", hAWB2.CS_HAWBInfo, "These are the house bills that have duplicate combination of MAWB and HAWB and Co-Load Master");
			AssertNoMessageErrors("Duplicate, no warnings", hAWB2.CS_HAWBInfo);

			hAWB1.CS_IsPrealerted = true;
			hAWB1.CS_IsResponsePending = false;
			hAWB2.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Duplicate, but warnings ", hAWB2.CS_HAWBInfo, "These are the house bills that have duplicate combination of MAWB and HAWB");
			AssertNoWarnings("Duplicate, no message error", hAWB2.CS_HAWBInfo);

			hAWB1.CS_IsPrealerted = false;
			hAWB1.CS_IsResponsePending = true;
			hAWB2.Validation.ValidateAll();
			AssertHasMessageErrorContaining("Duplicate, but warnings ", hAWB2.CS_HAWBInfo, "These are the house bills that have duplicate combination of MAWB and HAWB");
			AssertNoWarnings("Duplicate, no message error", hAWB2.CS_HAWBInfo);
		}

		public void TestChangeCoLoadMasterWithOtherDetailsResultInMessagesErrorWhenPrealerted()
		{
			HAWB.MAWB.CM_MAWB = "12345678901";
			HAWB.CS_IsPrealerted = true;
			HAWB.CS_MasterHouseBill = "1";
			Factory.Save();

			HAWB.CS_GoodsDescription = "Description";
			AssertEquals("PreCondition: House bill message changes", true, HAWB.HasMessageChanges);
			AssertEquals("PreCondition: Description has changed", true, HAWB.IsDescriptionDifferent);
			HAWB.CS_MasterHouseBill = "Changed";
			AssertEquals("PreCondition: Sub-master changed", true, HAWB.IsMasterHouseBillDifferent);
			AssertEquals("It is a message error", true, HAWB.CS_MasterHouseBillInfo.HasMessageErrors());
		}

		public void TestChangeCoLoadMasterWithOtherDetailsDoesntResultInMessageErrorWhenNotPrealerted()
		{
			HAWB.MAWB.CM_MAWB = "12345678901";
			HAWB.CS_IsPrealerted = false;
			HAWB.CS_MasterHouseBill = "1";
			Factory.Save();

			HAWB.CS_GoodsDescription = "Description";
			AssertEquals("PreCondition: House bill message changes as it not prealerted", false, HAWB.HasMessageChanges);
			AssertEquals("PreCondition: Description has changed", true, HAWB.IsDescriptionDifferent);
			HAWB.CS_MasterHouseBill = "Changed";
			AssertEquals("PreCondition: Sub-master changed", true, HAWB.IsMasterHouseBillDifferent);
			AssertEquals("It is not a message error", false, HAWB.CS_MasterHouseBillInfo.HasMessageErrors());
		}

		public void TestValidateCS_IsMasterHouseWhenUntickedAndBothAreCreated()
		{
			CusHAWB masterHouseBill = HAWB;
			masterHouseBill.CS_HAWB = "12345M";
			masterHouseBill.CS_IsMasterHouse = true;

			CusHAWB coLoadHouseBill = HAWB.MAWB.ChildBills.AddNew();
			coLoadHouseBill.CS_HAWB = "12345H";
			coLoadHouseBill.CS_MasterHouseBill = masterHouseBill.CS_HAWB;
			Assert("There is no error", !masterHouseBill.CS_IsMasterHouseInfo.HasMessageErrors());

			Factory.Save();

			masterHouseBill.CS_IsMasterHouse = false;
			Assert("Message error as there is a sub-house", masterHouseBill.CS_IsMasterHouseInfo.HasMessageErrors());

			masterHouseBill.CS_IsMasterHouse = true;
			Assert("There is no error", !masterHouseBill.CS_IsMasterHouseInfo.HasMessageErrors());
		}

		public void TestValidateCS_IsMasterHouseWhenUntickedButOnlyMasterCusHAWBIsCreated()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_HouseBill = "12345M";

			var coLoadShipment = Factory.New<ForwardingShipment>();
			coLoadShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			coLoadShipment.JS_HouseBill = "12345H";

			HAWB.CS_HAWB = "12345M";
			HAWB.CS_IsMasterHouse = true;
			HAWB.CS_JS = masterShipment.PK;
			Assert("There is no notification", !HAWB.CS_IsMasterHouseInfo.HasNotifications());

			//SubHouseBill typeof CusHAWB hasn't been created yet as users haven't clicked the plugin menu or tab 
			HAWB.CS_IsMasterHouse = false;
			Assert("There is difference warning", HAWB.CS_IsMasterHouseInfo.HasNotifications());
		}

		public void TestWeightUQ()
		{
			HAWB.CS_WeightUQ = "";
			AssertEquals("Empty is not valid", true, HAWB.CS_WeightUQInfo.HasMessageErrors());

			HAWB.CS_WeightUQ = "T";
			AssertEquals("T is not in the list", true, HAWB.CS_WeightUQInfo.HasMessageErrors());
		}

		protected new CusHAWB HAWB => (CusHAWB)base.HAWB;

		protected override CusHAWBBase GetNewHAWB()
		{
			var mawb = Factory.New<CusMAWB>();
			return mawb.ChildBills.AddNew();
		}
	}
}
