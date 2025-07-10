using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CFSShipmentWrapper))]
	public class CFSShipmentWrapperTest : SeaCargoDepotNonPersistantBusineesObjectTestCase
	{
		public void TestIMessageManageableBizObj()
		{
			Customs.Business.IMessageManageableBizObj bizObj = Wrapper;
			AssertEquals("MessageManager", typeof(SeaCargoDepotMultiMessageManager), bizObj.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestOutturn()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			DepotCusOutturn outturn = header.Outturns.AddNew();

			AssertEquals(0, Wrapper.Outturns.Count);
			AssertNull(Wrapper.Outturn);

			Wrapper.Outturns.Add(outturn);
			AssertEquals(1, Wrapper.Outturns.Count);
			AssertEquals(outturn, Wrapper.Outturn);

			Wrapper.Outturns.Add(header.Outturns.AddNew());
			AssertEquals(2, Wrapper.Outturns.Count);
			AssertNotNull(Wrapper.Outturn);
			AssertEquals(outturn, Wrapper.Outturn);
			AssertEquals("More than one depot cus outturn is linked to Shipment: " + Shipment.JS_UniqueConsignRef, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCreateAndLinkOutturnIfMissing()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			DepotCusOutturn outturn = header.Outturns.AddNew();

			Wrapper.Outturns.Add(outturn);

			AssertEquals(1, Wrapper.Outturns.Count);

			Wrapper.CreateAndLinkOutturnIfMissing(Factory.New<CusOutturnHeader>());
			AssertEquals(1, Wrapper.Outturns.Count);
			AssertEquals(outturn, Wrapper.Outturn);
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			AssertEquals(false, ((ICusUnderbondDependentCollectionParent)Wrapper).UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)Wrapper).DefaultTranshipmentPort);
		}

		public void TestIsForAirCargo()
		{
			AssertEquals(false, ((ICusUnderbondUnionCollectionParent)Wrapper).IsForAirCargo);
		}

		public void TestUnderbondHumanReadableName()
		{
			Shipment.JS_HouseBill = "12345";
			AssertEquals("UnderbondHumanReadableName", "Shipment: 12345", Wrapper.UnderbondHumanReadableName);
		}

		public void TestUnderbonds()
		{
			AssertNotNull("Underbonds", Wrapper.Underbonds);
		}

		public void TestCargoStatusForOutturnLine()
		{
			Shipment.JS_ShipmentStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			AssertEquals("Status should be carried through for Outturn Lines", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, ((IOutturnableLine)Wrapper).CargoStatus);
		}

		public void TestOutturns()
		{
			AssertNotNull("outturn collection nullness", Wrapper.Outturns);
			DepotCusOutturn outturn = Wrapper.Outturns.AddNew();
			AssertEquals(Wrapper, outturn.Parent);
			AssertEquals(Wrapper.Shipment.PK, outturn.C5_ParentID);
			AssertEquals(JobShipmentSchema.Constants.Prefix, outturn.C5_ParentTableCode);
			Assert(Wrapper.IsRegisteredEditableChildObject(Wrapper.Outturns));

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CFSShipment shipmentOnFactory2 = factory2.Load<CFSShipment>(Wrapper.Shipment.PK);
			CFSShipmentWrapper wrapperOnFactory2 = CFSShipmentWrapper.Load(shipmentOnFactory2);
			AssertEquals("calls .Load()", 1, wrapperOnFactory2.Outturns.Count);
		}

		public void TestIOutturnableLine_IsDeleted()
		{
			AssertEquals("IsDeleted", false, ((IOutturnableLine)Wrapper).IsDeleted);
			Shipment.Delete();
			AssertEquals("IsDeleted", true, ((IOutturnableLine)Wrapper).IsDeleted);
		}

		public void TestCanSendWithoutDelay()
		{
			Assert(((ICusUnderbondDependentCollectionParent)Wrapper).CanSendWithoutDelay);
		}

		public void TestIOutturnableLine_TableName()
		{
			AssertEquals("TableName", Enterprise.ZArchitecture.Schema.JobShipmentSchema.Constants.TableName, ((IOutturnableLine)Wrapper).LinkTableName);
		}

		public void TestIOutturnableLine_PK()
		{
			AssertEquals("PK", Shipment.PK, ((IOutturnableLine)Wrapper).LinkPK);
		}

		public void TestICusUnderbondDependentCollectionParent_Details()
		{
			Shipment.JS_HouseBill = "12345";
			AssertEquals("Details", "Shipment: 12345", ((ICusUnderbondDependentCollectionParent)Wrapper).Details);
		}

		public void TestICusUnderbondDependentCollectionParent_PK()
		{
			AssertEquals("PK", Shipment.PK, ((ICusUnderbondDependentCollectionParent)Wrapper).LinkPK);
		}

		public void TestICusUnderbondDependentCollectionParent_OutturnableLines()
		{
			IOutturnableLine[] lines = ((ICusUnderbondDependentCollectionParent)Wrapper).OutturnableLines;
			AssertEquals("Length", 1, lines.Length);
			//TODO: fix this
			AssertEquals("Lines[0]", Wrapper.GetType(), lines[0].GetType());
		}

		public void TestCMRConsolidatedStatusAndGatePassStatus()
		{
			StmALog sCDLog = Shipment.Logs.AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceClear + " 9914N");
			AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
			using (sCDLog.LockForUpdatingKeyFieldsForTesting())
			{
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceConditionalClear + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceDetailed + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceACSSEIZED + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceAQISSEIZED + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceClearHRM + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceHELD + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceSUBUBMOV + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceTRANSHIP + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceTRANSHPHRM + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
				sCDLog.SL_Reference = DepotEvents.CargoStatusAdviceTRANSIT + " 9914N";
				AssertEquals("Wrapper Message State Text", sCDLog.SL_Reference, Wrapper.MessageStateText);
			}
		}

		public void TestIOutturnableLinePackagesManifested()
		{
			Shipment.JS_OuterPacks = 3;
			AssertEquals(3, ((IOutturnableLine)Wrapper).PackagesManifested);
		}

		public void TestCorretType()
		{
			AssertType<CusUnderbondUnionCollection>(Wrapper.AllUnderbonds);
			AssertType<CusUnderbondCollection>(Wrapper.Underbonds);
		}

		#region Implementation

		CFSShipmentWrapper fWrapper;
		CFSShipmentWrapper Wrapper
		{
			get
			{
				if (fWrapper == null)
				{
					fWrapper = (CFSShipmentWrapper)GetNewBusinessObject();
				}
				return fWrapper;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CFSShipmentWrapper.Load(Shipment);
		}

		CFSShipment fShipment;
		CFSShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<CFSShipment>();
				}
				return fShipment;
			}
		}

		#endregion
	}
}
