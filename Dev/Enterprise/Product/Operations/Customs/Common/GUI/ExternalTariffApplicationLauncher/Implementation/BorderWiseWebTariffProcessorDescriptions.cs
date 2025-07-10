using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.GUI
{
	public abstract class BorderWiseWebTariffProcessorDescriptions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static MultilingualString InvoiceClassificationWarning = ResString.GetMultilingualString(
			"67b0c796-0848-40fa-b8ab-046725889bb1",
			@"The invoice lines are being classified in BorderWise.
Please avoid changing them in CargoWise One because they will be overridden once submitted from BorderWise."
		);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static MultilingualString InvoiceProcessingByOtherUserWarning = ResString.GetMultilingualString(
			"f119d86c-86d2-45e0-8f94-ffb91496c8f1",
			@"The invoice lines are being processed by another user.
Please avoid changing them in CargoWise One because they may be overridden."
		);

		public static MultilingualString NewClassificationsNotification = ResString.GetMultilingualString(
			"08492f5b-da36-4848-a0bc-e165a45625be",
			"New Classifications from BorderWise."
		);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static MultilingualString TariffClassificationsReceived = ResString.GetMultilingualString(
			"76329a39-1c0e-4fae-9cb5-b19a54d255fe",
			@"CargoWise One has just received tariff classifications from BorderWise.
Please review and save the form."
		);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static MultilingualString ProcessingInCargoWise = ResString.GetMultilingualString(
			"3e76c8c5-89d2-4f25-9d27-8fd0d8f55fbb",
			@"CargoWise One had already received tariff classifications from BorderWise.
Please save the data before re-initiating classification in BorderWise."
		);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static MultilingualString SaveJobDeclaration = ResString.GetMultilingualString(
			"4e76c8c5-89d2-4f25-9d27-8fd0d8f55fbb",
			@"Please save the data before initiating classification in BorderWise."
		);

		public static MultilingualString WaitingForBorderWiseResponse = ResString.GetMultilingualString(
			"e77196c7-b3ba-423a-8c28-43c8561fa685",
			"Waiting for response from BorderWise"
		);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static MultilingualString PendingBorderWiseResponse = ResString.GetMultilingualString(
			"d4d080d3-1316-47f0-98d1-95bb303bd74a",
			@"A response is pending from BorderWise.
Click on the Cancel button to stop waiting for the response.
If you choose to close this window, no information will return to CargoWise One."
		);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static MultilingualString UnableToConnectToBorderWise = ResString.GetMultilingualString(
			"dac497ee-059e-4f56-9eb3-292463ff9f82",
			@"Unable to connect to BorderWise. Please try after sometime."
		);

		public static MultilingualString NavigateButtonLabel = ResString.GetMultilingualString(
		"b2b9b2f8-86a3-4f4b-88b3-912f22f9d2ef",
		"Navigate to BorderWise"
		);

		public static MultilingualString OKButtonLabel = ResString.GetMultilingualString(
			"aff953d5-a770-48f1-bf61-ef4f0216c056",
			"&OK"
		);

		public static MultilingualString InformationLabel = ResString.GetMultilingualString(
			"6721755d-5a2d-40f2-9b81-fc38d18b4ef9",
			"Information"
		);

		public static MultilingualString CancelButtonLabel = ResString.GetMultilingualString(
			"ca0faa09-f6ca-432a-a00c-51bf2fe02696",
			"&Cancel"
		);

		public static MultilingualString Delete = ResString.GetMultilingualString(
			"ca0faa09-f6ca-432a-a00c-51bf2fe02697",
			"Delete"
		);

		public static MultilingualString Attach = ResString.GetMultilingualString(
			"ca0faa09-f6ca-432a-a00c-51bf2fe05678",
			"Attach"
		);
	}
}
