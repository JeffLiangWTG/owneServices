using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WARRELMessageBuilder : WarehouseMessageBuilder
	{
		public WARRELMessageBuilder(JobDeclaration declaration)
			: this(declaration.DeclarationNumber, declaration.JE_EstimatedDeliveryOrPickup, declaration.DepotOrCTOID, declaration.WarehouseID, GetWarehouseItems(declaration))
		{
			Messages = declaration.Messages;
		}

		public WARRELMessageBuilder(CusEntryHeader entryHeader)
			: this(entryHeader.Declaration.DeclarationNumber, entryHeader.Declaration.JE_EstimatedDeliveryOrPickup, entryHeader.Declaration.DepotOrCTOID, entryHeader.Declaration.WarehouseID, GetWarehouseItems(entryHeader))
		{
			Messages = entryHeader.Messages;
		}

		#region Implementation

		protected WARRELMessageBuilder(ZString eDN, ZDateTime releaseDateTime, ZString destinationEstablishmentID, ZString warehouseEstablishmentID, WarehouseItemWrapper[] items)
			: base(eDN, items)
		{
			this.releaseDateTime = releaseDateTime;
			this.destinationEstablishmentID = destinationEstablishmentID;
			this.warehouseEstablishmentID = warehouseEstablishmentID;
		}

		protected internal override ZString DocumentName => "WARREL";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.RegistrationDocument;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.WARREL;

		protected internal override Type TypeOfMessage => typeof(CMRWARRELMessage);

		protected override void PopulateDTM()
		{
			if (releaseDateTime.IsValid)
			{
				MessageUtilities.PopulateDTM(CUSCAR.DTM[0], DateTimePeriodFunctionCodeQualifierList.ReleaseDateCustoms, releaseDateTime.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
				MessageUtilities.PopulateDTM(CUSCAR.DTM[1], DateTimePeriodFunctionCodeQualifierList.ReleaseDateCustoms, releaseDateTime.ToString("HHmm"), DateTimePeriodFormatCodeList.Hhmm);
			}
			else
			{
				throw new ArgumentException("WARRELMessageBuilder: Release Date is " + (releaseDateTime.IsEmpty ? "empty." : "invalid."));
			}
		}

		protected override void PopulateLOCs()
		{
			if (!destinationEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.LOC[0], LocationFunctionCodeQualifierList.GoodsReceiptPlace, destinationEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			else
			{
				throw new ArgumentException("WARRELMessageBuilder: Destination Depot is blank.");
			}
			if (!warehouseEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.LOC[1], LocationFunctionCodeQualifierList.PlaceOfDeparture, warehouseEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			else
			{
				throw new ArgumentException("WARRELMessageBuilder: Warehouse is blank.");
			}
		}

		protected ZDateTime releaseDateTime;
		protected ZString destinationEstablishmentID;
		protected ZString warehouseEstablishmentID;
		#endregion
	}
}
