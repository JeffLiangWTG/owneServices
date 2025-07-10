using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageLineProvider : ITempStorageLine, IEquatable<CusTempStorageLineProvider>
	{
		public CusTempStorageLineProvider(CusTempStorageLine storageLine)
		{
			this.storageLine = Argument.NotNull(storageLine, nameof(storageLine));
			LineNumber = this.storageLine.TSL_LineNo;
			ReferenceNumber = this.storageLine.TSL_ReferenceNumber;
			ReferenceNumberLine = this.storageLine.TSL_ReferenceNumberLine;
			ReferenceNumber2 = this.storageLine.TSL_ReferenceNumber2;
			ReferenceNumber2Line = this.storageLine.TSL_ReferenceNumber2Line;
			UnionStatus = this.storageLine.TSL_UnionStatus;
			OwnerReferenceType = this.storageLine.TSL_OwnerReferenceType;
			OwnerReferenceNumber = this.storageLine.TSL_OwnerReferenceNumber;
			GoodsDescription = this.storageLine.TSL_GoodsDescription;
			GoodsType = this.storageLine.TSL_GoodsType;
			GrossWeight = this.storageLine.TSL_GrossWeight.Round(3).Normalize();
			DepartureCountry = this.storageLine.TSL_RN_NKDepartureCountry;
			DestinationPlace = this.storageLine.TSL_DestinationPlace;
			LocationOfGoods = this.storageLine.TSL_LocationOfGoods;
			IsFTZ = this.storageLine.TSL_IsFTZ;
			CustodianEoriNumber = this.storageLine.TSL_CustodianIdentifier;
			CustodianEoriBranch = this.storageLine.TSL_CustodianIdentifierBranchNo;
			DisposalEntitledTraderEoriNumber = this.storageLine.TSL_GoodsOwnerIdentifier;
			DisposalEntitledTraderEoriBranch = this.storageLine.TSL_GoodsOwnerIdentifierBranchNo;
			PackageType = this.storageLine.TSL_PackageType;
			PackageQuantity = this.storageLine.TSL_PackageQty;
		}
		protected readonly CusTempStorageLine storageLine;

		public int LineNumber { get; }

		public string ReferenceNumber { get; }

		public int ReferenceNumberLine { get; }

		public string ReferenceNumber2 { get; }

		public int ReferenceNumber2Line { get; }

		public string UnionStatus { get; }

		public string OwnerReferenceType { get; }

		public string OwnerReferenceNumber { get; }

		public string GoodsDescription { get; }

		public string GoodsType { get; }

		public decimal GrossWeight { get; }

		public string DepartureCountry { get; }

		public string DestinationPlace { get; }

		public string LocationOfGoods { get; }

		public bool IsFTZ { get; }

		public string CustodianEoriNumber { get; }

		public string CustodianEoriBranch { get; }

		public string DisposalEntitledTraderEoriNumber { get; }

		public string DisposalEntitledTraderEoriBranch { get; }

		public string PackageType { get; }

		public int PackageQuantity { get; }

		public static bool operator ==(CusTempStorageLineProvider left, CusTempStorageLineProvider right)
		{
			return left?.Equals(right) ?? right is null;
		}

		public static bool operator !=(CusTempStorageLineProvider left, CusTempStorageLineProvider right)
		{
			return !(left == right);
		}

		public bool Equals(CusTempStorageLineProvider other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return LineNumber.Equals(other.LineNumber)
					&& ReferenceNumber.Equals(other.ReferenceNumber)
					&& ReferenceNumberLine.Equals(other.ReferenceNumberLine)
					&& ReferenceNumber2.Equals(other.ReferenceNumber2)
					&& ReferenceNumber2Line.Equals(other.ReferenceNumber2Line)
					&& UnionStatus.Equals(other.UnionStatus)
					&& OwnerReferenceType.Equals(other.OwnerReferenceType)
					&& OwnerReferenceNumber.Equals(other.OwnerReferenceNumber)
					&& GoodsDescription.Equals(other.GoodsDescription)
					&& GoodsType.Equals(other.GoodsType)
					&& GrossWeight.Equals(other.GrossWeight)
					&& DepartureCountry.Equals(other.DepartureCountry)
					&& DestinationPlace.Equals(other.DestinationPlace)
					&& LocationOfGoods.Equals(other.LocationOfGoods)
					&& IsFTZ.Equals(other.IsFTZ)
					&& CustodianEoriNumber.Equals(other.CustodianEoriNumber)
					&& CustodianEoriBranch.Equals(other.CustodianEoriBranch)
					&& DisposalEntitledTraderEoriNumber.Equals(other.DisposalEntitledTraderEoriNumber)
					&& DisposalEntitledTraderEoriBranch.Equals(other.DisposalEntitledTraderEoriBranch)
					&& PackageType.Equals(other.PackageType)
					&& PackageQuantity.Equals(other.PackageQuantity);
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is CusTempStorageLineProvider other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = LineNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ ReferenceNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ ReferenceNumberLine.GetHashCode();
				hashCode = (hashCode * 397) ^ ReferenceNumber2.GetHashCode();
				hashCode = (hashCode * 397) ^ ReferenceNumber2Line.GetHashCode();
				hashCode = (hashCode * 397) ^ UnionStatus.GetHashCode();
				hashCode = (hashCode * 397) ^ OwnerReferenceType.GetHashCode();
				hashCode = (hashCode * 397) ^ OwnerReferenceNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ GoodsDescription.GetHashCode();
				hashCode = (hashCode * 397) ^ GoodsType.GetHashCode();
				hashCode = (hashCode * 397) ^ GrossWeight.GetHashCode();
				hashCode = (hashCode * 397) ^ DepartureCountry.GetHashCode();
				hashCode = (hashCode * 397) ^ DestinationPlace.GetHashCode();
				hashCode = (hashCode * 397) ^ LocationOfGoods.GetHashCode();
				hashCode = (hashCode * 397) ^ IsFTZ.GetHashCode();
				hashCode = (hashCode * 397) ^ CustodianEoriNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ CustodianEoriBranch.GetHashCode();
				hashCode = (hashCode * 397) ^ DisposalEntitledTraderEoriNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ DisposalEntitledTraderEoriBranch.GetHashCode();
				hashCode = (hashCode * 397) ^ PackageType.GetHashCode();
				hashCode = (hashCode * 397) ^ PackageQuantity.GetHashCode();
				return hashCode;
			}
		}
	}
}
