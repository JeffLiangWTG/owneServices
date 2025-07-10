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
	public class OrgAddressAddressLineSubscriber : OrgAddressSubscriber
	{
		public override string Code
		{
			get { return "OAA"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description
		{
			get { return "Org Address Address Line Subscriber"; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get { return new SchemaColumn[] { OrgAddressSchema.OA_Address1, OrgAddressSchema.OA_Address2, OrgAddressSchema.OA_City, OrgAddressSchema.OA_PostCode, OrgAddressSchema.OA_State, OrgAddressSchema.OA_RN_NKCountryCode, OrgAddressSchema.OA_RL_NKRelatedPortCode }; }
		}

		protected override ICollection<string> ColumnsToHash
		{
			get
			{
				return new string[] { string.Empty };
			}
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgAddressSchema.OA_Address1.Name] == DBNull.Value || Convert.ToString(row[OrgAddressSchema.OA_Address1.Name]).Length <= 0)
			{
				row.Delete();
			}
		};

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesAddress = new OrgPatternMatchingSubscriberUtilities<PatternMatchingAddress>();
			return subscriberUtilitiesAddress.CreatePatternMatchingBusinessObject(factory, BizO.Header.PK, hashedValue, OrgAddressSchema.Constants.Prefix, PatternMatchingAddressSchema.PMA_ParentId, BizO.PK, BizO.Header.CountryCode);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			var subscriberUtilitiesAddress = new OrgPatternMatchingSubscriberUtilities<PatternMatchingAddress>();
			return subscriberUtilitiesAddress.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingAddressSchema.PMA_ParentId, PatternMatchingAddressSchema.PMA_HashedValue, hashedValue, originalHashedValue, BizO.Header.PK, OrgAddressSchema.Constants.Prefix, BizO.Header.CountryCode);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesAddress = new OrgPatternMatchingSubscriberUtilities<PatternMatchingAddress>();
			return subscriberUtilitiesAddress.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingAddressSchema.PMA_ParentId, PatternMatchingAddressSchema.PMA_HashedValue, hashedValue);
		}

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();

			var orgAddressAddress1 = changeRow[OrgAddressSchema.Constants.OA_Address1, rowVersion].ToStringSafe();
			var orgAddressAddress2 = changeRow[OrgAddressSchema.Constants.OA_Address2, rowVersion].ToStringSafe();
			var orgAddressCity = changeRow[OrgAddressSchema.Constants.OA_City, rowVersion].ToStringSafe();
			var orgAddressPostcode = changeRow[OrgAddressSchema.Constants.OA_PostCode, rowVersion].ToStringSafe();
			var orgAddressState = changeRow[OrgAddressSchema.Constants.OA_State, rowVersion].ToStringSafe();

			var address = GetValueToUpper(orgAddressAddress1 + orgAddressAddress2 + orgAddressCity + orgAddressPostcode + orgAddressState);

			if (!TextStandardizerHelper.IsPlaceholderAddress(orgAddressAddress1))
			{
				valuesToHash.Add(address);
			}
			else if (string.IsNullOrEmpty(orgAddressAddress1) &&
							 !TextStandardizerHelper.IsPlaceholderAddress(orgAddressAddress2))
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
					changeRow[OrgAddressSchema.Constants.PK].ToString(),
					changeRow[OrgAddressSchema.Constants.OA_Address1].ToString().Trim(),
					changeRow[OrgAddressSchema.Constants.OA_Address2].ToString().Trim(),
					changeRow[OrgAddressSchema.Constants.OA_City].ToString().Trim(),
					changeRow[OrgAddressSchema.Constants.OA_PostCode].ToString().Trim(),
					changeRow[OrgAddressSchema.Constants.OA_State].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgAddressAddressLineSubscriber);
		}

		protected override IEnumerable<SchemaColumn> GetCountryCodeColumns
		{
			get
			{
				return new SchemaColumn[] { OrgAddressSchema.OA_RL_NKRelatedPortCode, OrgAddressSchema.OA_RN_NKCountryCode };
			}
		}

		protected override BusinessObject GetMaster(BusinessObjectFactory factory)
		{
			var query = new ZQuery(OrgAddressCapabilitySchema.PZ_IsMainAddress, true);

			query.AddToFilter(OrgAddressCapabilitySchema.PZ_OA, BizO.PK);
			query.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, "OFC");

			var orgAddressCapability = factory.LoadTop1<OrgAddressCapability>(query);

			if (orgAddressCapability != null)
			{
				return BizO.Header;
			}

			return null;
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(BizO.Header, factory);
			yield return new PatternMasterDetail(PatternMasterType.OrgHeader, BizO.Header.PK, queued);
		}
	}
}
