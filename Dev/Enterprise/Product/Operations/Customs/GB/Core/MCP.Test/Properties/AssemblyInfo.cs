using System.Reflection;
using System.Runtime.CompilerServices;
[assembly: AssemblyTitle("Enterprise.Customs.GB.MCP.Test")]
[assembly: AssemblyDescription("Messaging Module for direct communication with MCP (Test Only)")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.UnitedKingdom)]
#if DEBUG
#pragma warning disable CS0436 // Type conflicts with imported type
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.MCP.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#pragma warning restore CS0436 // Type conflicts with imported type
#endif
