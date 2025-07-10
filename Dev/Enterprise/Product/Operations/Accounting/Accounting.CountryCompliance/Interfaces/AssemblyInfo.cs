using System.Runtime.CompilerServices;

//DO NOT ADD ANY NEW ASSEMBLIES HERE. 
//This is to allow some interfaces, like IInstanceProvider, to be used in below 2 assemblies only and not to be visible outside of CountryCompliance solution. 
[assembly: InternalsVisibleTo("Enterprise.Accounting.CountryCompliance.GlobalCountryFactory, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Accounting.CountryCompliance.Implementation, PublicKey=" + CommonAssemblyInfo.PublicKey)]
