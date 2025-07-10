using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Registry;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.PayableOrder
{
	[CodeProperty(Schema.APH_OrderNumberAndSplit)]
	[DescriptionProperty(Schema.APH_GoodsDescription)]
	public class AccPayableOrderHeader : AutoAccPayableOrderHeader, IWorkflowProvider, IDocumentSupportable, IDocAddresses, IDocManagerSupport, IEDocsParsingSupport
	{
		public new class Schema : AutoAccPayableOrderHeader.Schema
		{
			public const string APH_OrderNumberAndSplit = "APH_OrderNumberAndSplit";
			public const string APH_Calc_LineCount = "APH_Calc_LineCount";
			public const string APH_Calc_InnerPacks = "APH_Calc_InnerPacks";
			public const string APH_Calc_OuterPacks = "APH_Calc_OuterPacks";
			public const string APH_Calc_TotalQuantity = "APH_Calc_TotalQuantity";
			public const string APH_Calc_TotalLinePrice = "APH_Calc_TotalLinePrice";
			public const string APH_Calc_TotalQuantityInvoiced = "APH_Calc_TotalQuantityInvoiced";
			public const string APH_Calc_TotalQuantityReceived = "APH_Calc_TotalQuantityReceived";
			public const string APH_Calc_TotalQuantityRemaining = "APH_Calc_TotalQuantityRemaining";
			public const string APH_Calc_TotalInvoicedPrice = "APH_Calc_TotalInvoicedPrice";
			public const string APH_OrderCurrencyCode = "APH_OrderCurrencyCode";
		}

		public AccPayableOrderHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fPreviousAPH_Stage = APH_Stage;
			fPreviousAPH_Disposition = APH_Disposition;

			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(APH_AH), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(APH_InvoiceNumber), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(APH_InvoiceDate), ConcurrencyPolicy.Strict);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (IsCancelled)
			{
				UpdateReadOnlyForWhenCancelled();
			}

			if (APH_Disposition == Constants.PayableOrderDisposition.Complete || APH_Disposition == Constants.PayableOrderDisposition.PendingGoodsReceivedAudit)
			{
				SetLinesReadOnly();
				SetSupplierReadOnly();
			}
		}

		ZString fPreviousAPH_Stage;
		ZString fPreviousAPH_Disposition;
		ZGuid APInvoicePK;

		public override bool ReadOnly
		{
			get
			{
				return APH_Disposition == Constants.PayableOrderDisposition.Complete
					|| IsCancelled;
			}
			set
			{
				base.ReadOnly = value;
			}
		}

		public override ZBool APH_IsActive
		{
			get
			{
				return base.APH_IsActive;
			}
			set
			{
				if (APH_IsActive != value)
				{
					base.APH_IsActive = value;
					UpdateReadOnlyForWhenCancelled();
				}
			}
		}

		void UpdateReadOnlyForWhenCancelled()
		{
			if (!IsDeleted)
			{
				SetReadOnlyIncludingChildren(!APH_IsActive);
			}
		}

		public override void RegisterEditableChildObject(IBusiness child)
		{
			base.RegisterEditableChildObject(child);
			if (!IsDeleted && !APH_IsActive)
			{
				child.IncrementReadOnlyIncludingChildren();
			}
		}

		public override string CanCancel()
		{
			return ReasonForNotAbleToDelete;
		}

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return !IsPosted;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString reason = (NoResString)string.Empty;
				if (IsPosted)
				{
					reason = ResString.GetMultilingualString("52c771e4-8f7c-4d67-b8c2-83995524fc60", "Purchase order {0} cannot be Deactivated due to the following reason:\r\n* Posted Payable Invoice Number {1} has not been reversed.", APH_OrderNumber, TransactionHeader.AH_TransactionNum);
				}
				return reason;
			}
		}

		#endregion

		[ChildEditable(true)]
		public AccPayableOrderLineCollection OrderLines
		{
			get
			{
				if (orderLines == null)
				{
					orderLines = GetOrderLinesCore();
					RegisterEditableChildObject(orderLines);
					OrderLines.CountChanged += new EventHandler(OnOrderLines_CountChanged);
				}
				return orderLines;
			}
		}
		protected AccPayableOrderLineCollection orderLines;

		protected virtual AccPayableOrderLineCollection GetOrderLinesCore()
		{
			return new AccPayableOrderLineCollection(this);
		}

		public AccPayableOrderLinesTotalByProductCollection ProductQuantitySummary
		{
			get
			{
				if (this.productQuantitySummary == null)
				{
					this.productQuantitySummary = new AccPayableOrderLinesTotalByProductCollection(this, Factory);
					this.productQuantitySummary.Populate();
				}
				return this.productQuantitySummary;
			}
		}
		AccPayableOrderLinesTotalByProductCollection productQuantitySummary;

		#region APH_Stage

		[List("APH_Stage_List")]
		public override ZString APH_Stage
		{
			get { return base.APH_Stage; }
			set
			{
				base.APH_Stage = value;
			}
		}

		protected bool APH_Stage_ReadOnly
		{
			get { return !AccountingConfigurationRegistry.Instance.EnableStageAndDispositionOverride.Value; }
		}

		public CodeDescriptionPairList APH_Stage_List
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.PayableOrderStage);
			}
		}

		#endregion

		#region APH_Disposition

		[List("APH_Disposition_List")]
		public override ZString APH_Disposition
		{
			get { return base.APH_Disposition; }
			set
			{
				bool hasChanges = APH_Disposition != value;
				if (hasChanges)
				{
					base.APH_Disposition = value;
					switch (value)
					{
						case Constants.PayableOrderDisposition.OrderIncomplete:
						case Constants.PayableOrderDisposition.PendingApproval:
							APH_Stage = Constants.PayableOrderStage.Request;
							break;
						case Constants.PayableOrderDisposition.OrderToBePlaced:
						case Constants.PayableOrderDisposition.PendingConfirmation:
							APH_Stage = Constants.PayableOrderStage.Order;
							break;
						case Constants.PayableOrderDisposition.ExpectedDLVPending:
						case Constants.PayableOrderDisposition.DeliveryInProgress:
							APH_Stage = Constants.PayableOrderStage.Track;
							break;
						case Constants.PayableOrderDisposition.APInvoiceToBePosted:
						case Constants.PayableOrderDisposition.PendingGoodsReceivedAudit:
						case Constants.PayableOrderDisposition.Complete:
							APH_Stage = Constants.PayableOrderStage.Receive;
							break;
						default:
							break;
					}
				}
				APH_DispositionInfo.RefreshBinding();
				if (APH_Disposition == Constants.PayableOrderDisposition.Complete)
				{
					APH_OA_BuyerInfo.RefreshBinding();
				}

				if (OnDispositionChanged != null)
				{
					OnDispositionChanged();
				}
			}
		}

		public delegate void OnDispositionChangedHandler();
		public event OnDispositionChangedHandler OnDispositionChanged;

		protected bool APH_Disposition_ReadOnly
		{
			get { return !AccountingConfigurationRegistry.Instance.EnableStageAndDispositionOverride.Value; }
		}

		public CodeDescriptionPairList APH_Disposition_List
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.PayableOrderDisposition);
			}
		}

		#endregion 

		#region APH_Type

		[List("APH_Type_List")]
		public override ZString APH_Type
		{
			get { return base.APH_Type; }
			set
			{
				base.APH_Type = value;
			}
		}

		public CodeDescriptionPairList APH_Type_List
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.PayableOrderType);
			}
		}

		#endregion

		#region APH_GoodsReceivedStatus

		[List("APH_GoodsReceivedStatus_List")]
		public override ZString APH_GoodsReceivedStatus
		{
			get { return base.APH_GoodsReceivedStatus; }
			set
			{
				base.APH_GoodsReceivedStatus = value;
			}
		}

		public ReadOnlyCodeDescriptionPairList APH_GoodsReceivedStatus_List
		{
			get
			{
				if (fAPH_GoodsReceivedStatus_List == null)
				{
					fAPH_GoodsReceivedStatus_List = AccountingConfigurationRegistry.Instance.GoodsReceivedStatusCodesList.Value;
				}
				return fAPH_GoodsReceivedStatus_List;
			}
		}
		ReadOnlyCodeDescriptionPairList fAPH_GoodsReceivedStatus_List;

		#endregion

		#region APH_Calc_Currency

		public ZExchangeRate APH_Calc_Currency
		{
			get
			{
				if (fAPH_Calc_Currency == null)
				{
					fAPH_Calc_Currency = new ZExchangeRate(this, ZArchitecture.Core.ExchangeRateType.Buy, APH_EstimatedExchangeRateInfo, (ZPropertyInfoString)APH_RX_NKOrderCurrencyInfo);
					//fAPH_Calc_Currency.IsCurrencyRequired = true;
					fAPH_Calc_Currency.IsRateRequired = false;
				}
				return fAPH_Calc_Currency;
			}
			set { fAPH_Calc_Currency = value; }
		}
		ZExchangeRate fAPH_Calc_Currency;

		#endregion

		#region ReadOnly

		void SetLinesReadOnly()
		{
			if (this.APH_AH != ZGuid.Empty || IsCancelled)
			{
				this.OrderLines.SetReadOnlyIncludingChildren(true);
			}
		}

		void SetSupplierReadOnly()
		{
			if (this.APH_AH != ZGuid.Empty || IsCancelled)
			{
				this.SupplierDocumentaryAddress.SetReadOnlyIncludingChildren(true);
			}
		}

		public bool APH_GoodsDescription_ReadOnly
		{
			get { return APH_AH != ZGuid.Empty; }
		}

		public bool APH_GSTInclusive_ReadOnly
		{
			get { return APH_AH != ZGuid.Empty; }
		}

		public bool APH_DueDate_ReadOnly
		{
			get { return APH_AH != ZGuid.Empty; }
		}

		public bool APH_RX_NKOrderCurrency_ReadOnly
		{
			get { return APH_AH != ZGuid.Empty; }
		}

		public bool APH_InvoiceDate_ReadOnly
		{
			get { return APH_AH != ZGuid.Empty; }
		}

		public bool APH_InvoiceNumber_ReadOnly
		{
			get { return APH_AH != ZGuid.Empty; }
		}

		#endregion

		public override ZString APH_InvoiceNumber
		{
			get
			{
				return base.APH_InvoiceNumber;
			}
			set
			{
				base.APH_InvoiceNumber = value;

				if (!IsCopying && !APH_InvoiceNumberInfo.HasErrors())
				{
					if (!APH_InvoiceNumber.IsEmpty && APH_InvoiceDate.IsEmpty)
					{
						APH_InvoiceDate = new ZDate(Env.Time.CurrentLocalDate);
					}
				}
			}
		}

		public override ZDate APH_InvoiceDate
		{
			get { return base.APH_InvoiceDate; }
			set
			{
				base.APH_InvoiceDate = value;
				if (APH_InvoiceDate.IsValid && APH_DueDate.IsEmpty && Supplier != null && Supplier.CompanyData != null)
				{
					APH_DueDate = new InvoiceAndDueDateCalculator(null, APH_InvoiceDate, Supplier).DueDate.Date;
				}
			}
		}

		public override ZString APH_BookingConfRef
		{
			get
			{
				return base.APH_BookingConfRef;
			}
			set
			{
				base.APH_BookingConfRef = value;

				if (!IsCopying && !APH_BookingConfRefInfo.HasErrors())
				{
					if (!APH_BookingConfRef.IsEmpty && APH_BookingConfDate.IsEmpty)
					{
						APH_BookingConfDate = new ZDate(Env.Time.CurrentLocalDate);
					}
				}
			}
		}

		public override ZDate APH_BookingConfDate
		{
			get
			{
				return base.APH_BookingConfDate;
			}
			set
			{
				if (base.APH_BookingConfDate != value)
				{
					base.APH_BookingConfDate = value;
					Logs.CreateRecreateOrUpdateEventLog(Events.BookingConfirmed, EstimateActual.Actual, value.ToZDateTime().ToOffset());
					if (value.IsValid)
					{
						APH_Disposition = Core.Constants.PayableOrderDisposition.ExpectedDLVPending;
					}
				}
			}
		}

		public ZDateTime APInvoicePostedDate
		{
			get
			{
				return TransactionHeader != null ? TransactionHeader.AH_PostDate : ZDateTime.Empty;
			}
		}

		public ZDateTime APH_DeactivatedDate
		{
			get
			{
				StmALog log = Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(s => s.SL_SE_NKEvent == Events.SetToInactiveCode);
				return log != null ? log.SL_EventTime : ZDateTime.Empty;
			}
		}

		public override ZDate APH_ReadyForDelivery
		{
			get
			{
				return base.APH_ReadyForDelivery;
			}
			set
			{
				if (base.APH_ReadyForDelivery != value)
				{
					base.APH_ReadyForDelivery = value;
					Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Estimate, value.ToZDateTime().ToOffset());
				}
			}
		}

		public override ZDate APH_ExpectedDelivery
		{
			get
			{
				return base.APH_ExpectedDelivery;
			}
			set
			{
				if (base.APH_ExpectedDelivery != value)
				{
					base.APH_ExpectedDelivery = value;
					Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Estimate, value.ToZDateTime().ToOffset());
					if (value.IsValid && !IsPosted && !IsAudited
						&& APH_Calc_TotalQuantityRemaining != 0m)
					{
						APH_Disposition = Core.Constants.PayableOrderDisposition.DeliveryInProgress;
					}
				}
			}
		}

		#region Convert ZDateTime to ZDate value which from LogEvent

		[EventDateProperty(AutoEvents.BookingConfirmedCode, EstimateActual.Actual)]
		public ZDateTime BookingConfDateConvert
		{
			get
			{
				return APH_BookingConfDate;
			}
			set
			{
				if (APH_Disposition != Constants.PayableOrderDisposition.Complete)
				{
					APH_BookingConfDate = value.Date;
					BookingConfDateConvertInfo.RefreshBinding();
				}
			}
		}

		public ZWrappedPropertyInfo BookingConfDateConvertInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(BookingConfDateConvert), x => APH_BookingConfDateInfo); }
		}

		[EventDateProperty(AutoEvents.DepartureCode, EstimateActual.Estimate)]
		public ZDateTime ReadyForDeliveryConvert
		{
			get
			{
				return APH_ReadyForDelivery;
			}
			set
			{
				if (APH_Disposition != Constants.PayableOrderDisposition.Complete)
				{
					APH_ReadyForDelivery = value.Date;
					ReadyForDeliveryConvertInfo.RefreshBinding();
				}
			}
		}

		public ZWrappedPropertyInfo ReadyForDeliveryConvertInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ReadyForDeliveryConvert), x => APH_ReadyForDeliveryInfo); }
		}

		[EventDateProperty(AutoEvents.ArrivalCode, EstimateActual.Estimate)]
		public ZDateTime ExpectedDeliveryConvert
		{
			get
			{
				return APH_ExpectedDelivery;
			}
			set
			{
				if (APH_Disposition != Constants.PayableOrderDisposition.Complete)
				{
					APH_ExpectedDelivery = value.Date;
					ExpectedDeliveryConvertInfo.RefreshBinding();
				}
			}
		}

		public ZWrappedPropertyInfo ExpectedDeliveryConvertInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ExpectedDeliveryConvert), x => APH_ExpectedDeliveryInfo); }
		}

		#endregion

		public OrgHeader OrderedBy
		{
			get
			{
				if (APH_OA_Buyer_ZAddress != null)
				{
					return APH_OA_Buyer_ZAddress.OrgHeader as OrgHeader;
				}
				else
				{
					return null;
				}
			}
		}

		public OrgHeader Supplier
		{
			get
			{
				if (SupplierDocumentaryAddress != null)
				{
					return SupplierDocumentaryAddress.Organisation;
				}
				else
				{
					return null;
				}
			}
		}

		[List("Lookups.OrderedBy_OrgList")]
		public override ZGuid APH_OA_Buyer
		{
			get { return base.APH_OA_Buyer; }
			set
			{
				base.APH_OA_Buyer = value;
			}
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public ZAddressWithContact OrderedByAddressWithContact
		{
			get
			{
				if (fOrderedByAddressWithContact == null)
				{
					fOrderedByAddressWithContact = GetOrderedByAddressWithContact();
				}
				return fOrderedByAddressWithContact;
			}
		}
		ZAddressWithContact fOrderedByAddressWithContact;

		ZAddressWithContact GetOrderedByAddressWithContact()
		{
			var result = new ZAddressWithContact(APH_OC_BuyerContactInfo, APH_OA_BuyerInfo);
			result.GetDefaultAddress = GetDefaultAddress;
			return result;
		}

		protected ZGuid GetDefaultAddress(IOrgHeader orderedBy)
		{
			var result = ZGuid.Empty;
			if (orderedBy != null)
			{
				var address = ((OrgHeader)orderedBy).MainAddress;
				result = address != null ? address.PK : ZGuid.Empty;
			}
			return result;
		}

		protected override ZAddress GetNewAPH_OA_Buyer_ZAddress()
		{
			ZAddress result = base.GetNewAPH_OA_Buyer_ZAddress();

			// use lookup logic to find a suitable org.
			if (result.OrgHeader == null)
			{
				OrgHeaderCollection orderedByOrgs = Lookups.OrderedBy_OrgList;
				orderedByOrgs.Load();
				if (orderedByOrgs.Count > 0)
				{
					result.OrgPK = orderedByOrgs[0].PK;
					if (orderedByOrgs[0].MainAddress != null)
					{
						result.AddressFK = orderedByOrgs[0].MainAddress.PK;
					}
				}
			}

			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = GetDefaultAddressByHeader;
			return result;
		}

		protected ZGuid GetDefaultAddressByHeader(IOrgHeader org)
		{
			OrgAddress result = null;
			OrgHeader header = org as OrgHeader;
			if (header != null)
			{
				result = header.Addresses.DefaultAddressOfType(OrgAddressType.Postal)
					?? header.Addresses.DefaultAddressOfType(OrgAddressType.Office);
			}
			return result == null ? ZGuid.Empty : result.PK;
		}

		public virtual JobDocAddress SupplierDocumentaryAddress
		{
			get
			{
				if (fSupplierDocumentaryAddress == null || fSupplierDocumentaryAddress.IsDeleted)
				{
					UnRegisterListChangedCalledRefreshBinding(fSupplierDocumentaryAddress);
					fSupplierDocumentaryAddress = GetNewSupplierDocumentaryAddress();
					AddEventHandlersToSupplierDocumentaryAddressEvents();
					RegisterListChangedCalledRefreshBinding(fSupplierDocumentaryAddress);
				}
				return fSupplierDocumentaryAddress;
			}
		}

		JobDocAddress fSupplierDocumentaryAddress;

		protected virtual JobDocAddress AddEventHandlersToSupplierDocumentaryAddressEvents()
		{
			//fSupplierDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += new EventHandler(ConsigneeDocumentaryAddressChanged);
			//fSupplierDocumentaryAddress.DocAddressChanged += new EventHandler(ConsigneeDocumentaryAddress_DocAddressChanged);
			return fSupplierDocumentaryAddress;
		}

protected virtual JobDocAddress GetNewSupplierDocumentaryAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(SupplierDocAddressRequirement);
		}

		public JobDocAddressRequirement SupplierDocAddressRequirement
		{
			get
			{
				if (fSupplierDocAddressRequirement == null)
				{
					fSupplierDocAddressRequirement = GetSupplierDocAddressRequirement();
				}

				return fSupplierDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fSupplierDocAddressRequirement;

		protected virtual JobDocAddressRequirement GetSupplierDocAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.SupplierDocumentaryAddress, AddressType.OFC);
			AddSupplierLinkedRequirement(requirement);
			requirement.ValidateOrganisationPK = Validation.ValidateSupplierPK;
			return requirement;
		}

		void AddSupplierLinkedRequirement(JobDocAddressRequirement requirement)
		{
			if (!Globals.IsWeb)
			{
				requirement.AddLinkedRequirement(SupplierPickupDeliveryAddressRequirement);
			}
		}

		public JobDocAddressRequirement SupplierPickupDeliveryAddressRequirement
		{
			get
			{
				if (fSupplierPickupDeliveryAddressRequirement == null)
				{
					fSupplierPickupDeliveryAddressRequirement = GetSupplierPickupDeliveryAddressRequirement();
				}
				return fSupplierPickupDeliveryAddressRequirement;
			}
		}
		JobDocAddressRequirement fSupplierPickupDeliveryAddressRequirement;

		JobDocAddressRequirement GetSupplierPickupDeliveryAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.SupplierPickupDeliveryAddress, AddressType.DLV, ContactType.LocalTransport);
			AddSupplierDeliveryAddressLinkedRequirement(requirement);
			return requirement;
		}

		void AddSupplierDeliveryAddressLinkedRequirement(JobDocAddressRequirement requirement)
		{
			if (Globals.IsWeb)
			{
				requirement.AddLinkedRequirement(SupplierDocAddressRequirement);
			}
		}

		[ChildEditable(true)]
		public virtual JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}

		JobDocAddressDependentCollection fDocAddresses;

		public int LocalDecimals => Company.GetLocalDecimals();
		public int ExchangeRateDecimals => Company.ExchangeRateDecimalPlaces;
		public int OrderCurrencyDecimals => OrderCurrency?.Decimals ?? LocalDecimals;

		#region APH_EstimatedExchangeRate

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal APH_EstimatedExchangeRate
		{
			get => base.APH_EstimatedExchangeRate;
			set => base.APH_EstimatedExchangeRate = value;
		}

		#endregion

		#region APH_OrderCurrencyCode

		public ZString APH_OrderCurrencyCode
		{
			get { return APH_RX_NKOrderCurrency; }
		}

		public ZPropertyInfo APH_OrderCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.APH_OrderCurrencyCode); }
		}

		#endregion

		#region APH_Calc_TotalAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal APH_Calc_TotalAmount
		{
			get
			{
				return OrderLines.Sum(x => Utilities.Round(x.APL_LinePrice, OrderCurrencyDecimals));
			}
		}

		#endregion

		#region APH_Calc_CreatedUser

		public ZGuid APH_Calc_CreatedUser
		{
			get
			{
				return Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.AddedARecordToTheSystem.Code).Select(x => x.User.PK).FirstOrDefault();
			}
		}

		#endregion

		#region APH_Calc_LineCount

		public ZInt APH_Calc_LineCount
		{
			get { return OrderLines.Count; }
		}

		public ZPropertyInfo APH_Calc_LineCountInfo
		{
			get { return GetZPropertyInfo(Schema.APH_Calc_LineCount); }
		}

		#endregion

		#region APH_Calc_InnerPacks

		public ZInt APH_Calc_InnerPacks
		{
			get
			{
				if (aph_Calc_InnerPacks == null)
				{
					aph_Calc_InnerPacks = new CachedProperty<ZInt>(Factory, delegate
					{
						ZInt result = 0;
						foreach (AccPayableOrderLine orderLine in OrderLines)
						{
							result += (ZInt)orderLine.APL_InnerPacks;
						}

						return result;
					});
				}

				return aph_Calc_InnerPacks.Value;
			}
		}
		CachedProperty<ZInt> aph_Calc_InnerPacks;

		public ZPropertyInfo APH_Calc_InnerPacksInfo
		{
			get { return GetZPropertyInfo(Schema.APH_Calc_InnerPacks); }
		}

		#endregion

		#region APH_Calc_OuterPacks

		public ZInt APH_Calc_OuterPacks
		{
			get
			{
				if (aph_Calc_OuterPacks == null)
				{
					aph_Calc_OuterPacks = new CachedProperty<ZInt>(Factory, delegate
					{
						ZInt result = 0;
						foreach (AccPayableOrderLine orderLine in OrderLines)
						{
							result += (ZInt)orderLine.APL_OuterPacks;
						}

						return result;
					});
				}

				return aph_Calc_OuterPacks.Value;
			}
		}
		CachedProperty<ZInt> aph_Calc_OuterPacks;

		public ZPropertyInfo APH_Calc_OuterPacksInfo
		{
			get { return GetZPropertyInfo(Schema.APH_Calc_OuterPacks); }
		}

		#endregion

		#region APH_Calc_TotalLinePrice

		[DecimalPlaces(nameof(OrderCurrencyDecimals))]
		public ZDecimal APH_Calc_TotalLinePrice
		{
			get
			{
				if (aph_Calc_TotalLinePrice == null)
				{
					aph_Calc_TotalLinePrice = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0M;
						foreach (AccPayableOrderLine orderLine in OrderLines)
						{
							result += (ZDecimal)Utilities.Round(orderLine.APL_LinePrice, OrderCurrencyDecimals);
						}

						return result;
					});
				}

				return aph_Calc_TotalLinePrice.Value;
			}
		}
		CachedProperty<ZDecimal> aph_Calc_TotalLinePrice;

		public ZPropertyInfo APH_Calc_TotalLinePriceInfo
		{
			get { return GetZPropertyInfo(Schema.APH_Calc_TotalLinePrice); }
		}

		#endregion

		#region APH_Calc_TotalInvoicedPrice

		[DecimalPlaces(nameof(OrderCurrencyDecimals))]
		public ZDecimal APH_Calc_TotalInvoicedPrice
		{
			get
			{
				if (aph_Calc_TotalInvoicedPrice == null)
				{
					aph_Calc_TotalInvoicedPrice = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0M;
						foreach (AccPayableOrderLine orderLine in OrderLines)
						{
							result += (ZDecimal)Utilities.Round(orderLine.APL_InvoicedPrice, OrderCurrencyDecimals);
						}

						return result;
					});
				}

				return aph_Calc_TotalInvoicedPrice.Value;
			}
		}
		CachedProperty<ZDecimal> aph_Calc_TotalInvoicedPrice;

		public ZPropertyInfo APH_Calc_TotalInvoicedPriceInfo
		{
			get { return GetZPropertyInfo(Schema.APH_Calc_TotalInvoicedPrice); }
		}

		#endregion

		#region APH_Calc_TotalQuantity

		[DecimalPlaces(nameof(OrderCurrencyDecimals))]
		public ZDecimal APH_Calc_TotalQuantity
		{
			get
			{
				if (aph_Calc_TotalQuantity == null)
				{
					aph_Calc_TotalQuantity = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0M;
						foreach (AccPayableOrderLine orderLine in OrderLines)
						{
							result += orderLine.APL_Quantity;
						}

						return result;
					});
				}

				return aph_Calc_TotalQuantity.Value;
			}
		}
		CachedProperty<ZDecimal> aph_Calc_TotalQuantity;

		public ZPropertyInfo APH_Calc_TotalQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.APH_Calc_TotalQuantity); }
		}

		#endregion

		#region APH_Calc_TotalQuantityInvoiced

		[DecimalPlaces(nameof(OrderCurrencyDecimals))]
		public ZDecimal APH_Calc_TotalQuantityInvoiced
		{
			get
			{
				if (aph_Calc_TotalQuantityInvoiced == null)
				{
					aph_Calc_TotalQuantityInvoiced = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0M;
						foreach (AccPayableOrderLine orderLine in OrderLines)
						{
							result += orderLine.APL_QtyInvoiced;
						}

						return result;
					});
				}

				return aph_Calc_TotalQuantityInvoiced.Value;
			}
		}
		CachedProperty<ZDecimal> aph_Calc_TotalQuantityInvoiced;

		public ZPropertyInfo APH_Calc_TotalQuantityInvoicedInfo
		{
			get { return GetZPropertyInfo(Schema.APH_Calc_TotalQuantityInvoiced); }
		}

		#endregion

		#region APH_Calc_TotalQuantityReceived

		[DecimalPlaces(nameof(OrderCurrencyDecimals))]
		public ZDecimal APH_Calc_TotalQuantityReceived
		{
			get
			{
				if (aph_Calc_TotalQuantityReceived == null)
				{
					aph_Calc_TotalQuantityReceived = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0M;
						foreach (AccPayableOrderLine orderLine in OrderLines)
						{
							result += orderLine.APL_QtyReceived;
						}

						return result;
					});
				}

				return aph_Calc_TotalQuantityReceived.Value;
			}
		}
		CachedProperty<ZDecimal> aph_Calc_TotalQuantityReceived;

		public ZPropertyInfo APH_Calc_TotalQuantityReceivedInfo
		{
			get { return GetZPropertyInfo(Schema.APH_Calc_TotalQuantityReceived); }
		}

		#endregion

		#region APH_Calc_TotalQuantityRemaining

		[DecimalPlaces(nameof(OrderCurrencyDecimals))]
		public ZDecimal APH_Calc_TotalQuantityRemaining
		{
			get
			{
				if (aph_Calc_TotalQuantityRemaining == null)
				{
					aph_Calc_TotalQuantityRemaining = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0M;
						foreach (AccPayableOrderLine orderLine in OrderLines)
						{
							result += orderLine.APL_QuantityRemaining;
						}

						return result;
					});
				}

				return aph_Calc_TotalQuantityRemaining.Value;
			}
		}
		CachedProperty<ZDecimal> aph_Calc_TotalQuantityRemaining;

		public ZPropertyInfo APH_Calc_TotalQuantityRemainingInfo
		{
			get { return GetZPropertyInfo(Schema.APH_Calc_TotalQuantityRemaining); }
		}

		#endregion

		void OnOrderLines_CountChanged(object sender, EventArgs e)
		{
			APH_Calc_LineCountInfo.RefreshBinding();
			APH_Calc_InnerPacksInfo.RefreshBinding();
			APH_Calc_OuterPacksInfo.RefreshBinding();
			APH_Calc_TotalQuantityInfo.RefreshBinding();
			APH_Calc_TotalQuantityInvoicedInfo.RefreshBinding();
			APH_Calc_TotalQuantityReceivedInfo.RefreshBinding();
			APH_Calc_TotalQuantityRemainingInfo.RefreshBinding();
		}

		#region Properties used on Module grid

		public ZString APH_Calc_OrderedByCode
		{
			get { return OrderedBy != null ? OrderedBy.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo APH_Calc_OrderedByCodeInfo
		{
			get { return GetZPropertyInfo(nameof(APH_Calc_OrderedByCode)); }
		}

		public ZString APH_Calc_SupplierCode
		{
			get { return Supplier != null ? Supplier.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo APH_Calc_SupplierCodeInfo
		{
			get { return GetZPropertyInfo(nameof(APH_Calc_SupplierCode)); }
		}

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region APH_OrderNumberSplit

		[ReadOnly(true)]
		public override ZByte APH_OrderNumberSplit
		{
			get { return base.APH_OrderNumberSplit; }
			set
			{
				base.APH_OrderNumberSplit = value;
				RefreshOrderNumberReadOnly();
			}
		}

		void RefreshOrderNumberReadOnly()
		{
			RefreshBinding();
		}

		#endregion

		#region APH_OrderNumberAndSplit

		public ZString APH_OrderNumberAndSplit
		{
			get { return (APH_OrderNumberSplit == 0) ? APH_OrderNumber.ToString() : (APH_OrderNumber + "-" + APH_OrderNumberSplit); }
		}

		public ZPropertyInfo APH_OrderNumberAndSplitInfo
		{
			get { return GetZPropertyInfo(Schema.APH_OrderNumberAndSplit); }
		}

		#endregion

		#region OrderSplitSiblings

		AccPayableOrderHeaderCollection fOrderSplitSiblings;
		public AccPayableOrderHeaderCollection OrderSplitSiblings
		{
			get
			{
				if (fOrderSplitSiblings == null)
				{
					ZQuery filter = new ZQuery();
					filter.AddToFilter(JoinCondition.And, AccPayableOrderHeaderSchema.APH_OrderNumber, SQLComparisonOperator.Equal, APH_OrderNumber);
					filter.AddToFilter(JoinCondition.And, AccPayableOrderHeaderSchema.APH_GC, SQLComparisonOperator.Equal, APH_GC);
					filter.AddToFilter(JoinCondition.And, AccPayableOrderHeaderSchema.APH_OrderNumberSplit, SQLComparisonOperator.NotEqual, APH_OrderNumberSplit);
					filter.AddToFilter(JoinCondition.And, AccPayableOrderHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
					fOrderSplitSiblings = new AccPayableOrderHeaderCollection(Factory, filter);
				}
				return fOrderSplitSiblings;
			}
		}

		internal void RefreshOrderSplitSiblings()
		{
			fOrderSplitSiblings = null;
		}

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var noteTypes = new NoteTypeCollection();

				noteTypes.Add(PredefinedNoteTypes.Instance.SpecialInstructions);
				noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
				noteTypes.Add(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog);

				return noteTypes;
			}
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (IsSavedByFactory)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		int FactoryCountUsedToGenerateAPH_Order
		{
			get
			{
				return AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(this, FactoryCountUsedToGenerateAPH_OrderNumName);
			}
			set
			{
				AccountingIServices.FactoryCountForNumberFountainUsage.SetDataRowRelatedValue(this, FactoryCountUsedToGenerateAPH_OrderNumName, value);
			}
		}
		const string FactoryCountUsedToGenerateAPH_OrderNumName = "FactoryCountUsedToGenerateAPH_OrderNumName";

		public override void OnSaving()
		{
			base.OnSaving();
			if (!Factory.IsEqualToCurrentSaveCount(FactoryCountUsedToGenerateAPH_Order))
			{
				if (NeedToUpdateOrderNumberFromFountain && !IsSplitOrder)
				{
					APH_OrderNumber = Env.NumberFountains.PayableOrderNumber(APH_GC.ToGuid()).GetNextFormatted(Factory);
					FactoryCountUsedToGenerateAPH_Order = Factory.SaveCount;
				}
			}
		}

		bool NeedToUpdateOrderNumberFromFountain
		{
			get { return !IsInDatabase; }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (OrderLines.Count == 0)
			{
				APH_Disposition = Core.Constants.PayableOrderDisposition.OrderIncomplete;
			}
			else if (APH_Disposition == Core.Constants.PayableOrderDisposition.PendingGoodsReceivedAudit &&
				   IsPosted &&
				   Logs.LogsNotInDB.Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.RecordAuditedCode))
			{
				APH_Disposition = Core.Constants.PayableOrderDisposition.Complete;
			}
			else if (APH_Disposition == Core.Constants.PayableOrderDisposition.DeliveryInProgress &&
				   ((APH_Calc_TotalQuantityRemaining < APH_Calc_TotalQuantity) || (!IsPosted && IsAudited)))
			{
				APH_Disposition = Core.Constants.PayableOrderDisposition.APInvoiceToBePosted;
			}
			else if (OrderLines.ShouldResetDispositionWhenSaving)
			{
				APH_Disposition = Core.Constants.PayableOrderDisposition.PendingApproval;
			}
			else if (APH_Disposition == Constants.PayableOrderDisposition.Complete && !Logs.GetAllLogs().Cast<StmALog>().Any(s => !s.IsCancelled && s.SL_SE_NKEvent == Events.RecordAuditedCode))
			{
				APH_Disposition = Constants.PayableOrderDisposition.PendingGoodsReceivedAudit;
			}

			// add logs to record state changes
			if (APH_Stage != fPreviousAPH_Stage && !fPreviousAPH_Stage.IsEmpty)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format("Stage change from {0} to {1}", fPreviousAPH_Stage, APH_Stage));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				fPreviousAPH_Stage = APH_Stage;
			}
			if (APH_Disposition != fPreviousAPH_Disposition && !fPreviousAPH_Disposition.IsEmpty)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format("Disposition change from {0} to {1}", fPreviousAPH_Disposition, APH_Disposition));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				fPreviousAPH_Disposition = APH_Disposition;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			APH_Stage = Constants.PayableOrderStage.Request;
			APH_Disposition = Constants.PayableOrderDisposition.OrderIncomplete;
			APH_Type = Constants.PayableOrderType.VariableOverhead;
			APH_GoodsReceivedStatus = Constants.PayableOrderGoodsStatus.NotReceived;
			if (GlbBranch.CurrentBranch.OrgProxy != null && GlbBranch.CurrentBranch.OrgProxy.MainAddress != null)
			{
				APH_OA_Buyer = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			}
			APH_GC = GlbCompany.CurrentCompany.PK;
			APH_RX_NKOrderCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			APH_EstimatedExchangeRate = 1m;
			APH_FollowupDate = ZDate.Today.AddDays(7);
		}

