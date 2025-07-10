using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingTransactionReference : NonPersistentBusinessObject
	{
		public NettingTransactionReference()
		{
		}

		public NettingTransactionReference(NettingStatement statement, IEnumerable<ISupportTransactionReference> collection)
		{
			Argument.NotNull(statement, "Statement");
			Argument.NotNull(collection, "Collection");

			Statement = statement;
			Collection = collection;
		}

		readonly NettingStatement Statement;
		readonly IEnumerable<ISupportTransactionReference> Collection;

		//*********************************************************************
		//Please do not change the names of the public properties. If you do, you will also need to change the column names in vw_NettingTransactionReference
		//*********************************************************************

		#region InvoiceNumber
		[MaxLength(35)]
		public ZString InvoiceNumber
		{
			get
			{
				if (!invoiceNumber.IsEmpty)
				{
					return invoiceNumber;
				}
				else if (InvoiceNumberIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionReferenceField(Collection, x => InvoiceNumberInfo);
					return invoiceNumber;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != invoiceNumber)
					{
						invoiceNumber = value;
					}
				}
				else
				{
					InvoiceNumberIsEmpty = true;
				}
			}
		}
		ZString invoiceNumber;

		ZPropertyInfo InvoiceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceNumber)); }
		}

		ZBool InvoiceNumberIsEmpty { get; set; }

		#endregion

		#region JobInvoiceNumber
		[MaxLength(35)]
		public ZString JobInvoiceNumber
		{
			get
			{
				if (!jobInvoiceNumber.IsEmpty)
				{
					return jobInvoiceNumber;
				}
				else if (JobInvoiceNumberIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionReferenceField(Collection, x => JobInvoiceNumberInfo);
					return jobInvoiceNumber;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != jobInvoiceNumber)
					{
						jobInvoiceNumber = value;
					}
				}
				else
				{
					JobInvoiceNumberIsEmpty = true;
				}
			}
		}
		ZString jobInvoiceNumber;

		ZPropertyInfo JobInvoiceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(JobInvoiceNumber)); }
		}

		ZBool JobInvoiceNumberIsEmpty { get; set; }

		#endregion

		#region ShipmentNumber
		[MaxLength(35)]
		public ZString ShipmentNumber
		{
			get
			{
				if (!shipmentNumber.IsEmpty)
				{
					return shipmentNumber;
				}
				else if (ShipmentNumberIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionReferenceField(Collection, x => ShipmentNumberInfo);
					return shipmentNumber;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != shipmentNumber)
					{
						shipmentNumber = value;
					}
				}
				else
				{
					ShipmentNumberIsEmpty = true;
				}
			}
		}
		ZString shipmentNumber;

		ZPropertyInfo ShipmentNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ShipmentNumber)); }
		}

		ZBool ShipmentNumberIsEmpty { get; set; }

		#endregion

		#region ConsolNumber
		[MaxLength(35)]
		public ZString ConsolNumber
		{
			get
			{
				if (!consolNumber.IsEmpty)
				{
					return consolNumber;
				}
				else if (ConsolNumberIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionReferenceField(Collection, x => ConsolNumberInfo);
					return consolNumber;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != consolNumber)
					{
						consolNumber = value;
					}
				}
				else
				{
					ConsolNumberIsEmpty = true;
				}
			}
		}
		ZString consolNumber;

		ZPropertyInfo ConsolNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolNumber)); }
		}

		ZBool ConsolNumberIsEmpty { get; set; }

		#endregion

		#region VesselVoyageNumber
		[MaxLength(35)]
		public ZString VesselVoyageNumber
		{
			get
			{
				if (!vesselAndVoyageNumber.IsEmpty)
				{
					return vesselAndVoyageNumber;
				}
				else if (VesselAndVoyageNumberIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionReferenceField(Collection, x => VesselAndVoyageNumberInfo);
					return vesselAndVoyageNumber;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != vesselAndVoyageNumber)
					{
						vesselAndVoyageNumber = value;
					}
				}
				else
				{
					VesselAndVoyageNumberIsEmpty = true;
				}
			}
		}
		ZString vesselAndVoyageNumber;

		ZPropertyInfo VesselAndVoyageNumberInfo
		{
			get { return GetZPropertyInfo(nameof(VesselVoyageNumber)); }
		}

		ZBool VesselAndVoyageNumberIsEmpty { get; set; }

		#endregion

		#region ConsolContainerNumber
		[MaxLength(35)]
		public ZString ConsolContainerNumber
		{
			get
			{
				if (!consolContainerNumber.IsEmpty)
				{
					return consolContainerNumber;
				}
				else if (ConsolContainerNumberIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionReferenceField(Collection, x => ConsolContainerNumberInfo);
					return consolContainerNumber;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != consolContainerNumber)
					{
						consolContainerNumber = value;
					}
				}
				else
				{
					ConsolContainerNumberIsEmpty = true;
				}
			}
		}
		ZString consolContainerNumber;

		ZPropertyInfo ConsolContainerNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolContainerNumber)); }
		}

		ZBool ConsolContainerNumberIsEmpty { get; set; }

		#endregion

		#region MasterWayBillNumber
		[MaxLength(35)]
		public ZString MasterWayBillNumber
		{
			get
			{
				if (!masterWayBillNumber.IsEmpty)
				{
					return masterWayBillNumber;
				}
				else if (MasterWayBillNumberIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionReferenceField(Collection, x => MasterWayBillNumberInfo);
					return masterWayBillNumber;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != masterWayBillNumber)
					{
						masterWayBillNumber = value;
					}
				}
				else
				{
					MasterWayBillNumberIsEmpty = true;
				}
			}
		}
		ZString masterWayBillNumber;

		ZPropertyInfo MasterWayBillNumberInfo
		{
			get { return GetZPropertyInfo(nameof(MasterWayBillNumber)); }
		}

		ZBool MasterWayBillNumberIsEmpty { get; set; }

		#endregion

		#region BookingConfirmationReferenceNumber
		[MaxLength(35)]
		public ZString BookingConfirmationReferenceNumber
		{
			get
			{
				if (!bookingConfirmationReferenceNumber.IsEmpty)
				{
					return bookingConfirmationReferenceNumber;
				}
				else if (BookingConfirmationReferenceNumberIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionReferenceField(Collection, x => BookingConfirmationReferenceNumberInfo);
					return bookingConfirmationReferenceNumber;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != bookingConfirmationReferenceNumber)
					{
						bookingConfirmationReferenceNumber = value;
					}
				}
				else
				{
					BookingConfirmationReferenceNumberIsEmpty = true;
				}
			}
		}
		ZString bookingConfirmationReferenceNumber;

		ZPropertyInfo BookingConfirmationReferenceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(BookingConfirmationReferenceNumber)); }
		}

		ZBool BookingConfirmationReferenceNumberIsEmpty { get; set; }

		#endregion

		#region AgentReferenceNumber
		[MaxLength(35)]
		public ZString AgentReferenceNumber
		{
			get
			{
				if (!agentReferenceNumber.IsEmpty)
				{
					return agentReferenceNumber;
				}
				else if (AgentReferenceNumberIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionReferenceField(Collection, x => AgentReferenceNumberInfo);
					return agentReferenceNumber;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != agentReferenceNumber)
					{
						agentReferenceNumber = value;
					}
				}
				else
				{
					AgentReferenceNumberIsEmpty = true;
				}
			}
		}
		ZString agentReferenceNumber;

		ZPropertyInfo AgentReferenceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(AgentReferenceNumber)); }
		}

		ZBool AgentReferenceNumberIsEmpty { get; set; }

		#endregion

		internal void SetValue(ZString propertyName, ZString value)
		{
			var propertyInfo = GetZPropertyInfo(propertyName);
			propertyInfo.SetValueFromString(value);
		}
	}
}
