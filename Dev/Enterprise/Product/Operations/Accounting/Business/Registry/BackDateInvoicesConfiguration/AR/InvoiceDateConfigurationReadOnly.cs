using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class InvoiceDateConfigurationReadOnly
	{
		public InvoiceDateConfigurationReadOnly(IInvoiceDateConfiguration parent)
		{
			this.Parent = parent;
		}
		readonly IInvoiceDateConfiguration Parent;

		#region Direction

		public bool DirectionCode_ReadOnly
		{
			get
			{
				var result = !Parent.JobType.IsEmpty
					&& Parent.JobType != "SHP"
					&& Parent.JobType != "CSH"
					&& Parent.JobType != "CLL"
					&& Parent.JobType != "FCN"
					&& Parent.JobType != "GCN";
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
				var result = !Parent.JobType.IsEmpty
					&& Parent.JobType != "SHP"
					&& Parent.JobType != "CSH"
					&& Parent.JobType != "CLL"
					&& Parent.JobType != "FCN"
					&& Parent.JobType != "GCN";
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
				bool result = Parent.SignificantDateCode == InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate;
#if DEBUG
				if (Globals.IsTest)
				{
					result = false;
				}
#endif
				return result;
			}
		}

		#endregion

		#region PriorOpenPeriod

		public bool PriorOpenPeriod_ReadOnly
		{
			get
			{
				bool result = Parent.SignificantDateCode == InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate;
#if DEBUG
				if (Globals.IsTest)
				{
					result = false;
				}
#endif
				return result;
			}
		}

		#endregion

		#region FuturePeriod

		public bool FuturePeriod_ReadOnly
		{
			get
			{
				bool result = Parent.SignificantDateCode == InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate;
#if DEBUG
				if (Globals.IsTest)
				{
					result = false;
				}
#endif
				return result;
			}
		}

		#endregion

	}
}