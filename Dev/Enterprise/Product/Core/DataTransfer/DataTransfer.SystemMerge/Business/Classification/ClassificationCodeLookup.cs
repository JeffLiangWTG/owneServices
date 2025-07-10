using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	class ClassificationCodeLookup
	{
		public string GetUniqueLookupCode(BusinessObjectFactory factory, string lookupCode, string classificationType, string countryCode)
		{
			Argument.NotNull(lookupCode, nameof(lookupCode));
			Argument.NotNullOrEmpty(classificationType, nameof(classificationType));
			Argument.NotNullOrEmpty(countryCode, nameof(countryCode));

			lookupCode = lookupCode.Trim();

			string originalCode = (lookupCode.Length >= CusClassificationSchema.CC_LookupCode.MaxLength) ?
				lookupCode.Substring(0, CusClassificationSchema.CC_LookupCode.MaxLength - 1) :
				lookupCode;

			int nextUniqueNumber = GetNextUniqueSequenceNumber(
				factory, originalCode, classificationType, countryCode, CusClassificationSchema.CC_LookupCode.MaxLength);

			string result = originalCode + nextUniqueNumber.ToString();
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		int GetNextUniqueSequenceNumber(BusinessObjectFactory factory, string lookupCode, string classificationType, string countryCode, int maxCodeLength)
		{
			string sqlText = @"
					DECLARE @StartNumberPosition int; SET @StartNumberPosition = LEN(@OriginalCode) + 1;

					SELECT
						isnull(max(CONVERT(int, substring(CC_LookupCode, @StartNumberPosition, @MaxLength))), 0) + 1
					FROM
						dbo.CusClassification
					WHERE
						CC_ClassificationType = @ClassificationType
						AND CC_RN_NKCountryCode = @CountryCode
						AND
						(
							(CC_LookupCode = @OriginalCode) OR
							(
								left(CC_LookupCode, @StartNumberPosition - 1) = @OriginalCode
								AND isnumeric(substring(CC_LookupCode, @StartNumberPosition, @MaxLength)) = 1
								AND substring(CC_LookupCode, @StartNumberPosition, @MaxLength) not like '%[.+-]%'
							)
						)";

			DbCommand cmd = ((IDbConnected)factory).Connection.Command(sqlText);
			cmd.AddParameter("@MaxLength", SqlDbType.Int, maxCodeLength);
			cmd.AddParameterBasedOnDbColumn("@OriginalCode", lookupCode, CusClassificationSchema.CC_LookupCode);
			cmd.AddParameterBasedOnDbColumn("@ClassificationType", classificationType, CusClassificationSchema.CC_ClassificationType);
			cmd.AddParameterBasedOnDbColumn("@CountryCode", countryCode, CusClassificationSchema.CC_RN_NKCountryCode);

			int result = Convert.ToInt32(cmd.ExecuteScalar());
			return result;
		}
	}
}
