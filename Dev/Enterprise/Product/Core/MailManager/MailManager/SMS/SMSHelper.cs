using System;

namespace Enterprise.MailManager.SMS
{
	public static class SMSHelper
	{
		public static string GetRandId()
		{
			string fMaxRand = "99999";
			int randomNumber = (new Random()).Next(Int32.Parse(fMaxRand));
			return randomNumber.ToString().PadLeft(fMaxRand.Length, '0');
		}

		public static bool IsSMSValid(string sMSNumber)
		{
			bool result = !string.IsNullOrWhiteSpace(sMSNumber) && sMSNumber.Length > 10;

			if (result)
			{
				char[] fInvalidCharacters = new char[51] { '+','a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
															 '@', '!', '#', '$', '%', '^', '&', '*', '(', ')', '=', '<', '>', '?', '/', ':', ';', '"', '{', '}', '[', ']', '\\', '|' };
				result = sMSNumber.ToLower().IndexOfAny(fInvalidCharacters) < 0;
			}

			return result;
		}

		public static bool IsSMSTextValid(string sMSText)
		{
			return !string.IsNullOrEmpty(sMSText) && sMSText.Length <= 160;
		}
	}
}
