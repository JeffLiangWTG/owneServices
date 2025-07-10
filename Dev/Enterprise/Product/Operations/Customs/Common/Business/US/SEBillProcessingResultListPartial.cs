using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.US
{
	public static partial class SEBillProcessingResultList
	{
		public static bool IsHold(string status)
		{
			return status == ManifestHoldCBP ||
				status == ManifestHoldAgriculture ||
				status == CBPHold;
		}

		public static bool IsHoldRemoved(string status)
		{
			return status == CBPManifestHoldRemoved ||
				status == AgricultureManifestHoldRemoved ||
				status == CBPHoldRemoved;
		}

		public const string ManifestHoldCBP = "51";
		public const string ManifestHoldAgriculture = "52";
		public const string CBPHold = "53";
		public const string CBPManifestHoldRemoved = "54";
		public const string AgricultureManifestHoldRemoved = "55";
		public const string CBPHoldRemoved = "56";

		public static string Multiple = (NoResString)"Multiple";
		public static string BillStatusHoldOrExam = "HLD/EXM";
		public static MultilingualString BillStatusHoldOrExamDesc => ResString.GetMultilingualString("DC6708E8-F52F-4374-AE36-BC226EB21DA2", "Bills have hold or exam status");
	}
}
