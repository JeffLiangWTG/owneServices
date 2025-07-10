using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	static class PatternMatchingCountryCodeProcessor
	{
		public static bool UpdatePatternMatchingTables<T>(T bizO, IEnumerable<SchemaColumn> countryCodeColumns, DataRow changeRow, BusinessObjectFactory factory, BusinessObject master, PatternMasterType masterType) where T : BusinessObject
		{
			var shouldSave = false;

			foreach (var column in countryCodeColumns)
			{
				var originalValue = TrimCountryCode(changeRow[column.Name, DataRowVersion.Original]);
				var currentValue = TrimCountryCode(changeRow[column.Name, DataRowVersion.Current]);

				if (!originalValue.Equals(currentValue, System.StringComparison.OrdinalIgnoreCase))
				{
					var addressUtilities = PatternMatchingSubscriberUtilitiesFactory<PatternMatchingAddress>.Provider(masterType);
					var domainUtilities = PatternMatchingSubscriberUtilitiesFactory<PatternMatchingDomain>.Provider(masterType);
					var emailUtilities = PatternMatchingSubscriberUtilitiesFactory<PatternMatchingEmail>.Provider(masterType);
					var nameUtilities = PatternMatchingSubscriberUtilitiesFactory<PatternMatchingName>.Provider(masterType);
					var phoneUtilities = PatternMatchingSubscriberUtilitiesFactory<PatternMatchingPhone>.Provider(masterType);
					var regCodeUtilities = PatternMatchingSubscriberUtilitiesFactory<PatternMatchingRegCode>.Provider(masterType);

					shouldSave |= addressUtilities.UpdatePatternMatchingCountryCode(factory, PatternMatchingAddressSchema.PMA_ParentId, addressUtilities.PatternMatchingAddressMasterIdColumn, bizO.PK, master, currentValue);
					shouldSave |= domainUtilities.UpdatePatternMatchingCountryCode(factory, PatternMatchingDomainSchema.PMD_ParentId, domainUtilities.PatternMatchingDomainMasterIdColumn, bizO.PK, master, currentValue);
					shouldSave |= emailUtilities.UpdatePatternMatchingCountryCode(factory, PatternMatchingEmailSchema.PME_ParentId, emailUtilities.PatternMatchingEmailMasterIdColumn, bizO.PK, master, currentValue);
					shouldSave |= nameUtilities.UpdatePatternMatchingCountryCode(factory, PatternMatchingNameSchema.PMN_ParentId, nameUtilities.PatternMatchingNameMasterIdColumn, bizO.PK, master, currentValue);
					shouldSave |= phoneUtilities.UpdatePatternMatchingCountryCode(factory, PatternMatchingPhoneSchema.PMP_ParentId, phoneUtilities.PatternMatchingPhoneMasterIdColumn, bizO.PK, master, currentValue);
					shouldSave |= regCodeUtilities.UpdatePatternMatchingCountryCode(factory, PatternMatchingRegCodeSchema.PMR_ParentId, regCodeUtilities.PatternMatchingRegCodeMasterIdColumn, bizO.PK, master, currentValue);
				}
			}

			return shouldSave;
		}

		static string TrimCountryCode(object inputValue)
		{
			var result = inputValue.ToStringSafe();

			return result.Length >= 2 ? result.Substring(0, 2) : string.Empty;   //	CountryCode Length is 2 characters
		}
	}
}
