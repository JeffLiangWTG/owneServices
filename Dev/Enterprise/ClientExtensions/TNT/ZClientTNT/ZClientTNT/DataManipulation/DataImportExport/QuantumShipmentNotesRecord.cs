using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
//using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.TNT
{
	public class QuantumShipmentNotesRecord : ConsignmentNoteRecord
	{
		public QuantumShipmentNotesRecord(ZString line)
			: base(line)
		{
		}

		#region Field Property Override

		public override ZString HouseBill
		{
			get { return base.HouseBill.Trim(); }
		}

		public override ZString ECN
		{
			get { return base.ECN.Trim(); }
		}

		#endregion

		#region CreateShipmentNoteIfNotExists

		public void CreateShipmentNoteIfNotExists(BusinessObjectFactory factory, CommonShipment shipment)
		{
			if (shipment != null)
			{
				StmNote[] shipmentNotes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);

				if (shipmentNotes.Length == 0)
				{
					((ISupportDataImporting)shipment).IsImportingData = true;
					try
					{
						shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, NoteText);
						if (shipment.OuterPackLines.Count > 0)
						{
							shipment.OuterPackLines[0].JL_Description = NoteText.SubstringSafe(0, 65);
						}
					}
					finally
					{
						((ISupportDataImporting)shipment).IsImportingData = false;
					}
				}

				UpdateGoodsDescriptionFromNote(shipment);
			}
		}

		protected void UpdateGoodsDescriptionFromNote(CommonShipment shipment)
		{
			shipment.JS_GoodsDescription = NoteText.Left(shipment.JS_GoodsDescriptionInfo.MaxLength);
			if (shipment.Declarations.Length > 0)
			{
				BaseJobDeclaration declaration = (BaseJobDeclaration)shipment.Declarations[0];
				declaration.JE_GoodsDescription = NoteText.Left(BaseJobDeclaration.Schema.JE_GoodsDescriptionMaxLength);
			}
		}

		#endregion
		#region Implementation
		#endregion

	}
}
