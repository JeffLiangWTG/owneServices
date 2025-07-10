using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class PostDateConfigurationReadOnly
	{
		public PostDateConfigurationReadOnly(PostDateConfiguration parent)
		{
			this.Parent = parent;
		}
		readonly PostDateConfiguration Parent;

		#region Direction

		public bool DirectionCode_ReadOnly
		{
			get
			{
				bool result = !Parent.JobType.IsEmpty
					&& Parent.JobType != JobInvoicingConsumerTypes.Shipment.Code
					&& Parent.JobType != JobInvoicingConsumerTypes.CFSShipment.Code
					&& Parent.JobType != JobInvoicingConsumerTypes.CFSLoadList.Code
					&& Parent.JobType != JobInvoicingConsumerTypes.ForwardingConsol.Code
					&& Parent.JobType != JobInvoicingConsumerTypes.GatewayConsol.Code;
#if DEBUG
				if (Globals.IsTest && !Parent.JobTypeList.ContainsCode(Parent.JobType))
				{
					result = false;
				}
#endif
				return result;
			}
		}

		#endregion

		#region Mode

		public bool Mode_ReadOnly
		{
			get
			{
				bool result = !Parent.JobType.IsEmpty
					&& Parent.JobType != JobInvoicingConsumerTypes.Shipment.Code
					&& Parent.JobType != JobInvoicingConsumerTypes.CFSShipment.Code
					&& Parent.JobType != JobInvoicingConsumerTypes.CFSLoadList.Code
					&& Parent.JobType != JobInvoicingConsumerTypes.ForwardingConsol.Code
					&& Parent.JobType != JobInvoicingConsumerTypes.GatewayConsol.Code;
#if DEBUG
				if (Globals.IsTest && !Parent.JobTypeList.ContainsCode(Parent.JobType))
				{
					result = false;
				}
#endif
				return result;
			}
		}

		#endregion

		#region Broker

		public bool BrokerCode_ReadOnly
		{
			get
			{
				bool result = Parent.JobType != "SHP" || (Parent.DirectionCode != Constants.FreightShipmentDirection.Code.Export && Parent.DirectionCode != Constants.FreightShipmentDirection.Code.Import);
#if DEBUG
				if (Globals.IsTest && !Parent.JobTypeList.ContainsCode(Parent.JobType))
				{
					result = false;
				}
#endif
				return result;
			}
		}

		#endregion

		#region PriorClosedPeriod

		public bool PriorClosedPeriod_ReadOnly
		{
			get
			{
				return Parent.SignificantDateCode == PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate;
			}
		}

		#endregion

		#region PriorOpenPeriod

		public bool PriorOpenPeriod_ReadOnly
		{
			get
			{
				return Parent.SignificantDateCode == PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate;
			}
		}

		#endregion

		#region FuturePeriod

		public bool FuturePeriod_ReadOnly
		{
			get
			{
				return Parent.SignificantDateCode == PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate;
			}
		}

		#endregion

	}
}
