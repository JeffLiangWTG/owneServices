using System.Collections.Generic;

namespace Enterprise.Customs.IE.PBN.Business
{
	public partial class IEPBNDeclarationTypes
	{
		public static IEnumerable<string> CustomsDeclarationItemCodes
		{
			get
			{
				yield return Codes.AIS;
				yield return Codes.AES;
				yield return Codes.ICS;
			}
		}

		public static IEnumerable<string> TransitDeclarationItemCodes
		{
			get
			{
				yield return Codes.NCTS;
			}
		}
	}
}
