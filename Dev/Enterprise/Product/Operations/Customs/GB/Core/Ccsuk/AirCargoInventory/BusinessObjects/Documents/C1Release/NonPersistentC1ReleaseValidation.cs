
namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentC1ReleaseValidation : AutoNonPersistentC1ReleaseValidation
	{
		public NonPersistentC1ReleaseValidation(AutoNonPersistentC1Release parent)
			: base(parent)
		{ }

		protected override void CheckNumberOfPieces()
		{
			base.CheckNumberOfPieces();
			if (Parent.NumberOfPieces < 1)
			{
				Parent.NumberOfPiecesInfo.AddError("You must select at least one piece to release");
			}
		}

		protected override void CheckStatus2Granted()
		{
			base.CheckStatus2Granted();
			if (LicenceAndPimaHelper.IsFullShed(Parent.Awb) && !Parent.Awb.Status2Granted)
			{
				Parent.Status2GrantedInfo.AddWarning("Note that status 2 is not set on this AWB. Release with caution.");
			}
		}

		public new NonPersistentRelease Parent
		{
			get { return (NonPersistentRelease)base.Parent; }
		}
	}
}
