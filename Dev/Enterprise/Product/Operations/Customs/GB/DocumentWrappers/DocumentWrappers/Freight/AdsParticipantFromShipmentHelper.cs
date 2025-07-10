using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.DocumentWrappers.Freight
{
	public class AdsParticipantFromShipmentHelper : IAdsParticipant
	{
		public AdsParticipantFromShipmentHelper(ForwardingShipment shipment)
		{
			this.shipment = shipment;
		}

		ZString IAdsParticipant.CtStatus
		{
			get { return shipment.JS_CommunityTransitStatus; }
		}

		ZDecimal IAdsParticipant.TotalWeightAds
		{
			get { return new ZWeight(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight).InKilogramsSafe; }
		}

		ZString IAdsParticipant.HouseBill
		{
			get { return shipment.JS_HouseBill; }
		}

		ZString IAdsParticipant.Origin
		{
			get { return shipment.Origin.ToIataSafe(); }
		}

		ZString IAdsParticipant.FinalDestination
		{
			get { return shipment.Destination.ToIataSafe(); }
		}

		OrgHeader IAdsParticipant.Consignor
		{
			get { return shipment.Consignor; }
		}

		OrgHeader IAdsParticipant.Consignee
		{
			get { return shipment.Consignee; }
		}

		ZInt IAdsParticipant.TotalNoOfPacks
		{
			get { return shipment.JS_OuterPacks; }
		}

		ZString IAdsParticipant.GoodsDescription
		{
			get { return shipment.JS_GoodsDescription; }
		}

		int IAdsParticipant.DeclarationsCount
		{
			get { return shipment.JS_CommunityTransitStatus == ExportCommunityTransitStatusList.Codes.C ? 0 : GetDucrs().Length; }
		}

		ZString IAdsParticipant.DeclarationUCRs
		{
			get { return String.Join(System.Environment.NewLine, GetDucrs()); }
		}

		ZString[] GetDucrs()
		{
			if (shipment.JS_CommunityTransitStatus == ExportCommunityTransitStatusList.Codes.C)
			{
				return new ZString[] { Helpers.CStatusGoods };
			}
			else
			{
				var sb = new List<ZString>();
				foreach (CusEntryNumber number in (from CusEntryNumber cen in shipment.Numbers orderby cen.CE_EntryNum where cen.CE_EntryType == CusEntryNumberTypes.Standard.UniqueConsignementReference select cen))
				{
					sb.Add(number.CE_EntryNum);
				}
				return sb.ToArray();
			}
		}

		ZString IAdsParticipant.ChiefEntryReferences
		{
			get
			{
				return ""; // external/third-party entries omit the chief entry number. By agreement with Navinder at HMRC. 
			}
		}

		readonly ForwardingShipment shipment;
	}
}
