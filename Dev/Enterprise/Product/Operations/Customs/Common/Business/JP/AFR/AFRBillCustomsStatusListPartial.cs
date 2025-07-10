using CargoWise.Types;

namespace Enterprise.Customs.Common.JP.AFR
{
	partial class AFRBillCustomsStatusList
	{
		public static bool IsRegisteredType(ZString code)
		{
			return code == Codes.Registered ||
				code == Codes.DoNotLoad ||
				code == Codes.DoNotUnload ||
				code == Codes.HLD ||
				code == Codes.NL1 ||
				code == Codes.NL2 ||
				code == Codes.NL3 ||
				code == Codes.NL4 ||
				code == Codes.NL5 ||
				code == Codes.ReleasedHold ||
				code == Codes.ReleasedDoNotLoad ||
				code == Codes.ReleasedDoNotUnload;
		}

		public static bool IsRiskAssessmentReceivedType(ZString code)
		{
			return code == Codes.DoNotLoad ||
				code == Codes.DoNotUnload ||
				code == Codes.HLD;
		}
	}
}
