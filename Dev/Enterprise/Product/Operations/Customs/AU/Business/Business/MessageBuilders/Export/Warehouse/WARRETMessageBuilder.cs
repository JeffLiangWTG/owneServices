using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WARRETMessageBuilder : WarehouseMessageBuilder
	{
		public WARRETMessageBuilder(JobDeclaration declaration)
			: this(declaration.DeclarationNumber, declaration.WarehouseID, GetWarehouseItems(declaration))
		{
			Messages = declaration.Messages;
		}

		public WARRETMessageBuilder(CusEntryHeader entryHeader)
			: this(entryHeader.Declaration.DeclarationNumber, entryHeader.Declaration.WarehouseID, GetWarehouseItems(entryHeader))
		{
			Messages = entryHeader.Messages;
		}

		#region Implementation

		protected WARRETMessageBuilder(ZString eDN, ZString warehouseEstablishmentID, WarehouseItemWrapper[] items)
			: base(eDN, items)
		{
			this.warehouseEstablishmentID = warehouseEstablishmentID;
		}

		protected internal override ZString DocumentName => "WARRET";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.Restow;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.WARRET;

		protected internal override Type TypeOfMessage => typeof(CMRWARRETMessage);

		protected override void PopulateDTM()
		{
		}

		protected override void PopulateLOCs()
		{
			if (!warehouseEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.LOC[0], LocationFunctionCodeQualifierList.PlaceOfReceipt, warehouseEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			else
			{
				throw new ArgumentException("WARRETMessageBuilder: Warehouse is blank.");
			}
		}

		protected ZString warehouseEstablishmentID;

		#endregion
	}
}
