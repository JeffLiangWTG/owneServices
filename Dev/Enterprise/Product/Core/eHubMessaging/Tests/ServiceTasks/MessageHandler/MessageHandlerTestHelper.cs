using System;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks
{
	public static class MessageHandlerTestHelper
	{
		public const string AnyTextKeyWord = "****CanBeAnyText****";

		static public bool CompareXmlString(string text1, string text2)
		{
			if (text1.IndexOf(AnyTextKeyWord) == -1 && text2.IndexOf(AnyTextKeyWord) == -1)
			{
				return TextComparisionByChar(text1, text2);
			}
			else
			{
				if (text1.IndexOf(AnyTextKeyWord) != -1)
				{
					return Text1ContainsText2(text2, text1, AnyTextKeyWord);
				}
				else
				{
					return Text1ContainsText2(text1, text2, AnyTextKeyWord);
				}
			}
		}

		static bool Text1ContainsText2(string text1, string text2, string anyTextKeyWord)
		{
			text1 = text1.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
			text2 = text2.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");

			string[] parts = text2.Split(new string[] { anyTextKeyWord }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string part in parts)
			{
				if (text1.IndexOf(part) == -1)
				{
					return false;
				}
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodingConvention", "WTG1007:DoNotCompareBoolLiterals", Justification = "This is a test.")]
		static bool TextComparisionByChar(string text1, string text2)
		{
			char[] array1 = text1.ToCharArray();
			char[] array2 = text2.ToCharArray();
			char[] array;

			int i1 = 0;
			int i2 = 0;

			while (true)
			{
				if (i1 == array1.Length)
				{
					array = new char[array2.Length - i2];
					Array.Copy(array2, i2, array, 0, array2.Length - i2);
					break;
				}

				if (i2 == array2.Length)
				{
					array = new char[array1.Length - i1];
					Array.Copy(array1, i1, array, 0, array1.Length - i1);
					break;
				}

				char s1 = array1[i1];
				char s2 = array2[i2];

				if (s1 != s2)
				{
					bool shiftDone = false;
					if (s1 == ' ' || s1 == '\r' || s1 == '\n' || s1 == '\t')
					{
						i1++;
						shiftDone = true;
					}

					if (s2 == ' ' || s2 == '\r' || s2 == '\n' || s2 == '\t')
					{
						i2++;
						shiftDone = true;
					}

					if (shiftDone == false)
					{
						return false;
					}
				}
				else
				{
					i1++;
					i2++;
				}
			}

			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != ' ' && array[i] != '\r' && array[i] != '\n' && array[i] != '\t')
				{
					return false;
				}
			}

			return true;
		}
	}
}
