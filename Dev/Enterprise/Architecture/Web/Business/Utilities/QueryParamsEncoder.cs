using System;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public class QueryParamsEncoder
	{
		public string Encrypt(string value)
		{
			value = Encoder.Encrypt(value);
			return ReplaceProhibitedCharacters(value, charsProhibitedInUrl, charsToReplace);
		}

		public string Decrypt(string value)
		{
			value = ReplaceProhibitedCharacters(value, charsToReplace, charsProhibitedInUrl);
			return Encoder.Decrypt(value);
		}

		#region Encoder

		TwoWayEncoder Encoder
		{
			get
			{
				return encoder ?? (encoder = new TwoWayEncoder(new Guid("E39E3B02-376B-4936-8BE0-D9F3DF66B28C")));
			}
		}
		TwoWayEncoder encoder;

		#endregion

		#region Implementation

		string ReplaceProhibitedCharacters(string value, char[] oldChars, char[] newChars)
		{
			if (!string.IsNullOrEmpty(value))
			{
				for (var i = 0; i < oldChars.Length; i++)
				{
					value = value.Replace(oldChars[i], newChars[i]);
				}
			}

			return value;
		}

		readonly char[] charsProhibitedInUrl = new[] { '=', '/', '+' };
		readonly char[] charsToReplace = new[] { '!', '|', '-' };

		#endregion
	}
}
