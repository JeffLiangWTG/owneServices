using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	public class RevenueRecognitionTypeConverter : EnumConverter<RevenueRecognitionType>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				"ARV", "DEP", "EAD", "EDD", "AWB", "CUS", "DEL", "IMM", "JCL", "JOP", "JOB", "PIC", "FAR", "VAD", "VDD"
			};
		}

		protected override RevenueRecognitionType[] GetEnumValues()
		{
			return new RevenueRecognitionType[]
			{
				RevenueRecognitionType.ARV,
				RevenueRecognitionType.DEP,
				RevenueRecognitionType.EAD,
				RevenueRecognitionType.EDD,
				RevenueRecognitionType.AWB,
				RevenueRecognitionType.CUS,
				RevenueRecognitionType.DEL,
				RevenueRecognitionType.IMM,
				RevenueRecognitionType.JCL,
				RevenueRecognitionType.JOP,
				RevenueRecognitionType.JOB,
				RevenueRecognitionType.PIC,
				RevenueRecognitionType.FAR,
				RevenueRecognitionType.VAD,
				RevenueRecognitionType.VDD
			};
		}
	}
}
