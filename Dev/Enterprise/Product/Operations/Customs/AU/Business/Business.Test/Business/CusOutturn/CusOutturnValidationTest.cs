using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckC5_LastMessageDate()
		{
			const string errorText = "This outturn line has not been sent, or was rejected by Customs";
			var mawb = Factory.New<CusMAWB>();
			var underbond = mawb.Underbonds.AddNew();
			var outturn = underbond.Outturns.AddNew();
			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			outturn.C5_LastMessageDate = ZDateTime.Now;
			AssertNoWarning(outturn.C5_LastMessageDateInfo, errorText);
			outturn.C5_LastMessageDate = ZDateTime.Empty;
			AssertHasWarning(outturn.C5_LastMessageDateInfo, errorText);
			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.NotSent;
			outturn.Validation.ValidateC5_LastMessageDate();
			AssertNoWarning(outturn.C5_LastMessageDateInfo, errorText);
			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			outturn.Validation.ValidateC5_LastMessageDate();
			AssertHasWarning(outturn.C5_LastMessageDateInfo, errorText);
			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentRejected;
			outturn.Validation.ValidateC5_LastMessageDate();
			AssertHasWarning(outturn.C5_LastMessageDateInfo, errorText);

			var hawb = Factory.New<CusHAWB>();
			underbond = hawb.Underbonds.AddNew();
			outturn = underbond.Outturns.AddNew();
			underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			outturn.C5_LastMessageDate = ZDateTime.Now;
			AssertNoWarning(outturn.C5_LastMessageDateInfo, errorText);
			outturn.C5_LastMessageDate = ZDateTime.Empty;
			AssertNoWarning(outturn.C5_LastMessageDateInfo, errorText);
		}

		public void TestValidateC5_OutturnResultType()
		{
			Outturn.C5_OutturnResultType = "";
			Outturn.Validation.ValidateC5_OutturnResultType();
			AssertHasErrors("by default", Outturn.C5_OutturnResultTypeInfo);

			Outturn.C5_OutturnResultType = "NIL";
			AssertNoNotifications("when valid", Outturn.C5_OutturnResultTypeInfo);

			Outturn.C5_OutturnResultType = "!!!";
			AssertHasErrors("when invalid", Outturn.C5_OutturnResultTypeInfo);

			Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			AssertHasErrors("Ensure setting outturn result validates Goods Desc field. 'SC' code requires Goods Desc", Outturn.C5_GoodsDescriptionInfo);

			Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			AssertNoErrors("Ensure setting outturn result validates Goods Desc field. 'NIL' code can have empty Goods Desc", Outturn.C5_GoodsDescriptionInfo);
		}

		public void TestValidateC5_PackagesOutturned()
		{
			Outturn.Validation.ValidateC5_PackagesOutturned();
			AssertNoMessageErrors("by default", Outturn.C5_PackagesOutturnedInfo);

			Outturn.C5_PackagesOutturned = 1;
			AssertNoNotifications("When valid", Outturn.C5_PackagesOutturnedInfo);

			Outturn.C5_PackagesOutturned = -1;
			AssertHasMessageErrors("when negative", Outturn.C5_PackagesOutturnedInfo);

			Outturn.C5_PackagesOutturned = 0;
			AssertNoMessageErrors("This is valid for zero landing", Outturn.C5_PackagesOutturnedInfo);
		}

		public void TestErrorIfDuplicateOutturnOnUnderbond()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "123";
			CusUnderbond underbond = mAWB.FakeFlightOuturnUnderbond;
			CusOutturn outturn1 = underbond.Outturns.AddNew();
			CusOutturn outturn2 = underbond.Outturns.AddNew();
			outturn1.ParentStringRepresentation = ((IOutturnableLine)hAWB).UnderbondHumanReadableName;
			AssertEquals("Outturn1.HasErrors", false, outturn1.ParentStringRepresentationInfo.HasErrors());
			outturn2.ParentStringRepresentation = ((IOutturnableLine)hAWB).UnderbondHumanReadableName;
			AssertEquals("Outturn2.HasErrors", true, outturn2.ParentStringRepresentationInfo.HasErrors());
		}

		public void TestErrorOnEmptyOutturnParent()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusUnderbond underbond = mAWB.FakeFlightOuturnUnderbond;
			CusOutturn outturn1 = underbond.Outturns.AddNew();
			outturn1.ParentStringRepresentation = ((IOutturnableLine)hAWB).UnderbondHumanReadableName;
			AssertEquals("Outturn1.HasErrors", false, outturn1.ParentStringRepresentationInfo.HasErrors());
			outturn1.ParentStringRepresentation = "";
			AssertEquals("Outturn2.HasErrors", true, outturn1.ParentStringRepresentationInfo.HasErrors());
		}

		public void TestErrorIfWeAreNotInTheListOfOutturnableLines()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.CN_ContainerNumber = "1";
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			house.CA_HouseBill = "1";
			var pivot = house.Pivot.AddNew();
			pivot.CV_CN = container.PK;
			CusUnderbond underbond = container.Underbonds.AddNew();

			CusOutturn outturn = underbond.Outturns.AddNew();
			outturn.Parent = pivot;
			AssertEquals("Container 1 - Housebill 1", outturn.ParentStringRepresentation);

			outturn.Validation.ValidateParentStringRepresentation();
			AssertHasError(outturn.ParentStringRepresentationInfo, "This line is not valid for outturning");
		}

		public void TestIssue56325WhenOutturnHasNoLinkedUnderbondButIsLinkedToAParent()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			CusUnderbond underbond = mAWB.FakeFlightOuturnUnderbond;
			CusOutturn outturn1 = underbond.Outturns.AddNew();
			outturn1.C5_C4_Underbond = ZGuid.Empty;
			outturn1.ParentStringRepresentation = ((IOutturnableLine)hAWB).UnderbondHumanReadableName;
			AssertEquals("Outturn1.HasErrors", false, outturn1.ParentStringRepresentationInfo.HasErrors());
		}

		public void TestCheckC5_HouseBill()
		{
			CombineAssertions("Standalone Air Cargo Depot Outturn", () =>
			{
				var oceanBill = Factory.New<CusSCAOceanBill>();
				var container = oceanBill.Containers.AddNew();
				container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
				container.CN_ContainerNumber = "1";
				var house = oceanBill.HouseBills.AddNew();
				house.CA_HouseBill = "1";
				var pivot = house.Pivot.AddNew();
				pivot.CV_CN = container.PK;
				var underbond1 = container.Underbonds.AddNew();

				var outturn11 = underbond1.Outturns.AddNew();
				outturn11.C5_HouseBill = "HB1";
				outturn11.Validation.ValidateC5_HouseBill();
				AssertNoMessageErrors("by default", outturn11.C5_HouseBillInfo);

				var outturn21 = underbond1.Outturns.AddNew();
				outturn21.C5_HouseBill = "HB1";
				outturn21.Validation.ValidateC5_HouseBill();
				AssertHasMessageErrorContaining(outturn21.C5_HouseBillInfo, "Outturn Line with this Housebill value already exists.");

				var outturn3 = underbond1.Outturns.AddNew();
				outturn3.C5_HouseBill = "738829";
				outturn3.Validation.ValidateC5_HouseBill();
				AssertNoMessageErrors("House Bill value not duplicated", outturn3.C5_HouseBillInfo);
			});

			CombineAssertions("Consol Air Cargo Depot Outturn", () =>
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb1 = mawb.CurrentHouseBills.AddNew();
				hawb1.CS_HAWB = "111";
				var hawb2 = mawb.CurrentHouseBills.AddNew();
				hawb2.CS_HAWB = "222";
				var underbond = mawb.Underbonds.AddNew();
				var outturn1 = underbond.Outturns.AddNew();
				outturn1.ParentStringRepresentation = ((IOutturnableLine)hawb1).UnderbondHumanReadableName;
				var outturn2 = underbond.Outturns.AddNew();
				outturn2.ParentStringRepresentation = ((IOutturnableLine)hawb2).UnderbondHumanReadableName;
				outturn2.Validation.ValidateC5_HouseBill();
				AssertNoMessageErrorContaining(outturn2.C5_HouseBillInfo, "Outturn Line with this Housebill value already exists.");
			});
		}

		CusOutturn outturn;
		CusOutturn Outturn => outturn ?? (outturn = Factory.New<CusOutturn>());
	}
}
