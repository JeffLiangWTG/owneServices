using System;
using System.Globalization;
using System.Text;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	#region Licence Part Difference

	public abstract class LicencePartDifference<T>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected LicencePartDifference(T licence1Part, T licence2Part)
		{
			this.Licence1Part = licence1Part;
			this.Licence2Part = licence2Part;
			textualDifference = GetTextualDifferenceCore();
		}

		public readonly T Licence1Part;
		public readonly T Licence2Part;

		#region Are Parts Different

		public bool ArePartsDifferent
		{
			get { return !string.IsNullOrEmpty(GetTextualDifference()); }
		}

		#endregion

		#region Textual Difference

		protected abstract string GetTextualDifferenceCore();

		public string GetTextualDifference()
		{
			return textualDifference;
		}
		readonly string textualDifference;

		#endregion

		#region Formatted Text Item

		protected string GetFormattedTextItem(string heading, string part1Value, string part2Value)
		{
			return string.Format(CultureInfo.CurrentCulture, "\t{0}:\r\n\t\tCLIENT: {1}\r\n\t\tPROD: {2}", heading, part1Value, part2Value);
		}

		#endregion
	}

	#endregion

	#region Licence Checkpoint Difference

	public class LicenceCheckpointDifference : LicencePartDifference<LegacyLicenceCheckpoint>
	{
		public LicenceCheckpointDifference(LegacyLicenceCheckpoint clientCheckpoint, LegacyLicenceCheckpoint ediCheckpoint)
			: base(clientCheckpoint, ediCheckpoint)
		{
		}

		protected override string GetTextualDifferenceCore()
		{
			StringBuilder result = new StringBuilder();
			if (Licence1Part.LicenceType != Licence2Part.LicenceType)
			{
				result.AppendLine(GetFormattedTextItem("TYPE", Licence1Part.LicenceType, Licence2Part.LicenceType));
			}

			DateTime d1 = Licence1Part.ExpiryDate.Date;
			DateTime d2 = Licence2Part.ExpiryDate.Date;

			// If both expiry dates are in the past then treat them as equal
			if (d1 != d2 && !(d1 < ZDateTime.Today && d2 <= ZDateTime.Today && d1 != DateTime.MinValue && d2 != DateTime.MinValue))
			{
				string clientExpiryDateFormatted = d1 == DateTime.MinValue ? "(NONE)" : d1.Date.ToShortDateString();
				string ediExpiryDateFormatted = d2 == DateTime.MinValue ? "(NONE)" : d2.Date.ToShortDateString();
				result.AppendLine(GetFormattedTextItem("EXPIRY", clientExpiryDateFormatted, ediExpiryDateFormatted));
			}

			if (Licence1Part.UserLimit != Licence2Part.UserLimit)
			{
				result.AppendLine(GetFormattedTextItem("USER COUNT", Licence1Part.UserLimit.ToString(CultureInfo.InvariantCulture), Licence2Part.UserLimit.ToString(CultureInfo.InvariantCulture)));
			}

			if (result.Length > 0)
			{
				result.Insert(0, string.Format(CultureInfo.CurrentCulture, "MODULE: {0}\r\n", Licence1Part.DisplayName));
			}

			return result.ToString();
		}
	}

	#endregion
}

