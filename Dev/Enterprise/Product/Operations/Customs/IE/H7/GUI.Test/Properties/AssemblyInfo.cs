using System.Reflection;
using CargoWise.Definitions;

[assembly: AssemblyTitle("IE.H7 Customs GUI Test")]
[assembly: AssemblyDescription("IE.H7 Customs GUI Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Ireland)]
[assembly: Enterprise.Licensing.Billing.Business.Testing.FeatureDataTest(LicenceFeatureCodeList.Codes.EcommerceH7Feature)]
