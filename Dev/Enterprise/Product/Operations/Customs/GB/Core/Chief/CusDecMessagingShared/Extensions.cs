using System;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief
{
	public static class Extensions
	{
		public static ZString GetCusDecValue(this ZString inputValue, string chiefDataElementName, Func<ZString, ZString> textClass)
		{
			var clean = textClass(inputValue);
			return GetCusDecValue(clean, chiefDataElementName);
		}

		public static ZString GetCusDecValue(this ZString inputValue, string chiefDataElementName)
		{
			LengthAndPadFlag lengthAndPadding = GetMaxLengthAndPaddingFlag(chiefDataElementName);
			if (lengthAndPadding == null)
			{
				return inputValue;
			}

			if (lengthAndPadding.ShouldPad && !inputValue.IsEmpty)
			{
				if (inputValue.Length > lengthAndPadding.Length)
				{
					return inputValue.Left(lengthAndPadding.Length);
				}
				else
				{
					return inputValue.PadRight(lengthAndPadding.Length);
				}
			}
			else
			{
				return inputValue.Left(lengthAndPadding.Length);
			}
		}

		public static LengthAndPadFlag GetMaxLengthAndPaddingFlag(string chiefDataElementName)
		{
			ChiefDataElementsLengths fields = new ChiefDataElementsLengths();
			ZString desc = fields.GetDescriptionFromCode(chiefDataElementName);
			if (!desc.IsEmpty)
			{
				string v = "v";
				LengthAndPadFlag landP = new LengthAndPadFlag(true);
				if (desc.StartsWith(v))
				{   // v=variable - should not pad
					landP.ShouldPad = false;
				}
				int length = int.Parse(desc.Replace(v, string.Empty));
				landP.Length = length;
				return landP;
			}
			return null;
		}

		public class LengthAndPadFlag
		{
			public LengthAndPadFlag(bool shouldPad)
			{
				this.ShouldPad = shouldPad;
			}
			public ZInt Length;
			public ZBool ShouldPad;
		}
	}

	public static class ChiefTextClass
	{
		public static ZString T1(ZString input)
		{
			//Text Format 1: EDIFACT Level B character set. This consists of upper and lower case alphas, numeric digits, spaces and certain special characters. 
			//'A' - 'Z', 'a' - 'z', '0' - '9, ‘ ‘ . , - ( ) / ' + : = ? ! " % & * ; < > 
			return Regex.Replace(input, @"[^A-Za-z0-9 \.\,\-\(\)\/\'\+\:\=\?\!""\%\&\*\;\<\>]", "");
		}

		public static ZString T6(ZString input)
		{
			//Text Format 6: Character string of upper case alphas, numeric digits, spaces and certain special characters. 
			//'A' - 'Z', '0' - '9, ‘ ‘ . , - ( ) / ' + : = ? ! " % & * ; < > 
			return Regex.Replace(input.ToUpper(), @"[^A-Z0-9 \.\,\-\(\)\/\'\+\:\=\?\!""\%\&\*\;\<\>]", "");
		}
	}
}
