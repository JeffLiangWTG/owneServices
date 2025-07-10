using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CargoWise.Common
{
	public class SecureQueryString : QueryString
	{
		public const string QueryStringKey = "qdata"; // May be a part of SQL expression.
		public const string TimeStampKey = "__TimeStamp__";

		public SecureQueryString()
		{
		}

		public SecureQueryString(string encryptedQueryString)
			: base(encryptedQueryString)
		{
		}

		public TimeSpan ExpireTime
		{
			set { absoluteExpireTime = DateTime.UtcNow.Add(value); }
		}

		public DateTime AbsoluteExpireTime
		{
			get { return absoluteExpireTime; }
			private set { absoluteExpireTime = value; }
		}
		DateTime absoluteExpireTime = new DateTime(2079, 06, 06);

		#region Serialize / Deserialize

		public override string SerializeCore()
		{
			StringBuilder result = new StringBuilder(base.SerializeCore());
			if (result.Length > 0)
			{
				result.Append('&');
				result.Append(TimeStampKey);
				result.Append('=');
				result.Append(AbsoluteExpireTime.ToString(DateTimeFormatInfo.UniversalSortableDateTimePattern, DateTimeFormatInfo));
			}
			return result.ToString();
		}

		protected override void DeserializeCore(string queryString)
		{
			base.DeserializeCore(queryString);
			if (base[TimeStampKey] != null)
			{
				AbsoluteExpireTime = DateTime.ParseExact(base[TimeStampKey], DateTimeFormatInfo.UniversalSortableDateTimePattern, DateTimeFormatInfo);
				if (DateTime.Compare(AbsoluteExpireTime, DateTime.UtcNow) < 0) // No reference to CargoWise.Types from CargoWise.Common
				{
					throw new ExpiredQueryStringException();
				}
			}
		}

		#endregion

		#region Encrypt / Decrypt

		protected override string Encode(string queryString)
		{
			byte[] buffer = Encoding.ASCII.GetBytes(queryString);
			byte[] encryptedBuffer = EncryptCore(buffer);
			return Convert.ToBase64String(encryptedBuffer);
		}

		byte[] EncryptCore(byte[] buffer)
		{
			Argument.NotNull(buffer, nameof(buffer)); // Suggested By ReviewBot
			var encryptor = Cryptographer.CreateEncryptor();
			return encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
		}

		protected override string Decode(string encodedQueryString)
		{
			try
			{
				byte[] buffer = Convert.FromBase64String(encodedQueryString);
				byte[] decryptedBuffer = DecryptCore(buffer);
				return Encoding.ASCII.GetString(decryptedBuffer);
			}
			catch (FormatException ex)
			{
				InvalidQueryStringException exception = new InvalidQueryStringException(ex);
				throw exception;
			}
		}

		byte[] DecryptCore(byte[] buffer)
		{
			Argument.NotNull(buffer, nameof(buffer)); // Suggested By ReviewBot
			try
			{
				var decryptor = Cryptographer.CreateDecryptor();
				return decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
			}
			catch (CryptographicException)
			{
				throw new InvalidQueryStringException();
			}
		}

		#endregion

		#region Implementation

		TripleDES Cryptographer
		{
			get
			{
				if (cryptographer == null)
				{
					cryptographer = TripleDES.Create();
					cryptographer.InitializeForCargoWise();
				}
				return cryptographer;
			}
		}
		TripleDES cryptographer;

		DateTimeFormatInfo DateTimeFormatInfo
		{
			get
			{
				return System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat;
			}
		}

		#endregion
	}
}
