using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentSplitsAndFlightData : AutoNonPersistentSplitFlightData
	{
		public NonPersistentSplitsAndFlightData(BusinessObjectFactory factory)
			: base(factory)
		{
			SplitLines = new NonPersistentSplitLineCollection(factory);
		}

		public NonPersistentSplitsAndFlightData(ZString flightNumber, ZDateTime flightDate, NonPersistentSplitLineCollection splitLines, ICcsukCusAwb awb)
			: base(awb.Factory)
		{
			base.FlightNumber = flightNumber;
			base.FlightArrivalDate = flightDate;
			SplitLines = splitLines;
			this.Awb = awb;
		}

		[ReadOnlyMember(nameof(SendFlightInfoToo_Not))]
		public override ZString FlightNumber
		{
			get { return base.FlightNumber; }
			set
			{
				base.FlightNumber = value;
				FlightNumberInfo.RefreshBinding();
			}
		}

		// Allocate NPR to existing splits
		[ReadOnlyMember(nameof(SendFlightInfoTooReadOnly))]
		public override ZBool SendFlightInfoToo
		{
			get { return base.SendFlightInfoToo; }
			set
			{
				base.SendFlightInfoToo = value;
				SendFlightInfoTooInfo.RefreshBinding();
				SplitLines.IsAllocatingNprWithFlightData = value;
				SplitLines.RefreshBindingIncludingChildren();
			}
		}
		// Shed do not need to send messages containing the flight number to allocate NPR
		bool SendFlightInfoTooReadOnly
		{
			get { return LicenceAndPimaHelper.IsFullShed(Awb) || LicenceAndPimaHelper.IsFallbackShed(Awb); }
		}

		[ReadOnlyMember(nameof(SendFlightInfoToo_Not))]
		public override ZDateTime FlightArrivalDate
		{
			get { return base.FlightArrivalDate; }
			set
			{
				base.FlightArrivalDate = value;
				FlightArrivalDateInfo.RefreshBinding();
			}
		}

		bool SendFlightInfoToo_Not
		{
			get { return !SendFlightInfoToo; }
		}

		public bool ReadOnlyFlightDetails
		{
			get { return !Awb.HasSplits || Awb.NumberOfPiecesReceived == 0 || !HasFlightArrived; }
		}

		bool HasFlightArrived
		{
			get { return !FlightArrivalDate.IsEmpty; }
		}

		public void AllocatePiecesReceivedToDatabaseSplitsIfAllSplitsFound()
		{
			if (SendFlightInfoToo)
			{
				foreach (SplitConsignment awbSplit in Awb.Splits)
				{
					foreach (NonPersistentSplitLine split in SplitLines)
					{
						if (awbSplit.SplitReference == split.SplitNumber)
						{
							awbSplit.NumberOfPiecesReceived = (ZShort)split.NumberOfPieces;
							break;
						}
					}
				}
				Awb.Factory.Save();
			}
		}

		[BusinessObjectTestExclude]
		public NonPersistentSplitLineCollection SplitLines { get; set; }

		internal ICcsukCusAwb Awb;

		public void UpdateTotal()
		{
			TotalPieces = (from NonPersistentSplitLine l in SplitLines select (int)l.NumberOfPieces).Sum();
		}

		[ReadOnly(true)]
		public override ZInt TotalPieces
		{
			get { return base.TotalPieces; }
			set { base.TotalPieces = value; }
		}
	}
}
