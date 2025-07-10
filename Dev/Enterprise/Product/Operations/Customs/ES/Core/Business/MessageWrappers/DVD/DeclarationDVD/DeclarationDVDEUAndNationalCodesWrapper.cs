using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDEUAndNationalCodesWrapper : IDeclarationDVDEUAndNationalCodes
	{
		public DeclarationDVDEUAndNationalCodesWrapper(ZString euCode, ZString nationalCode)
		{
			EUCode = euCode;
			NationalCode = nationalCode;
		}

		public DeclarationDVDEUAndNationalCodesWrapper(ZString code)
		{
			if (Regex.IsMatch(code, @"^\d"))
			{
				NationalCode = code;
			}
			else
			{
				EUCode = code;
			}
		}

		public ZString EUCode { get; }

		public ZString NationalCode { get; }
	}
}
