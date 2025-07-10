using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DEPRELMessageBuilder : DepotMessageBuilder
	{
		public DEPRELMessageBuilder(ICMROtherMessageHeader header)
			: base(header.CAN)
		{
			Messages = header.Messages;
			this.DepotEstablishmentID = header.DepotEstablishmentID;
			this.DestinationEstablishmentID = header.DestinationEstablishmentID;
		}

		protected readonly ZString DestinationEstablishmentID;
		protected readonly ZString DepotEstablishmentID;

		#region Implementation

		protected override void PopulateLOCs()
		{
			if (!DestinationEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.GoodsReceiptPlace, DestinationEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			else
			{
				throw new ArgumentException("DEPRELMessageBuilder: Destination is blank.");
			}

			if (!DepotEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PlaceOfDeparture, DepotEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			else
			{
				throw new ArgumentException("DEPRELMessageBuilder: Depot is blank.");
			}
		}

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.TransportCargoReleaseOrder;

		protected internal override ZString DocumentName => "DEPREL";

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.DEPREL;

		protected internal override Type TypeOfMessage => typeof(CMRDEPRELMessage);

		#endregion
	}
}
