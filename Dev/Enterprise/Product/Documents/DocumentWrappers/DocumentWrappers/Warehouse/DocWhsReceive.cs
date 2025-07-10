using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsReceive : DocWhsDocket
	{
		#region Static

		public static DocWhsReceive New(WhsReceive whsReceive, BusinessObjectFactory factoryToWrap)
		{
			return (whsReceive == null) ? null : new DocWhsReceive(whsReceive, factoryToWrap);
		}

		public static DocWhsReceive New(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
		{
			return (docketLabel == null) ? null : ((docketLabel.Docket == null) ? null : new DocWhsReceive(docketLabel, factoryToWrap));
		}

		#endregion

		#region Contructors

		DocWhsReceive(WhsReceive whsReceive, BusinessObjectFactory factoryToWrap)
			: base(whsReceive, factoryToWrap)
		{
		}

		DocWhsReceive(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
			: base(docketLabel, factoryToWrap)
		{
		}

		#endregion

		#region Related Business Objects

		protected override DocOrganisation TransportCoCore => WhsReceive.GetTransportCompanyLegacy(Factory);

		protected override DocDocAddress TransportCoAddressCore => WhsReceive.GetTransportCoAddressLegacy(Factory);

		WhsReceive WhsReceive
		{
			get { return (WhsReceive)WrappedObject; }
		}

		#endregion

		#region Business Objects Overrides

		protected override DocWhsDocketLineCollection GetDocketLines()
		{
			return new DocWhsReceiveLineCollection(WhsReceive.Lines, Factory);
		}

		#endregion

		#region ZString Fields

		protected override ZString CarrierNameCore => WhsReceive.GetCarrierName();

		public ZString InventoryDateWithLabel
		{
			get { return Res.GetString("7cb7c7c8-5ea4-431f-82fb-53217431cf0d", "(As at: {0})", ZDateTime.Today.ToShortDateString()); }
		}

		public ZString ArrivalDateWithLabel
		{
			get { return WhsReceive.WD_ArrivalDate.IsEmpty ? "" : Res.GetString("b7c0007d-6c9a-4ab5-b027-620fce2b7318", "(Arrived: {0})", WhsReceive.WD_ArrivalDate.ToShortDateString()); }
		}

		public ZString ConfirmationInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				StmNote[] notes = WhsReceive.Notes.FindByDescription(PredefinedNoteTypes.Instance.ReceiveConfirmationInstructionsNote.Description);
				if (notes.Length > 0)
				{
					result = notes[0].ST_NoteDataAsText;
				}
				return result;
			}
		}

		public ZString ConfirmationInstructionsLabel
		{
			get { return this.ConfirmationInstructions.IsEmpty ? ZString.Empty : new ZString(Res.GetString("adac96c6-6056-401b-a933-e2604165aa91", "Notes:") + " "); }
		}

		#endregion

		#region Collections

		public DocWhsPalletIDLabelCollection PalletIDLabels
		{
			get
			{
				var result = new DocWhsPalletIDLabelCollection(Factory);

				if (TotalNumberOfLabels > 0)
				{
					var ids = PalletIDGenerator.GenerateIDs(WhsReceive, TotalNumberOfLabels, shouldPrompt: false, 1).ToArray();
					for (var i = 0; i < TotalNumberOfLabels; i++)
					{
						var label = new WhsLabel();
						label.Number = i + 1;
						var docLabel = DocWhsPalletIDLabel.New(label, Factory);
						docLabel.PalletID = (i < ids.Length) ? ids[i] : ZString.Empty;
						if (WhsReceive.WD_ArrivalDate.IsEmpty)
						{
							if (WhsReceive.WD_ETA.IsEmpty)
							{
								docLabel.PrintDate = ZDateTime.Now;
							}
							else
							{
								docLabel.PrintDate = WhsReceive.WD_ETA.ToZDateTime();
							}
						}
						else
						{
							docLabel.PrintDate = WhsReceive.WD_ArrivalDate.ToZDateTime();
						}
						result.Add(docLabel);
					}
				}

				return result;
			}
		}

		public DocWhsInventoryCollection Inventory
		{
			get
			{
				return new DocWhsInventoryCollection(WhsReceive.Lines.Cast<WhsDocketLine>().ToArray(), Factory);
			}
		}

		IPalletIDGenerator PalletIDGenerator
		{
			get => palletIDGenerator ??= ObjectFactory.Get<IPalletIDGenerator>();
		}

		IPalletIDGenerator palletIDGenerator;

		#region PalletizedInventory

		public DocWhsPalletLabelCollection PalletizedInventory
		{
			get
			{
				return (WhsReceive.InventoryToPrintPalletLabelFor != null) ? PalletizedInventory_ForSelectedReceiveLines : PalletizedInventory_ForAllReceiveLines;
			}
		}

		DocWhsPalletLabelCollection PalletizedInventory_ForSelectedReceiveLines
		{
			get
			{
				var result = new DocWhsPalletLabelCollection(Factory);

				if (InventoryRequiresPalletLabel(WhsReceive.InventoryToPrintPalletLabelFor.InDocketLine))
				{
					var palletIDInventory = new PalletIDInventory(WhsReceive.InventoryToPrintPalletLabelFor.InDocketLine.WE_PalletID, WhsReceive.InventoryToPrintPalletLabelFor.InDocketLine);
					var label = new WhsLabel(palletIDInventory.PalletID);
					result.Add(DocWhsPalletLabel.New(label, palletIDInventory.GroupedInventoryCollection[0], null, Factory));
				}
				return result;
			}
		}

		DocWhsPalletLabelCollection PalletizedInventory_ForAllReceiveLines
		{
			get
			{
				var result = new DocWhsPalletLabelCollection(Factory);

				foreach (PalletIDInventory palletNumber in GroupedPalletsInventory())
				{
					for (int i = 0; i < palletNumber.GroupedInventoryCollection.Count; i += 2)
					{
						var inventoryLine = (WhsDocketLine)palletNumber.GroupedInventoryCollection[i].WrappedObject;
						var label = new WhsLabel(palletNumber.PalletID);
						if (i < palletNumber.GroupedInventoryCollection.Count - 1)
						{
							result.Add(DocWhsPalletLabel.New(label, palletNumber.GroupedInventoryCollection[i], palletNumber.GroupedInventoryCollection[i + 1], Factory));
						}
						else
						{
							result.Add(DocWhsPalletLabel.New(label, palletNumber.GroupedInventoryCollection[i], null, Factory));
						}
					}
				}
				return result;
			}
		}

		List<PalletIDInventory> GroupedPalletsInventory()
		{
			var list = new List<PalletIDInventory>();
			var sortedInventoryCollection = new DocWhsInventoryCollection(WhsReceive.Lines.Cast<WhsDocketLine>().ToArray(), Factory);

			foreach (DocWhsInventory docInventoryLine in sortedInventoryCollection)
			{
				var docketLine = (WhsDocketLine)docInventoryLine.WrappedObject;
				if (InventoryRequiresPalletLabel(docketLine))
				{
					if (!list.Exists(delegate(PalletIDInventory palletNumber) { return (palletNumber.PalletID == docketLine.WE_PalletID); }))
					{
						list.Add(new PalletIDInventory(docketLine.WE_PalletID, docketLine));
					}
					else
					{
						list.Find(delegate(PalletIDInventory palletNumber)
						{ return (palletNumber.PalletID == docketLine.WE_PalletID); }).AddInventoryLine(docketLine);
					}
				}
			}
			return list;
		}

		ZBool InventoryRequiresPalletLabel(WhsDocketLine docketLine)
		{
			return docketLine != null && !docketLine.WE_PalletID.IsEmpty && (docketLine.WE_TransactionQuantity > 0 || docketLine.WE_StockOnHand > 0);
		}

		#endregion

		#endregion

		#region Implementation

		protected class PalletIDInventory
		{
			public readonly ZString PalletID;

			public DocWhsInventoryCollection GroupedInventoryCollection
			{
				get { return groupedInventoryCollection; }
			}
			DocWhsInventoryCollection groupedInventoryCollection;

			public PalletIDInventory(ZString palletNumber, WhsDocketLine docketLine)
			{
				PalletID = palletNumber;
				AddInventoryLine(docketLine);
			}

			public void AddInventoryLine(WhsDocketLine docketLine)
			{
				if (groupedInventoryCollection == null)
				{
					groupedInventoryCollection = new DocWhsInventoryCollection(docketLine.Factory);
				}

				bool addInventoryLine = true;
				foreach (DocWhsInventory inventory in groupedInventoryCollection)
				{
					if (IsProductWithSameAttributes((WhsDocketLine)inventory.WrappedObject, docketLine))
					{
						inventory.GroupedReceiveUnits += docketLine.WE_TransactionQuantity;
						inventory.GroupedInventoryUnits += docketLine.WE_StockOnHand;
						addInventoryLine = false;
						break;
					}
				}
				if (addInventoryLine)
				{
					DocWhsInventory docWhsInventory = DocWhsInventory.New(docketLine, docketLine.Factory);
					docWhsInventory.GroupedReceiveUnits += docketLine.WE_TransactionQuantity;
					docWhsInventory.GroupedInventoryUnits += docketLine.WE_StockOnHand;
					groupedInventoryCollection.Add(docWhsInventory);
				}
			}

			ZBool IsProductWithSameAttributes(WhsDocketLine originalDocketLine, WhsDocketLine docketLine)
			{
				return
					originalDocketLine.WE_OP == docketLine.WE_OP &&
					originalDocketLine.WE_AdjustmentArrivalDate == docketLine.WE_AdjustmentArrivalDate &&
					originalDocketLine.WE_BondedEntryKey == docketLine.WE_BondedEntryKey &&
					originalDocketLine.WE_ExpiryDate == docketLine.WE_ExpiryDate &&
					originalDocketLine.WE_PackingDate == docketLine.WE_PackingDate &&
					originalDocketLine.WE_PartAttrib1 == docketLine.WE_PartAttrib1 &&
					originalDocketLine.WE_PartAttrib2 == docketLine.WE_PartAttrib2 &&
					originalDocketLine.WE_PartAttrib3 == docketLine.WE_PartAttrib3 &&
					originalDocketLine.WE_SerialNumber == docketLine.WE_SerialNumber &&
					originalDocketLine.WE_WL == docketLine.WE_WL;
			}
		}

		#endregion
	}
}
