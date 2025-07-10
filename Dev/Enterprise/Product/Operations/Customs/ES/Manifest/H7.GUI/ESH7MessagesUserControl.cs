using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.H7.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public partial class ESH7MessagesUserControl : EUH7MessagesUserControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SourceKey is set in SetAdditionalTabPageCaption")]
		const string G3MessageCaption = "G3 Messages";

		protected override ResourceStringData SetAdditionalTabPageCaption() => new ResourceStringData("1ce7f501-87db-499e-ba0b-318a2e427435", G3MessageCaption);
	}
}
