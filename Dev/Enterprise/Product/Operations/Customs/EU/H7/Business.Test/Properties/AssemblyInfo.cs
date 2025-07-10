using System.Reflection;
using CargoWise.Definitions;

[assembly: AssemblyTitle("EU.H7 Customs Business Test")]
[assembly: AssemblyDescription("EU.H7 Customs Business Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Latvia)]
[assembly: Enterprise.Licensing.Billing.Business.Testing.FeatureDataTest(LicenceFeatureCodeList.Codes.EcommerceH7Feature)]
