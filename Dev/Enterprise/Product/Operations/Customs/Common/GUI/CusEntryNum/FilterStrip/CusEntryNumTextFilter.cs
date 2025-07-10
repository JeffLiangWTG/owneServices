using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common.Module
{
	public class CusEntryNumTextFilter : ModuleTextFilter
	{
		#region Construction

		public CusEntryNumTextFilter(ZString description, GetTextQueryWithOperator queryDelegate, Type bizObjType)
			: base(description, queryDelegate)
		{
			if (bizObjType == null)
			{
				throw new ArgumentNullException(nameof(bizObjType));
			}

			this.bizObjType = bizObjType;
		}

		public CusEntryNumTextFilter(ZString description, Type bizObjType)
			: base(description, CusEntryNumSchema.CE_EntryNum)
		{
			if (bizObjType == null)
			{
				throw new ArgumentNullException(nameof(bizObjType));
			}

			this.bizObjType = bizObjType;
		}

		readonly Type bizObjType;

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			base.ClearCore();
			EntryType = ZString.Empty;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && EntryType.IsEmpty && ComparisonOperator != ComparisonConstants.IsBlank && ComparisonOperator != ComparisonConstants.IsNotBlank;

		#endregion

		#region Properties

		[MaxLength(CusEntryNumber.Schema.CE_EntryTypeMaxLength)]
		public ZString EntryType
		{
			get { return fEntryType; }
			set
			{
				if (fEntryType != value)
				{
					CheckMaximumLength(EntryTypeInfo, value);
					fEntryType = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty();
					}

					EntryTypeInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo EntryTypeInfo
		{
			get { return GetZPropertyInfo(nameof(EntryType)); }
		}

		ZString fEntryType;

		#endregion

		#region Query

		protected override bool ShouldReevaluateQuery()
		{
			return base.ShouldReevaluateQuery() || currentCompanyCountryCodeWhenQueryWasLastGenerated != GetCurrentCompanyCountryCode();
		}

		static ZString GetCurrentCompanyCountryCode()
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected override ZQuery GetQuery()
		{
			return !IsEmpty ? CusEntryNumberModuleFilterQueryBuilder.BuildQuery(bizObjType, GetCusEntryNumQuery(), Array.Empty<ZDBOnlySubQuery>()) : new ZQuery();
		}

		ZString currentCompanyCountryCodeWhenQueryWasLastGenerated;

		protected virtual ZDBOnlySubQuery GetCusEntryNumQuery()
		{
			currentCompanyCountryCodeWhenQueryWasLastGenerated = GetCurrentCompanyCountryCode();
			ZDBOnlySubQuery resultSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, IsNotIn);
			resultSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, currentCompanyCountryCodeWhenQueryWasLastGenerated);

			if (Property.IsValid)
			{
				resultSubQuery.AddToFilter(base.GetQuery(), JoinCondition.And);
			}

			if (!EntryType.IsEmpty)
			{
				resultSubQuery = GetEntryTypeFilter(resultSubQuery);
			}

			return resultSubQuery;
		}

		ZDBOnlySubQuery GetEntryTypeFilter(ZDBOnlySubQuery cusEntryNumSubQuery)
		{
			cusEntryNumSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, EntryType);
			return cusEntryNumSubQuery;
		}

		bool IsNotIn
		{
			get
			{
				switch (ComparisonOperator)
				{
					case ComparisonConstants.NotEqual:
					case ComparisonConstants.NotContain:
					case ComparisonConstants.NotStartsWith:
					case ComparisonConstants.IsNotBlank:
						return true;
					default:
						return false;
				}
			}
		}

		public override SQLComparisonOperator SqlComparisonOperator
		{
			get
			{
				switch (ComparisonOperator)
				{
					case ComparisonConstants.NotEqual:
						return SQLComparisonOperator.Equal;
					case ComparisonConstants.NotContain:
						return SQLComparisonOperator.Contains;
					case ComparisonConstants.NotStartsWith:
						return SQLComparisonOperator.StartsWith;
					case ComparisonConstants.IsNotBlank:
						return SpecialComparisonOperator.IsBlank;
					default:
						return base.SqlComparisonOperator;
				}
			}
			set => base.SqlComparisonOperator = value;
		}
		#endregion

		#region XML Serialization

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			if (reader.Name == "EntryType")
			{
				EntryType = reader.ReadElementString("EntryType");
			}
		}

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("EntryType", EntryType);
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList AdditionalReferenceNumberTypes
		{
			get
			{
				return CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
		}

		#endregion
	}
}
