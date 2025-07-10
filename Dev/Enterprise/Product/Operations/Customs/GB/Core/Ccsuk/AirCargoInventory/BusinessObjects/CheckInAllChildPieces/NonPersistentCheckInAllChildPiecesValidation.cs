using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentCheckInAllChildPiecesValidation : AutoNonPersistentCheckInAllChildPiecesValidation
	{
		public NonPersistentCheckInAllChildPiecesValidation(AutoNonPersistentCheckInAllChildPieces parent) : base(parent)
		{
		}

		protected override void CheckShedStorageLocationId()
		{
			base.CheckShedStorageLocationId();
			ListValidation.ErrorIfInvalidPK(Parent.ShedStorageLocationIdInfo);
		}

		public new NonPersistentCheckInAllChildPieces Parent
		{
			get { return (NonPersistentCheckInAllChildPieces)base.Parent; }
		}

		protected override void CheckMarksAndNumbers()
		{
			base.CheckMarksAndNumbers();
			if (Parent.MarksAndNumbers.IsEmpty && Parent.AnyChildBillHasSplits())
			{
				Parent.MarksAndNumbersInfo.AddError("Splits exist, marks & numbers are required");
			}
		}

		override protected void CheckPackagesUnits()
		{
			base.CheckPackagesUnits();
			if (!Parent.PackagesUnits.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.PackagesUnitsInfo);
			}
		}
	}
}
