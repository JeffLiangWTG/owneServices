using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers
{
	public class WarehouseStocktakeLineWrapper : WarehouseGenericLineWrapper
	{
		public WarehouseStocktakeLineWrapper(WhsStocktakeLine stocktakeLine, BusinessObjectFactory factoryToWrap)
			: base(stocktakeLine ?? factoryToWrap.GetNull<WhsStocktakeLine>(), factoryToWrap)
		{
		}

		#region Properties

		#region SystemUnits

		protected override ZDecimal SystemUnitsCore
		{
			get { return StocktakeLine.WU_SystemUnits; }
		}

		#endregion

		#region ClientCode

		protected override ZString ClientCodeCore
		{
			get { return (StocktakeLine.Client != null) ? StocktakeLine.Client.OH_Code : ZString.Empty; }
		}

		#endregion

		#region ProductCore

		protected override ProductWrapper ProductCore
		{
			get { return new ProductWrapper(StocktakeLine.SupplierPart, Factory); }
		}

		#endregion

		#region ProductCode

		protected override ZString ProductCodeCore
		{
			get
			{
				var supplierPart = StocktakeLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_PartNum : ZString.Empty;
			}
		}

		#endregion

		#region ProductDescription

		protected override ZString ProductDescriptionCore
		{
			get
			{
				var supplierPart = StocktakeLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Desc : ZString.Empty;
			}
		}

		#endregion

		#region PalletID

		protected override ZString PalletIDCore
		{
			get { return StocktakeLine.WU_PalletID; }
		}

		#endregion

		#region LocationString

		protected override ZString LocationStringCore
		{
			get { return StocktakeLine.LocationString; }
		}

		#endregion

		#region IsEmptyLocation

		protected override ZBool IsEmptyLocationCore
		{
			get { return StocktakeLine.WU_Status == "EMP"; }
		}

		#endregion

		#region PartAttribute1

		protected override ZString PartAttribute1Core
		{
			get { return StocktakeLine.WU_PartAttrib1; }
		}

		#endregion

		#region TrackedSerialNumber

		protected override ZString TrackedSerialNumberCore
		{
			get { return StocktakeLine.WU_SerialNumber; }
		}

		#endregion

		#region PartAttribute1Name

		protected override MultilingualString PartAttribute1NameCore
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;
				if (StocktakeLine.Client != null)
				{
					if (StocktakeLine.Client.MiscServ != null)
					{
						result = StocktakeLine.Client.MiscServ.OM_IMPartAttrib1NameMultilingual;
					}
				}
				return result;
			}
		}

		#endregion

		#region PartAttribute1WithLabel

		protected override MultilingualString PartAttribute1WithLabelCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)ZString.Empty;
				if (StocktakeLine.Client != null)
				{
					if (StocktakeLine.Client.MiscServ != null)
					{
						attributeName = StocktakeLine.Client.MiscServ.OM_IMPartAttrib1NameMultilingual;
					}
				}
				return BuildAttribute(StocktakeLine.WU_PartAttrib1, attributeName, ResString.GetMultilingualString("1811d45d-b30b-4dc1-9e58-91489bab6ec5", "Attribute 1"));
			}
		}

		#endregion

		#region PartAttribute2WithLabel

		protected override MultilingualString PartAttribute2WithLabelCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)ZString.Empty;
				if (StocktakeLine.Client != null)
				{
					if (StocktakeLine.Client.MiscServ != null)
					{
						attributeName = StocktakeLine.Client.MiscServ.OM_IMPartAttrib2NameMultilingual;
					}
				}
				return BuildAttribute(StocktakeLine.WU_PartAttrib2, attributeName, ResString.GetMultilingualString("69cbf66f-74d3-4e9f-926b-ee44e4173a07", "Attribute 2"));
			}
		}

		#endregion

		#region PartAttribute3WithLabel

		protected override MultilingualString PartAttribute3WithLabelCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)ZString.Empty;
				if (StocktakeLine.Client != null)
				{
					if (StocktakeLine.Client.MiscServ != null)
					{
						attributeName = StocktakeLine.Client.MiscServ.OM_IMPartAttrib3NameMultilingual;
					}
				}
				return BuildAttribute(StocktakeLine.WU_PartAttrib3, attributeName, ResString.GetMultilingualString("f93455a0-a403-4a0d-8f2c-576fcd3f139b", "Attribute 3"));
			}
		}

		#endregion

		#region TrackedSerialWithLabel

		protected override ZString TrackedSerialWithLabelCore => BuildAttribute(TrackedSerialNumber, Res.GetString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number"), string.Empty);

		#endregion

		#region ExpiryDateWithLabel

		protected override ZString ExpiryDateWithLabelCore
		{
			get { return BuildAttribute(StocktakeLine.WU_ExpiryDate.ToShortDateString(), Res.GetString("945fdfdd-b1a9-4306-862e-53a3985817db", "Expiry Date"), ""); }
		}

		#endregion

		#region PackingDateWithLabel

		protected override ZString PackingDateWithLabelCore
		{
			get { return BuildAttribute(StocktakeLine.WU_PackingDate.ToShortDateString(), Res.GetString("4ab0f87b-dc1d-4565-a363-adcd88b607f5", "Packing Date"), ""); }
		}

		#endregion

		#region Status

		protected override ZString StatusCore
		{
			get { return StocktakeLine.WU_Status; }
		}

		#endregion

		#region LastCount

		protected override ZDecimal LastCountCore
		{
			get { return StocktakeLine.CurrentCount; }
		}

		#endregion

		#region Variance

		protected override ZDecimal VarianceCore
		{
			get { return (LastCount - StocktakeLine.WU_SystemUnits); }
		}

		#endregion

		#region InventoryStatus

		protected override ZString InventoryStatusCore
		{
			get { return StocktakeLine.WU_InventoryStatus; }
		}

		protected override ZString InventoryStatusDescriptionCore
		{
			get { return Statuses.GetDescriptionFromCode(StocktakeLine.WU_InventoryStatus); }
		}

		InventoryStatus Statuses
		{
			get { return new InventoryStatus(); }
		}

		#endregion

		#region OrgPartRelation

		protected override OrgPartRelation OrgPartRelationCore
		{
			get
			{
				return StocktakeLine.SupplierPart != null && StocktakeLine.Client != null
					? StocktakeLine.SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(StocktakeLine.Client, OrgPartRelation.RelationshipTypes.Owner)
					: null;
			}
		}

		#endregion

		#endregion

		WhsStocktakeLine StocktakeLine => (WhsStocktakeLine)WrappedObject;
	}
}
