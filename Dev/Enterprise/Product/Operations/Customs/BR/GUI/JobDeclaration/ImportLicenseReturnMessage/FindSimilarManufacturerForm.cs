using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public class FindSimilarManufacturerForm : FindSimilarOrganisationForm
	{
		public FindSimilarManufacturerForm(OrganisationFinder finder) : base(finder)
		{
		}

		public override string FormHeading => Res.GetString("02069c15-be59-42f9-9443-4f8dee34e382", "Select Manufacturer");

		protected override string HeadingLabelText => Res.GetString("bde7a3c8-4659-4e59-bbfc-adb6bb0a8f74", "Cannot find manufacturer. No manufacturer code provided.");

		protected override ResourceStringData OrganisationLabelResourceString => Res.GetData("1992d2e8-dbe4-427f-8211-19da00d5b02e", "Organizations similar to the manufacturer details provided. Select an organization to set the manufacturer.");
	}
}
