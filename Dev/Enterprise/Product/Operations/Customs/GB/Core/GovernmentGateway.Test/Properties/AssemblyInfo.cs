using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Enterprise.Customs.GB.GovernmentGateway.Test")]
[assembly: AssemblyDescription("")]
#if DEBUG
#pragma warning disable CS0436 // Type conflicts with imported type
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#pragma warning restore CS0436 // Type conflicts with imported type
#endif

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.UnitedKingdom)]