#if DEBUG

#endif

		public DocumentSupporter DocumentSupporter
		{
			get { return new AccPayableOrderHeaderDocumentSupporter(this); }
		}

		#region IWorkflowSupporter

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new AccPayableOrderHeaderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return new AccPayableOrderHeaderWorkflowDescriptor().Code; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, APH_Type, ZString.Empty);
			return result;
		}

		#endregion

		#region IDocAddresses

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			ChangeOrderDispositionOnSupplierChanged();
		}

		void ChangeOrderDispositionOnSupplierChanged()
		{
			if (IsOrderFullyApproved)
			{
				APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			}
		}

		Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.SupplierDocumentaryAddress: return SupplierDocAddressRequirement;
				case DocAddressType.SupplierPickupDeliveryAddress: return SupplierPickupDeliveryAddressRequirement;
				default: return null;
			}
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		ZString IDocAddresses.HumanReadableName
		{
			get { return Res.GetString("ba0e749d-22e8-4e13-849e-43ec609f4b24", "Purchase Order {0}", APH_OrderNumber); }
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return PiggyBackedDocAddressValidation(addressToValidate);
		}

		protected virtual PayableOrderDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new PayableOrderDocAddressValidation(addressToValidate);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return SupportedAddressTypes; }
		}

		protected virtual DocAddressType[] SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.SupplierDocumentaryAddress,
					DocAddressType.SupplierPickupDeliveryAddress,
				};
			}
		}

		#endregion

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.PayableOrder);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region Post

		public string GetErrorMessageForNotAbleToPost()
		{
			string result = "";

			if (HasChanges || !IsInDatabase)
			{
				result = Res.GetString("101a7a95-b5e6-470e-a7b2-178fde9d6507", "Please save this form before posting the order");
			}
			else if (IsSplitOrder)
			{
				result = Res.GetString("ec793ab7-3a57-4bcf-a381-8decf7586a7e", "The Payables Invoice for Split Orders should be posted on the primary Purchase Order which includes the order lines for all of the related split orders as well");
			}
			else if (IsPosted)
			{
				result = Res.GetString("4a0e6ce6-c374-4722-b817-9bbb683d0997", "A Payables Invoice has already been posted for this Purchase Order, please reverse this invoice to process any amendments");
			}
			else if (APH_Stage == Constants.PayableOrderStage.Request)
			{
				result = Res.GetString("4d2ce1ef-6f59-41cc-8d9e-bfa69e72b9e6", "The Payable Invoice may not be posted for a Purchase Order that is pending approval");
			}
			else if (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.Value && APH_InvoiceDate > (APInvoicePostedDate.IsEmpty ? ZDate.Today : APInvoicePostedDate))
			{
				result = Res.GetString("5CB3D977-DFEF-4684-A63D-7FCCB9AFEF18", @"Unable to post AP Invoice.
Invoice Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date.");
			}

			return result;
		}

		bool IsSplitOrder
		{
			get
			{
				return APH_OrderNumberSplit != 0;
			}
		}

		bool IsPosted
		{
			get
			{
				return TransactionHeader != null;
			}
		}

		bool IsAudited
		{
			get
			{
				return Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.RecordAuditedCode);
			}
		}

		public bool IsBooked
		{
			get
			{
				return Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.BookedCode);
			}
		}

		public bool IsBookingConfirmed
		{
			get
			{
				return Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.BookingConfirmedCode);
			}
		}

		public InvoicingBase PopulateInvoiceFromOrder()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.SetContext(BusinessContext.PayableOrder);
			newFactory.Saved += new BusinessObjectFactory.SavedEventHandler(InvoiceFactory_Saved);

			RefreshOrderSplitSiblings();

			var invoicePopulatedFromOrder = newFactory.New<APInvoice>();
			invoicePopulatedFromOrder.AH_ChequeOrReference = APH_OrderNumberAndSplit;
			invoicePopulatedFromOrder.AH_OH = Supplier != null ? Supplier.PK : ZGuid.Empty;
			invoicePopulatedFromOrder.AH_Desc = APH_GoodsDescription;
			invoicePopulatedFromOrder.AH_TransactionNum = APH_InvoiceNumber;
			invoicePopulatedFromOrder.AH_InvoiceDate = APH_InvoiceDate;
			invoicePopulatedFromOrder.UseJobExchangeRate = true;
			invoicePopulatedFromOrder.AH_RX_NKTransactionCurrency = APH_RX_NKOrderCurrency;
			invoicePopulatedFromOrder.AH_ExchangeRate = APH_EstimatedExchangeRate;
			invoicePopulatedFromOrder.AH_DueDate = APH_DueDate;
			invoicePopulatedFromOrder.GSTInclusiveAmounts = APH_GSTInclusive;

			invoicePopulatedFromOrder.AddWritableProperties(new string[] { InvoicingBase.Schema.AH_RequisitionDate, InvoicingBase.Schema.AH_RequisitionStatus });

			foreach (var orderLine in OrderLinesIncludingSiblingsToBePosted)
			{
				var invoiceLine = (InvoiceLine)invoicePopulatedFromOrder.Lines.AddNew();
				invoiceLine.AL_GB = orderLine.APL_GB;
				invoiceLine.AL_GE = orderLine.APL_GE;
				invoiceLine.GenericCharge = orderLine.GenericCharge;
				invoiceLine.AL_Desc = orderLine.APL_Desc;
				var amount = orderLine.APL_QtyInvoiced * orderLine.APL_ItemPrice;
				if (APH_GSTInclusive)
				{
					invoiceLine.GSTInclusiveAmount = amount;
				}
				else
				{
					invoiceLine.AL_OSExTaxAmount = amount;
				}
				List<string> editableFields = new List<string>();
				editableFields.Add(AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.Value ? invoiceLine.AL_ATInfo.Name : null);
				invoiceLine.AddWritableProperties(editableFields.ToArray());
			}

			APInvoicePK = invoicePopulatedFromOrder.PK;

			return invoicePopulatedFromOrder;
		}

		void InvoiceFactory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= new BusinessObjectFactory.SavedEventHandler(InvoiceFactory_Saved);
				APH_AH = APInvoicePK;
				//if (!IsAudited)
				//{
					APH_Disposition = Constants.PayableOrderDisposition.PendingGoodsReceivedAudit;
					foreach (var splitOrder in OrderSplitSiblings)
					{
						splitOrder.APH_AH = APInvoicePK;
						splitOrder.APH_Disposition = Constants.PayableOrderDisposition.PendingGoodsReceivedAudit;
						splitOrder.Logs.AddNew(Events.ServiceInvoicePosted);
					}
				//}
				Logs.AddNew(Events.ServiceInvoicePosted);
				Logs.Factory.Save();

				SetSupplierReadOnly();
				SetLinesReadOnly();
				RefreshBindingIncludingChildren();
			}
		}

		AccPayableOrderLineCollection OrderLinesIncludingSiblingsToBePosted
		{
			get
			{
				if (fOrderLinesIncludingSiblings == null)
				{
					var orderSplitSiblingsPks = OrderSplitSiblings.Select(x => x.PK).ToList();
					orderSplitSiblingsPks.Add(this.PK);

					var filter = new ZQuery(AccPayableOrderLineSchema.APL_APH, orderSplitSiblingsPks);
					filter.AddToFilter(AccPayableOrderLineSchema.APL_QtyInvoiced, SQLComparisonOperator.GreaterThan, 0);
					fOrderLinesIncludingSiblings = new AccPayableOrderLineCollection(Factory, filter);
				}
				return fOrderLinesIncludingSiblings;
		}
		}
		AccPayableOrderLineCollection fOrderLinesIncludingSiblings;

		#endregion

		#region Split

		public enum CreateOrderType
		{
			Split,
			New
		}

		public bool ShouldPromptSplitOnSave
		{
			get
			{
				return
					this.OrderLines.IsOrderPartiallyComplete &&
					!this.IsAlreadySplit;
			}
		}

		public string CanSplitOrder()
		{
			string result = "";
			if (!this.IsInDatabase || this.HasChanges)
			{
				result = Res.GetString("1e0e383f-c3c7-4a0d-924f-f9578cb5dd48", "The order must be saved before splitting can occur");
			}

			return result;
		}

		public bool IsAlreadySplit
		{
			get
			{
				ZQuery filter1 = new ZQuery(AccPayableOrderHeaderSchema.APH_OrderNumber, SQLComparisonOperator.Equal, APH_OrderNumber);
				ZQuery filter2 = new ZQuery(AccPayableOrderHeaderSchema.APH_OrderNumberSplit, SQLComparisonOperator.Equal, (byte)(APH_OrderNumberSplit + 1));

				return Factory.LoadTop1<AccPayableOrderHeader>(new ZQuery(filter1, JoinCondition.And, filter2)) != null;
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public AccPayableOrderHeader SplitOrder(CreateOrderType splitType)
		{
			AccPayableOrderHeader result = (AccPayableOrderHeader)this.Clone();
			result.OrderLines.DeleteAll();

			if (splitType == CreateOrderType.Split)
			{
				result.SupplierDocumentaryAddress.OrganisationPK = SupplierDocumentaryAddress != null ? SupplierDocumentaryAddress.OrganisationPK : ZGuid.Empty;
				result.APH_OrderNumber = APH_OrderNumber;
				result.APH_OrderNumberSplit = FindNextOrderSplitNumber();
				result.APH_OA_Buyer = APH_OA_Buyer;
				result.APH_Type = APH_Type;
				result.APH_GoodsDescription = APH_GoodsDescription;
				result.APH_BookingConfRef = APH_BookingConfRef;
				result.APH_BookingConfDate = APH_BookingConfDate;
				result.APH_InvoiceNumber = APH_InvoiceNumber;
				result.APH_InvoiceDate = APH_InvoiceDate;
				result.APH_RX_NKOrderCurrency = APH_RX_NKOrderCurrency;
				result.APH_GSTInclusive = APH_GSTInclusive;
				result.APH_DueDate = APH_DueDate;
				result.APH_ReadyForDelivery = APH_ReadyForDelivery;
				result.APH_ExpectedDelivery = APH_ExpectedDelivery;
				result.APH_GoodsReceivedStatus = APH_GoodsReceivedStatus;
				result.APH_FollowupDate = APH_FollowupDate;
			}
			else
			{
				result.APH_OrderNumber = string.Empty;
			}

			result.SetRemainingOrderLinesBasedOnOrder(this);
			RefreshOrderNumberReadOnly();
			RefreshOrderSplitSiblings();
			result.RefreshOrderSplitSiblings();

			SetHasChangesToTrueToRefreshStatusOnFactorySave();
			return result;
		}

		byte FindNextOrderSplitNumber()
		{
			ZQuery filter = new ZQuery(AccPayableOrderHeaderSchema.APH_OrderNumber, SQLComparisonOperator.Equal, APH_OrderNumber);
			filter.AddToFilter(JoinCondition.And, AccPayableOrderHeaderSchema.APH_OA_Buyer, SQLComparisonOperator.Equal, APH_OA_Buyer);
			AccPayableOrderHeaderCollection orders = new AccPayableOrderHeaderCollection(Factory);
			filter.IgnoreActiveFilter = true;
			orders.AdditionalFilter = filter;
			orders.ApplySort(AccPayableOrderHeader.Schema.APH_OrderNumberSplit, ListSortDirection.Descending);

			return orders.Count > 0 ? (byte)(orders[0].APH_OrderNumberSplit + 1) : (byte)0;
		}

		void SetRemainingOrderLinesBasedOnOrder(AccPayableOrderHeader order)
		{
			foreach (AccPayableOrderLine orderLine in new ArrayList(order.OrderLines))
			{
				if (orderLine.APL_QuantityRemaining > 0)
				{
					AccPayableOrderLine newOrderLine = (AccPayableOrderLine)orderLine.Clone();
					OrderLines.Add(newOrderLine);

					newOrderLine.GenericCharge = orderLine.GenericCharge;
					newOrderLine.APL_Quantity = orderLine.APL_QuantityRemaining;
					newOrderLine.APL_QtyReceived = 0;
					newOrderLine.APL_QtyInvoiced = 0;
				}
			}
		}

		void SetHasChangesToTrueToRefreshStatusOnFactorySave()
		{
			HasChanges = true;
		}

		#endregion

		#region Approve

		public bool Approve()
		{
			var result = true;
			if (!IsOrderFullyApproved)
			{
				if (IsReadyForApproval)
				{
					result = new PayableOrderValidationHelper().CheckSecurityLevelsAndPromptForAuthorisation(this);
					if (result)
					{
						if (!IsBookingRequestDocumentDeliveredSinceLastApproval)
						{
							APH_Disposition = Constants.PayableOrderDisposition.OrderToBePlaced;
						}
						else if (APH_BookingConfDate == ZDate.Empty)
						{
							APH_Disposition = Constants.PayableOrderDisposition.PendingConfirmation;
						}
						else
						{
							if (!Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.ArrivalCode))
							{
								APH_Disposition = Constants.PayableOrderDisposition.ExpectedDLVPending;
							}
							else
							{
								APH_Disposition = Constants.PayableOrderDisposition.DeliveryInProgress;
							}
						}

						Logs.AddNew(Events.Authorised);
					}
				}
				else
				{
					result = false;
					OnShowApprovalMessage(ApprovalMessage.NotReadyForApproval);
				}
			}
			else
			{
				result = false;
				OnShowApprovalMessage(ApprovalMessage.AlreadyApproved);
			}

			return result;
		}

		bool IsReadyForApproval
		{
			get
			{
				return IsInDatabase && APH_Disposition == Constants.PayableOrderDisposition.PendingApproval && OrderLines.Count > 0;
			}
		}

		public string HasRegistryRestrictions()
		{
			if (this.Supplier == null && PayableOrderRegistryHelper.GetRegistryValue(PayableOrderRestrictionList.Codes.BlankSuppliers))
			{
				return PayableOrderRegistryHelper.BlankSupplierRestrictionMessage;
			}
			else if (DocAddresses.Count > 0)
			{
				var docAddresses = DocAddresses.Cast<JobDocAddress>();

				if (docAddresses.Any(x => x.E2_AddressOverride) && PayableOrderRegistryHelper.GetRegistryValue(PayableOrderRestrictionList.Codes.OverriddenSupplier))
				{
					return PayableOrderRegistryHelper.OverriddenSupplierRestrictionMessage;
				}
				else if (docAddresses.Any(x => (x.Organisation != null && x.Organisation.OH_IsTempAccount)) && PayableOrderRegistryHelper.GetRegistryValue(PayableOrderRestrictionList.Codes.SupplierIsTempOrg))
				{
					return PayableOrderRegistryHelper.TemporaryOrganizationRestrictionMessage;
				}
			}

			return string.Empty;
		}

		bool IsOrderFullyApproved
		{
			get
			{
				return APH_Stage != Constants.PayableOrderStage.Request && Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.AuthorisedCode);
			}
		}

		bool IsBookingRequestDocumentDeliveredSinceLastApproval
		{
			get
			{
				var result = false;
				var bookedEvent = Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.BookedCode).OrderByDescending(y => y.SL_PostedTimeUtc).FirstOrDefault();

				if (bookedEvent != null)
				{
					var authorizedEvent = Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.AuthorisedCode).OrderByDescending(y => y.SL_PostedTimeUtc).FirstOrDefault();
					if (authorizedEvent == null || authorizedEvent.SL_PostedTimeUtc < bookedEvent.SL_PostedTimeUtc)
					{
						result = true;
					}
				}

				return result;
			}
		}

		public delegate void ApprovalMessageEventHandler(ApprovalMessage approvalMessage);
		public event ApprovalMessageEventHandler ShowApprovalMessage;
		public void OnShowApprovalMessage(ApprovalMessage approvalMessage)
		{
			if (ShowApprovalMessage != null)
			{
				ShowApprovalMessage(approvalMessage);
			}
		}
		public enum ApprovalMessage
		{
			NotReadyForApproval,
			AlreadyApproved
		}

		#endregion
	}
}

