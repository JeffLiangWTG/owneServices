namespace Enterprise.BufferManagement.Business
{
	public class AcceptabilityBandBoundaryValues
	{
		public AcceptabilityBandBoundaryValues(int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax,
			int cautionMax)
		{
			CautionMin = cautionMin;
			GoodMin = goodMin;
			ExcellentMin = excellentMin;
			ExcellentMax = excellentMax;
			GoodMax = goodMax;
			CautionMax = cautionMax;
		}

		public int CautionMin { get; }
		public int GoodMin { get; }
		public int ExcellentMin { get; }
		public int ExcellentMax { get; }
		public int GoodMax { get; }
		public int CautionMax { get; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((AcceptabilityBandBoundaryValues)obj);
		}

		protected bool Equals(AcceptabilityBandBoundaryValues other)
		{
			return
				CautionMin == other.CautionMin &&
				GoodMin == other.GoodMin &&
				ExcellentMin == other.ExcellentMin &&
				ExcellentMax == other.ExcellentMax &&
				GoodMax == other.GoodMax &&
				CautionMax == other.CautionMax;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = CautionMin;
				hashCode = hashCode * 397 ^ GoodMin;
				hashCode = hashCode * 397 ^ ExcellentMin;
				hashCode = hashCode * 397 ^ ExcellentMax;
				hashCode = hashCode * 397 ^ GoodMax;
				hashCode = hashCode * 397 ^ CautionMax;
				return hashCode;
			}
		}
	}
}
