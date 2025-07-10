using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.MessagingRules
{
	public static class AccountNumbersValidationHelper
	{
		public static void CheckHasAccountNumberNoPrefix(ZPropertyInfo dan, ZPropertyInfo danPrefix, ZPropertyInfo infoForError)
		{
			if (!dan.Value.IsEmpty && danPrefix.Value.IsEmpty)
			{
				infoForError.AddMessageError(ErrorMessageHasAccountNumberNoPrefix);
			}
		}

		public static void CheckNoAccountNumberHasPrefix(ZPropertyInfo dan, ZPropertyInfo danPrefix, ZPropertyInfo infoForError)
		{
			if (dan.Value.IsEmpty && !danPrefix.Value.IsEmpty)
			{
				infoForError.AddMessageError(ErrorMessageNoAccountNumberHasPrefix);
			}
		}

		public static void CheckFirstIsEmptySecondNot(ZPropertyInfo firstDan, ZPropertyInfo secondDan, ZPropertyInfo infoForError)
		{
			if (firstDan.Value.IsEmpty && !secondDan.Value.IsEmpty)
			{
				infoForError.AddMessageError(ErrorMessageFirstIsEmptySecondNot);
			}
		}
 
		public static void CheckBothAccountNoSame(ZPropertyInfo firstDan, ZPropertyInfo secondDan, ZPropertyInfo infoForError)
		{
			if (firstDan.Value.Equals(secondDan.Value) && !firstDan.Value.IsEmpty)
			{
				infoForError.AddMessageError(ErrorMessageBothAccountNoSame);
			}
		}

		internal static void SecondDanForImportsOnly(ZPropertyInfo secondDanNumber, ZPropertyInfo secondDanPrefix, ZPropertyInfo infoForError, JobDeclaration declaration)
		{
			if (declaration.IsExport)
			{
				if (!secondDanNumber.Value.IsEmpty || !secondDanPrefix.Value.IsEmpty)
				{
					infoForError.AddMessageError("Second DANs are for imports only");
				}
			}
		}

		public const string ErrorMessageHasAccountNumberNoPrefix = "DAN declared, prefix required";
		public const string ErrorMessageNoAccountNumberHasPrefix = "The DAN prefix & DAN must both be present or absent, The DAN prefix field has been completed without a DAN being entered. Complete the DAN field with a valid DAN.";
		public const string ErrorMessageFirstIsEmptySecondNot = "First DAN absent therefore all DAN fields must also be absent.";
		public const string ErrorMessageBothAccountNoSame = "First DAN prefix only valid with a second prefix of A. (This usually means that the two DANs must be different)";
	}
}
