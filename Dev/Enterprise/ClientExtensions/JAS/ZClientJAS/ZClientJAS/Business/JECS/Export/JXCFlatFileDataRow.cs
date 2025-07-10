
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class JXCFlatFileDataRow : FlatFileDataRow
	{
		public JXCFlatFileDataRow(int fieldCount)
			: base(fieldCount)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
		public void SetField(int fieldPositions, IZType value)
		{
			ZString valueAsString;

			if (value is ZDecimal)
			{
				valueAsString = ((ZDecimal)value).ToString(Format.Decimal);
			}
			else if (value is ZDateTime)
			{
				valueAsString = ((ZDateTime)value).ToString(Format.Date);
			}
			else
			{
				valueAsString = value.ToString();
			}

			base.SetField(fieldPositions, valueAsString);
		}

		public void SetOrganisationNameField(int fieldPositions, ZString fullName, int maxLength)
		{
			ZString cleanName = GetCleanedUpOrganisationName(fullName, maxLength);
			SetField(fieldPositions, cleanName);
		}

		public void SetField(int fieldPositions, ZString stringValue, int maxStringLength)
		{
			SetField(fieldPositions, stringValue.Trim().Left(maxStringLength));
		}

		public void SetField(int fieldPositions, char charValue)
		{
			SetField(fieldPositions, charValue.ToString());
		}

		#region Implementation

		ZString GetCleanedUpOrganisationName(ZString fullName, int maxLength)
		{
			ZString result = fullName.Trim();
			if (fullName.Length > maxLength)
			{
				result = OrgPatternLanguageSetting.Get(Constants.Languages.English).RemoveExtraSpaces(fullName);
				result = OrgPatternLanguageSetting.Get(Constants.Languages.English).RemoveIgnoredOrganisationWords(result);
				result = result.Left(maxLength);
			}
			return result;
		}

		#region Formats

		protected static class Format
		{
			public const string Decimal = "0.###";
			public const string Date = "dd/MM/yyyy";
		}

		#endregion

		#endregion
	}
}
