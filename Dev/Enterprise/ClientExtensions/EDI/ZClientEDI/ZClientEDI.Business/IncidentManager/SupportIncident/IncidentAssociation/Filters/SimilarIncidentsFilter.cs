using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public class SimilarIncidentsFilter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SupportIncident ParentIncident { get; }
		public SimilarIncidentsFilter(SupportIncident parentIncident)
		{
			ParentIncident = parentIncident;
		}
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IncidentStatusFilter = SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.All;
			DateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last12Mths;
		}

		#region IncidentStatus

		[List(nameof(Lookups) + "." + nameof(SimilarIncidentsFilterLookups.IncidentStatus))]
		public ZString IncidentStatusFilter
		{
			get => incidentStatusFilter;
			set => SetNonPersistentPropertyValue(IncidentStatusFilterInfo, ref incidentStatusFilter, value);
		}
		ZString incidentStatusFilter;

		public ZPropertyInfo IncidentStatusFilterInfo => GetZPropertyInfo(nameof(IncidentStatusFilter));

		#endregion

		#region AdditionalFilters

		public ZBool SameClient
		{
			get => sameClient;
			set => SetNonPersistentPropertyValue(SameIncidentInfo, ref sameClient, value);
		}
		ZBool sameClient;

		public ZPropertyInfo SameIncidentInfo => GetZPropertyInfo(nameof(SameClient));

		public ZBool SameProduct
		{
			get => sameProduct;
			set => SetNonPersistentPropertyValue(SameIncidentInfoProduct, ref sameProduct, value);
		}
		ZBool sameProduct;

		public ZPropertyInfo SameIncidentInfoProduct => GetZPropertyInfo(nameof(SameProduct));

		public ZBool SameProductArea
		{
			get => sameProductArea;
			set => SetNonPersistentPropertyValue(SameIncidentInfoProductArea, ref sameProductArea, value);
		}
		ZBool sameProductArea;

		public ZPropertyInfo SameIncidentInfoProductArea => GetZPropertyInfo(nameof(SameProductArea));

		#endregion

		public SimilarIncidentsDateFilter DateFilter
		{
			get
			{
				if (dateFilter == null)
				{
					dateFilter = new SimilarIncidentsDateFilter("Date range");
					RegisterEditableChildObject(dateFilter);
				}
				return dateFilter;
			}
		}
		SimilarIncidentsDateFilter dateFilter;

		public SimilarIncidentsFilterLookups Lookups => new SimilarIncidentsFilterLookups(this);

		#region PrepareSearchOptions

		public SimilarIncidentSearchOptions PrepareSearchOptions()
		{
			var options = new SimilarIncidentSearchOptions { SourceIncidentPK = ParentIncident.PK };

			SetSearchOptionsIncidentStatus(options);
			SetSearchOptionsDateRange(options);

			options.CompanyGuid = SameClient ? ParentIncident.IM_OH_Client : null;
			options.Product = SameProduct || SameProductArea ? ParentIncident.IM_Product : (ZString?)null;
			options.ProductArea = SameProductArea ? ParentIncident.ProductArea : (ZString?)null;

			return options;
		}

		void SetSearchOptionsIncidentStatus(SimilarIncidentSearchOptions options)
		{
			if (IncidentStatusFilter.EqualsIgnoringCase(SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.All))
			{
				options.IncidentStatus = IncidentStatus.Closed | IncidentStatus.Open;
			}
			else
			{
				if (IncidentStatusFilter.EqualsIgnoringCase(SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.Closed))
				{
					options.IncidentStatus = IncidentStatus.Closed;
				}
				else if (IncidentStatusFilter.EqualsIgnoringCase(SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.Open))
				{
					options.IncidentStatus = IncidentStatus.Open;
				}
			}
		}

		void SetSearchOptionsDateRange(SimilarIncidentSearchOptions options)
		{
			if (DateFilter.IsPropertySearchUsingPastDate)
			{
				options.FromTime = null;
				options.ToTime = ZDateTime.UtcNow.ToDateTime();
			}
			else if (DateFilter.IsPropertySearchUsingFutureDate ||
				DateFilter.IsPropertySearchUsingHasDateEntered || DateFilter.IsPropertySearchUsingHasNoDateEntered ||
				DateFilter.IsPropertySearchUsingSpecifiedDayOffsetRange || DateFilter.IsPropertySearchUsingSpecifiedHourOffsetRange || DateFilter.IsPropertySearchUsingSpecifiedWorkHourOffsetRange)
			{
				throw new InvalidOperationException("Not supported incident created date filter type.");
			}
			else
			{
				if (DateFilter.FromDate.IsValid)
				{
					options.FromTime = DateFilter.FromDate.IsValid ? DateFilter.FromDate.ToDateTime() : null;
				}
				if (DateFilter.ToDate.IsValid)
				{
					options.ToTime = DateFilter.ToDate.IsValid ? DateFilter.ToDate.ToDateTime() : null;
				}
			}
		}
		#endregion

		public (string DateRange, bool AllCustomers, string IncidentStatus) SearchOptionsToTuple()
		{
			return (DateRange: dateFilter.PropertySearch, AllCustomers: !sameClient, IncidentStatus: incidentStatusFilter);
		}
	}
}
