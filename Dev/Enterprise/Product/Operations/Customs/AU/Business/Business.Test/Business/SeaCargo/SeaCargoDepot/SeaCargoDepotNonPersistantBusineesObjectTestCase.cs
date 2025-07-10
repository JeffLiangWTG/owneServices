using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class SeaCargoDepotNonPersistantBusineesObjectTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected void EnsureEventTimeGreaterThanPreviousEvent(StmALog depotEvent, BusinessObject owner)
		{
			if (owner.GetLogs().GetAllLogs().Count > 1)
			{
				StmALog previousEvent = owner.GetLogs().GetAllLogs()[owner.GetLogs().GetAllLogs().Count - 2];
				if (previousEvent.SL_EventTime >= depotEvent.SL_EventTime)
				{
					owner.GetLogs().GetAllLogs().Remove(depotEvent);
					depotEvent.SL_EventTime = previousEvent.SL_EventTime.AddMilliseconds(30);
					owner.GetLogs().GetAllLogs().Add(depotEvent);
				}
			}
		}

		protected const string SomeForwarderClientID = "C839382921";

		protected CommonShipment AddShipmentToContainer(ZString houseBill, CommonContainer container)
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.JS_HouseBill = houseBill;
			shipment.JS_OH_HandledOnBehalfOfForwarder = SomeForwarder().PK;
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[0].JL_PackageCount = 1;
			shipment.OuterPackLines[0].SetContainer(container.PK);
			return shipment;
		}

		protected OrgHeader SomeForwarder()
		{
			return SomeForwarder(SomeForwarderClientID);
		}

		protected OrgHeader SomeForwarder(ZString clientID)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "Forwarder " + clientID;
			result.MainAddress.OA_Address1 = "Address for Forwarder";
			result.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			result.LocalManifestID = clientID;
			return result;
		}

		protected void CreateOrgProxyForCurrentBranch()
		{
			OrgHeader orgProxy = GlbBranch.CurrentBranch.Factory.New<OrgHeader>();
			orgProxy.OH_FullName = "Testing OrgProxy For Depot";
			orgProxy.MainAddress.OA_Address1 = "Somewhere by the docks";
			orgProxy.OH_RL_NKClosestPort = "AUSYD";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxy.PK;
		}

		protected ZGuid previousGB_OH;
		protected override void SetUp()
		{
			base.SetUp();
			previousGB_OH = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			if (GlbBranch.CurrentBranch.OrgProxy == null)
			{
				CreateOrgProxyForCurrentBranch();
			}
		}

		protected override void TearDown()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = previousGB_OH;
			base.TearDown();
		}

		#endregion
	}
}
