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
	public class CusEntryNumDateFilter : ModuleDateFilter
	{
		#region Construction

		public CusEntryNumDateFilter(ZString description, Type bizObjType)
			: base(description, CusEntryNumSchema.CE_IssueDate)
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

		protected override bool IsEmptyCore => base.IsEmptyCore && EntryType.IsEmpty;

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
						Validation.ValidatePropertySearch();
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

		protected override ZQuery GetQuery()
		{
			return !IsEmpty ? CusEntryNumberModuleFilterQueryBuilder.BuildQuery(bizObjType, GetCusEntryNumQuery(), Array.Empty<ZDBOnlySubQuery>()) : new ZQuery();
		}

		protected virtual ZDBOnlySubQuery GetCusEntryNumQuery()
		{
			ZDBOnlySubQuery resultSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);

			if (FromDate.IsValid || ToDate.IsValid || IsPropertySearchValid)
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
