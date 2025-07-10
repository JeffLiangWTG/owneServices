using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DEPRECMessageBuilder : DepotMessageBuilder
	{
		public DEPRECMessageBuilder(ICMROtherMessageHeader header)
			: base(header.CAN)
		{
			Messages = header.Messages;
			this.DepotEstablishmentID = header.DepotEstablishmentID;
		}

		protected readonly ZString DepotEstablishmentID;

		#region Implementation

		protected override void PopulateLOCs()
		{
			if (!DepotEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.LOC[0], LocationFunctionCodeQualifierList.PlaceOfReceipt, DepotEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			else
			{
				throw new ArgumentException("DEPRECMessageBuilder: Depot is blank.");
			}
		}

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.ShippingNote;

		protected internal override ZString DocumentName => "DEPREC";

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.DEPREC;

		protected internal override Type TypeOfMessage => typeof(CMRDEPRECMessage);

		#endregion
	}
}
