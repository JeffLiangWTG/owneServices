namespace Enterprise.ZArchitecture.Design
{
	/// <summary>
	/// Type names for various .net and ntier designer classes.
	/// </summary>
	internal static class DesignerTypes
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public const string BindingMemberEditor =
			"CargoWise.ComponentModel.Design.BindingMemberEditor, CargoWise.Design, Version=" + AssemblyVersion + ", Culture=neutral, PublicKeyToken=" + PublicKeyToken;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public const string TypeValueIntellisenseEditor =
			"CargoWise.ComponentModel.Design.TypeValueIntellisenseEditor, CargoWise.Design, Version=" + AssemblyVersion + ", Culture=neutral, PublicKeyToken=" + PublicKeyToken;

		const string PublicKeyToken = "4f570df270576350";
		const string AssemblyVersion = "2.0.0.0";
	}
}
