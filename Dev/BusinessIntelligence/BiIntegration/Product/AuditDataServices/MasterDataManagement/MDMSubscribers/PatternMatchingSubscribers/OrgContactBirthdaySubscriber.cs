using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class OrgContactBirthdaySubscriber : OrgContactSubscriber
	{
		public override string Code => "OCB";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Org Contact Birthday Subscriber";

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[] { OrgContactSchema.OC_Birthday };

		protected override ICollection<string> ColumnsToHash => new[] { OrgContactSchema.Constants.OC_Birthday };

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgContactSchema.OC_Birthday.Name] == DBNull.Value)
			{
				row.Delete();
			}
		};

		readonly PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode> subscriberUtilitiesRegCode = new PersonPatternMatchingSubscriberUtilities<PatternMatchingRegCode>();

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesRegCode.CreatePatternMatchingBusinessObject(factory, BizO.OC_PER, hashedValue, OrgContactSchema.Constants.Prefix, PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK, BizO.Person.PER_RN_NKCountry);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			return subscriberUtilitiesRegCode.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue);
		}

		static readonly DateTime beginDate = new DateTime(1753, 1, 1); // minimum date in SQL Server

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var valuesToHash = new List<string>();

			if (changeRow[changeRowField, rowVersion] is DateTime birthday)
			{
				var birthdayStr = (birthday - beginDate).Days.ToString(CultureInfo.InvariantCulture);
				valuesToHash.Add(birthdayStr);
			}
			else
			{
				valuesToHash.Add(string.Empty);
			}

			return valuesToHash;
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			var birthdateStr = string.Empty;
			if (changeRow[OrgContactSchema.Constants.OC_Birthday] is DateTime birthdate)
			{
				birthdateStr = birthdate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
			}

			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2}",
				changeRow.RowState.ToString(),
				changeRow[OrgContactSchema.Constants.PK].ToString(),
				birthdateStr
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgContactBirthdaySubscriber);
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			return subscriberUtilitiesRegCode.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_HashedValue, hashedValue, originalHashedValue, BizO.OC_PER, OrgContactSchema.Constants.Prefix, BizO.Person.PER_RN_NKCountry);
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
