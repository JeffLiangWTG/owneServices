namespace Enterprise.ZArchitecture.GUI
{
	internal partial class GroupPanel
	{
		protected override string ControlStyleString => base.ControlStyleString + BorderStyleString;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Class name")]
		protected override string ClassName => "grouppanel";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Style")]
		static string BorderStyleString => "border: 1px solid silver;";
	}
}
