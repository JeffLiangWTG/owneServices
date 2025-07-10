using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Client.Wow
{
	[Enterprise.Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.NotApplied, "Enterprise.Client.Wow.Metadata.WoolworthsOrder, ZClientWOW, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")]
	public class WoolworthsOrder : Order
	{
		public const string DeliverPointIDProperty = OrderLineDelivery.Schema.J4_CustomAttribute2;
		public static readonly SchemaColumn HasMatchedProductBuyersProperty = JobOrderHeaderSchema.JD_CustomFlag1;
		public const string JD_PaymentType_CustomPropertyName = Schema.JD_CustomAttrib1;

		public WoolworthsOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation

		internal new WoolworthsOrderValidation Validation
		{
			get { return (WoolworthsOrderValidation)base.Validation; }
		}

		protected override JobOrderHeaderValidation GetNewValidation()
		{
			return new WoolworthsOrderValidation(this);
		}

		#endregion

		public ZQuery GetDeliverPointFilter(ZString deliverPoint)
		{
			var filter = new ZQuery(OrgAddressSchema.OA_Code, deliverPoint.Trim());
			filter.AddToFilter(OrgAddressSchema.OA_OH, BuyerPK);
			return filter;
		}

		public void MatchBuyerOnOrderLineProducts(INotifications notify)
		{
			NotificationBuffer buffer = new NotificationBuffer(notify);
			foreach (OrderLine line in OrderLines)
			{
				WoolworthsProduct product = WowStringToBusinessObjectFieldConverter.Instance.LoadPartForBuyerTakeOn(Factory, JD_OrderNumber + "," + line.JO_Partno, line.JO_Partno, buffer, false);
				if (product != null)
				{
					product.RelatedOrganisations.AddOrganisationIfNotExist(BuyerPK, OrgPartRelation.RelationshipTypes.Owner, true);
				}
			}
			if (!buffer.HasErrors)
			{
				this[HasMatchedProductBuyersProperty.Name] = ZBool.True;
			}
		}

		public bool ShouldSendToEdiTrack()
		{
			return JD_OrderStatus != Constants.OrderStatus.Incomplete &&
				!HasOrderLineDeliveryContainer && !HasJobComInvoiceLineLink;
		}

		#region UpdateOrderStatusesForReport

		[ThreadStatic]
		internal static bool WasUpdateOrderStatusesForReportCalled;
		internal static void UpdateOrderStatusesForReport(Report report)
		{
			WasUpdateOrderStatusesForReportCalled = true;
			ArrayList orderPKs = new ArrayList();

			string whereClause = report.FilterCollection.WhereClause();
			string sQL = "select OrderPK from vw_Report_ClientOrdersSignOff " + (string.IsNullOrWhiteSpace(whereClause) ? "" : "where " + whereClause);
			using (DbCommand cmd = Db.Connection.Command(sQL))
			{
				foreach (var param in report.FilterCollection.SqlParameters())
				{
					cmd.AddParameter(param.ParameterName, param.SqlDbType, param.Size, param.Precision, param.Scale, param.Value);
				}

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						orderPKs.Add((Guid)reader[0]);
					}
				}
			}

			BusinessObjectFactory factory = new BusinessObjectFactory();
			foreach (Guid orderPK in orderPKs)
			{
				WoolworthsOrder order = (WoolworthsOrder)factory.Load(typeof(WoolworthsOrder), orderPK);
				order.FlagIndentOrderReportAsRead();
			}
			factory.Save();
		}

		void FlagIndentOrderReportAsRead()
		{
			if (JD_OrderStatus == Constants.OrderStatus.Incomplete)
			{
				JD_OrderStatus = Constants.OrderStatus.Open;
			}
		}

		#endregion

		#region InvoiceOrderMismatching Event and related property overrides

		public override ZString JD_OrderNumber
		{
			get { return base.JD_OrderNumber; }
			set
			{
				if (IsCopying || !HasJobComInvoiceLineLink)
				{
					base.JD_OrderNumber = value;
				}
				else if (OnInvoiceOrderMismatching())
				{
					base.JD_OrderNumber = value;
				}
				else
				{
					JD_OrderNumberInfo.RefreshBinding();
				}
			}
		}

		public event CancelEventHandler InvoiceOrderMismatching;

		internal bool OnInvoiceOrderMismatching()
		{
			CancelEventArgs e = new CancelEventArgs(false);
			if (InvoiceOrderMismatching != null)
			{
				InvoiceOrderMismatching(this, e);
			}
			return !e.Cancel;
		}

		#endregion

		#region IndentOrVendorOrderRequested Event

		public event CancelEventHandler IndentOrVendorOrderRequested;
		bool OnIndentOrVendorOrderRequested()
		{
			CancelEventArgs e = new CancelEventArgs(false);
			if (IndentOrVendorOrderRequested != null)
			{
				IndentOrVendorOrderRequested(this, e);
			}
			return !e.Cancel;
		}

		#endregion

		#region Business Object Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			NotificationBuffer buffer = new NotificationBuffer(null);
			MatchBuyerOnOrderLineProducts(buffer);

			if (WowDataRegistry.Instance.EnableOrderNumberFountain && !IsInDatabase && JD_OrderNumber.IsEmpty)
			{
				JD_OrderNumber = Env.NumberFountains.OrderNumber.GetNextFormatted(Factory);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.JD_PaymentType = WowConstants.PaymentTypes.TelegraphicTransfer;
		}

		public override DocumentSupporter DocumentSupporter
		{
			get { return new WoolworthsOrderDocumentSupporter(this); }
		}

		#endregion

		#region New Properties

		public ZGuid JD_RN_CountryOfOrigin
		{
			get
			{
				return Supplier == null ?
							ZGuid.Empty :
							Supplier.MiscServ.EXDefaultCntryOfOrigin == null ?
									ZGuid.Empty :
									Supplier.MiscServ.EXDefaultCntryOfOrigin.PK;
			}
		}

		#region JD_PaymentType

		public ZString JD_PaymentType
		{
			get { return (ZString)this[JD_PaymentType_CustomPropertyName]; }
			set
			{
				this[JD_PaymentType_CustomPropertyName] = value;
				JD_PaymentTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JD_PaymentTypeInfo
		{
			get { return GetZPropertyInfo(nameof(JD_PaymentType)); }
		}

		public int JD_PaymentType_MaxLength
		{
			get { return JD_CustomAttrib1Info.MaxLength; }
		}

		#endregion

		#endregion

		#region Property Overrides

		public override ZGuid JD_OA_BuyerAddress
		{
			get { return base.JD_OA_BuyerAddress; }
			set
			{
				base.JD_OA_BuyerAddress = value;
				if (!IsCopying)
				{
					PopulateDeliverPointsOnAllOrderLines();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateJD_OA_BuyerAddress();
				}
			}
		}

		#endregion

		#region List Properties

		public CodeDescriptionPairList JD_PaymentType_List
		{
			get { return WowConstants.GetPaymentTypeList(); }
		}

		public override CodeDescriptionPairList JD_OrderStatus_List
		{
			get
			{
				CodeDescriptionPairList result = base.JD_OrderStatus_List;
				result.AddPair(WowConstants.OrderStatuses.ContainersAttached, "Containers Attached");
				return result;
			}
		}

		#endregion

		#region WoolworthOrderDocumentSupporter
		public class WoolworthsOrderDocumentSupporter : OrderDocumentSupporter
		{
			public WoolworthsOrderDocumentSupporter(WoolworthsOrder order)
				: base(order)
			{
			}

			WoolworthsOrder WoolworthsOrder
			{
				get { return (WoolworthsOrder)BusinessObject; }
			}

			#region Overrides
			protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				if (dataContext == Enterprise.Core.Constants.DataContext.WowIndentOrder)
				{
					if (WoolworthsOrder.JD_OrderStatus == Constants.OrderStatus.Incomplete &&
						WoolworthsOrder.OnIndentOrVendorOrderRequested())
					{
						if (HasChanges)
						{
							ErrorReporter.ReportOnce(
								"HasChangesWasTrueWhenRequestingAWoolliesReport",
								"HasChanges = True when requesting a report");
						}
						WoolworthsOrder.JD_OrderStatus = Constants.OrderStatus.Open;
						try
						{
							Factory.Save();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ErrorReporter.ReportOnce(ex.Message, ex);
						}
					}
					return new DocumentWrapper[] { DocOrder.New(WoolworthsOrder, Factory) };
				}
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}

			protected override Constants.DataContext[] GetSupportedDataContexts()
			{
				ArrayList result = new ArrayList();
				result.AddRange(base.GetSupportedDataContexts());
				result.Add(Enterprise.Core.Constants.DataContext.WowIndentOrder);
				return (Constants.DataContext[])result.ToArray(typeof(Constants.DataContext));
			}
			#endregion

		}

		#endregion

		#region Implementation

		internal ArrayList DeliveryPopulationWarnings = new ArrayList();

		void PopulateDeliverPointsOnAllOrderLines()
		{
			DeliveryPopulationWarnings.Clear();
			foreach (OrderLine line in this.OrderLines)
			{
				foreach (OrderLineDelivery delivery in line.Deliveries)
				{
					if (delivery.J4_OA_NKDeliveryPoint.IsEmpty &&
						!delivery.J4_CustomAttribute2.IsEmpty)
					{
						PopulateDeliverPoints(delivery);
					}
				}
			}
		}

		void PopulateDeliverPoints(OrderLineDelivery delivery)
		{
			ZQuery filter = GetDeliverPointFilter((ZString)delivery[DeliverPointIDProperty]);
			OrgAddress[] address = (OrgAddress[])Factory.Load(typeof(OrgAddress), filter);

			if (address.Length >= 2)
			{
				DeliveryPopulationWarnings.Add("More than 1 match for populating the deliver point address for " + delivery.J4_CustomAttribute2);
			}
			else if (address.Length == 0)
			{
				DeliveryPopulationWarnings.Add(
					"Could not find deliver point with ID " + delivery.J4_CustomAttribute2 +
					". Either set the deliver point manually or ensure the deliver point has the correct ID under it's 'Usage Comment'.");
			}
			else
			{
				delivery.J4_OA_NKDeliveryPoint = address[0].OA_Code;
			}
		}

		protected bool HasJobComInvoiceLineLink
		{
			get
			{
				if (!JD_OrderNumber.IsEmpty)
				{
					ZQuery filter = new ZQuery();
					filter.AddToFilter(JobComInvoiceLineSchema.JI_OrderNumber, SQLComparisonOperator.Equal, JD_OrderNumber);
					filter.AddToFilter(JobComInvoiceLineSchema.JI_OrderNumber, SQLComparisonOperator.NotEqual, ZString.Empty);
					var invoiceLines = Factory.Load<BaseJobComInvoiceLine>(filter).Where(line => line is JobComInvoiceLine);

					foreach (var invoiceLine in invoiceLines)
					{
						if (invoiceLine.Declaration?.JE_IsCancelled == false)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		protected bool HasOrderLineDeliveryContainer
		{
			get
			{
				if (OrderLines != null)
				{
					foreach (OrderLine orderLine in OrderLines)
					{
						if (orderLine.Deliveries != null)
						{
							foreach (OrderLineDelivery orderLineDelivery in orderLine.Deliveries)
							{
								if (orderLineDelivery.Containers != null && orderLineDelivery.Containers.Count > 0)
								{
									return true;
								}
							}
						}
					}
				}
				return false;
			}
		}

		protected sealed override void UpdateOrderStatus()
		{
			base.UpdateOrderStatus();
			UpdateOrderStatusCore();
		}

		protected virtual void UpdateOrderStatusCore()
		{
			if (JD_OrderStatus == Constants.OrderStatus.Open)
			{
				foreach (OrderLine line in this.OrderLines)
				{
					foreach (OrderLineDelivery delivery in line.Deliveries)
					{
						if (delivery.Containers.Count > 0)
						{
							JD_OrderStatus = WowConstants.OrderStatuses.ContainersAttached;
							break;
						}
					}
				}
			}
		}

		#endregion
	}
}
