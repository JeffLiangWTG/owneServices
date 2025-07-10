namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Type names for various .net and cargowise designer classes.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Type constant strings")]
	static class DesignerTypes
	{
		public const string UITypeEditor = "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		public const string BindingMemberEditor = "CargoWise.ComponentModel.Design.BindingMemberEditor, CargoWise.Design, Version=" + CommonAssemblyInfo.AssemblyVersion + ", Culture=neutral, PublicKeyToken=" + CommonAssemblyInfo.PublicKeyToken;
	}
}
