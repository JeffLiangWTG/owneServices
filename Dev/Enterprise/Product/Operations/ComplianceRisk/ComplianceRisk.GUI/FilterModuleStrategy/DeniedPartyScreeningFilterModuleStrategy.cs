using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI
{
	public class DeniedPartyScreeningFilterModuleStrategy : FilterModuleStrategy
	{
		public override void RunOnFilterControlInitialisation(IFilterControl control, IBusinessObjectCollection gridCollection)
		{
			if (ComplianceRiskHelper.CheckIfComplianceRiskEnabled(gridCollection.TypeOfElements, allowViewType: true))
			{
				AddComplianceRiskStatusColumn(control, ComplianceRisk.Types.Overall, ComplianceRisk.Descriptions.Overall);
				AddComplianceRiskStatusColumn(control, ComplianceRisk.Types.Party, ComplianceRisk.Descriptions.Party);
				AddComplianceRiskStatusColumn(control, ComplianceRisk.Types.Location, ComplianceRisk.Descriptions.Location);
				AddComplianceRiskStatusColumn(control, ComplianceRisk.Types.Commodity, ComplianceRisk.Descriptions.Commodity);
			}
		}

		protected override IEnumerable<ModuleFilter> FiltersToAdd
		{
			get
			{
				ModuleTextFilter deniedPartyStatusFilter = null;

				if (ComplianceRiskHelper.CheckIfComplianceRiskEnabled(BizoType, allowViewType: true))
				{
					yield return AddComplianceRiskStatusFilter(ComplianceRisk.Types.Overall, ComplianceRisk.Descriptions.Overall, GetOverallComplianceRiskStatusFilterQuery);
					yield return AddComplianceRiskStatusFilter(ComplianceRisk.Types.Party, ComplianceRisk.Descriptions.Party, GetPartyComplianceRiskStatusFilterQuery);
					yield return AddComplianceRiskStatusFilter(ComplianceRisk.Types.Location, ComplianceRisk.Descriptions.Location, GetLocationComplianceRiskStatusFilterQuery);
					yield return AddComplianceRiskStatusFilter(ComplianceRisk.Types.Commodity, ComplianceRisk.Descriptions.Commodity, GetCommodityComplianceRiskStatusFilterQuery);

					deniedPartyStatusFilter = new ModuleTextFilter("Legacy Screening Status", GetDeniedPartyScreeningStatusFilter, ScreeningStatus_List);
					deniedPartyStatusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|DeniedPartyStatusFilter|LegacyScreeningStatus", "Legacy Screening Status");
				}
				else
				{
					deniedPartyStatusFilter = new ModuleTextFilter("Screening Status", GetDeniedPartyScreeningStatusFilter, ScreeningStatus_List);
					deniedPartyStatusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|DeniedPartyStatusFilter|ScreeningStatus", "Screening Status");
				}

				if (typeof(IScreeningPartyProvider).IsAssignableFrom(BizoType) || typeof(IViewQuotedBooking).IsAssignableFrom(BizoType))
				{
					deniedPartyStatusFilter.Category = FilterCategories.StatusAndFlags;
					yield return deniedPartyStatusFilter;
				}
			}
		}

		ModuleTextFilter AddComplianceRiskStatusFilter(string filterDescription, MultilingualString resourceCaption, GetTextQueryWithOperator textQueryOperator)
		{
			var complianceRiskFilter = new ModuleTextFilter(filterDescription, textQueryOperator, filterDescription switch
			{
				ComplianceRisk.Types.Overall => RiskStatusHelper.GetOverallCodeDescriptionPairListForFilter(),
				ComplianceRisk.Types.Party => RiskStatusHelper.GetPartyCodeDescriptionPairListForFilter(),
				ComplianceRisk.Types.Location => RiskStatusHelper.GetLocationCodeDescriptionPairListForFilter(),
				_ => RiskStatusHelper.GetCommodityCodeDescriptionPairListForFilter(),
			});

			complianceRiskFilter.Category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("B1314046-C2D2-452E-946F-A5A4B34B3E66", "Compliance"));
			complianceRiskFilter.MultilingualDescription = resourceCaption;
			complianceRiskFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			complianceRiskFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			complianceRiskFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			complianceRiskFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			complianceRiskFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			return complianceRiskFilter;
		}

		#region Implementation

		ZQuery GetComplianceRiskStatusQuery(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn schemaColumn)
		{
			var query = new ZDBOnlyQuery(BizoType);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(ComplianceRiskStatus), ComplianceRiskStatusSchema.COR_ParentID, notIn: true);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}
			else if (comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(ComplianceRiskStatus), ComplianceRiskStatusSchema.COR_ParentID);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}
			else if (value.IsEmpty)
			{
				return new ZQuery();
			}
			else
			{
				var subQuery = new ZDBOnlySubQuery(typeof(ComplianceRiskStatus), ComplianceRiskStatusSchema.COR_ParentID);
				subQuery.AddToFilter(schemaColumn, comparisonOperator, value);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetOverallComplianceRiskStatusFilterQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetComplianceRiskStatusQuery(comparisonOperator, value, ComplianceRiskStatusSchema.COR_OverallRisk);

		ZQuery GetPartyComplianceRiskStatusFilterQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetComplianceRiskStatusQuery(comparisonOperator, value, ComplianceRiskStatusSchema.COR_PartyRisk);

		ZQuery GetLocationComplianceRiskStatusFilterQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetComplianceRiskStatusQuery(comparisonOperator, value, ComplianceRiskStatusSchema.COR_LocationRisk);

		ZQuery GetCommodityComplianceRiskStatusFilterQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetComplianceRiskStatusQuery(comparisonOperator, value, ComplianceRiskStatusSchema.COR_CommodityRisk);

		void AddComplianceRiskStatusColumn(IFilterControl filterControl, string complianceRiskType, MultilingualString resourceCaption)
		{
			Argument.NotNull(filterControl, nameof(filterControl), "Should have valid Filter Control");
			Argument.NotNullOrEmpty(complianceRiskType, nameof(complianceRiskType), "Should have valid Compliance Risk Type");

			var filterStripControl = (filterControl as ZFilterStripControl);
			var container = GetComplianceRiskCustomPropertyContainer(complianceRiskType, resourceCaption);

			new ComplianceRiskCustomColumnsInitializer(filterStripControl.FilteredGrid, filterStripControl.GridCollection, groupName: ResourceStringData.Empty, container).AddCustomColumns(container.CustomProperties);

			SetFetchHintForView(filterStripControl.GridCollection);
		}

		CustomPropertyContainer GetComplianceRiskCustomPropertyContainer(string complianceRiskType, MultilingualString resourceCaption)
		{
			var container = new CustomPropertyContainer();
			container.AddCustomProperty(new CustomPropertyImplementation<BusinessObject>(
				identifier: complianceRiskType,
				caption: resourceCaption,
				type: typeof(ZString),
				valueGetter: (bizo) =>
				{
					var complianceRiskStatus = bizo.Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, ((IIdentified)bizo).Identifier));
					var complianceRiskStatusFilterValue = string.Empty;

					switch (complianceRiskType)
					{
						case ComplianceRisk.Types.Overall:
							complianceRiskStatusFilterValue = complianceRiskStatus?.COR_OverallRisk;
							break;
						case ComplianceRisk.Types.Party:
							complianceRiskStatusFilterValue = complianceRiskStatus?.COR_PartyRisk;
							break;
						case ComplianceRisk.Types.Location:
							complianceRiskStatusFilterValue = complianceRiskStatus?.COR_LocationRisk;
							break;
						case ComplianceRisk.Types.Commodity:
							complianceRiskStatusFilterValue = complianceRiskStatus?.COR_CommodityRisk;
							break;
					}
					return complianceRiskStatusFilterValue;
				}));

			return container;
		}

		void SetFetchHintForView(IBusinessObjectCollection collection)
		{
			if (collection?.FetchStrategy != null)
			{
				collection.FetchStrategy.AdditionalFetchForView += (sender, eventArgs) =>
				{
					var matchingColumns = eventArgs.TableColumns.Where(x =>
											x.ColumnName == ComplianceRisk.Types.Overall ||
											x.ColumnName == ComplianceRisk.Types.Party ||
											x.ColumnName == ComplianceRisk.Types.Location ||
											x.ColumnName == ComplianceRisk.Types.Commodity);

					if (matchingColumns.Any())
					{
						foreach (var bizo in eventArgs.BusinessObjects)
						{
							bizo.Factory.AddFetchHint(ComplianceRiskStatusSchema.COR_ParentID, bizo.PK);
						}
					}
				};
			}
		}

		ZQuery GetDeniedPartyScreeningStatusFilter(ZString value)
		{
			var tableCode = BusinessObjectFactory.GetTableCodeFromType(BizoType);
			if (string.IsNullOrEmpty(tableCode))
			{
				return new ZQuery();
			}

			var result = new ZDBOnlyQuery(BizoType);
			if (tableCode == ViewQuotedBookingSchema.Constants.Prefix)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(IForwardingShipment), JobShipmentSchema.PK);
				subQuery.AddToFilter(JobShipmentSchema.JS_ScreeningStatus, value);
				result.AddSubQuery(ViewQuotedBookingSchema.VB_JS, subQuery, JoinCondition.And);
			}
			else
			{
				var suffix = IsSupportedHVLVTable(tableCode) ? "_DeniedPartyScreeningStatus" : "_ScreeningStatus";
				var columnName = tableCode + suffix;
				var filterString = string.Format(" {0} = '{1}'", columnName, value);
				result.AddFilterAndZSQLParameterCollection(filterString, new ZSqlParameterCollection());
			}

			return result;
		}

		bool IsSupportedHVLVTable(string tableCode) => tableCode == HVLVBookingHeaderSchema.Constants.Prefix || tableCode == HVLVConsignmentSchema.Constants.Prefix;

		internal CodeDescriptionPairList ScreeningStatus_List
		{
			get
			{
				if (fScreeningStatus_List == null)
				{
					fScreeningStatus_List = new ScreeningStatusesList();
					fScreeningStatus_List.RemoveCode(ScreeningStatusesList.Codes.Block);
					fScreeningStatus_List.RemoveCode(ScreeningStatusesList.Codes.Release);
					fScreeningStatus_List.RemoveCode(ScreeningStatusesList.Codes.Canceled);
					fScreeningStatus_List.RemoveCode(ScreeningStatusesList.Codes.NeedsScreening);
				}

				if (!typeof(IBaseJobDeclaration).IsAssignableFrom(BizoType) || !OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.Value)
				{
					fScreeningStatus_List.RemoveCode(ScreeningStatusesList.Codes.JobBlockedExternal);
					fScreeningStatus_List.RemoveCode(ScreeningStatusesList.Codes.JobClearedExternal);
				}

				return fScreeningStatus_List;
			}
		}
		CodeDescriptionPairList fScreeningStatus_List;

		class ComplianceRisk
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
			public class Types
			{
				public const string Overall = "Overall";
				public const string Party = "Party";
				public const string Location = "Location";
				public const string Commodity = "Commodity";
			}

			public class Descriptions
			{
				public static MultilingualString Overall => ResString.GetMultilingualString("CF7D65A7-62BC-4FD5-81F5-48825B5C8FF5", "Job Compliance Status");
				public static MultilingualString Party => ResString.GetMultilingualString("B8F39E3E-8C42-416C-A310-82E67384E053", "Party Compliance Risk");
				public static MultilingualString Location => ResString.GetMultilingualString("5FA6770D-BC07-4684-874A-F2B7D4C8645A", "Location Compliance Risk");
				public static MultilingualString Commodity => ResString.GetMultilingualString("10A656EC-D5C1-454C-8C42-10F33BD93938", "Commodity Compliance Risk");
			}
		}

		#endregion
	}

	class ComplianceRiskCustomColumnsInitializer : ZGridCustomColumnsInitializer
	{
		public ComplianceRiskCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ResourceStringData groupName, ICustomPropertyContainer propertyContainer)
			: base(grid, collection, groupName)
		{
			this.propertyContainer = propertyContainer;
		}

		readonly ICustomPropertyContainer propertyContainer;

		protected override PropertyDescriptor GetPropertyDescriptor(ICustomProperty property) => new ComplianceRiskCustomPropertyDescriptor(propertyContainer, property);
	}

	class ComplianceRiskCustomPropertyDescriptor : ZCustomPropertyDescriptor
	{
		public ComplianceRiskCustomPropertyDescriptor(ICustomPropertyContainer propertyContainer, ICustomProperty property)
			: base(property.Identifier, property.Info.Type)
		{
			this.propertyContainer = propertyContainer;
		}

		readonly ICustomPropertyContainer propertyContainer;

		public override bool IsReadOnly => true;

		protected override ICustomPropertyContainer GetCustomPropertyContainer(object component) => propertyContainer;
	}
}
