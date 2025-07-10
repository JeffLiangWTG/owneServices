using System.Net;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals
{
	public static class ZAutoCompleteTextWithGuidValueHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded pattern")]
		public const string PKPattern = @"([a-fA-F0-9]{8})((-[a-fA-F0-9]{4}){3})-([a-fA-F0-9]{12})";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded pattern")]
		public const string PKWithDivPattern = @"<div[^>]*>.*<.*>";
		public const string CodeCuttingPattern = @"^[^|]*";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded pattern")]
		public const string CodePattern = @"[*][a-z0-9*]*";

		static readonly object ValueLocker = new object();
		public static string GetValue(this string value)
		{
			string result = value;
			if (value != null)
			{
				lock (ValueLocker)
				{
					Regex r = new Regex(PKWithDivPattern, RegexOptions.IgnoreCase);
					Match m = r.Match(value);
					if (m.Success)
					{
						result = result.Replace(m.Value, "");
						r = new Regex(PKPattern, RegexOptions.IgnoreCase);
						m = r.Match(result);
						if (m.Success)
						{
							result = result.Replace(m.Value, "");
						}
					}
					WebUtility.HtmlDecode(result);
				}
			}
			return result;
		}

		static readonly object KeyLocker = new object();
		public static IZType TryGetKey(this string value)
		{
			lock (KeyLocker)
			{
				IZType result = null;
				if (!string.IsNullOrEmpty(value))
				{
					ZGuid pK = value.GetGuid();
					if (pK != null && pK.IsValid)
					{
						result = pK;
					}
					else
					{
						ZString code = value.TryGetCode();
						if (!code.IsEmpty)
						{
							result = code;
						}
					}
				}
				return result;
			}
		}

		static ZString TryGetCode(this string value)
		{
			ZString result = ZString.Empty;
			if (!string.IsNullOrEmpty(value))
			{
				Regex r = new Regex(PKWithDivPattern, RegexOptions.IgnoreCase);
				Match m = r.Match(value);
				{
					r = new Regex(CodePattern, RegexOptions.IgnoreCase);
					m = r.Match(m.Value);
					if (m.Success)
					{
						result = m.Value.Replace("*", "");
					}
				}
			}
			return result;
		}

		static readonly object ValueWithKeyLocker = new object();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded pattern")]
		public const string PKHolder = "{0}<div id=\"PK\" style=\"display:{1}\">{2}</div>";
		public const string CodeHolder = "*{0}*";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static ZString GetValueWithHiddenPK(string code, string value1)
		{
			ZString result = ZString.Empty;
			var value = Regex.Replace(value1, "&(?!(amp|apos|quot|lt|gt);)", "&amp;");
			lock (ValueWithKeyLocker)
			{
				string displaymode =
					//#if DEBUG
					//                        "inline";
					//#else
					"none";
				//#endif
				ZGuid guid = code.GetGuid();
				if (guid.IsValid && !guid.IsEmpty)
				{
					result = ZString.Format(PKHolder, value, displaymode, code);
				}
				else
				{
					ZString codeValue = ZString.Format(CodeHolder, code);
					result = ZString.Format(PKHolder, value, displaymode, codeValue);
				}
			}

			return result;
		}

		static readonly object GuidLocker = new object();
		static ZGuid GetGuid(this string value)
		{
			ZGuid result = ZGuid.Empty;
			lock (GuidLocker)
			{
				Regex r = new Regex(PKPattern, RegexOptions.IgnoreCase);
				Match m = r.Match(value);
				if (m.Success)
				{
					ZGuid.TryParse(m.Value, out result);
				}
			}
			return result;
		}
	}
}
