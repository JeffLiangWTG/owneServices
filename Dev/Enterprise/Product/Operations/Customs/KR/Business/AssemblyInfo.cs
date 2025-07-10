using System.Reflection;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("KR Customs Business")]
[assembly: AssemblyDescription("KR Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoKRAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.KoreaSouth)]
