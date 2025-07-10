using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentSplitLineValidation : AutoNonPersistentSplitLineValidation
	{
		public NonPersistentSplitLineValidation(AutoNonPersistentSplitLine parent)
			: base(parent)
		{ }

		public new NonPersistentSplitLine Parent
		{
			get { return (NonPersistentSplitLine)base.Parent; }
		}

		protected override void CheckSplitNumber()
		{
			base.CheckSplitNumber();
			MandatoryValidation.CheckEntered(Parent.SplitNumberInfo);
			CheckSplitNumberingSequence(Parent.SplitNumberInfo);
		}

		void CheckSplitNumberingSequence(ZPropertyInfo splitNumberInfo)
		{
			if (Parent.ParentCollection != null)
			{
				var orderedSplits = Parent.ParentCollection.OfType<NonPersistentSplitLine>().OrderBy(s => s.SplitNumber);
				var lastNumber = 0;
				foreach (var split in orderedSplits.Where(s => !s.IsDeletingOrRemovingFromCollection))
				{
					var thisNumber = 0;
					var parsedOk = int.TryParse(split.SplitNumber, out thisNumber);
					if (!parsedOk)
					{
						splitNumberInfo.AddError("Split numbers must be numeric");
						break;
					}
					var nextNumber = lastNumber + 1;
					if (nextNumber != thisNumber)
					{
						splitNumberInfo.AddError(string.Format(CultureInfo.CurrentCulture, "Split numbers must be sequential. Split number {0} is missing.", nextNumber.ToString("0#", CultureInfo.CurrentCulture)));
					}
					else
					{
						split.Validation.ValidateSplitNumber();
					}
					lastNumber = thisNumber;
				}
			}
		}

		protected override void CheckWeight()
		{
			base.CheckWeight();
			if (!Parent.NumberOfPieces.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.WeightInfo);
			}
		}

		protected override void CheckWeightUQ()
		{
			base.CheckWeightUQ();
			MandatoryValidation.CheckEntered(Parent.WeightUQInfo);
			if (Parent.WeightUQ != Core.Constants.Weight.Kilograms)
			{
				Parent.WeightUQInfo.AddError("Weight must be in kilograms");
			}
		}

		protected override void CheckNumberOfPieces()
		{
			base.CheckNumberOfPieces();
			if (Parent.NumberOfPieces == 0)
			{
				Parent.NumberOfPiecesInfo.AddWarning("Setting the number of pieces on this split to zero means you are reducing the split and removing this item.");
			}
			ValidateHandlingDetail();
		}

		protected override void CheckHandlingDetail()
		{
			base.CheckHandlingDetail();
			if (Parent.HandlingDetail.IsEmpty && Parent.Awb != null && !LicenceAndPimaHelper.IsSimpleAgentProfile(Parent.Awb))
			{
				Parent.HandlingDetailInfo.AddError("Please enter marks & numbers");
			}
		}

		protected override void CheckWarehouseLocationID()
		{
			base.CheckWarehouseLocationID();
			if (Parent.WarehouseLocationID.IsEmpty && Parent.NumberOfPiecesReceived > 0)
			{
				Parent.WarehouseLocationIDInfo.AddError("Please record an SSL if NPR is set");
			}
			ValidateNumberOfPiecesReceived();
		}

		protected override void CheckNumberOfPiecesReceived()
		{
			base.CheckNumberOfPiecesReceived();
			if (Parent.NumberOfPiecesReceived == 0)
			{
				if (!Parent.WarehouseLocationID.IsEmpty)
				{
					Parent.NumberOfPiecesReceivedInfo.AddError("Please set NPR since SSL is set");
				}
			}
			ValidateWarehouseLocationID();
		}
	}
}
