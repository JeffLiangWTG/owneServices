using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public interface IPickingSlipLineWrapper
	{
		ZDecimal Units { get; set; }
	}

	public interface IPickingSlipLineWrapperCollection
	{
		void AddPickLine(IPickingSlipLineWrapper line);
	}

	public abstract class PickingLineWrapperCollectionHelperBase
	{
		public PickingLineWrapperCollectionHelperBase(WhsPick pick, BusinessObjectFactory factory, bool isPickByBiggestEnabled)
		{
			this.pick = pick;
			this.Factory = factory;
			this.isPickByBiggestEnabled = isPickByBiggestEnabled;
		}
		readonly protected WhsPick pick;
		readonly protected BusinessObjectFactory Factory;
		readonly protected bool isPickByBiggestEnabled;

		public IPickingSlipLineWrapperCollection PickingLines
		{
			get
			{
				var allPickLines = pick.GetAllPickLines().Where(l => l.WZ_Units > 0).ToArray();
				WhsPickByBOMHelper.AddFetchHintsForIsPickByBOMKitPickLine(Factory, allPickLines);
				var picklines = allPickLines.Where(l => !l.IsPickByBOMKitPickLine()).ToArray();
				Array.Sort(picklines, new SortPickLinesForPickingSlip());
				return RollupPickLineCollectionIntoDocWrapperCollection(picklines);
			}
		}

		protected abstract IPickingSlipLineWrapperCollection GetRolledUpLines(WhsPickLine[] pickLines);
		protected abstract IPickingSlipLineWrapper GetDocPickLine(WhsPickLine pickLine);
		protected abstract IPickingSlipLineWrapperCollection SplitItems(IPickingSlipLineWrapperCollection rolledUpLines);

		IPickingSlipLineWrapperCollection RollupPickLineCollectionIntoDocWrapperCollection(WhsPickLine[] pickLines)
		{
			var rolledUpLines = GetRolledUpLines(pickLines);
			var docPickLinesDictionary = new Dictionary<string, IPickingSlipLineWrapper>();

			foreach (var pickLine in pickLines)
			{
				var key = GetDictionaryKey(pickLine);

				if (docPickLinesDictionary.TryGetValue(key, out var docPickLine))
				{
					docPickLine.Units += pickLine.WZ_Units;
				}
				else if (pickLine.WZ_Units != 0.0m)
				{
					docPickLine = GetDocPickLine(pickLine);
					rolledUpLines.AddPickLine(docPickLine);
					docPickLinesDictionary.Add(key, docPickLine);
				}
			}

			return SplitItems(rolledUpLines);
		}

		string GetDictionaryKey(WhsPickLine pickLine)
		{
			string result;
			var separator = "^"; // use it in situations when there is impossible to say when one value end and another start.
			result = GetNewImplementationDictionaryKey(pickLine, separator);

			return result;
		}

		string GetNewImplementationDictionaryKey(WhsPickLine pickLine, string separator)
		{
			var result = "";
			var inventoryLine = pickLine.InventoryLineForAvailableInventory;

			var docket = inventoryLine?.Docket;
			result += docket?.WD_OH_Client ?? ZGuid.Empty;
			result += inventoryLine.WE_OP;
			result += inventoryLine.WE_WL;
			result += inventoryLine.WE_PalletID;

			var part = inventoryLine.SupplierPart;
			var client = docket?.Client;
			if (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(part))
			{
				result += GetOrderedAttributesDictionaryKey(pickLine, separator);
			}
			else
			{
				result += inventoryLine.WE_PartAttrib1 + separator;
				result += inventoryLine.WE_PartAttrib2 + separator;
				result += inventoryLine.WE_PartAttrib3 + separator;
				result += inventoryLine.WE_SerialNumber + separator;
				result += inventoryLine.WE_PackingDate + separator;
				result += inventoryLine.WE_ExpiryDate + separator;
			}

			result += pickLine.DocketLine.WE_PickGroup;

			return result;
		}

		string GetOrderedAttributesDictionaryKey(WhsPickLine pickLine, string separator)
		{
			var result = "";
			var docketLine = pickLine.DocketLine;
			result += docketLine.WE_PartAttrib1 + separator;
			result += docketLine.WE_PartAttrib2 + separator;
			result += docketLine.WE_PartAttrib3 + separator;
			result += docketLine.WE_SerialNumber + separator;
			result += docketLine.WE_ExpiryDate + separator;
			result += docketLine.WE_PackingDate + separator;
			return result;
		}
	}
}
