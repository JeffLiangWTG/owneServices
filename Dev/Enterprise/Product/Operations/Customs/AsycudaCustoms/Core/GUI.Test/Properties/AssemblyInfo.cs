using System.Reflection;

[assembly: AssemblyTitle("AsycudaCustoms Customs GUI Tests")]
[assembly: AssemblyDescription("AsycudaCustoms Customs GUI Tests Project")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Botswana)]
[assembly: Enterprise.Customs.Business.Testing.AsycudaCustomsCountries(Enterprise.Core.Constants.CountryCodes.Namibia, Enterprise.Core.Constants.CountryCodes.Lesotho, Enterprise.Core.Constants.CountryCodes.Botswana, Enterprise.Core.Constants.CountryCodes.Swaziland)]
