//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	partial class IntendedUseCodes
	{
		public static CodeDescriptionPairList GetIntendedCodeForProgram(string programCode)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			switch (programCode)
			{
				case ECCCPGADepartmentCodes.Codes.WEN:
					result.AddPair(Codes.EC01, Descriptions.EC01);
					result.AddPair(Codes.EC02, Descriptions.EC02);
					result.AddPair(Codes.EC03, Descriptions.EC03);
					result.AddPair(Codes.EC04, Descriptions.EC04);
					result.AddPair(Codes.EC05, Descriptions.EC05);
					result.AddPair(Codes.EC06, Descriptions.EC06);
					result.AddPair(Codes.EC07, Descriptions.EC07);
					result.AddPair(Codes.EC08, Descriptions.EC08);
					result.AddPair(Codes.EC09, Descriptions.EC09);
					result.AddPair(Codes.EC10, Descriptions.EC10);
					result.AddPair(Codes.EC11, Descriptions.EC11);
					result.AddPair(Codes.EC12, Descriptions.EC12);
					break;
				case ECCCPGADepartmentCodes.Codes.WRM:
					result.AddPair(Codes.EC0501, Descriptions.EC0501);
					result.AddPair(Codes.EC0502, Descriptions.EC0502);
					result.AddPair(Codes.EC0503, Descriptions.EC0503);
					result.AddPair(Codes.EC0504, Descriptions.EC0504);
					result.AddPair(Codes.EC0505, Descriptions.EC0505);
					result.AddPair(Codes.EC0506, Descriptions.EC0506);
					result.AddPair(Codes.EC0507, Descriptions.EC0507);
					result.AddPair(Codes.EC0508, Descriptions.EC0508);
					result.AddPair(Codes.EC0509, Descriptions.EC0509);
					result.AddPair(Codes.EC0510, Descriptions.EC0510);
					result.AddPair(Codes.EC0511, Descriptions.EC0511);
					result.AddPair(Codes.EC0512, Descriptions.EC0512);
					result.AddPair(Codes.EC0513, Descriptions.EC0513);
					result.AddPair(Codes.EC0514, Descriptions.EC0514);
					result.AddPair(Codes.EC0515, Descriptions.EC0515);
					result.AddPair(Codes.EC0518, Descriptions.EC0518);
					result.AddPair(Codes.EC1601, Descriptions.EC1601);
					result.AddPair(Codes.EC1602, Descriptions.EC1602);
					result.AddPair(Codes.EC1603, Descriptions.EC1603);
					result.AddPair(Codes.EC1604, Descriptions.EC1604);
					result.AddPair(Codes.EC1605, Descriptions.EC1605);
					result.AddPair(Codes.EC1606, Descriptions.EC1606);
					result.AddPair(Codes.EC1607, Descriptions.EC1607);
					result.AddPair(Codes.EC1608, Descriptions.EC1608);
					result.AddPair(Codes.EC1609, Descriptions.EC1609);
					result.AddPair(Codes.EC1610, Descriptions.EC1610);
					result.AddPair(Codes.EC1612, Descriptions.EC1612);
					break;
			}
			return result;
		}
	}
}
