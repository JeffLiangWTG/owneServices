using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1017", Justification = "Initialization projects cannot reference scaling tool. ZArchitecture DPI-awareness checks not done")]
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Loader.VersionInfoInitializer.ranOnce")] // Common/Product/ediLoad/Enterprise.Loader/Installers/VersionInfoInitializer.cs:44,14
