using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	/// Generic Warehouse Docket Line Wrapper
	/// </summary>
	/// <remarks>
	/// THIS CLASS IS A STANDARD INTERFACE FOR DOCBUILDER DOCUMENTS ONLY AND COMMON TO ALL WAREHOUSE DOCKETS DOWN TO WHSORDER.
	/// </remarks>
	public abstract class WarehouseDocketLineWrapper : WarehouseGenericLineWrapper
	{
		public WarehouseDocketLineWrapper(BusinessObject whsLineBO, BusinessObjectFactory factory)
			: base(whsLineBO, factory)
		{
		}

		#region Properties

		#region ArrivalDateCore

		protected override ZDateTime ArrivalDateCore
		{
			get { return (DocketLineBO != null) ? DocketLineBO.WE_AdjustmentArrivalDate.ToZDateTime() : ZDateTime.Empty; }
		}

		#endregion

		#region Customs Fields

		protected override ZString CustomsEntryKeyCore
		{
			get
			{
				ZString result = CustomsEntryNo + " / " + CustomsEntryLineNo;
				if (result == " / 0")
				{
					result = "";
				}

				return result;
			}
		}

		protected override ZString CustomsEntryNoCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_EntryKey : ZString.Empty; }
		}

		protected override ZShort CustomsEntryLineNoCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_EntryLineNo : (ZShort)0; }
		}

		protected override ZString CustomsTariffItemCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsTariffItem : ZString.Empty; }
		}

		protected override ZDateTime CustomsEntryDateCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_EntryDate : ZDateTime.Empty; }
		}

		protected override ZDecimal CustomsQuantityCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_CustomsQty : ZDecimal.Zero; }
		}

		protected override ZString CustomsQuantityUQCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_CustomsUnitOfQty : ZString.Empty; }
		}

		protected override ZDecimal CustomsVFDCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_ValueForDuty : ZDecimal.Zero; }
		}

		protected override ZDecimal CustomsTILVCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_TILV : ZDecimal.Zero; }
		}

		protected override ZString CustomsCtryOfOriginCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_RN_NKCountryOfOrigin : ZString.Empty; }
		}

		protected override ZString CustomsAddInfoCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_AddInfo : ZString.Empty; }
		}

		protected override ZDecimal CustomsSecondQuantityCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_CustomsSecondQuantity : ZDecimal.Zero; }
		}

		protected override ZString CustomsSecondUnitQtyCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_CustomsSecondUnitQty : ZString.Empty; }
		}

		protected override ZString TariffCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_Tariff : ZString.Empty; }
		}

		protected override ZString PrimaryPreferenceCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_PrimaryPreference : ZString.Empty; }
		}

		protected override ZDecimal CustomsThirdQuantityCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_CustomsThirdQuantity : ZDecimal.Zero; }
		}

		protected override ZString CustomsThirdUnitQtyCore
		{
			get { return DocketLineBO != null ? DocketLineBO.CustomsData.WB_CustomsThirdUnitQty : ZString.Empty; }
		}

		protected override AddressWrapper ManufacturerAddressCore
		{
			get { return DocketLineBO != null && DocketLineBO.CustomsData.ManufacturerAddress != null ? new AddressWrapper(DocketLineBO.CustomsData.ManufacturerAddress, ContactType.Warehouse, Factory) : base.ManufacturerAddressCore; }
		}

		#endregion

		#region LineCommentCore

		protected override ZString LineCommentCore
		{
			get { return (DocketLineBO != null) ? DocketLineBO.WE_LineComment : ZString.Empty; }
		}

		#endregion

		#region LineNoCore

		protected override ZShort LineNoCore => DocketLineBO?.WE_LineNo ?? 0;

		#endregion

		#region LocationStringCore

		protected override ZString LocationStringCore
		{
			get { return (DocketLineBO != null) ? DocketLineBO.LocationString : ZString.Empty; }
		}

		#endregion

		#region LocationString2Core

		protected override ZString LocationString2Core
		{
			get { return ""; }
		}

		#endregion

		#region PacksCore

		protected override ZDecimal PacksCore
		{
			get { return (DocketLineBO != null) ? DocketLineBO.WE_PackQuantity : ZDecimal.Zero; }
		}

		#endregion

		#region PacksUQCore

		protected override ZString PacksUQCore
		{
			get { return (DocketLineBO != null) ? DocketLineBO.WE_F3_NKPackType : ZString.Empty; }
		}

		#endregion

		#region PartAttributes

		#region PartAttribute1Core

		protected override ZString PartAttribute1Core
		{
			get { return DocketLineBO != null ? DocketLineBO.WE_PartAttrib1 : ZString.Empty; }
		}

		#endregion

		#region PartAttribute2Core

		protected override ZString PartAttribute2Core
		{
			get { return DocketLineBO != null ? DocketLineBO.WE_PartAttrib2 : ZString.Empty; }
		}

		#endregion

		#region PartAttribute3Core

		protected override ZString PartAttribute3Core
		{
			get { return DocketLineBO != null ? DocketLineBO.WE_PartAttrib3 : ZString.Empty; }
		}

		#endregion

		#region TrackedSerialNumberCore

		protected override ZString TrackedSerialNumberCore
			=> DocketLineBO?.WE_SerialNumber ?? ZString.Empty;

		#endregion

		#region PackingDateCore

		protected override ZDateTime PackingDateCore
		{
			get { return DocketLineBO != null ? DocketLineBO.WE_PackingDate : ZDateTime.Empty; }
		}

		#endregion

		#region ExpiryDateCore

		protected override ZDateTime ExpiryDateCore
		{
			get { return DocketLineBO != null ? DocketLineBO.WE_ExpiryDate : ZDateTime.Empty; }
		}

		#endregion

		#region PartAttribute1WithLabelCore

		protected override MultilingualString PartAttribute1WithLabelCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";

				var client = this.Client;
				if (client != null)
				{
					attributeName = client.MiscServ.OM_IMPartAttrib1NameMultilingual;
				}
				return BuildAttribute(PartAttribute1, attributeName, ResString.GetMultilingualString("39ed8c65-29b2-427a-80fb-e0c7a6d704f4", "Attribute 1"));
			}
		}

		#endregion

		#region PartAttribute2WithLabelCore

		protected override MultilingualString PartAttribute2WithLabelCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";

				var client = this.Client;
				if (client != null)
				{
					attributeName = client.MiscServ.OM_IMPartAttrib2NameMultilingual;
				}
				return BuildAttribute(PartAttribute2, attributeName, ResString.GetMultilingualString("852a4f4d-b382-46a0-9131-dcf2a38744e1", "Attribute 2"));
			}
		}

		#endregion

		#region PartAttribute3WithLabelCore

		protected override MultilingualString PartAttribute3WithLabelCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";

				var client = this.Client;
				if (client != null)
				{
					attributeName = client.MiscServ.OM_IMPartAttrib3NameMultilingual;
				}
				return BuildAttribute(PartAttribute3, attributeName, ResString.GetMultilingualString("9f01a9fc-7427-4768-806f-1ee5c14b7e43", "Attribute 3"));
			}
		}

		#endregion

		#region TrackedSerialNumberCore

		protected override ZString TrackedSerialWithLabelCore
			=> BuildAttribute(TrackedSerialNumber, Res.GetString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number"), "");

		#endregion

		#region PackingDateWithLabelCore

		protected override ZString PackingDateWithLabelCore
		{
			get { return BuildAttribute(PackingDate.ToShortDateString(), Res.GetString("92053308-3c69-4c66-828f-95583171a8a9", "Packing Date"), ""); }
		}

		#endregion

		#region ExpiryDateWithLabelCore

		protected override ZString ExpiryDateWithLabelCore
		{
			get { return BuildAttribute(ExpiryDate.ToShortDateString(), Res.GetString("3aa475f0-b058-4a1d-ade4-4024a0fe887d", "Expiry Date"), ""); }
		}

		#endregion

		#region PartAttribute1NameCore

		protected override MultilingualString PartAttribute1NameCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";

				var client = this.Client;
				if (client != null && client.MiscServ != null)
				{
					attributeName = client.MiscServ.OM_IMPartAttrib1NameMultilingual;
				}
				return attributeName;
			}
		}

		#endregion

		#region PartAttribute2NameCore

		protected override MultilingualString PartAttribute2NameCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";

				var client = this.Client;
				if (client != null && client.MiscServ != null)
				{
					attributeName = client.MiscServ.OM_IMPartAttrib2NameMultilingual;
				}
				return attributeName;
			}
		}

		#endregion

		#region PartAttribute3NameCore

		protected override MultilingualString PartAttribute3NameCore
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";

				var client = this.Client;
				if (client != null && client.MiscServ != null)
				{
					attributeName = client.MiscServ.OM_IMPartAttrib3NameMultilingual;
				}
				return attributeName;
			}
		}

		#endregion

		//PartAttribute2NameCore
		//PartAttribute3NameCore

		//ExpiryDateNameCore
		//PackingDateNameCore

		#endregion

		#region OrgPartRelation

		protected override OrgPartRelation OrgPartRelationCore
		{
			get
			{
				return DocketLineBO.SupplierPart != null && Client != null
					? DocketLineBO.SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(Client, OrgPartRelation.RelationshipTypes.Owner)
					: null;
			}
		}

		#endregion

		#region ProductCodeCore

		protected override ZString ProductCodeCore
		{
			get
			{
				OrgSupplierPart part = (DocketLineBO != null) ? DocketLineBO.SupplierPart : null;
				return part != null ? part.OP_PartNum : ZString.Empty;
			}
		}

		#endregion

		#region ProductDescriptionCore

		protected override ZString ProductDescriptionCore
		{
			get
			{
				OrgSupplierPart part = (DocketLineBO != null) ? DocketLineBO.SupplierPart : null;
				return part != null ? part.OP_Desc : ZString.Empty;
			}
		}

		#endregion

		#region UnitsCore

		protected override ZDecimal UnitsCore
		{
			get { return DocketLineBO != null ? DocketLineBO.WE_TransactionQuantity : ZDecimal.Zero; }
			set { }
		}

		#endregion

		#region UnitsUQCore

		protected override ZString UnitsUQCore
		{
			get { return DocketLineBO != null ? DocketLineBO.ProductUQ : ZString.Empty; }
		}

		#endregion

		#region Product

		protected override ProductWrapper ProductCore
		{
			get { return new ProductWrapper(SupplierPart, Factory); }
		}

		protected OrgSupplierPart SupplierPart
		{
			get { return DocketLineBO != null ? DocketLineBO.SupplierPart : null; }
		}

		#endregion

		#region HoldCode

		protected override ZString HoldCodeCore => DocketLineBO?.WE_WHC_NKCurrentInventoryHeldCode ?? ZString.Empty;

		#endregion

		#region HoldReason

		protected override ZString HoldReasonCore => DocketLineBO?.WE_CurrentHoldReason ?? ZString.Empty;

		#endregion

		#endregion

		#region Implementation

		#region Bill Of Materials (BOM) and Work Order

		internal override ZBool IsWorkOrder
		{
			get { return (DocketBO is WhsWorkOrder); }
		}

		public override ZInt BOMLevel
		{
			get { return IsWorkOrder ? ((WhsWorkOrderLine)DocketLineBO).WE_Level.ToZInt() : ZInt.Zero; }
		}

		#endregion

		protected virtual OrgHeader Client
		{
			get
			{
				WhsDocket docket = DocketBO;
				return docket != null ? docket.Client : null;
			}
		}

		protected virtual WhsDocket DocketBO
		{
			get { return DocketLineBO != null ? DocketLineBO.Docket : null; }
		}

		protected virtual WhsDocketLine DocketLineBO
		{
			get { return (WhsDocketLine)WrappedBO; }
		}

		#endregion
	}
}
