namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using CargoWise.Tools.DuplicateDetector;
	using CargoWise.Types;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.MasterData.Business;
	using Enterprise.MasterData.Common;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	public class OrgHeaderSubscriber : PatternMatchingSubscriber<OrgHeader>
	{
		protected override string PKColumn
		{
			get { return OrgHeaderSchema.Constants.PK; }
		}

		public override string Code
		{
			get { return "OHF"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description
		{
			get { return "Org Header Subscriber"; }
		}

		public override ITableSchema Table
		{
			get { return OrgHeaderSchema.Instance; }
		}

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get
			{
				return new SchemaColumn[] { OrgHeaderSchema.OH_FullName };
			}
		}

		protected override ICollection<string> ColumnsToHash
		{
			get
			{
				return new string[] { OrgHeaderSchema.Constants.OH_FullName };
			}
		}

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[OrgHeaderSchema.OH_IsActive.Name] == DBNull.Value
				|| Convert.ToInt32(row[OrgHeaderSchema.OH_IsActive.Name]) != 1)
			{
				row.Delete();
			}
		};

		protected override List<string> GetValuesToHash(DataRow changeRow, string changeRowField, DataRowVersion rowVersion)
		{
			var orgHeaderFullName = changeRow[changeRowField, rowVersion].ToStringSafe();
			return new List<string> { TextStandardizerHelper.StandardizeCompanyName(orgHeaderFullName, BizO.CountryCode) };
		}

		protected override string GetRowDetail(DataRow changeRow)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"{0} - {1} - {2} - {3}",
				changeRow.RowState.ToString(),
				changeRow[OrgHeaderSchema.Constants.PK].ToString(),
				changeRow[OrgHeaderSchema.Constants.OH_Code].ToString().Trim(),
				changeRow[OrgHeaderSchema.Constants.OH_FullName].ToString().Trim()
			);
		}

		protected override string GetSubscriberName()
		{
			return nameof(OrgHeaderSubscriber);
		}

		protected override bool CreateNewRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var shouldSave = false;

			var subscriberUtilitiesName = new OrgPatternMatchingSubscriberUtilities<PatternMatchingName>();
			var subscriberUtilitiesPhone = new OrgPatternMatchingSubscriberUtilities<PatternMatchingPhone>();
			var subscriberUtilitiesAddress = new OrgPatternMatchingSubscriberUtilities<PatternMatchingAddress>();
			var subscriberUtilitiesEmail = new OrgPatternMatchingSubscriberUtilities<PatternMatchingEmail>();
			var subscriberUtilitiesDomain = new OrgPatternMatchingSubscriberUtilities<PatternMatchingDomain>();
			var subscriberUtilitiesRegCode = new OrgPatternMatchingSubscriberUtilities<PatternMatchingRegCode>();

			shouldSave |= subscriberUtilitiesName.CreatePatternMatchingBusinessObject(factory, BizO.PK, hashedValue, OrgHeaderSchema.Constants.Prefix, PatternMatchingNameSchema.PMN_ParentId, BizO.PK, BizO.CountryCode);

			foreach (OrgAddress address in BizO.Addresses)
			{
				var addressLineValueToHash = address.OA_Address1 + address.OA_Address2 + address.OA_City + address.OA_PostCode + address.OA_State;
				shouldSave |= CreateChildRecords(subscriberUtilitiesAddress, factory, addressLineValueToHash, OrgAddressSchema.Constants.Prefix, PatternMatchingAddressSchema.PMA_ParentId, address.PK, BizO.CountryCode);

				var addressCompanyNameValueToHash = TextStandardizerHelper.StandardizeCompanyName(address.OA_CompanyNameOverride, BizO.CountryCode);
				shouldSave |= CreateChildRecords(subscriberUtilitiesName, factory, addressCompanyNameValueToHash, OrgAddressSchema.Constants.Prefix, PatternMatchingNameSchema.PMN_ParentId, address.PK, BizO.CountryCode);

				var addressEmailDomainValueToHash = TextStandardizerHelper.ExtractEmailDomain(address.OA_Email);
				var addressEmailCleansedValueToHash = TextStandardizerHelper.StandardizeEmail(address.OA_Email);
				shouldSave |= CreateChildRecords(subscriberUtilitiesDomain, factory, addressEmailDomainValueToHash, OrgAddressSchema.Constants.Prefix, PatternMatchingDomainSchema.PMD_ParentId, address.PK, BizO.CountryCode);
				shouldSave |= CreateChildRecords(subscriberUtilitiesEmail, factory, addressEmailCleansedValueToHash, OrgAddressSchema.Constants.Prefix, PatternMatchingEmailSchema.PME_ParentId, address.PK, BizO.CountryCode);

				var addressPhoneValueToHash = address.OA_Phone;
				var addressFaxValueToHash = address.OA_Fax;
				var addressMobileValueToHash = address.OA_Mobile;
				shouldSave |= CreateChildRecords(subscriberUtilitiesPhone, factory, addressPhoneValueToHash, OrgAddressSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, address.PK, BizO.CountryCode);
				shouldSave |= CreateChildRecords(subscriberUtilitiesPhone, factory, addressFaxValueToHash, OrgAddressSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, address.PK, BizO.CountryCode);
				shouldSave |= CreateChildRecords(subscriberUtilitiesPhone, factory, addressMobileValueToHash, OrgAddressSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, address.PK, BizO.CountryCode);
			}

			foreach (OrgBrandOrRelatedName brand in BizO.BrandsOrRelatedNames)
			{
				var brandValueToHash = TextStandardizerHelper.StandardizeCompanyName(brand.P1_RelatedName, BizO.CountryCode);
				shouldSave |= CreateChildRecords(subscriberUtilitiesName, factory, brandValueToHash, OrgBrandOrRelatedNameSchema.Constants.Prefix, PatternMatchingNameSchema.PMN_ParentId, brand.PK, BizO.CountryCode);
			}

			foreach (OrgContact contact in BizO.Contacts)
			{
				var contactNameValueToHash = TextStandardizerHelper.StandardizePersonName(contact.OC_ContactName);
				shouldSave |= CreateChildRecords(subscriberUtilitiesName, factory, contactNameValueToHash, OrgContactSchema.Constants.Prefix, PatternMatchingNameSchema.PMN_ParentId, contact.PK, BizO.CountryCode);

				var contactEmailDomainValueToHash = TextStandardizerHelper.ExtractEmailDomain(contact.OC_Email);
				var contactEmailStandardisedValueToHash = TextStandardizerHelper.StandardizeEmail(contact.OC_Email);
				shouldSave |= CreateChildRecords(subscriberUtilitiesDomain, factory, contactEmailDomainValueToHash, OrgContactSchema.Constants.Prefix, PatternMatchingDomainSchema.PMD_ParentId, contact.PK, BizO.CountryCode);
				shouldSave |= CreateChildRecords(subscriberUtilitiesEmail, factory, contactEmailStandardisedValueToHash, OrgContactSchema.Constants.Prefix, PatternMatchingEmailSchema.PME_ParentId, contact.PK, BizO.CountryCode);

				var contactPhoneValueToHash = contact.OC_Phone;
				shouldSave |= CreateChildRecords(subscriberUtilitiesPhone, factory, contactPhoneValueToHash, OrgContactSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, contact.PK, BizO.CountryCode);

				var contactPhoneHomeValueToHash = contact.OC_HomePhone;
				shouldSave |= CreateChildRecords(subscriberUtilitiesPhone, factory, contactPhoneHomeValueToHash, OrgContactSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, contact.PK, BizO.CountryCode);

				var contactMobileValueToHash = contact.OC_Mobile;
				shouldSave |= CreateChildRecords(subscriberUtilitiesPhone, factory, contactMobileValueToHash, OrgContactSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, contact.PK, BizO.CountryCode);

				var contactOtherPhoneValueToHash = contact.OC_OtherPhone;
				shouldSave |= CreateChildRecords(subscriberUtilitiesPhone, factory, contactOtherPhoneValueToHash, OrgContactSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, contact.PK, BizO.CountryCode);

				var contactFaxValueToHash = contact.OC_Fax;
				shouldSave |= CreateChildRecords(subscriberUtilitiesPhone, factory, contactFaxValueToHash, OrgContactSchema.Constants.Prefix, PatternMatchingPhoneSchema.PMP_ParentId, contact.PK, BizO.CountryCode);
			}

			foreach (OrgCusCode cusCode in BizO.CustomsCodes)
			{
				var cusCodeValueToHash = cusCode.OK_CustomsRegNo;
				shouldSave |= CreateChildRecords(subscriberUtilitiesRegCode, factory, cusCodeValueToHash, OrgCusCodeSchema.Constants.Prefix, PatternMatchingRegCodeSchema.PMR_ParentId, cusCode.PK, BizO.CountryCode);
			}

			foreach (OrgWebURL webURL in BizO.OrgWebURLs)
			{
				var webURLDomain = TextStandardizerHelper.ExtractEmailDomain(webURL.PU_URL);
				shouldSave |= CreateChildRecords(subscriberUtilitiesDomain, factory, webURLDomain, OrgWebURLSchema.Constants.Prefix, PatternMatchingDomainSchema.PMD_ParentId, webURL.PK, BizO.CountryCode);
			}

			return shouldSave;
		}

		protected override bool UpdateExistingRecords(BusinessObjectFactory factory, int index, int hashedValue, int originalHashedValue)
		{
			var subscriberUtilitiesName = new OrgPatternMatchingSubscriberUtilities<PatternMatchingName>();
			return subscriberUtilitiesName.UpdatePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_HashedValue, hashedValue, originalHashedValue, BizO.PK, OrgHeaderSchema.Constants.Prefix, BizO.CountryCode);
		}

		protected override bool DeleteExistingRecords(BusinessObjectFactory factory, int index, int hashedValue)
		{
			var subscriberUtilitiesName = new OrgPatternMatchingSubscriberUtilities<PatternMatchingName>();
			return subscriberUtilitiesName.DeletePatternMatchingBusinessObject(factory, BizO.PK, PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_HashedValue, hashedValue);
		}

		bool CreateChildRecords<T>(PatternMatchingSubscriberUtilities<T> subscriberUtilitiesPatternMatching, BusinessObjectFactory factory, string valueToHash, string tableCode, SchemaGuidColumn parentIdColumn, ZGuid parentId, string countryCode) where T : BusinessObject, IPatternMatchingBusinessObjects
		{
			if (!string.IsNullOrEmpty(valueToHash))
			{
				var hashedValue = TextStandardizerHelper.ComputeStringHashFast(valueToHash);
				return subscriberUtilitiesPatternMatching.CreatePatternMatchingBusinessObject(factory, BizO.PK, hashedValue, tableCode, parentIdColumn, parentId, countryCode);
			}

			return false;
		}

		protected override IEnumerable<PatternMasterDetail> ProcessPatternMatchingResultsCore(BusinessObjectFactory factory)
		{
			var queued = PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(BizO, factory);
			yield return new PatternMasterDetail(PatternMasterType.OrgHeader, BizO.PK, queued);
		}
	}
}
