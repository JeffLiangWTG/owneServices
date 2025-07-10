using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class NumberToString_AR_AE : INumberToWords
	{
		long fNumber;
		string INumberToWords.DecimalSeperatorAsString => "";

		public string GetNumberAsString(long number)
		{
			fNumber = number;
			var tempNumber = fNumber;

			if (tempNumber == 0)
			{
				return (NoResString)"صفر";
			}

			var retVal = string.Empty;
			byte group = 0;
			while (tempNumber >= 1)
			{
				var numberToProcess = (int)(tempNumber % 1000);

				tempNumber = tempNumber / 1000;

				var groupDescription = ProcessArabicGroup(numberToProcess, group, tempNumber);

				if (!string.IsNullOrEmpty(groupDescription))
				{
					if (group > 0)
					{
						if (!string.IsNullOrEmpty(retVal))
						{
							retVal = string.Format("{0} {1}", (NoResString)"و", retVal);
						}

						if (numberToProcess != 2)
						{
							if (numberToProcess % 100 != 1)
							{
								if (numberToProcess >= 3 && numberToProcess <= 10)
								{
									retVal = string.Format("{0} {1}", arabicPluralGroups[group], retVal);
								}
								else
								{
									if (!string.IsNullOrEmpty(retVal))
									{
										retVal = string.Format("{0} {1}", arabicAppendedGroup[group], retVal);
									}
									else
									{
										retVal = string.Format("{0} {1}", arabicGroup[group], retVal);
									}
								}
							}
							else
							{
								retVal = string.Format("{0} {1}", arabicGroup[group], retVal);
							}
						}
					}

					retVal = string.IsNullOrEmpty(retVal) ? groupDescription : string.Format("{0} {1}", groupDescription, retVal);
				}

				group++;
			}

			return retVal;
		}

		string ProcessArabicGroup(int groupNumber, int groupLevel, decimal remainingNumber)
		{
			var tens = groupNumber % 100;

			var hundreds = groupNumber / 100;

			var retVal = string.Empty;

			if (hundreds > 0)
			{
				if (tens == 0 && hundreds == 2)
				{
					retVal = string.Format("{0}", arabicAppendedTwos[0]);
				}
				else
				{
					retVal = string.Format("{0}", arabicHundreds[hundreds]);
				}
			}

			if (tens > 0)
			{
				if (tens < 20)
				{
					if (tens == 2 && hundreds == 0 && groupLevel > 0)
					{
						if (fNumber == 2000 || fNumber == 2000000 || fNumber == 2000000000 || fNumber == 2000000000000 || fNumber == 2000000000000000 || fNumber == 2000000000000000000)
						{
							retVal = string.Format("{0}", arabicAppendedTwos[groupLevel]);
						}
						else
						{
							retVal = string.Format("{0}", arabicTwos[groupLevel]);
						}
					}
					else
					{
						if (!string.IsNullOrEmpty(retVal))
						{
							retVal += (NoResString)" و ";
						}

						if (tens == 1 && groupLevel > 0 && hundreds == 0)
						{
							retVal += " ";
						}
						else
						{
							retVal += arabicOnes[tens];
						}
					}
				}
				else
				{
					int ones = tens % 10;
					tens = (tens / 10) - 2;

					if (ones > 0)
					{
						if (!string.IsNullOrEmpty(retVal))
						{
							retVal += (NoResString)" و ";
						}

						retVal += arabicOnes[ones];
					}

					if (!string.IsNullOrEmpty(retVal))
					{
						retVal += (NoResString)" و ";
					}

					retVal += arabicTens[tens];
				}
			}

			return retVal;
		}

		#region static fields

		static readonly List<string> arabicOnes =
		   new List<string> { string.Empty, (NoResString)"واحد", (NoResString)"اثنان", (NoResString)"ثلاثة", (NoResString)"أربعة", (NoResString)"خمسة", (NoResString)"ستة", (NoResString)"سبعة", (NoResString)"ثمانية", (NoResString)"تسعة",
			(NoResString)"عشرة", (NoResString)"أحد عشر", (NoResString)"اثنا عشر", (NoResString)"ثلاثة عشر", (NoResString)"أربعة عشر", (NoResString)"خمسة عشر", (NoResString)"ستة عشر", (NoResString)"سبعة عشر", (NoResString)"ثمانية عشر", (NoResString)"تسعة عشر" };

		static readonly List<string> arabicTens =
			new List<string> { (NoResString)"عشرون", (NoResString)"ثلاثون", (NoResString)"أربعون", (NoResString)"خمسون", (NoResString)"ستون", (NoResString)"سبعون", (NoResString)"ثمانون", (NoResString)"تسعون" };

		static readonly List<string> arabicHundreds =
			new List<string> { string.Empty, (NoResString)"مائة", (NoResString)"مئتان", (NoResString)"ثلاثمائة", (NoResString)"أربعمائة", (NoResString)"خمسمائة", (NoResString)"ستمائة", (NoResString)"سبعمائة", (NoResString)"ثمانمائة", (NoResString)"تسعمائة" };

		static readonly List<string> arabicAppendedTwos =
			new List<string> { (NoResString)"مئتا", (NoResString)"ألفا", (NoResString)"مليونا", (NoResString)"مليارا", (NoResString)"تريليونا", (NoResString)"كوادريليونا", (NoResString)"كوينتليونا", (NoResString)"سكستيليونا" };

		static readonly List<string> arabicTwos =
			new List<string> { (NoResString)"مئتان", (NoResString)"ألفان", (NoResString)"مليونان", (NoResString)"ملياران", (NoResString)"تريليونان", (NoResString)"كوادريليونان", (NoResString)"كوينتليونان", (NoResString)"سكستيليونان" };

		static readonly List<string> arabicGroup =
			new List<string> { (NoResString)"مائة", (NoResString)"ألف", (NoResString)"مليون", (NoResString)"مليار", (NoResString)"تريليون", (NoResString)"كوادريليون", (NoResString)"كوينتليون", (NoResString)"سكستيليون" };

		static readonly List<string> arabicAppendedGroup =
			new List<string> { string.Empty, (NoResString)"ألفاً", (NoResString)"مليوناً", (NoResString)"ملياراً", (NoResString)"تريليوناً", (NoResString)"كوادريليوناً", (NoResString)"كوينتليوناً", (NoResString)"سكستيليوناً" };

		static readonly List<string> arabicPluralGroups =
			new List<string> { string.Empty, (NoResString)"آلاف", (NoResString)"ملايين", (NoResString)"مليارات", (NoResString)"تريليونات", (NoResString)"كوادريليونات", (NoResString)"كوينتليونات", (NoResString)"سكستيليونات" };

		#endregion
	}
}
