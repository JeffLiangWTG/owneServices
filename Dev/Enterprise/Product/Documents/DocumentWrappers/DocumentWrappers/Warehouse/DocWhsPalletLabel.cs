using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPalletLabel : DocWhsLabel
	{
		#region Constructors

		DocWhsPalletLabel(WhsLabel label, BusinessObjectFactory factoryToWrap)
			: base(label, factoryToWrap)
		{
		}

		DocWhsPalletLabel(WhsLabel label, DocWhsInventory line1, DocWhsInventory line2, BusinessObjectFactory factoryToWrap)
			: base(label, factoryToWrap)
		{
			InventoryLine1 = line1;
			InventoryLine2 = line2;
		}

		#endregion

		#region New

		public new static DocWhsPalletLabel New(WhsLabel label, BusinessObjectFactory factoryToWrap)
		{
			return (label != null ? new DocWhsPalletLabel(label, factoryToWrap) : null);
		}

		public static DocWhsPalletLabel New(WhsLabel label, DocWhsInventory line1, DocWhsInventory line2, BusinessObjectFactory factoryToWrap)
		{
			return (label != null ? new DocWhsPalletLabel(label, line1, line2, factoryToWrap) : null);
		}

		#endregion

		#region Related Business Objects

		public DocWhsInventory InventoryLine1
		{
			get
			{
				return inventoryLine1;
			}
			set
			{
				inventoryLine1 = value;
			}
		}
		DocWhsInventory inventoryLine1;

		public DocWhsInventory InventoryLine2
		{
			get
			{
				return inventoryLine2;
			}
			set
			{
				inventoryLine2 = value;
			}
		}
		DocWhsInventory inventoryLine2;

		#endregion

		#region Properties

		#region ZString  Fields

		public ZString TotalWeight
		{
			get
			{
				ZString weightUQ = ZString.Empty;
				ZDecimal weight = ZDecimal.Zero;

				var inventory1 = (InventoryLine1 != null) ? (WhsDocketLine)InventoryLine1.WrappedObject : null;
				var inventory2 = (InventoryLine2 != null) ? (WhsDocketLine)InventoryLine2.WrappedObject : null;

				if (inventory1 != null && inventory1.Docket != null)
				{
					OrgSupplierPart part1 = inventory1.SupplierPart;
					weightUQ = inventory1.Docket.WD_TotalWeightUnit;
					weight += ZArchitecture.Core.Utilities.Round(part1.UnitConverter.Convert(part1.OP_Weight, part1.OP_WeightUQ, inventory1.Docket.WD_TotalWeightUnit) * InventoryLine1.GroupedReceiveUnits, 2);
				}
				if (inventory2 != null && inventory2.Docket != null)
				{
					OrgSupplierPart part2 = inventory2.SupplierPart;
					if (weightUQ.IsEmpty)
					{
						weightUQ = inventory2.Docket.WD_TotalWeightUnit;
					}

					weight += ZArchitecture.Core.Utilities.Round(part2.UnitConverter.Convert(part2.OP_Weight, part2.OP_WeightUQ, inventory2.Docket.WD_TotalWeightUnit) * InventoryLine2.GroupedReceiveUnits, 2);
				}
				return new ZString(weight.ToString() + " " + weightUQ);
			}
		}

		public ZString UnitsUQ1
		{
			get { return inventoryLine1 != null ? inventoryLine1.UnitsUQ : ZString.Empty; }
		}

		public ZString UnitsUQ2
		{
			get { return inventoryLine2 != null ? inventoryLine2.UnitsUQ : ZString.Empty; }
		}

		public ZString ReceivedQty1
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine1 != null)
				{
					if (((WhsDocketLine)InventoryLine1.WrappedObject).SupplierPart != null)
					{
						result = ZArchitecture.Core.Utilities.Round(InventoryLine1.GroupedReceiveUnits, ((WhsDocketLine)InventoryLine1.WrappedObject).SupplierPart.OP_CountDecimalPlaces).ToString();
					}
					else
					{
						result = InventoryLine1.GroupedReceiveUnits.ToString();
					}
				}
				return result;
			}
		}

		public ZString InventoryQty1
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine1 != null)
				{
					if (((WhsDocketLine)InventoryLine1.WrappedObject).SupplierPart != null)
					{
						result = ZArchitecture.Core.Utilities.Round(InventoryLine1.GroupedInventoryUnits, ((WhsDocketLine)InventoryLine1.WrappedObject).SupplierPart.OP_CountDecimalPlaces).ToString();
					}
					else
					{
						result = InventoryLine1.GroupedInventoryUnits.ToString();
					}
				}
				return result;
			}
		}

		public ZString ReceivedQty2
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine2 != null)
				{
					if (((WhsDocketLine)InventoryLine2.WrappedObject).SupplierPart != null)
					{
						result = ZArchitecture.Core.Utilities.Round(InventoryLine2.GroupedReceiveUnits, ((WhsDocketLine)InventoryLine2.WrappedObject).SupplierPart.OP_CountDecimalPlaces).ToString();
					}
					else
					{
						result = InventoryLine2.GroupedReceiveUnits.ToString();
					}
				}
				return result;
			}
		}

		public ZString InventoryQty2
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine2 != null)
				{
					if (((WhsDocketLine)InventoryLine2.WrappedObject).SupplierPart != null)
					{
						result = ZArchitecture.Core.Utilities.Round(InventoryLine2.GroupedInventoryUnits, ((WhsDocketLine)InventoryLine2.WrappedObject).SupplierPart.OP_CountDecimalPlaces).ToString();
					}
					else
					{
						result = InventoryLine2.GroupedInventoryUnits.ToString();
					}
				}
				return result;
			}
		}

		public ZString LeftOverAttributes1
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine1 != null)
				{
					if (!InventoryLine1.PartAttribute1.IsEmpty)
					{
						result = InventoryLine1.PartAttribute1Name + ": " + InventoryLine1.PartAttribute1;
					}

					if (!InventoryLine1.PartAttribute2.IsEmpty)
					{
						if (result.IsEmpty)
						{
							result = InventoryLine1.PartAttribute2Name + ": " + InventoryLine1.PartAttribute2;
						}
						else
						{
							result += ", " + InventoryLine1.PartAttribute2Name + ": " + InventoryLine1.PartAttribute2;
						}
					}
					if (!InventoryLine1.PartAttribute3.IsEmpty)
					{
						if (result.IsEmpty)
						{
							result = InventoryLine1.PartAttribute3Name + ": " + InventoryLine1.PartAttribute3;
						}
						else
						{
							result += ", " + InventoryLine1.PartAttribute3Name + ": " + InventoryLine1.PartAttribute3;
						}
					}
				}
				return result;
			}
		}

		public ZString LeftOverAttributes2
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine2 != null)
				{
					if (!InventoryLine2.PartAttribute1.IsEmpty)
					{
						result = InventoryLine2.PartAttribute1Name + ": " + InventoryLine2.PartAttribute1;
					}

					if (!InventoryLine2.PartAttribute2.IsEmpty)
					{
						if (result.IsEmpty)
						{
							result = InventoryLine2.PartAttribute2Name + ": " + InventoryLine2.PartAttribute2;
						}
						else
						{
							result += ", " + InventoryLine2.PartAttribute2Name + ": " + InventoryLine2.PartAttribute2;
						}
					}
					if (!InventoryLine2.PartAttribute3.IsEmpty)
					{
						if (result.IsEmpty)
						{
							result = InventoryLine2.PartAttribute3Name + ": " + InventoryLine2.PartAttribute3;
						}
						else
						{
							result += ", " + InventoryLine2.PartAttribute3Name + ": " + InventoryLine2.PartAttribute3;
						}
					}
				}
				return result;
			}
		}

		public ZString FirstDate1
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine1 != null)
				{
					if (!InventoryLine1.ExpiryDate.IsEmpty)
					{
						result = Res.GetString("362ecdb2-5e88-4bff-8d39-7ba294b548aa", "Expiry Date: {0}", ExpiryDate1.ToShortDateString());
					}
					else
					{
						if (!InventoryLine1.PackingDate.IsEmpty)
						{
							result = Res.GetString("e734d8bb-7c02-453e-a35f-feb5c8b0d1ff", "Packing Date: {0}", InventoryLine1.PackingDate.ToShortDateString());
						}
					}
				}
				return result;
			}
		}

		public ZString FirstDate2
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine2 != null)
				{
					if (!InventoryLine2.ExpiryDate.IsEmpty)
					{
						result = Res.GetString("362ecdb2-5e88-4bff-8d39-7ba294b548aa", "Expiry Date: {0}", ExpiryDate2.ToShortDateString());
					}
					else
					{
						if (!InventoryLine2.PackingDate.IsEmpty)
						{
							result = Res.GetString("e734d8bb-7c02-453e-a35f-feb5c8b0d1ff", "Packing Date: {0}", InventoryLine2.PackingDate.ToShortDateString());
						}
					}
				}
				return result;
			}
		}

		public ZString SecondDate1
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine1 != null)
				{
					if (!InventoryLine1.ExpiryDate.IsEmpty && !InventoryLine1.PackingDate.IsEmpty)
					{
						result = Res.GetString("1c1e9ce8-9544-42e8-bff4-4d48d79fa618", "Packing Date:");
					}
				}
				return result;
			}
		}

		public ZString SecondDate2
		{
			get
			{
				ZString result = ZString.Empty;
				if (InventoryLine2 != null)
				{
					if (!InventoryLine2.ExpiryDate.IsEmpty && !InventoryLine2.PackingDate.IsEmpty)
					{
						result = Res.GetString("1c1e9ce8-9544-42e8-bff4-4d48d79fa618", "Packing Date:");
					}
				}
				return result;
			}
		}

		public ZString LocationString
		{
			get
			{
				var result = ZString.Empty;
				if (InventoryLine1 != null && InventoryLine1.LocationString != ".................")
				{
					result = InventoryLine1.LocationString;
				}

				return result;
			}
		}

		#endregion

		#region ZDateTime  Fields

		public ZDateTime ExpiryDate1
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (InventoryLine1 != null)
				{
					result = InventoryLine1.ExpiryDate;
					if (result.IsEmpty)
					{
						result = InventoryLine1.PackingDate;
					}
				}
				return result;
			}
		}

		public ZDateTime ExpiryDate2
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (InventoryLine2 != null)
				{
					result = InventoryLine2.ExpiryDate;
					if (result.IsEmpty)
					{
						result = InventoryLine2.PackingDate;
					}
				}
				return result;
			}
		}

		public ZDateTime PackingDateTemporary1
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (InventoryLine1 != null)
				{
					if (!InventoryLine1.ExpiryDate.IsEmpty)
					{
						result = InventoryLine1.PackingDate;
					}
				}
				return result;
			}
		}

		public ZDateTime PackingDateTemporary2
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (InventoryLine2 != null)
				{
					if (!InventoryLine2.ExpiryDate.IsEmpty)
					{
						result = InventoryLine2.PackingDate;
					}
				}
				return result;
			}
		}

		#endregion

		#endregion
	}
}
