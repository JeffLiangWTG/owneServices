using System.Reflection;

[assembly: AssemblyTitle("AsycudaCustoms Customs Module Test")]
[assembly: AssemblyDescription("AsycudaCustoms Customs Module Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Botswana)]
[assembly: Enterprise.Customs.Business.Testing.AsycudaCustomsCountries(Enterprise.Core.Constants.CountryCodes.Namibia, Enterprise.Core.Constants.CountryCodes.Lesotho, Enterprise.Core.Constants.CountryCodes.Botswana, Enterprise.Core.Constants.CountryCodes.Swaziland)]
