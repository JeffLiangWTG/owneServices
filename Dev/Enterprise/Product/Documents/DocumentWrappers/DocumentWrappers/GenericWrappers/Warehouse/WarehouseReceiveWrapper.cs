using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseReceiveWrapper : WarehouseDocketWrapper, IWhsDocumentInventory
	{
		public WarehouseReceiveWrapper(WhsReceive receive, BusinessObjectFactory factory)
			: base(receive, factory)
		{
		}

		#region Addresses

		#region GoodsBilledToAddress

		protected override AddressWrapper GoodsBillToAddressCore => Receive != null ? goodsBillToAddress ?? (goodsBillToAddress = new AddressWrapper(Receive.GoodsBillToDocAddress, Factory)) : null;
		AddressWrapper goodsBillToAddress;

		#endregion

		#region PickUpAddress

		protected override AddressWrapper PickUpAddressCore => Receive != null ? pickUpAddress ?? (pickUpAddress = new AddressWrapper(Receive.PickUpDocAddress, Factory)) : null;
		AddressWrapper pickUpAddress;

		#endregion

		#region DropOffAddress

		protected override AddressWrapper DropOffAddressCore => Receive != null ? dropOffAddress ?? (dropOffAddress = new AddressWrapper(Receive.DropOffDocAddress, Factory)) : null;
		AddressWrapper dropOffAddress;

		#endregion

		#region SupplierDocAddress

		protected override AddressWrapper SupplierDocAddressCore => Receive != null ? supplierDocAddress ?? (supplierDocAddress = new AddressWrapper(Receive.SupplierDocAddress, Factory)) : null;
		AddressWrapper supplierDocAddress;

		#endregion

		#region TransportCoAddress

		protected override AddressWrapper TransportCoAddressCore => Receive.GetTransportCoAddress(Factory);

		#endregion

		#endregion

		#region Consignor

		public override OrganisationWrapper Consignor
		{
			get
			{
				if (DocketBO != null && consignor == null)
				{
					consignor = new OrganisationWrapper(OrganisationUsageType.Consignor, DocketBO.Supplier, ContactType.Consignor, Factory);
				}
				return consignor;
			}
		}

		OrganisationWrapper consignor;

		#endregion

		#region Consignee

		public override OrganisationWrapper Consignee
		{
			get
			{
				var docketBO = DocketBO;
				if (docketBO != null && consignee == null)
				{
					var warehouse = docketBO.Warehouse;
					if (warehouse != null && warehouse.WarehouseAddress != null)
					{
						var warehouseOrg = Factory.Load<OrgHeader>(warehouse.WarehouseAddress.OA_OH);
						if (warehouseOrg != null)
						{
							consignee = new OrganisationWrapper(OrganisationUsageType.Consignee, warehouseOrg, ContactType.All, Factory);
						}
					}
				}

				return consignee;
			}
		}

		OrganisationWrapper consignee;

		#endregion

		#region Inventory

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description ZString")]
		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			if (MenuTitle.Equals("Putaway Sheet"))
			{
				Receive.Inventory.Sort("CurrentLocationPutawayPathSequence");
			}

			return Receive != null ? new WarehouseInventoryWrapperCollection(Receive.Inventory, Factory) : new WarehouseInventoryWrapperCollection(Factory);
		}

		#endregion

		#region JobCharge

		protected override DocWhsJobChargeCollection NewWarehouseJobChargeLineWrapperCollection() =>
			Receive?.JobHeader is Job receiveJob
			? DocWhsJobChargeCollection.GetCollection(this, nameof(NewWarehouseJobChargeLineWrapperCollection), receiveJob.Charges.Cast<JobCharge>().ToArray())
			: base.NewWarehouseJobChargeLineWrapperCollection();

		#endregion

		#region PalletizedInventory

		protected override WarehouseInventoryWrapperCollection PalletizedInventory_IfInventoryIsNotSelected
		{
			get
			{
				var result = new WarehouseInventoryWrapperCollection(Factory);
				foreach (WhsReceiveLine receiveLine in Receive.Lines)
				{
					if (InventoryRequiresPalletLabel(receiveLine))
					{
						RollUpInventoryByProductAndRelatedFields(result, receiveLine);
					}
				}
				result.Sort("PalletID");
				return result;
			}
		}

		void RollUpInventoryByProductAndRelatedFields(WarehouseInventoryWrapperCollection wrappers, WhsReceiveLine receiveLine)
		{
			foreach (WarehouseInventoryWrapper wrapper in wrappers)
			{
				if (IsProductWithSameAttributes(wrapper, receiveLine))
				{
					MarkAttributeAsRolledUp(wrapper, receiveLine);
					wrapper.IncrementUnitValues(receiveLine);
					return;
				}
			}

			wrappers.Add(new WarehouseInventoryWrapper(receiveLine, receiveLine.Factory));
		}

		void MarkAttributeAsRolledUp(WarehouseInventoryWrapper wrapperWithOriginalInventory, WhsReceiveLine receiveLine)
		{
			wrapperWithOriginalInventory.MarkSerialRolledUp();
		}

		ZBool IsProductWithSameAttributes(WarehouseInventoryWrapper wrapper, WhsReceiveLine receiveLine)
		{
			var inventoryToRollupInto = (WhsReceiveLine)wrapper.WrappedObject;

			return
				inventoryToRollupInto.WE_OP == receiveLine.WE_OP &&
				inventoryToRollupInto.WE_PalletID == receiveLine.WE_PalletID &&
				inventoryToRollupInto.WE_AdjustmentArrivalDate == receiveLine.WE_AdjustmentArrivalDate &&
				inventoryToRollupInto.WE_BondedEntryKey == receiveLine.WE_BondedEntryKey &&
				inventoryToRollupInto.WE_ExpiryDate == receiveLine.WE_ExpiryDate &&
				inventoryToRollupInto.WE_PackingDate == receiveLine.WE_PackingDate &&
				inventoryToRollupInto.WE_WL == receiveLine.WE_WL &&
				IsPartAttribsMatch(wrapper, receiveLine);
		}

		bool IsPartAttribsMatch(WarehouseInventoryWrapper wrapperWithOriginalInventory, WhsReceiveLine receiveLine)
		{
			return
				(wrapperWithOriginalInventory.PartAttribute1 == receiveLine.WE_PartAttrib1) &&
				(wrapperWithOriginalInventory.PartAttribute2 == receiveLine.WE_PartAttrib2) &&
				(wrapperWithOriginalInventory.PartAttribute3 == receiveLine.WE_PartAttrib3);
		}

		#endregion

		#region Implementation

		WhsReceive Receive => receive ?? (receive = (WhsReceive)DocketBO);
		WhsReceive receive;

		#endregion

		#region SecondaryReference

		protected override LabelValuePairWrapper SecondaryReferenceCore => Receive != null ? new LabelValuePairWrapper(Res.GetString("9a62a371-23cb-4775-ab2b-2ebbabab74b7", "Receive Reference"), Receive.WD_ExternalReference, Factory) : LabelValuePairWrapper.Empty;

		#endregion

		#region ConfirmationInstructionsCore

		protected override LabelValuePairWrapper ConfirmationInstructionsCore => confirmationInstructions ?? (confirmationInstructions = newConfirmationInstructions());
		protected LabelValuePairWrapper confirmationInstructions;

		protected LabelValuePairWrapper newConfirmationInstructions()
		{
			ZString value = ZString.Empty;
			StmNote[] notes = Receive.Notes.FindByDescription(PredefinedNoteTypes.Instance.ReceiveConfirmationInstructionsNote.Description);
			if (notes.Length > 0)
			{
				value = notes[0].ST_NoteDataAsText;
			}
			ZString label = new ZString(Res.GetString("adac96c6-6056-401b-a933-e2604165aa91", "Notes:") + " ");
			return new LabelValuePairWrapper(label, value, Factory);
		}

		#endregion

		#region HasOversAndUndersCore

		protected override ZBool HasOversAndUndersCore => Receive.HasOversAndUnders;

		#endregion

		#region IWhsDocumentInventory

		void IWhsDocumentInventory.SetInventory(WhsDocumentInventory documentInventory)
		{
			JobLines.RemoveAll();

			for (int i = 0; i < documentInventory.LabelsToPrint; i++)
			{
				JobLines.Add(new WarehouseInventoryWrapper(documentInventory.Inventory.InDocketLine, Factory));
			}
		}

		#endregion

		#region JobType

		protected override ZString JobTypeCore => Res.GetString("2a3de863-2d54-40ac-abfe-281dc4604f37", "Receive");

		#endregion

		#region JobLinesVariances

		protected override WarehouseGroupedLinesForVarianceWrapperCollection GetJobLinesVariances()
		{
			return Receive != null
				? new WarehouseGroupedLinesForVarianceWrapperCollection(Receive.Lines, Factory)
				: new WarehouseGroupedLinesForVarianceWrapperCollection(Factory);
		}

		#endregion
	}
}
