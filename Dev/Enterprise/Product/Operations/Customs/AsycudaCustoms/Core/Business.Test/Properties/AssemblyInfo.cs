using System.Reflection;

[assembly: AssemblyTitle("AsycudaCustoms Customs Business Test")]
[assembly: AssemblyDescription("AsycudaCustoms Customs Business Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Botswana)]
[assembly: Enterprise.Customs.Business.Testing.AsycudaCustomsCountries(Enterprise.Core.Constants.CountryCodes.Namibia, Enterprise.Core.Constants.CountryCodes.Lesotho, Enterprise.Core.Constants.CountryCodes.Botswana, Enterprise.Core.Constants.CountryCodes.Swaziland, Enterprise.Core.Constants.CountryCodes.CoteDivoire)]
