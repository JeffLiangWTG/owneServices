using System;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class MembershipFilter : ModuleTextBaseFilter
	{
		public MembershipFilter() : base(new ZString("Has Membership"), (GetEmptyDelegate)EmptyQuery)
		{
			Category = new FilterCategory(ResString.GetMultilingualString("950574BC-E23A-44EB-91C0-2C7C8A64D4F9", "Memberships"));
		}

		#region Query

		delegate ZQuery GetEmptyDelegate();

		static ZQuery EmptyQuery()
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : GetMembershipQuery();
		}

		#endregion
		ZQuery GetMembershipQuery()
		{
			var query = new ZDBOnlyQuery(typeof(EDIOrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(EdiOrgMembership), EdiOrgMembershipSchema.EOR_OH);
			if (!MembershipType.IsEmpty)
			{
				subQuery.AddToFilter(EdiOrgMembershipSchema.EOR_MembershipType, MembershipType);
			}

			if (!Organisation.IsEmpty)
			{
				subQuery.AddToFilter(EdiOrgMembershipSchema.EOR_OH_Organisation, Organisation);
			}

			if (!ValidFromFrom.IsEmpty)
			{
				subQuery.AddToFilter(EdiOrgMembershipSchema.EOR_ValidFrom, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ValidFromFrom);
			}

			if (!ValidFromTo.IsEmpty)
			{
				subQuery.AddToFilter(EdiOrgMembershipSchema.EOR_ValidFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ValidFromTo);
			}

			if (!ValidToFrom.IsEmpty)
			{
				subQuery.AddToFilter(EdiOrgMembershipSchema.EOR_ValidTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ValidToFrom);
			}

			if (!ValidToTo.IsEmpty)
			{
				subQuery.AddToFilter(EdiOrgMembershipSchema.EOR_ValidTo, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ValidToTo);
			}

			if (!AgreementVersionFrom.IsEmpty)
			{
				subQuery.AddToFilter(EdiOrgMembershipSchema.EOR_AgreementVersion, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, AgreementVersionFrom);
			}

			if (!AgreementVersionTo.IsEmpty)
			{
				subQuery.AddToFilter(EdiOrgMembershipSchema.EOR_AgreementVersion, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, AgreementVersionTo);
			}

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#region Properties

		#region MembershipTypeList

		ZString membershipType;
		[List("MembershipTypeList")]
		public ZString MembershipType
		{
			get { return membershipType; }
			set
			{
				if (SetNonPersistentPropertyValue(MembershipTypeInfo, ref membershipType, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateMembershipType();
					}
					MembershipTypeInfo.RefreshBinding();

					if (Organisation_ReadOnly)
					{
						Organisation = ZGuid.Empty;
					}
				}
			}
		}

		public ZPropertyInfo MembershipTypeInfo
		{
			get { return GetZPropertyInfo(nameof(MembershipType)); }
		}

		#endregion

		#region Organisation

		ZGuid organisation;
		[List("Organisations")]
		public ZGuid Organisation
		{
			get { return organisation; }
			set
			{
				if (SetNonPersistentPropertyValue(OrganisationInfo, ref organisation, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateOrganisation();
					}
					OrganisationInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo OrganisationInfo
		{
			get { return GetZPropertyInfo(nameof(Organisation)); }
		}

		public bool Organisation_ReadOnly
		{
			get
			{
				return !MembershipTypeList.GetBoolFromCode(MembershipType);
			}
		}

		#endregion

		#region ValidFrom

		ZDate validFromFrom;
		public ZDate ValidFromFrom
		{
			get { return validFromFrom; }
			set
			{
				if (SetNonPersistentPropertyValue(ValidFromFromInfo, ref validFromFrom, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateValidFromFrom();
						Validation.ValidateValidFromTo();
					}
					ValidFromFromInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ValidFromFromInfo
		{
			get { return GetZPropertyInfo(nameof(ValidFromFrom)); }
		}

		ZDate validFromTo;
		public ZDate ValidFromTo
		{
			get { return validFromTo; }
			set
			{
				if (SetNonPersistentPropertyValue(ValidFromToInfo, ref validFromTo, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateValidFromFrom();
						Validation.ValidateValidFromTo();
					}
					ValidFromToInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ValidFromToInfo
		{
			get { return GetZPropertyInfo(nameof(ValidFromTo)); }
		}

		#endregion

		#region ValidTo

		ZDate validToFrom;
		public ZDate ValidToFrom
		{
			get { return validToFrom; }
			set
			{
				if (SetNonPersistentPropertyValue(ValidToFromInfo, ref validToFrom, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateValidToFrom();
						Validation.ValidateValidToTo();
					}
					ValidToFromInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ValidToFromInfo
		{
			get { return GetZPropertyInfo(nameof(ValidToFrom)); }
		}

		ZDate validToTo;
		public ZDate ValidToTo
		{
			get { return validToTo; }
			set
			{
				if (SetNonPersistentPropertyValue(ValidToToInfo, ref validToTo, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateValidToFrom();
						Validation.ValidateValidToTo();
					}
					ValidToToInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ValidToToInfo
		{
			get { return GetZPropertyInfo(nameof(ValidToTo)); }
		}

		#endregion

		ZDate agreementVersionFrom;
		public ZDate AgreementVersionFrom
		{
			get { return agreementVersionFrom; }
			set
			{
				if (SetNonPersistentPropertyValue(AgreementVersionFromInfo, ref agreementVersionFrom, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateAgreementVersionFrom();
					}
					AgreementVersionFromInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AgreementVersionFromInfo
		{
			get { return GetZPropertyInfo(nameof(AgreementVersionFrom)); }
		}

		ZDate agreementVersionTo;
		public ZDate AgreementVersionTo
		{
			get { return agreementVersionTo; }
			set
			{
				if (SetNonPersistentPropertyValue(AgreementVersionToInfo, ref agreementVersionTo, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateAgreementVersionTo();
					}
					AgreementVersionToInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AgreementVersionToInfo
		{
			get { return GetZPropertyInfo(nameof(AgreementVersionTo)); }
		}
		#endregion

		#region Overrides

		public new MembershipFilterValidation Validation
		{
			get { return (MembershipFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new MembershipFilterValidation(this);
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { MembershipType, Organisation, ValidFromFrom, ValidFromTo, ValidToFrom, ValidToTo }; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException();
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (MembershipFilter)filterToCopyFrom;
			MembershipType = filter.MembershipType;
			Organisation = filter.Organisation;
			ValidFromFrom = filter.ValidFromFrom;
			ValidFromTo = filter.ValidFromTo;
			ValidToFrom = filter.ValidToFrom;
			ValidToTo = filter.ValidToTo;
		}

		protected override void ClearCore()
		{
			MembershipType = string.Empty;
			Organisation = ZGuid.Empty;
			ValidFromFrom = ZDate.Empty;
			ValidFromTo = ZDate.Empty;
			ValidToFrom = ZDate.Empty;
			ValidToTo = ZDate.Empty;
		}

		protected override bool IsEmptyCore => MembershipType.IsEmpty && Organisation.IsEmpty && ValidFromFrom.IsEmpty && ValidFromTo.IsEmpty && ValidToFrom.IsEmpty && ValidToTo.IsEmpty && AgreementVersionFrom.IsEmpty && AgreementVersionTo.IsEmpty;

		public override bool IsExpensiveQuery => false;

		protected override FilterCategory DefaultCategory => FilterCategories.Other;

		#endregion

		#region Lookups

		CodeDescriptionBoolCollection memberships;
		public CodeDescriptionBoolCollection MembershipTypeList
		{
			get
			{
				if (memberships == null)
				{
					memberships = EdiOrgMembershipLookups.GetMembershipTypes();
				}

				return memberships;
			}
		}

		public OrgHeaderCollection Organisations
		{
			get
			{
				return new OrgHeaderCollection(new BusinessObjectFactory());
			}
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("MembershipType", MembershipType);
			writer.WriteElementString("Organisation", Organisation.ToString());
			writer.WriteElementString("ValidFromFrom", ValidFromFrom.ToISO8601ShortDateString());
			writer.WriteElementString("ValidFromTo", ValidFromTo.ToISO8601ShortDateString());
			writer.WriteElementString("ValidToFrom", ValidToFrom.ToISO8601ShortDateString());
			writer.WriteElementString("ValidToTo", ValidToTo.ToISO8601ShortDateString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "MembershipType")
			{
				MembershipType = reader.ReadElementString("MembershipType");
			}

			if (reader.Name == "Organisation")
			{
				if (!reader.IsEmptyElement)
				{
					var org = reader.ReadElementString("Organisation");
					if (!string.IsNullOrEmpty(org))
					{
						Organisation = new ZGuid(org);
					}
				}
			}

			ValidFromFrom = ParseDateFromReader(reader, "ValidFromFrom");
			ValidFromTo = ParseDateFromReader(reader, "ValidFromTo");
			ValidToFrom = ParseDateFromReader(reader, "ValidToFrom");
			ValidToTo = ParseDateFromReader(reader, "ValidToTo");
		}

		ZDate ParseDateFromReader(XmlReader reader, string name)
		{
			if (reader.Name == name && !reader.IsEmptyElement)
			{
				var value = reader.ReadElementString(name);
				if (ZDateTime.TryParseISO8601Date(value, out var result))
				{
					return result.Date;
				}
			}

			return ZDate.Empty;
		}

		#endregion
	}
}
