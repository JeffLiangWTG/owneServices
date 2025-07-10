using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSPrettierGoodsReferenceData : INCTSGoodsReference, IEquatable<NCTSPrettierGoodsReferenceData>
	{
		public NCTSPrettierGoodsReferenceData(ZString sequenceNumber, ZString declarationGoodsItemNumber)
		{
			SequenceNumber = int.TryParse(sequenceNumber, out var sequenceNo) ? sequenceNo : SequenceNumber;
			DeclarationGoodsItemNumber = int.TryParse(declarationGoodsItemNumber, out var goodsItemNumber) ? goodsItemNumber : DeclarationGoodsItemNumber;
		}

		public override bool Equals(object obj) => obj is NCTSPrettierGoodsReferenceData other && Equals(other);

		public bool Equals(NCTSPrettierGoodsReferenceData other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return SequenceNumber.Equals(other.SequenceNumber)
				&& DeclarationGoodsItemNumber.Equals(other.DeclarationGoodsItemNumber);
		}

		public override int GetHashCode() => (SequenceNumber, DeclarationGoodsItemNumber).GetHashCode();

		public int SequenceNumber { get; }

		public int DeclarationGoodsItemNumber { get; }
	}
}
