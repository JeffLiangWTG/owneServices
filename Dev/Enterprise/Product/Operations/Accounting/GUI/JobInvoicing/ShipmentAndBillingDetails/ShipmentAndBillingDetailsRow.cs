using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	internal class ShipmentAndBillingDetailsRow : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ShipmentAndBillingDetailsRow()
		{
		}

		public ShipmentAndBillingDetailsRow(ForwardingShipment relatedJob, ForwardingConsol consol)
		{
			RelatedJob = relatedJob;
			Consol = consol;
			Initialize();
		}

		void Initialize()
		{
			var consolJob = Consol.Job as Job;
			OrgHeader defaultDebtor;
			using (new DisposableAction(() => Consol.SetContext(BusinessContext.GetDefaultDebtorWithoutChargeCode), () => Consol.RemoveContext(BusinessContext.GetDefaultDebtorWithoutChargeCode)))
			{
				defaultDebtor = (consolJob.GetInvoicingSupporter() as ForwardingConsolInvoicingSupporter)?.GetDefaultDebtor(null, null, RelatedJobNum);
			}

			if (defaultDebtor != null && defaultDebtor.OH_IsDebtor && !defaultDebtor.IsCancelled)
			{
				Debtor = defaultDebtor;
				TargetJobNum = Debtor.GetInvoiceTarget(RelatedJobNum, consolJob);
			}
			RefreshBinding();
		}

		readonly ForwardingConsol Consol;
		readonly ForwardingShipment RelatedJob;

		#region Related Job

		public ZString RelatedJobNum => RelatedJob.JobNumber;
		public ZPropertyInfo RelatedJobNumInfo => GetZPropertyInfo(nameof(RelatedJobNum));

		public ZString RelatedJobOrigin => RelatedJob.InvoicingSupporter?.Origin?.Code ?? ZString.Empty;
		public ZPropertyInfo RelatedJobOriginInfo => GetZPropertyInfo(nameof(RelatedJobOrigin));

		public ZString RelatedJobDestination => RelatedJob.InvoicingSupporter?.Destination?.Code ?? ZString.Empty;
		public ZPropertyInfo RelatedJobDestinationInfo => GetZPropertyInfo(nameof(RelatedJobDestination));

		#endregion

		#region Pick Up Agent

		public ZString PickUpAgentCode => RelatedJob.PickupAgent?.OH_Code ?? ZString.Empty;
		public ZPropertyInfo PickUpAgentCodeInfo => GetZPropertyInfo(nameof(PickUpAgentCode));

		public ZString PickUpAgentName => RelatedJob.PickupAgent?.OH_FullName ?? ZString.Empty;
		public ZPropertyInfo PickUpAgentNameInfo => GetZPropertyInfo(nameof(PickUpAgentName));

		#endregion

		#region Delivery Agent

		public ZString DeliveryAgentCode => RelatedJob.DeliveryAgent?.OH_Code ?? ZString.Empty;
		public ZPropertyInfo DeliveryAgentCodeInfo => GetZPropertyInfo(nameof(DeliveryAgentCode));

		public ZString DeliveryAgentName => RelatedJob.DeliveryAgent?.OH_FullName ?? ZString.Empty;
		public ZPropertyInfo DeliveryAgentNameInfo => GetZPropertyInfo(nameof(DeliveryAgentName));

		#endregion

		#region Previous Sending Agent

		public ZString PreviousSendingAgentCode => PreviousSendingAgent?.OH_Code ?? ZString.Empty;
		public ZPropertyInfo PreviousSendingAgentCodeInfo => GetZPropertyInfo(nameof(PreviousSendingAgentCode));

		public ZString PreviousSendingAgentName => PreviousSendingAgent?.OH_FullName ?? ZString.Empty;
		public ZPropertyInfo PreviousSendingAgentNameInfo => GetZPropertyInfo(nameof(PreviousSendingAgentName));

		OrgHeader PreviousSendingAgent => ((Consol.Job as Job)?.GetInvoicingSupporter()?.GetPreviousConsol(RelatedJobNum) as ForwardingConsol)?.SendingForwarder;

		#endregion

		#region Debtor

		OrgHeader Debtor;

		public ZString DebtorCode => Debtor?.OH_Code ?? ZString.Empty;
		public ZPropertyInfo DebtorCodeInfo => GetZPropertyInfo(nameof(DebtorCode));

		public ZString DebtorName => Debtor?.OH_FullName ?? ZString.Empty;
		public ZPropertyInfo DebtorNameInfo => GetZPropertyInfo(nameof(DebtorName));

		public ZString DebtorAddress => Debtor?.AddressForSendingARDocuments?.Address1 ?? ZString.Empty;
		public ZPropertyInfo AddressInfo => GetZPropertyInfo(nameof(DebtorAddress));

		#endregion

		#region Target Job

		public ZString TargetJobNum
		{
			get { return targetJobNum; }
			private set
			{
				if (value != TargetJobNum)
				{
					SetNonPersistentPropertyValue(TargetJobNumInfo, ref targetJobNum, value);
					TargetJobNumInfo.RefreshBinding();
				}
			}
		}
		ZString targetJobNum;

		public ZPropertyInfo TargetJobNumInfo => GetZPropertyInfo(nameof(TargetJobNum));

		public ZString TargetJobRoute
		{
			get
			{
				if (!TargetJobNum.IsEmpty && targetJobRoute.IsEmpty)
				{
					var targetSupporter = Debtor.InvoiceTargets(RelatedJob).FirstOrDefault(x => x.JobNumber == TargetJobNum)?.InvoicingSupporter;
					targetJobRoute = FormattableString.Invariant($"{targetSupporter?.Origin?.Code} - {targetSupporter?.Destination?.Code}"); // UNLOCO not translated
				}
				return targetJobRoute;
			}
		}
		ZString targetJobRoute;
		public ZPropertyInfo TargetJobRouteInfo => GetZPropertyInfo(nameof(TargetJobRoute));

		#endregion
		[ResourceStringData("BaseCharge|ShowRelatedCharges",
				Caption = "Show Charges for this shipment",
				MediumCaption = "Show Charges")]
		public ZBool ShowRelatedCharges
		{
			get { return showRelatedCharges; }
			set
			{
				if (value != ShowRelatedCharges)
				{
					SetNonPersistentPropertyValue(ShowRelatedChargesInfo, ref showRelatedCharges, value);
					ShowRelatedChargesInfo.RefreshBinding();
				}
			}
		}
		ZBool showRelatedCharges;
		public ZPropertyInfo ShowRelatedChargesInfo => GetZPropertyInfo(nameof(ShowRelatedCharges));
	}
}
