using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("FormattedDetentionDays")]
	[WrapperTypeName("Detention Information")]
	public class DetentionWrapper : GenericWrapper
	{
		public DetentionWrapper(ZDateTime released, ZDateTime lastFreeDay, ZDateTime returned, BusinessObjectFactory factory)
			: this(released, lastFreeDay, returned, ZString.Empty, factory)
		{
		}

		public DetentionWrapper(ZDateTime released, ZDateTime lastFreeDay, ZDateTime returned, ZString portUNLOCO, BusinessObjectFactory factory)
			: base(null, factory)
		{
			Released = released;
			LastFreeDay = lastFreeDay;
			Returned = returned;
			Port = new LocationWrapper(portUNLOCO, Factory);
			FreeDays = CountDaysInclusive(released, lastFreeDay);

			if (lastFreeDay.IsValid && returned.IsValid)
			{
				DetentionDays = Math.Max((returned - lastFreeDay).Days, 0);
			}
		}

		public DetentionWrapper(ZDateTime released, ZInt freeDays, ZDateTime returned, BusinessObjectFactory factory)
			: this(released, freeDays, returned, ZString.Empty, factory)
		{
		}

		public DetentionWrapper(ZDateTime released, ZInt freeDays, ZDateTime returned, ZString portUNLOCO, BusinessObjectFactory factory)
			: base(null, factory)
		{
			Released = released;
			FreeDays = Math.Max(freeDays, 0);
			Returned = returned;
			Port = new LocationWrapper(portUNLOCO, Factory);
			DetentionDays = Math.Max(CountDaysInclusive(released, returned) - FreeDays, 0);

			if (released.IsValid && freeDays > 0)
			{
				LastFreeDay = released.AddDays(Math.Max(freeDays - 1, 0));
			}
		}

		public DetentionWrapper(ZDateTime released, ZDateTime lastFreeDay, ZDateTime returned, ZInt detentionDays, ZString portUNLOCO, BusinessObjectFactory factory)
			: base(null, factory)
		{
			Released = released;
			LastFreeDay = lastFreeDay;
			Returned = returned;
			DetentionDays = detentionDays;
			FreeDays = CountDaysInclusive(released, lastFreeDay);
			Port = new LocationWrapper(portUNLOCO, Factory);
		}

		DetentionWrapper()
			: base(null, null) { }

		public static DetentionWrapper Empty
		{
			get { return new DetentionWrapper(); }
		}

		public ZString FormattedDetentionDays
		{
			get
			{
				switch (DetentionDays)
				{
					case 0:
						return ZString.Empty;
					case 1:
						return Res.GetString("5bdf640f-68c9-43f7-9763-9808f6a62b8c", "1 day");
					default:
						return Res.GetString("4b400b8c-c7c3-4066-a39b-937efb1e601e", "{0} days", DetentionDays);
				}
			}
		}

		public ZDateTime Released { get; private set; }
		public ZInt FreeDays { get; private set; }
		public ZDateTime LastFreeDay { get; private set; }
		public ZInt DetentionDays { get; private set; }
		public ZDateTime Returned { get; private set; }
		public LocationWrapper Port { get; private set; }

		public static int CountDaysInclusive(ZDateTime fromDate, ZDateTime toDate)
		{
			if (fromDate.IsValid && toDate.IsValid)
			{
				return Math.Max((int)(toDate.Date - fromDate.Date).TotalDays, 0) + 1;
			}
			else
			{
				return 0;
			}
		}

		public ZBool IsOverdue
		{
			get { return GapDays > 0; }
		}

		public ZBool IsNearDue
		{
			get { return GapDays <= 0 && -GapDays < AgencyRegistry.Instance.DetentionAdviceWarningDays.Value - 1; }
		}

		public ZBool IsNotDue
		{
			get { return GapDays < 0 && -GapDays >= AgencyRegistry.Instance.DetentionAdviceWarningDays.Value - 1; }
		}

		public ZInt OverdueDays
		{
			get { return Math.Max(0, GapDays); }
		}

		ZInt GapDays
		{
			get
			{
				if (!gapDays.HasValue)
				{
					gapDays = GetGapDays();
				}
				return gapDays.Value;
			}
		}
		ZInt? gapDays;

		ZInt GetGapDays()
		{
			if (LastFreeDay.IsEmpty)
			{
				return 0;
			}
			else
			{
				ZDateTime toDate = Returned.IsEmpty ? ZDateTime.Now : Returned;
				return (int)(toDate - LastFreeDay).TotalDays;
			}
		}
	}
}
