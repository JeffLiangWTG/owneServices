using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierConsignmentData : INCTSPrettierConsignmentData, IEquatable<NCTSPrettierConsignmentData>
	{
		public NCTSPrettierConsignmentData(ZString ucrReference) : this(ucrReference, Array.Empty<INCTSPrettierGoodsItemData>())
		{ }

		public NCTSPrettierConsignmentData(ZString ucrReference, IReadOnlyCollection<INCTSPrettierGoodsItemData> goodsItems)
		{
			UCRReference = ucrReference;
			GoodsItems = Argument.NotNull(goodsItems, nameof(goodsItems));
		}

		public override bool Equals(object obj) => obj is NCTSPrettierConsignmentData other && Equals(other);

		public bool Equals(NCTSPrettierConsignmentData other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return UCRReference.Equals(other.UCRReference)
				&& GoodsItems.SequenceEqual(other.GoodsItems);
		}

		public override int GetHashCode() => (UCRReference, GoodsItems).GetHashCode();

		public ZString UCRReference { get; }
		public IReadOnlyCollection<INCTSPrettierGoodsItemData> GoodsItems { get; }
	}
}
