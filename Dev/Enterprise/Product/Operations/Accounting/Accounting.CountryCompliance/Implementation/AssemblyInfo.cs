using CargoWise.Common;

//Nothing should be referencing this assembly except GlobalCountryFactory. Testing should be done like it is used in production code. For more info see Readme.md in the solution folder.
[assembly: PreventAssemblyReferences("Enterprise.Accounting.CountryCompliance.GlobalCountryFactory")]
