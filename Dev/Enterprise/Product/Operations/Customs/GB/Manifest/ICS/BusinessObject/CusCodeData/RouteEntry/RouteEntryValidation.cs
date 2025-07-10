using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class RouteEntryValidation : Customs.Business.CusCodeDataValidation
	{
		public RouteEntryValidation(RouteEntry parent) : base(parent)
		{
		}

		new RouteEntry Parent => (RouteEntry)base.Parent;

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_DataInfo);
			HasAtLeastTwoItineraryRowsValidation();
		}

		protected override void CheckCY_Order()
		{
			base.CheckCY_Order();
			HasAtLeastTwoItineraryRowsValidation();
		}

		void HasAtLeastTwoItineraryRowsValidation()
		{
			var validationString = Res.GetString("990E94D2-ECFF-4316-8A1B-2F184E1E31C0", "At least two itinerary rows are required to show the routing of these goods from country of original departure to final destination.");
			if (Parent.Parent is AsycudaManifestHeaderSS header && !header.HasAtLeastTwoItineraryRows)
			{
				Parent.AddRowMessageError(validationString);
			}
			else
			{
				Parent.ClearRowNotificationsContaining(validationString);
			}
		}
	}
}
