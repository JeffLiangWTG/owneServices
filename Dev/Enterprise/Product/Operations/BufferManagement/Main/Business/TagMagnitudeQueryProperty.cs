using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class TagMagnitudeQueryProperty : TagQueryProperty
	{
		public TagMagnitudeQueryProperty(ZGuid pk, bool notIn, bool includeInherited) : base(pk, notIn, includeInherited)
		{
		}

		public TagMagnitude Magnitude { get; set; }
		public bool IsExclusive => Magnitude?.Definition?.TGD_IsExclusive ?? true;

		protected override TagGroupings GetGroupTypeCore()
		{
			if (IsExclusive)
			{
				return TagGroupings.IsExclusive;
			}

			return base.GetGroupTypeCore();
		}
	}
}
