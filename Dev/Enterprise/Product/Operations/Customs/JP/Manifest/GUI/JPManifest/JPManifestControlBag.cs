using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public sealed class JPManifestControlBag : ControlBag
	{
		public static JPManifestControlBag Instance => instance ?? (instance = new JPManifestControlBag());

		[ThreadStatic]
		static JPManifestControlBag instance;

		JPManifestControlBag()
		{
			PortOfLoadingUserControl = RegisterControl(nameof(PortOfLoadingUserControl));
			PortOfDischargeUserControl = RegisterControl(nameof(PortOfDischargeUserControl));
			ConsolidatorUserControl = RegisterControl(nameof(ConsolidatorUserControl));
			CustomsAgentCodeFindBox = RegisterControl(nameof(CustomsAgentCodeFindBox));
			CustomsAgentCredentialGuidDropEdit = RegisterControl(nameof(CustomsAgentCredentialGuidDropEdit));
			IsSubConsolidationCheckBox = RegisterControl(nameof(IsSubConsolidationCheckBox));
			IsCoLoadedCheckBox = RegisterControl(nameof(IsCoLoadedCheckBox));
			InputReferenceTextBox = RegisterControl(nameof(InputReferenceTextBox));
			BookingNumberTextBox = RegisterControl(nameof(BookingNumberTextBox));
			ViaLocationCodeFindBox = RegisterControl(nameof(ViaLocationCodeFindBox));
			MoveInDestinationCodeFindBox = RegisterControl(nameof(MoveInDestinationCodeFindBox));
			MasterBillCustomsStatusDropEdit = RegisterControl(nameof(MasterBillCustomsStatusDropEdit));
			MasterBillMessageStatusDropEdit = RegisterControl(nameof(MasterBillMessageStatusDropEdit));
		}

		protected override Control CreateTemplate()
		{
			return new JPManifestCountrySpecificUserControl();
		}

		public ControlReference MoveInDestinationCodeFindBox { get; }
		public ControlReference PortOfLoadingUserControl { get; }
		public ControlReference PortOfDischargeUserControl { get; }
		public ControlReference ConsolidatorUserControl { get; }
		public ControlReference CustomsAgentCodeFindBox { get; }
		public ControlReference CustomsAgentCredentialGuidDropEdit { get; }
		public ControlReference IsSubConsolidationCheckBox { get; }
		public ControlReference IsCoLoadedCheckBox { get; }
		public ControlReference InputReferenceTextBox { get; }
		public ControlReference BookingNumberTextBox { get; }
		public ControlReference ViaLocationCodeFindBox { get; }
		public ControlReference MasterBillCustomsStatusDropEdit { get; }
		public ControlReference MasterBillMessageStatusDropEdit { get; }
	}
}
