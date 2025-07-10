using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentSplitLine : AutoNonPersistentSplitLine, ISplitLine
	{
		public NonPersistentSplitLine()
			: base(new BusinessObjectFactory())
		{
		}

		public NonPersistentSplitLine(ZInt splitNumber, BusinessObjectFactory factory)
			: base(factory)
		{
			SplitNumber = splitNumber.ToString("0#");
		}

		public NonPersistentSplitLine(ZInt splitNumber, ICcsukCusAwb awb)
			: this(splitNumber, awb.Factory)
		{
			this.Awb = awb;
		}

		internal ICcsukCusAwb Awb;

		public NonPersistentSplitLine(ZString splitNumber, ZString weightUQ, ZInt pieces, ZDecimal weightInKilos, ZString handlingDetail, ZString customsActionCode, BusinessObjectFactory factory)
			: base(factory)
		{
			HandlingDetail = handlingDetail.Left(NonPersistentSplitLine.Schema.HandlingDetailMaxLength);
			SplitNumber = splitNumber;
			Weight = weightInKilos;
			NumberOfPieces = pieces;
			CustomsActionCode = customsActionCode;
		}

		public NonPersistentSplitLine(ZString splitNumber, ZString weightUQ, ZInt pieces, ZDecimal weightInKilos, ZString handlingDetail, ZString customsActionCode, ICcsukCusAwb awb)
			: this(splitNumber, weightUQ, pieces, weightInKilos, handlingDetail, customsActionCode, awb.Factory)
		{
			this.Awb = awb;
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsAllocatingNpr))]
		public override ZString SplitNumber
		{
			get { return base.SplitNumber; }
			set
			{
				base.SplitNumber = value.KeepNumericCharacters();
				SplitNumberInfo.RefreshBinding();
			}
		}

		public override bool CanDelete
		{
			get { return !ReadOnly && !IsAllocatingNpr; }
		}

		MultilingualString reasonForNotAbleToDelete;
		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return reasonForNotAbleToDelete; }
		}

		public override bool ReadOnly
		{
			get
			{
				var isReadOnly = false;
				if (!CustomsActionCode.IsEmpty)
				{
					var helper = new CusAwbIsReadOnlyHelper(null);
					if (helper.CACsLocked_ColumnTwo.Contains(CustomsActionCode))
					{
						reasonForNotAbleToDelete = ResString.GetMultilingualString("FCBCF920-D0D9-4403-BDEA-C66F4A140824", "The split has a locking customs action code and cannot be removed");
						isReadOnly = true;
					}
				}
				return isReadOnly;
			}
			set { base.ReadOnly = value; }
		}

		public override ZInt NumberOfPieces
		{
			get { return base.NumberOfPieces; }
			set
			{
				base.NumberOfPieces = value;
				if (Awb != null && !Awb.Status1Date.IsEmpty)
				{
					NumberOfPiecesReceived = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateWeight();
				}
			}
		}

		public ZString LineOrSplitNumber
		{
			get { return SplitNumber; }
		}

		public ZShort NumberOfPiecesExpected
		{
			get { return (ZShort)NumberOfPieces; }
		}

		public ZString DescriptionOfGoods
		{
			get { return HandlingDetail; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WeightUQ = Constants.Weight.Kilograms;
		}

		public ZString FormatForGenral()
		{
			var result = ZString.Format("  SRF {0}, {1} Piece(s), Wgt {2}{3}", SplitNumber, NumberOfPieces, Weight.ToStringTrimZeros(), WeightUQ).Left(70); // it will never be more than 70 chars as these fields cannot each hold enough... but just in cases
			if (!HandlingDetail.IsEmpty)
			{
				result = result + "\r\n   " + HandlingDetail.Left(67);  // it will never be more than 70 chars as HandlingDetail can only be 38 chars... but just in cases.  67 is 70 less the three leading spaces
			}
			return result;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Split"; }
		}

		[ReadOnlyMember(nameof(IsAllocatingNpr))]
		public override ZDecimal Weight
		{
			get { return base.Weight; }
			set { base.Weight = value; }
		}

		[ReadOnlyMember(nameof(IsAllocatingNpr))]
		public override ZString WeightUQ
		{
			get { return base.WeightUQ; }
			set { base.WeightUQ = value; }
		}

		bool IsAllocatingNpr
		{
			get { return ParentCollection != null && ParentCollection.IsAllocatingNprWithFlightData; }
		}

		[ReadOnlyMember(nameof(NumberOfPiecesReceivedReadOnly))]
		public override ZInt NumberOfPiecesReceived
		{
			get { return base.NumberOfPiecesReceived; }
			set { base.NumberOfPiecesReceived = value; }
		}

		bool NumberOfPiecesReceivedReadOnly
		{
			get { return Awb != null && !LicenceAndPimaHelper.IsFullShed(Awb) && !LicenceAndPimaHelper.IsFallbackShed(Awb); }
		}

		[List(nameof(WarehouseLocations))]
		public override ZGuid WarehouseLocationID
		{
			get { return base.WarehouseLocationID; }
			set { base.WarehouseLocationID = value; }
		}

		public WhsLocationCollection WarehouseLocations
		{
			get { return Awb != null ? CusOutTurn.GetWarehouseLocations(Awb) : new WhsLocationCollection(Factory); }
		}

		internal NonPersistentSplitLineCollection ParentCollection;

		internal bool IsDeletingOrRemovingFromCollection;
	}
}
