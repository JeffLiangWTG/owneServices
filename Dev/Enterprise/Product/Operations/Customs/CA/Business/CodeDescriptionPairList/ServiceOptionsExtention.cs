//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	partial class ServiceOptions
	{
		public static string GetShortDescription(string serviceOption)
		{
			switch (serviceOption)
			{
				case Codes.G7EDIExport:
					return Res.GetString("aa27faf1-13ef-4cd1-b30c-bf35033d9964", "G7 Export");
				case Codes.GenericSyntax:
					return Res.GetString("9495DA73-76A2-4D9B-9805-6C41FE8FBAD6", "Syntax");
				case Codes.PaperCash:
					return Res.GetString("03F7D455-1C7F-485F-9872-B3B76DF1CEFB", "Cash, Paper");
				case Codes.PaperEnterToArrive:
					return Res.GetString("8A6ED322-219A-421B-93A0-EA31445D2CF8", "ETA, Paper");
				case Codes.PaperFIRST:
					return Res.GetString("E8FFF17A-DFC8-41D5-BE05-5D78EB9DD776", "FIRST, Paper");
				case Codes.PaperPARS:
					return Res.GetString("69316A72-FE49-4C53-90DE-4D1130213345", "PARS, Paper");
				case Codes.IID:
					return Res.GetString("D06166D2-5010-41D3-B583-677F095EC6A5", "IID");
				case Codes.PaperRMD:
					return Res.GetString("EE4A9064-2743-4322-89CE-7A2E0B76F3B3", "RMD, Paper");
				case Codes.PaperValueIncluded:
					return Res.GetString("A122C274-AC3B-49AC-9390-A53CCACE591A", "VI, Paper");
				case Codes.PARS:
					return Res.GetString("6C837A86-DA66-438C-B3AB-F24B34EC7FAA", "PARS, EDI");
				case Codes.PARSOGD:
					return Res.GetString("54AA2E78-1292-4B1B-9225-FB7F1210669E", "PARS, OGD, EDI");
				case Codes.ReplaceRMDwithAQ:
					return Res.GetString("DA4CF175-D894-4B8F-BB96-BBA863941AD0", "AQ, EDI");
				case Codes.RMD:
					return Res.GetString("64EDECCC-78E0-4F26-8FCE-715EC2347ECD", "RMD, EDI");
				case Codes.RMDOGD:
					return Res.GetString("4A1CA8DF-D9CB-4F6F-B17F-5C93418C522F", "RMD, OGD, EDI");
				case Codes.SpecialRelease:
					return Res.GetString("A35CBB9D-8BEB-46C9-9B00-170FB6021E94", "Special Rel");
				case Codes.SupplementaryCargoReport:
					return Res.GetString("7CA56F1F-6F49-467E-ADB1-A635D4E8E503", "Supp ACI");
				case Codes.TemporaryRelease:
					return Res.GetString("5C6E0B8B-CA96-4E8A-B97A-36372B07EE0D", "Temp. Rel");
				default:
					return string.Empty;
			}
		}
	}
}
