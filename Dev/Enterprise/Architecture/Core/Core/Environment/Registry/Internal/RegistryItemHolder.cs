using System;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.Core.Environment.Internal
{
	public sealed class RegistryItemHolder
	{
		public RegistryItemHolder(IRegistryItem item)
		{
			Argument.NotNull(item, nameof(item));
			this.item = item;
			lastUsed = new UberWatch();
			elapsedSinceLastUse = TimeSpan.Zero;
		}

		public IRegistryItem Item
		{
			get
			{
				lastUsed.Restart();
				elapsedSinceLastUse = TimeSpan.Zero;
				return item;
			}
		}

		public int CompareTo(RegistryItemHolder other)
		{
			return ElapsedSinceLastUse.TotalMilliseconds.CompareTo(other.ElapsedSinceLastUse.TotalMilliseconds);
		}

		internal TimeSpan ElapsedSinceLastUse
		{
			get
			{
				return elapsedSinceLastUse == TimeSpan.Zero ? TimeSpan.FromMilliseconds(lastUsed.ElapsedMilliseconds) : elapsedSinceLastUse;
			}
			set
			{
				elapsedSinceLastUse = value;
			}
		}

		internal bool IsOlderThan(TimeSpan ageSinceLastAccess)
		{
			return ElapsedSinceLastUse >= ageSinceLastAccess;
		}

		TimeSpan elapsedSinceLastUse;
		readonly IRegistryItem item;
		internal UberWatch lastUsed;
	}
}
