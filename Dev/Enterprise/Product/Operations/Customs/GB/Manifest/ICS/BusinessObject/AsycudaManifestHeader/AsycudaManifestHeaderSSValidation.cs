using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Manifest.Business;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class AsycudaManifestHeaderSSValidation : ICSAsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderSSValidation(EU.Manifest.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}

		protected new AsycudaManifestHeaderSS Parent => (AsycudaManifestHeaderSS)base.Parent;

		protected override bool IsVoyageMandatory => Parent.IsRail;

		protected bool IsVehicleRegistrationNumberMandatory => Parent.IsRoad;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateHasAtLeastTwoItineraryRows();
		}

		public void ValidateHasAtLeastTwoItineraryRows()
		{
			ValidateCalculatedProperty(Parent.HasAtLeastTwoItineraryRowsInfo);
		}

		protected void CheckHasAtLeastTwoItineraryRows()
		{
			if (Parent.Itinerary.Count == 0)
			{
				Parent.HasAtLeastTwoItineraryRowsInfo.AddMessageError(Res.GetString("27BE8E3C-8BE7-4F86-9922-FD603938F81E", "At least two itinerary rows are required to show the routing of these goods from country of original departure to final destination. Please add the required data on the Itinerary tab."));
			}
		}

		protected override void CheckAMA_RN_NKConveyanceNationality()
		{
			if (Parent.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.AMA_RN_NKConveyanceNationalityInfo);
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_RN_NKConveyanceNationalityInfo);
			}
		}

		protected override void CheckAMA_VehicleRegistrationCore()
		{
			if (Parent.AMA_VehicleRegistration.IsEmpty && IsVehicleRegistrationNumberMandatory)
			{
				Parent.AMA_VehicleRegistrationInfo.AddMessageError(ResString.GetMultilingualString("E5F312CD-CE7F-4BB4-AE19-A4BD08F2D0D0", "Vehicle Registration Number is required for Transport Mode ") + Parent.AMA_TransportModeDescription);
			}
		}

		protected bool IsLloydsNumberMandatory => Parent.IsSea || Parent.IsInlandWaterway;

		protected override void CheckAMA_LloydsNumber()
		{
			base.CheckAMA_LloydsNumber();
			CheckAMA_LloydsNumberCore();
		}

		protected virtual void CheckAMA_LloydsNumberCore()
		{
			if (Parent.AMA_LloydsNumber.IsEmpty && IsLloydsNumberMandatory)
			{
				Parent.AMA_LloydsNumberInfo.AddMessageError(ResString.GetMultilingualString("F976DD9A-3D67-44BA-8E49-94DD37AAB827", "Vessel IMO Number is required for Transport Mode ") + Parent.AMA_TransportModeDescription);
			}
		}

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();
			var customsOffice = Parent.AMA_CustomsOffice;
			if (!customsOffice.IsEmpty && Parent.EUCustomsOffices.Cast<IcsOfficeCode>().Any(o => o.CY_Data == customsOffice && o.CY_Code == OfficeCodes_ICS.Codes.OfficeOfFirstEntry))
			{
				Parent.AMA_CustomsOfficeInfo.AddMessageError(ResString.GetMultilingualString("32586050-4660-4553-B893-535D437C98B9", "The Customs Office of Lodgement should be present if different from the Office of First Entry (OOF) otherwise it should not be provided."));
			}
		}

		protected override void CheckAMA_VoyageMandatory()
		{
			var parent = Parent;
			if (Parent.AMA_Voyage.IsEmpty)
			{
				Parent.AMA_VoyageInfo.AddMessageError(ResString.GetMultilingualString("4A61F42A-4BEC-4688-9A6A-89DB7EA2B676", "Please enter the Voyage Identification. This is a required field when Transport Mode = RAI."));
			}
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
		}
	}
}
