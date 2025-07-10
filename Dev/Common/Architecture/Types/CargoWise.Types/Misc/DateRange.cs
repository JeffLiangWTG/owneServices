using System;

namespace CargoWise.Types
{
	public struct DateRange
	{
		public DateRange(DateTime d1, DateTime d2)
		{
			if (d2 <= d1)
			{
				Start = d2.Date;
				End = d1.Date;
			}
			else
			{
				Start = d1.Date;
				End = d2.Date;
			}
		}

		public DateTime Start { get; }
		public DateTime End { get; }

		public bool Contains(DateTime dt) =>
			Start <= dt && dt < End.AddDays(1);

		public override string ToString() =>
			$"DateRange {{ {Start.ToString("d")} - {End.ToString("d")} }}";

		public override bool Equals(object obj) => base.Equals(obj);

		public override int GetHashCode() => ToString().GetHashCode();

		public static bool operator ==(DateRange a, DateRange b) => a.Equals(b);
		public static bool operator !=(DateRange a, DateRange b) => !a.Equals(b);
	}
}
