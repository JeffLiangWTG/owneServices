using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class GlbPersonAddressLineSubscriber : GlbPersonSubscriber
	{
		public override string Code => "GPA";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "GlbPerson Address Line Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbPersonSchema.PER_HomeAddress1, GlbPersonSchema.PER_HomeAddress2, GlbPersonSchema.PER_City, GlbPersonSchema.PER_Postcode, GlbPersonSchema.PER_State, GlbPersonSchema.PER_RN_NKCountry };

		protected override IEnumerable<SchemaColumn> GetCountryCodeColumns => new SchemaColumn[] { GlbPersonSchema.PER_RN_NKCountry };

		protected override BusinessObject GetMaster(BusinessObjectFactory factory)
		{
			return BizO;
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[GlbPersonSchema.PER_HomeAddress1.Name] == DBNull.Value || Convert.ToString(row[GlbPersonSchema.PER_HomeAddress1.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { string.Empty };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingAddress> subscriberUtilitiesAddress = new PersonPatternMatchingSubscriberUtilities<PatternMatchingAddress>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesAddress.CreatePatternMatchingBusinessObject(factory, BizO.PK, hashedValue, GlbPersonSchema.Constants.Prefix, PatternMatchingAddressSchema.PMA_ParentId, BizO.PK, BizO.PER_RN_NKCountry);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return subscriberUtilitiesAddress.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingAddressSchema.PMA_ParentId, PatternMatchingAddressSchema.PMA_HashedValue, hashedValue, originalHashedValue, BizO.PK, GlbPersonSchema.Constants.Prefix, BizO.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesAddress.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingAddressSchema.PMA_ParentId, PatternMatchingAddressSchema.PMA_HashedValue, hashedValue);
		}

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();

			var address1 = changeRow[GlbPersonSchema.Constants.PER_HomeAddress1, rowVersion].ToStringSafe();
			var address2 = changeRow[GlbPersonSchema.Constants.PER_HomeAddress2, rowVersion].ToStringSafe();
			var city = changeRow[GlbPersonSchema.Constants.PER_City, rowVersion].ToStringSafe();
			var postcode = changeRow[GlbPersonSchema.Constants.PER_Postcode, rowVersion].ToStringSafe();
			var state = changeRow[GlbPersonSchema.Constants.PER_State, rowVersion].ToStringSafe();

			var address = GetValueToUpper(address1 + address2 + city + postcode + state);

			if (!TextStandardizerHelper.IsPlaceholderAddress(address1))
			{
				valuesToHash.Add(address);
			}
			else if (string.IsNullOrEmpty(address1) && !TextStandardizerHelper.IsPlaceholderAddress(address2))
			{
				valuesToHash.Add(address);
			}
			else
			{
				valuesToHash.Add(string.Empty);
			}

			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2} - {3} - {4} - {5} - {6}",
				changeRow.RowState.ToString(),
				changeRow[GlbPersonSchema.Constants.PK].ToString(),
				changeRow[GlbPersonSchema.Constants.PER_HomeAddress1].ToString().Trim(),
				changeRow[GlbPersonSchema.Constants.PER_HomeAddress2].ToString().Trim(),
				changeRow[GlbPersonSchema.Constants.PER_City].ToString().Trim(),
				changeRow[GlbPersonSchema.Constants.PER_Postcode].ToString().Trim(),
				changeRow[GlbPersonSchema.Constants.PER_State].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbPersonAddressLineSubscriber);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO, factory);
			yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.PK, queued);
		}
	}
}
