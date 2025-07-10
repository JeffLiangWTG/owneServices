using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;
using AccGenericChargeCollection = Enterprise.Accounting.Business.GenericCharge.GenericChargeCollection;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderLine : AutoAccPayableOrderLine, IGenericChargeCollectionRequired
	{
		#region Schema

		public new class Schema : AutoAccPayableOrderLine.Schema
		{
			public const string APL_QuantityRemaining = "APL_QuantityRemaining";
			public const string GenericCharge = "GenericCharge";
		}

		#endregion

		public AccPayableOrderLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		[RelatedBusinessObject("Order")]
		public override ZGuid APL_APH
		{
			get { return base.APL_APH; }
			set { base.APL_APH = value; }
		}

		public AccPayableOrderHeader Order
		{
			get
			{
				return Factory.Load<AccPayableOrderHeader>(APL_APH);
			}
		}

		[List("APL_PartNo_List")]
		public override ZString APL_PartNo
		{
			get { return base.APL_PartNo; }
			set
			{
				base.APL_PartNo = value;

				OrgSupplierPart product = this.Product;

				if (!IsCopying && Order != null && product != null)
				{
					if (APL_Desc.IsEmpty)
					{
						APL_Desc = product.OP_Desc;
					}
					APL_InnerPacks = product.OP_OrderMultipleQty;
					APL_OuterPacks = product.OP_VendorPackQty;
					APL_F3_NKPackType = product.OP_StockKeepingUnit;
				}
			}
		}

		public virtual OrgSupplierPartCollection APL_PartNo_List
		{
			get
			{
				OrgSupplierPartCollection result;
				if (Order != null)
				{
					OrgHeader supplier = Order.SupplierDocumentaryAddress != null ? Order.SupplierDocumentaryAddress.Organisation : null;
					OrgHeader owner = Order.Buyer != null ? Order.Buyer.Header : null;
					result = new OrdersOrgSupplierPartCollection(Factory, supplier, owner);
				}
				else
				{
					result = new OrdersOrgSupplierPartCollection(Factory);
				}
				return result;
			}
		}
		/// <summary>
		/// Get the part referenced by APL_PartNo and the order's buyer/supplier.
		/// </summary>
		public OrgSupplierPart Product
		{
			get
			{
				if (Order != null && Order.Buyer != null && Order.SupplierDocumentaryAddress != null)
				{
					return new OrgSupplierPart.Loader(Factory).Load(APL_PartNo, Order.Buyer.OA_OH, Order.SupplierDocumentaryAddress.OrganisationPK);
				}
				else
				{
					return null;
				}
			}
		}

		public int OrderCurrencyDecimals => Order?.OrderCurrencyDecimals ?? Company.GetLocalDecimals();

		[List("APL_Status_List")]
		public override ZString APL_Status
		{
			get { return base.APL_Status; }
			set { base.APL_Status = value; }
		}

		public bool APL_Status_ReadOnly
		{
			get
			{
				return !AccountingConfigurationRegistry.Instance.AllowEditOrderLineStatus.Value;
			}
		}

		[List("APL_F3_NKPackType_List")]
		public override ZString APL_F3_NKPackType
		{
			get { return base.APL_F3_NKPackType; }
			set { base.APL_F3_NKPackType = value; }
		}

		[List("APL_F3_NKPackType_List")]
		public override ZString APL_InnerPacksUQ
		{
			get { return base.APL_InnerPacksUQ; }
			set { base.APL_InnerPacksUQ = value; }
		}

		[List("APL_F3_NKPackType_List")]
		public override ZString APL_OuterPacksUQ
		{
			get { return base.APL_OuterPacksUQ; }
			set { base.APL_OuterPacksUQ = value; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			APL_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Unit;
			APL_Status = Constants.OrderStatus.Open;
			APL_GB = GlbBranch.CurrentBranch.PK;
			APL_GC = GlbCompany.CurrentCompany.PK;
			APL_GE = GlbDepartment.CurrentDepartment.PK;
		}

		public ReadOnlyCodeDescriptionPairList APL_Status_List
		{
			get
			{
				if (fAPL_Status_List == null)
				{
					fAPL_Status_List = AccountingConfigurationRegistry.Instance.APOrderLineStatusCodesList.Value;
				}
				return fAPL_Status_List;
			}
		}
		ReadOnlyCodeDescriptionPairList fAPL_Status_List;

		public CodeDescriptionPairList APL_F3_NKPackType_List
		{
			get
			{
				return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();
			}
		}

		public int QuantityChargedDecimals => 5;
		public int QuantityRateDecimals => 4;
		public int UnitDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForIndividualUnits;

		[DecimalPlaces(nameof(QuantityChargedDecimals))]
		public override ZDecimal APL_Quantity
		{
			get
			{
				return base.APL_Quantity;
			}
			set
			{
				base.APL_Quantity = value;
				if (!IsCopying)
				{
					ZDecimal newLineValue = CalculateLinePrice(APL_ItemPrice, value);
					if (APL_LinePrice != newLineValue)
					{
						APL_LinePrice = newLineValue;
					}
				}

				if (Order != null)
				{
					Order.APH_Calc_TotalQuantityInfo.RefreshBinding();
					Order.APH_Calc_TotalQuantityRemainingInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(nameof(QuantityChargedDecimals))]
		public override ZDecimal APL_QtyReceived
		{
			get { return base.APL_QtyReceived; }
			set
			{
				base.APL_QtyReceived = value;
				if (Order != null)
				{
					Order.APH_Calc_TotalQuantityReceivedInfo.RefreshBinding();
					Order.APH_Calc_TotalQuantityRemainingInfo.RefreshBinding();
					Order.MarkAsNeedingValidation();
				}
				APL_QuantityRemainingInfo.RefreshBinding();
			}
		}

		#region APL_QuantityRemaining

		[DecimalPlaces(nameof(QuantityChargedDecimals))]
		public ZDecimal APL_QuantityRemaining
		{
			get
			{
				ZDecimal valueToBeBasedOn = AccountingConfigurationRegistry.Instance.APOrderLineQtyRemainingManagement.Value ? APL_QtyReceived : APL_QtyInvoiced;
				ZDecimal result = APL_Quantity - valueToBeBasedOn;
				return result < 0 ? 0 : result;
			}
		}

		public ZPropertyInfo APL_QuantityRemainingInfo
		{
			get { return GetZPropertyInfo(Schema.APL_QuantityRemaining); }
		}

		#endregion

		[DecimalPlaces(nameof(UnitDecimals))]
		public override ZDecimal APL_InnerPacks
		{
			get { return base.APL_InnerPacks; }
			set
			{
				base.APL_InnerPacks = value;

				if (Order != null)
				{
					Order.APH_Calc_InnerPacksInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(nameof(UnitDecimals))]
		public override ZDecimal APL_OuterPacks
		{
			get { return base.APL_OuterPacks; }
			set
			{
				base.APL_OuterPacks = value;

				if (Order != null)
				{
					Order.APH_Calc_OuterPacksInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(nameof(QuantityChargedDecimals))]
		public override ZDecimal APL_QtyInvoiced
		{
			get { return base.APL_QtyInvoiced; }
			set
			{
				base.APL_QtyInvoiced = value;
				if (AccountingConfigurationRegistry.Instance.APOrderLineQtyRemainingManagement.Value)
				{
					APL_QtyReceived = value;
				}

				if (Order != null)
				{
					Order.APH_Calc_TotalQuantityInvoicedInfo.RefreshBinding();
					Order.APH_Calc_TotalQuantityRemainingInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(nameof(QuantityRateDecimals))]
		public override ZDecimal APL_ItemPrice
		{
			get { return base.APL_ItemPrice; }
			set
			{
				base.APL_ItemPrice = value;
				if (!IsCopying)
				{
					ZDecimal newLineValue = CalculateLinePrice(value, APL_Quantity);
					if (APL_LinePrice != newLineValue)
					{
						APL_LinePrice = newLineValue;
					}
				}
			}
		}

		ZDecimal CalculateLinePrice(ZDecimal itemPrice, ZDecimal quantity)
		{
			return Utilities.Round(itemPrice * quantity, OrderCurrencyDecimals);
		}

		[DecimalPlaces(nameof(OrderCurrencyDecimals))]
		public override ZDecimal APL_LinePrice
		{
			get { return base.APL_LinePrice; }
			set
			{
				base.APL_LinePrice = value;
				if (!IsCopying && APL_Quantity > 0)
				{
					ZDecimal newItemValue = CalculateItemPrice(value, APL_Quantity);
					if (APL_ItemPrice != newItemValue)
					{
						APL_ItemPrice = newItemValue;
					}
				}
			}
		}

		[DecimalPlaces(nameof(QuantityRateDecimals))]
		public ZDecimal APL_InvoicedPrice
		{
			get
			{
				return APL_ItemPrice * APL_QtyInvoiced;
			}
		}

		ZDecimal CalculateItemPrice(ZDecimal linePrice, ZDecimal quantity)
		{
			return Utilities.Round(linePrice / quantity, QuantityRateDecimals);
		}

		#region GenericCharge

		[RelatedBusinessObject("GenericChargeBizO")]
		[List("ChargeList")]
		public virtual ZGuid GenericCharge
		{
			get
			{
				if (IsInDatabase && fGenericCharge.IsEmpty)
				{
					AccGenericCharge result = GenericChargeBizO;
					fGenericCharge = (result != null) ? result.PK : ZGuid.Empty;
				}
				return fGenericCharge;
			}
			set
			{
				if (value != fGenericCharge)
				{
					ZGuid chargeGuid = value;
					ResetGenericChargeDependantValues();

					if (chargeGuid.IsValid)
					{
						LoadGenericCharge(chargeGuid);
						if (GenericTransactionCharge != null)
						{
							APL_Desc = GenericTransactionCharge.VC_Description;
							if (GenericTransactionCharge.VC_IsGLAccount)
							{
								APL_AG = GenericTransactionCharge.PK;
							}
							else
							{
								APL_AC = GenericTransactionCharge.PK;
							}
						}
					}
					else
					{
						SetNonPersistentPropertyValue(GenericChargeInfo, ref fGenericCharge, ZGuid.Empty);
						AccPayableOrderLineValidation invoicingLineValidation = Validation;
						{
							invoicingLineValidation.ValidateGenericCharge();
						}
						GenericTransactionCharge = null;
					}
				}
				SetNonPersistentPropertyValue(GenericChargeInfo, ref fGenericCharge, value);
				GenericChargeInfo.RefreshBinding();
			}
		}

		ZGuid fGenericCharge;

		public ZPropertyInfo GenericChargeInfo
		{
			get { return GetZPropertyInfo(Schema.GenericCharge); }
		}

		public AccGenericCharge GenericChargeBizO
		{
			get { return Factory.LoadTop1<AccGenericCharge>(new ZQuery(ViewGenericChargeSchema.PK, (APL_AC.IsValid ? APL_AC : APL_AG))); }
		}

		void LoadGenericCharge(ZGuid chargePK)
		{
			ZQuery filter = new ZQuery(ViewGenericChargeSchema.PK, chargePK);
			GenericTransactionCharge = Factory.LoadTop1<AccGenericCharge>(filter);
		}

		public AccGenericCharge GenericTransactionCharge
		{
			get { return fGenericTransactionCharge; }
			set { fGenericTransactionCharge = value; }
		}

		AccGenericCharge fGenericTransactionCharge;

		void ResetGenericChargeDependantValues()
		{
			APL_AC = ZGuid.Empty;
			APL_AG = ZGuid.Empty;
		}

		public AccGenericChargeCollection ChargeList
		{
			get
			{
				return Factory.GetCachedValue("AccPayableOrderLine" + FindboxLookupCollections.CachingKey + GetGenericChargeCollectionBuilder().GetType().Name
					+ this.KeyForCollectionCachingInFactory(),
					() =>
					{
						var chargeCollectionBuilder = GetGenericChargeCollectionBuilder();
						chargeCollectionBuilder.SetJobInfo(this);
						return !IsInDatabase ? chargeCollectionBuilder.GetBuiltButNotLoadedCollection() : new AccGenericChargeCollection(Factory);
					});
			}
		}

		GenericChargeCollectionBuilder GetGenericChargeCollectionBuilder()
		{
			return new APGenericChargeCollectionBuilder(this);
		}

		bool IGenericChargeCollectionRequired.IsJobRelated
		{
			get
			{
				return false;
			}
		}

		GlbDepartment IGenericChargeCollectionRequired.Department
		{
			get
			{
				return Department;
			}
		}

		BusinessObjectFactory IGenericChargeCollectionRequired.Factory
		{
			get
			{
				return Factory;
			}
		}

		#endregion

		public GlbDepartmentCollection DepartmentCollection
		{
			get { return FindboxLookupCollections.GetDepartmentCollection_ActiveOnly(Factory); }
		}
	}
}

