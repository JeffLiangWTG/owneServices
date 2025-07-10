using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierGoodsItemData : INCTSPrettierGoodsItemData, IEquatable<NCTSPrettierGoodsItemData>
	{
		public NCTSPrettierGoodsItemData(ZString itemNumber, ZString ucrReference, ZString description, ZString harmonizedSubHeadingCode)
		{
			ItemNumber = itemNumber;
			UCRReference = ucrReference;
			Description = description;
			HarmonizedSubHeadingCode = harmonizedSubHeadingCode;
		}

		public override bool Equals(object obj)
			=> obj is NCTSPrettierGoodsItemData other && Equals(other);

		public bool Equals(NCTSPrettierGoodsItemData other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return ItemNumber.Equals(other.ItemNumber)
				&& UCRReference.Equals(other.UCRReference)
				&& Description.Equals(other.Description)
				&& HarmonizedSubHeadingCode.Equals(other.HarmonizedSubHeadingCode);
		}

		public override int GetHashCode() => (ItemNumber, UCRReference, Description, HarmonizedSubHeadingCode).GetHashCode();

		public ZString ItemNumber { get; }
		public ZString UCRReference { get; }
		public ZString Description { get; }
		public ZString HarmonizedSubHeadingCode { get; }
	}
}
