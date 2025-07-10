using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentErtsRelease : NonPersistentRelease
	{
		public NonPersistentErtsRelease(ICcsukCusAwb awb) : base(awb)
		{
		}

		protected override Event EventCode => NumberOfPiecesReleasedHelper.ShedEvent;
	}
	public class NonPersistentC1Release : NonPersistentRelease
	{
		public NonPersistentC1Release(ICcsukCusAwb awb) : base(awb)
		{
		}

		protected override Event EventCode => NumberOfPiecesReleasedHelper.AgentC1Event;
	}

	public abstract class NonPersistentRelease : AutoNonPersistentC1Release
	{
		protected NonPersistentRelease(ICcsukCusAwb awb)
			: base(awb.Factory)
		{
			Awb = awb;
			Validation.ValidateStatus2Granted();
		}

		[ReadOnlyMember(nameof(NumberOfPiecesReadOnly))]
		public override ZInt NumberOfPieces
		{
			get { return base.NumberOfPieces; }
			set
			{
				if (value > PiecesRemainingToRelease)
				{
					base.NumberOfPieces = PiecesRemainingToRelease;
				}
				else
				{
					base.NumberOfPieces = value;
				}
				NumberOfPiecesInfo.RefreshBinding();
			}
		}

		public bool NumberOfPiecesReadOnly
		{
			get;
			set;
		}

		public ZString LogOfReleases
		{
			get { return NumberOfPiecesReleasedHelper.ReleaseDatesAndCountsFormatted(Awb, EventCode); }
		}

		protected abstract Event EventCode { get; }

		public int PiecesRemainingToRelease
		{
			get { return Awb.NumberOfPiecesReceived - Awb.NumberOfPiecesReleasedSoFarCumulative(EventCode); }
		}

		[ReadOnly(true)]
		[BusinessObjectMaxLengthTestExclude] // Otherwise test tries to set "AAAAAAA" and cries like a little girl when it gets back "Granted"
		public override ZString Status2Granted
		{
			get { return Awb.Status2Granted ? "Granted" : "Revoked"; }
		}

		internal ICcsukCusAwb Awb;
	}
}
