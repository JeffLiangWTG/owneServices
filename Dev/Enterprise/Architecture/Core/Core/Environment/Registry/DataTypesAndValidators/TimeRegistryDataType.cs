using System;
using System.Globalization;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	/// <summary>
	/// Type enforcing an additional validation on string when a value is expected to be a time of day
	/// </summary>
	public class TimeRegistryDataType : StringRegistryDataType
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "datetime formats, no need for localization")]
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var supportedTimeFormats = new[]
			{
				"h:mm tt", "h:mm:ss tt", "H:mm", "H:mm:ss"
			};

			DateTime outVariablePlaceHolder;

			if (!DateTime.TryParseExact(proposedValue, supportedTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out outVariablePlaceHolder))
			{
				throw new RegistryValidationException(string.Format(CultureInfo.InvariantCulture, "{0} was not recognized as a valid time. Examples of valid values are:\n 9:45 am\n 22:00:05", proposedValue));
			}
		}
	}
}
