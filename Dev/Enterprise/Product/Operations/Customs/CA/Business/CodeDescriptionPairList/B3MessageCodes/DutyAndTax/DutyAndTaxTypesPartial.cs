using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public partial class DutyAndTaxTypes
	{
		public static ZBool IsSIMATaxCode(ZString code)
		{
			return SIMATaxCodes.Contains(code);
		}

		public static ZBool IsSIMATaxCodeIncludingSIMAType(ZString code)
		{
			return IsSIMATaxCode(code) || code == Codes.SIMADuty;
		}

		public static ZBool IsCustomsDutyOrExciseTax(ZString code)
		{
			return code == Codes.CustomsDuty || code == Codes.ExciseTax;
		}

		public static IEnumerable<ZString> SIMATaxCodes
		{
			get
			{
				yield return Codes.ADD;
				yield return Codes.CVD;
				yield return Codes.SUR;
			}
		}

		public static class Constant
		{
			public const string NO = "NO";
		}
	}
}
