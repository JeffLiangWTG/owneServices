namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher
{
	public class NonPersistentShipmentToHawbMatcherLineValidation : AutoNonPersistentShipmentToHawbMatcherLineValidation
	{
		public NonPersistentShipmentToHawbMatcherLineValidation(AutoNonPersistentShipmentToHawbMatcherLine parent)
			: base(parent)
		{ }

		public new NonPersistentShipmentToHawbMatcherLine Parent
		{
			get { return (NonPersistentShipmentToHawbMatcherLine)base.Parent; }
		}

		protected override void CheckCS()
		{
			base.CheckCS();
			foreach (NonPersistentShipmentToHawbMatcherLine brother in Parent.Header.Pivots)
			{
				if (brother != Parent)
				{
					if (brother.CS == Parent.CS && !Parent.CS.IsEmpty)
					{
						Parent.CSInfo.AddError("HAWB is already marked to be linked to another shipment");
					}
				}
			}

			if (Parent.CS.IsEmpty && !Parent.CreateNewHawb)
			{
				Parent.CSInfo.AddMessageError("Please select an existing HAWB or choose to create a new one");
			}
			ValidateCreateNewHawb();
		}

		protected override void CheckCreateNewHawb()
		{
			base.CheckCreateNewHawb();
			if (Parent.CS.IsEmpty && !Parent.CreateNewHawb)
			{
				Parent.CreateNewHawbInfo.AddMessageError("Please select an existing HAWB or choose to create a new one");
			}
			if (Parent.HawbNumber.IsEmpty && Parent.CreateNewHawb)
			{
				Parent.CreateNewHawbInfo.AddError("Please choose a HAWB number for the new record or untick the 'create new' box");
			}
			ValidateCS();
			ValidateHawbNumber();

			// Daniel says:
			// Maybe add red validation here, to prevent any new hawbs from being created, if the basic already has a split.
			// No red validation if the user is going to choose to delete the local splits.
			//     (i.e. if "Header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR")     //New Bool field on header xml.

			if (Parent.CreateNewHawb)
			{
				var header = Parent.Header;
				if ((!header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR) && (header.SelectedMAWB?.Splits.Count > 0))
				{
					Parent.CreateNewHawbInfo.AddError("You cannot create new HAWBs when splits exist. First tick the box to 'Remove all splits from basic'");
				}
			}
		}

		protected override void CheckHawbNumber()
		{
			base.CheckHawbNumber();
			if (Parent.HawbNumber.IsEmpty && Parent.CreateNewHawb)
			{
				Parent.HawbNumberInfo.AddError("Please choose a HAWB number for the new record or untick the 'create new' box");
			}
			ValidateCreateNewHawb();
		}
	}
}
