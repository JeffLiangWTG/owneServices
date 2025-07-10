using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class TagQueryProperty
	{
		public TagQueryProperty(ZGuid pk, bool notIn, bool includeInherited)
		{
			this.PK = pk;
			this.NotIn = notIn;
			this.IncludeInherited = includeInherited;
		}

		public ZGuid PK { get; }

		public bool NotIn { get; }
		public bool IncludeInherited { get; }

		public TagGroupings GroupType => GetGroupTypeCore();

		protected virtual TagGroupings GetGroupTypeCore()
		{
			if (IncludeInherited)
			{
				return TagGroupings.IncludeInherited;
			}

			if (NotIn)
			{
				return TagGroupings.IsNotIn;
			}

			return TagGroupings.IsIn;
		}
	}
}
