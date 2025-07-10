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
	public class GlbStaffAddressLineSubscriber : GlbStaffSubscriber
	{
		public override string Code => "GSA";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Staff Address Line Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { GlbStaffSchema.GS_UserAddress1, GlbStaffSchema.GS_UserAddress2, GlbStaffSchema.GS_City, GlbStaffSchema.GS_Postcode, GlbStaffSchema.GS_State, GlbStaffSchema.GS_RN_NKCountryCode };

		protected override IEnumerable<SchemaColumn> GetCountryCodeColumns => new SchemaColumn[] { GlbStaffSchema.GS_RN_NKCountryCode };

		protected override BusinessObject GetMaster(BusinessObjectFactory factory)
		{
			return BizO.Person;
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[GlbStaffSchema.GS_UserAddress1.Name] == DBNull.Value || Convert.ToString(row[GlbStaffSchema.GS_UserAddress1.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override ICollection<string> ColumnsToHash => new[] { string.Empty };

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingAddress> subscriberUtilitiesAddress = new PersonPatternMatchingSubscriberUtilities<PatternMatchingAddress>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesAddress.CreatePatternMatchingBusinessObject(factory, BizO.GS_PER, hashedValue, GlbStaffSchema.Constants.Prefix, PatternMatchingAddressSchema.PMA_ParentId, BizO.PK, BizO.Person.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesAddress.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingAddressSchema.PMA_ParentId, PatternMatchingAddressSchema.PMA_HashedValue, hashedValue);
		}

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();
			var address1 = changeRow[GlbStaffSchema.Constants.GS_UserAddress1, rowVersion].ToStringSafe();
			var address2 = changeRow[GlbStaffSchema.Constants.GS_UserAddress2, rowVersion].ToStringSafe();
			var city = changeRow[GlbStaffSchema.Constants.GS_City, rowVersion].ToStringSafe();
			var postcode = changeRow[GlbStaffSchema.Constants.GS_Postcode, rowVersion].ToStringSafe();
			var state = changeRow[GlbStaffSchema.Constants.GS_State, rowVersion].ToStringSafe();
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
				changeRow[GlbStaffSchema.Constants.PK].ToString(),
				changeRow[GlbStaffSchema.Constants.GS_UserAddress1].ToString().Trim(),
				changeRow[GlbStaffSchema.Constants.GS_UserAddress2].ToString().Trim(),
				changeRow[GlbStaffSchema.Constants.GS_City].ToString().Trim(),
				changeRow[GlbStaffSchema.Constants.GS_Postcode].ToString().Trim(),
				changeRow[GlbStaffSchema.Constants.GS_State].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(GlbStaffAddressLineSubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return BizO.Person != null && subscriberUtilitiesAddress.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingAddressSchema.PMA_ParentId, PatternMatchingAddressSchema.PMA_HashedValue, hashedValue, originalHashedValue, BizO.GS_PER, GlbStaffSchema.Constants.Prefix, BizO.Person.PER_RN_NKCountry);
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			if (BizO.Person != null)
			{
				var queued = PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(BizO.Person, factory);
				yield return new PatternMasterDetail(PatternMasterType.GlbPerson, BizO.Person.PK, queued);
			}
		}
	}
}
