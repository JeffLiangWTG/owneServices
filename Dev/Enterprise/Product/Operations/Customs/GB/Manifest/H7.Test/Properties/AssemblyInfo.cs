using System.Reflection;
using CargoWise.Definitions;

[assembly: AssemblyTitle("GB.H7 Customs Business Test")]
[assembly: AssemblyDescription("GB.H7 Customs Business Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.UnitedKingdom)]
[assembly: Enterprise.Licensing.Billing.Business.Testing.FeatureDataTest(LicenceFeatureCodeList.Codes.EcommerceH7Feature)]
