using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public class DocumentIZTypeFieldFormatter
	{
		public DocumentIZTypeFieldFormatter(IZType input)
		{
			Input = input;
		}

		public override string ToString()
		{
			return ToString(ZString.Empty);
		}

		public string ToString(ZString formatCode)
		{
			return ToFormattedZType(formatCode).ToString();
		}

		public IZType ToFormattedZType(ZString formatCode)
		{
			Type inputType = Input.GetType();
			if (inputType == typeof(ZString))
			{
				return GetFormattedZString((ZString)Input, formatCode);
			}
			if (inputType == typeof(ZShort))
			{
				return GetFormattedZInt(((ZShort)Input).ToZInt(), formatCode);
			}
			if (inputType == typeof(ZInt))
			{
				return GetFormattedZInt((ZInt)Input, formatCode);
			}
			if (inputType == typeof(ZDecimal))
			{
				return GetFormattedZDecimal((ZDecimal)Input, formatCode);
			}
			if (inputType == typeof(ZDateTime))
			{
				return GetFormattedZDateTime((ZDateTime)Input, formatCode);
			}
			if (inputType == typeof(ZDateTimeOffset))
			{
				return GetFormattedZDateTimeOffset((ZDateTimeOffset)Input, formatCode);
			}
			if (inputType == typeof(ZBool))
			{
				return GetFormattedZBool((ZBool)Input, formatCode);
			}
			return Input;
		}

		#region Implementation
		readonly IZType Input;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		IZType GetFormattedZString(ZString input, ZString formatCode)
		{
			IZType result = input;
			if (!formatCode.IsEmpty)
			{
				switch (formatCode)
				{
					case "Upper":
						result = input.ToUpper();
						break;
					case "Lower":
						result = input.ToLower();
						break;
					case "Proper":
						result = input.ToTitleCase();
						break;
				}
			}
			return result;
		}

		IZType GetFormattedZInt(ZInt input, ZString formatCode)
		{
			IZType result = input;
			if (!formatCode.IsEmpty)
			{
				if ("CEFNPD".Contains(formatCode.Left(1)) && formatCode.Length <= 3)
				{
					ZString rightBit = formatCode.SubstringSafe(1);
					if (rightBit.IsEmpty || rightBit.IsNumbersOnlyOrEmpty)
					{
						return new ZString(input.ToString(formatCode));
					}
				}
				result = new ZString(input.ToString());
			}
			return result;
		}

		IZType GetFormattedZDecimal(ZDecimal input, ZString formatCode)
		{
			IZType result = input;
			if (!formatCode.IsEmpty)
			{
				string leftChar = formatCode.Left(1);
				if (formatCode.Length <= 3)
				{
					ZString rightBit = formatCode.SubstringSafe(1);
					if ("CEFNP".Contains(leftChar))
					{
						if (rightBit.IsEmpty || rightBit.IsNumbersOnlyOrEmpty)
						{
							return new ZString(input.ToString(formatCode, Culture.Current));
						}
					}
					else if (leftChar == "T")
					{
						ZDecimal numericResult = input;
						ZInt requiredDecimals;
						if (rightBit.IsNumbersOnlyOrEmpty)
						{
							ZInt.TryParse(rightBit, out requiredDecimals);
							if (requiredDecimals > 0)
							{
								numericResult = numericResult.Round(requiredDecimals);
							}
						}
						return new ZString(numericResult.ToStringTrimZeros());
					}
					else
					{
						ZString formattedResult = new ZString(input.ToString());
						ZInt requiredLength;
						if (leftChar == "D" && ZInt.TryParse(rightBit, out requiredLength) && requiredLength > formattedResult.Length)
						{
							formattedResult = formattedResult.PadLeft(requiredLength, '0');
						}
						return formattedResult;
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "I want explicit formatting here.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		IZType GetFormattedZDateTime(ZDateTime input, ZString formatCode)
		{
			IZType result = input;
			if (!formatCode.IsEmpty)
			{
				if (input.IsEmpty)
				{
					result = ZString.Empty;
				}
				else
				{
					switch (formatCode)
					{
						case "LongDate":
							ZInt day = input.Day;
							result = new ZString(day.ToString() + GetCountSuffix(day) + " " + input.ToString("MMMM yyyy"));
							break;
						case "AmericanDate":
							result = new ZString(input.ToString("MM/dd/yy"));
							break;
						case "AmericanDateWithCentury":
							result = new ZString(input.ToString("MM/dd/yyyy"));
							break;
						case "BritishDate":
							result = new ZString(input.ToString("dd/MM/yy"));
							break;
						case "Day":
							result = new ZString(input.ToString("dddd"));
							break;
						case "Time24h":
							result = new ZString(input.ToString("HH:mm"));
							break;
						case "Time12h":
							result = new ZString(input.ToString("h:mmt").ToLower());
							break;
						case "Time12hAMPM":
							result = new ZString(input.ToString("h:mm tt").ToLower());
							break;
						case "SpanishDate":
							result = new ZString(input.ToString("dd-MM-yyyy"));
							break;
						default:
							result = new ZString(input.ToString("dd-MMM-yy"));
							break;
					}
				}
			}
			return result;
		}

		IZType GetFormattedZDateTimeOffset(ZDateTimeOffset input, ZString formatCode)
		{
			return GetFormattedZDateTime(input.ToZDateTime(), formatCode);
		}

		ZString GetCountSuffix(ZInt count)
		{
			ZString result = (NoResString)"th";
			ZString countAsString = count.ToString();
			if (countAsString.Length < 2 || countAsString.Substring(countAsString.Length - 2, 1) != "1")
			{
				switch (countAsString.Right(1))
				{
					case "1":
						result = (NoResString)"st";
						break;
					case "2":
						result = (NoResString)"nd";
						break;
					case "3":
						result = (NoResString)"rd";
						break;
				}
			}
			return result;
		}

		IZType GetFormattedZBool(ZBool input, ZString formatCode)
		{
			IZType result = input;
			if (!formatCode.IsEmpty)
			{
				switch (formatCode)
				{
					case "X":
						result = new ZString(input ? "X" : "");
						break;
					case "YesNo":
						result = new ZString(input ? Res.GetString("30ae9e76-5a5a-4839-b43c-449cfb57a8c5", "Yes") : Res.GetString("d93c5ea9-7ac0-4aed-9135-56ec1de859e0", "No"));
						break;
					case "TrueFalse":
						result = new ZString(input ? Res.GetString("f04934bd-16bb-49ab-b1a7-b9cce46fe92f", "True") : Res.GetString("6fd2c89f-4652-4e30-8794-8ae292a62e14", "False"));
						break;
					default:
						result = new ZString(input ? Res.GetString("17dd4a9f-c645-4a3a-ae2c-ad6a7d19a43a", "Y") : Res.GetString("9aa6fc1e-2702-4df6-88a8-792e147a4ed7", "N"));
						break;
				}
			}
			return result;
		}
		#endregion
	}
}
