using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher
{
	public class NonPersistentShipmentToHawbMatcherHeaderValidation : AutoNonPersistentShipmentToHawbMatcherHeaderValidation
	{
		public NonPersistentShipmentToHawbMatcherHeaderValidation(AutoNonPersistentShipmentToHawbMatcherHeader parent) : base(parent)
		{
			Parent = parent as NonPersistentShipmentToHawbMatcherHeader;
		}

		public new NonPersistentShipmentToHawbMatcherHeader Parent { get; set; }

		protected override void CheckSelectedMawbWrapperGUID()
		{
			base.CheckSelectedMawbWrapperGUID();

			if (Parent.SelectedMawbWrapperGUID.IsEmpty)
			{
				Parent.SelectedMawbWrapperGUIDInfo.AddError("You must select a MAWB from the list.");
			}
			else
			{
				var actionCode = Parent.SelectedMAWB?.CustomsActionCode ?? ZString.Empty;

				if (!actionCode.IsEmpty && actionCode != CustomsStatusCodes.Codes.EntryOrRequestCancelled)
				{
					Parent.SelectedMawbWrapperGUIDInfo.AddMessageError("This basic already has a customs action status and should not be selected.");
				}
			}
		}
	}
}
