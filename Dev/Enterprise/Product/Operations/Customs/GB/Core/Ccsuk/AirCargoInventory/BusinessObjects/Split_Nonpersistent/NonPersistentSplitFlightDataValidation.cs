namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentSplitFlightDataValidation : AutoNonPersistentSplitFlightDataValidation
	{
		public NonPersistentSplitFlightDataValidation(AutoNonPersistentSplitFlightData parent)
			: base(parent)
		{ }

		public new NonPersistentSplitsAndFlightData Parent
		{
			get { return (NonPersistentSplitsAndFlightData)base.Parent; }
		}

		protected override void CheckTotalPieces()
		{
			base.CheckTotalPieces();
			if (Parent.Awb != null)
			{
				if (Parent.Awb.NumberOfPiecesExpected != Parent.TotalPieces)
				{
					Parent.TotalPiecesInfo.AddError("Total piece count does not match AWB's NPX");
				}
			}
		}
	}
}
