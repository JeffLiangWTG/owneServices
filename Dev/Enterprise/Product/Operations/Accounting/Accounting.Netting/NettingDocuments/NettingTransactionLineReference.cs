using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingTransactionLineReference : NonPersistentBusinessObject
	{
		public NettingTransactionLineReference()
		{
		}

		public NettingTransactionLineReference(NettingStatement statement, IEnumerable<ISupportTransactionReference> collection)
		{
			Argument.NotNull(statement, "Statement");
			Argument.NotNull(collection, "Collection");

			Statement = statement;
			Collection = collection;
		}

		readonly NettingStatement Statement;
		readonly IEnumerable<ISupportTransactionReference> Collection;

		//*********************************************************************
		//Please do not change the names of the public properties. If you do, you will also need to change the column names in vw_NettingTransactionLineReference
		//*********************************************************************

		#region ShipmentNumbers
		[MaxLength(350)]
		public ZString ShipmentNumbers
		{
			get
			{
				if (!shipmentNumbers.IsEmpty)
				{
					return shipmentNumbers;
				}
				else if (ShipmentNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => ShipmentNumbersInfo);
					return shipmentNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != shipmentNumbers)
					{
						shipmentNumbers = value;
					}
				}
				else
				{
					ShipmentNumbersIsEmpty = true;
				}
			}
		}
		ZString shipmentNumbers;

		ZPropertyInfo ShipmentNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(ShipmentNumbers)); }
		}

		ZBool ShipmentNumbersIsEmpty { get; set; }

		#endregion

		#region ConsolNumbers
		[MaxLength(350)]
		public ZString ConsolNumbers
		{
			get
			{
				if (!consolNumbers.IsEmpty)
				{
					return consolNumbers;
				}
				else if (ConsolNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => ConsolNumbersInfo);
					return consolNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != consolNumbers)
					{
						consolNumbers = value;
					}
				}
				else
				{
					ConsolNumbersIsEmpty = true;
				}
			}
		}
		ZString consolNumbers;

		ZPropertyInfo ConsolNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolNumbers)); }
		}

		ZBool ConsolNumbersIsEmpty { get; set; }

		#endregion

		#region VesselVoyageNumbers
		[MaxLength(350)]
		public ZString VesselVoyageNumbers
		{
			get
			{
				if (!vesselVoyageNumbers.IsEmpty)
				{
					return vesselVoyageNumbers;
				}
				else if (VesselVoyageNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => VesselVoyageNumbersInfo);
					return vesselVoyageNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != vesselVoyageNumbers)
					{
						vesselVoyageNumbers = value;
					}
				}
				else
				{
					VesselVoyageNumbersIsEmpty = true;
				}
			}
		}
		ZString vesselVoyageNumbers;

		ZPropertyInfo VesselVoyageNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(VesselVoyageNumbers)); }
		}

		ZBool VesselVoyageNumbersIsEmpty { get; set; }

		#endregion

		#region ConsolContainerNumbers
		[MaxLength(350)]
		public ZString ConsolContainerNumbers
		{
			get
			{
				if (!consolContainerNumbers.IsEmpty)
				{
					return consolContainerNumbers;
				}
				else if (ConsolContainerNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => ConsolContainerNumbersInfo);
					return consolContainerNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != consolContainerNumbers)
					{
						consolContainerNumbers = value;
					}
				}
				else
				{
					ConsolContainerNumbersIsEmpty = true;
				}
			}
		}
		ZString consolContainerNumbers;

		ZPropertyInfo ConsolContainerNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolContainerNumbers)); }
		}

		ZBool ConsolContainerNumbersIsEmpty { get; set; }

		#endregion

		#region MasterWayBillNumbers
		[MaxLength(350)]
		public ZString MasterWayBillNumbers
		{
			get
			{
				if (!masterWayBillNumbers.IsEmpty)
				{
					return masterWayBillNumbers;
				}
				else if (MasterWayBillNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => MasterWayBillNumbersInfo);
					return masterWayBillNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != masterWayBillNumbers)
					{
						masterWayBillNumbers = value;
					}
				}
				else
				{
					MasterWayBillNumbersIsEmpty = true;
				}
			}
		}
		ZString masterWayBillNumbers;

		ZPropertyInfo MasterWayBillNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(MasterWayBillNumbers)); }
		}

		ZBool MasterWayBillNumbersIsEmpty { get; set; }

		#endregion

		#region BookingConfirmationReferenceNumbers
		[MaxLength(350)]
		public ZString BookingConfirmationReferenceNumbers
		{
			get
			{
				if (!bookingConfirmationReferenceNumbers.IsEmpty)
				{
					return bookingConfirmationReferenceNumbers;
				}
				else if (BookingConfirmationReferenceNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => BookingConfirmationReferenceNumbersInfo);
					return bookingConfirmationReferenceNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != bookingConfirmationReferenceNumbers)
					{
						bookingConfirmationReferenceNumbers = value;
					}
				}
				else
				{
					BookingConfirmationReferenceNumbersIsEmpty = true;
				}
			}
		}
		ZString bookingConfirmationReferenceNumbers;

		ZPropertyInfo BookingConfirmationReferenceNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(BookingConfirmationReferenceNumbers)); }
		}

		ZBool BookingConfirmationReferenceNumbersIsEmpty { get; set; }

		#endregion

		#region AgentReferenceNumbers
		[MaxLength(350)]
		public ZString AgentReferenceNumbers
		{
			get
			{
				if (!agentReferenceNumbers.IsEmpty)
				{
					return agentReferenceNumbers;
				}
				else if (AgentReferenceNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => AgentReferenceNumbersInfo);
					return agentReferenceNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != agentReferenceNumbers)
					{
						agentReferenceNumbers = value;
					}
				}
				else
				{
					AgentReferenceNumbersIsEmpty = true;
				}
			}
		}
		ZString agentReferenceNumbers;

		ZPropertyInfo AgentReferenceNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(AgentReferenceNumbers)); }
		}

		ZBool AgentReferenceNumbersIsEmpty { get; set; }

		#endregion

		#region PackingReferenceNumbers
		[MaxLength(350)]
		public ZString PackingReferenceNumbers
		{
			get
			{
				if (!packingReferenceNumbers.IsEmpty)
				{
					return packingReferenceNumbers;
				}
				else if (PackingReferenceNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => PackingReferenceNumbersInfo);
					return packingReferenceNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != packingReferenceNumbers)
					{
						packingReferenceNumbers = value;
					}
				}
				else
				{
					PackingReferenceNumbersIsEmpty = true;
				}
			}
		}
		ZString packingReferenceNumbers;

		ZPropertyInfo PackingReferenceNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(PackingReferenceNumbers)); }
		}

		ZBool PackingReferenceNumbersIsEmpty { get; set; }

		#endregion

		#region HouseBillNumbers
		[MaxLength(350)]
		public ZString HouseBillNumbers
		{
			get
			{
				if (!houseBillNumbers.IsEmpty)
				{
					return houseBillNumbers;
				}
				else if (HouseBillNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => HouseBillNumbersInfo);
					return houseBillNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != houseBillNumbers)
					{
						houseBillNumbers = value;
					}
				}
				else
				{
					HouseBillNumbersIsEmpty = true;
				}
			}
		}
		ZString houseBillNumbers;

		ZPropertyInfo HouseBillNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(HouseBillNumbers)); }
		}

		ZBool HouseBillNumbersIsEmpty { get; set; }

		#endregion

		#region OrderReferenceNumbers
		[MaxLength(350)]
		public ZString OrderReferenceNumbers
		{
			get
			{
				if (!orderReferenceNumbers.IsEmpty)
				{
					return orderReferenceNumbers;
				}
				else if (OrderReferenceNumbersIsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					Statement?.PopulateTransactionLineReferenceField(Collection, x => OrderReferenceNumbersInfo);
					return orderReferenceNumbers;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					if (value != orderReferenceNumbers)
					{
						orderReferenceNumbers = value;
					}
				}
				else
				{
					OrderReferenceNumbersIsEmpty = true;
				}
			}
		}
		ZString orderReferenceNumbers;

		ZPropertyInfo OrderReferenceNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(OrderReferenceNumbers)); }
		}

		ZBool OrderReferenceNumbersIsEmpty { get; set; }

		#endregion

		internal void SetValue(ZString propertyName, ZString value)
		{
			var propertyInfo = GetZPropertyInfo(propertyName);
			propertyInfo.SetValueFromString(value);
		}
	}
}
