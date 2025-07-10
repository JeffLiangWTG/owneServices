using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class ExportCustomsManifestFilterBusinessObject : FilterStripBusinessObject
	{
		public ExportCustomsManifestFilterBusinessObject(bool allowAir, bool allowSea)
		{
			this.allowAir = allowAir;
			this.allowSea = allowSea;
		}

		public ExportCustomsManifestFilterBusinessObject()
		{
		}

		#region Constants

		internal abstract class FilterConstants
		{
			public const string Vessel = "Vessel";
			public const string Voyage = "Voyage Number";
			public const string Flight = "Flight Number";
			public const string CountryOfDestination = "Ctry/Rgn. of Destination";
			public const string DepartureDate = "Departure Date";
			public const string DocumentStatusConditions = "Conditions";
			public const string DocumentStatus = "Document Status";
			public const string TransportMode = "Transport Mode";
			public const string ManifestType = "Manifest Type";
			public const string PortOfDeparture = "Departure Port";
			public const string PortOfDestination = "Destination Port";
			public const string AirWayBill = "Air Way Bill";
			public const string JobNo = "Job Number";
			public const string FolioReference = "Folio Reference";
			public const string CAN = "CAN";
		}

		#endregion

		#region Lists

		RefUNLOCOCollection PortList
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		RefVesselCollection VesselList
		{
			get { return new RefVesselCollection(Factory); }
		}

		CodeDescriptionPairList ManifestTypeList
		{
			get
			{
				if (fManifestTypeList == null)
				{
					fManifestTypeList = new ManifestTypeList();
				}
				return fManifestTypeList;
			}
		}
		CodeDescriptionPairList fManifestTypeList;

		CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (fTransportModeList == null)
				{
					fTransportModeList = new ManifestTransportModeList();
				}
				return fTransportModeList;
			}
		}
		CodeDescriptionPairList fTransportModeList;

		CodeDescriptionPairList DocumentStatusList
		{
			get
			{
				if (fDocumentStatusList == null)
				{
					fDocumentStatusList = new CMR3CharDocumentStatusList();
				}
				return fDocumentStatusList;
			}
		}
		CodeDescriptionPairList fDocumentStatusList;

		CodeDescriptionPairList DocumentStatusConditionsList
		{
			get
			{
				if (fDocumentStatusConditionsList == null)
				{
					fDocumentStatusConditionsList = new CMR3CharDocumentStatusConditionsList();
				}
				return fDocumentStatusConditionsList;
			}
		}
		CodeDescriptionPairList fDocumentStatusConditionsList;

		#endregion

		#region ModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			if (allowSea)
			{
				filters.AddTextFilter(FilterConstants.Vessel, ExportCustomsManifestHeaderSchema.ED_VesselName, VesselList);
			}

			if (allowSea)
			{
				filters.AddTextFilter(FilterConstants.Voyage, ExportCustomsManifestHeaderSchema.ED_VoyageNumber);
			}

			if (allowAir)
			{
				filters.AddTextFilter(FilterConstants.Flight, ExportCustomsManifestHeaderSchema.ED_FlightNumber);
			}

			filters.AddNkFilter(FilterConstants.CountryOfDestination, ExportCustomsManifestHeaderSchema.ED_RN_NKCountryOfDestination, ModuleIDs.RefCountry, CountryList);
			filters.AddDateFilter(FilterConstants.DepartureDate, ExportCustomsManifestHeaderSchema.ED_DepartureDate);
			filters.AddTextFilter(FilterConstants.DocumentStatusConditions, ExportCustomsManifestHeaderSchema.ED_DocumentStatusConditions, DocumentStatusConditionsList);
			filters.AddTextFilter(FilterConstants.DocumentStatus, ExportCustomsManifestHeaderSchema.ED_DocumentStatus, DocumentStatusList);
			if (allowSea && allowAir)
			{
				filters.AddTextFilter(FilterConstants.TransportMode, ExportCustomsManifestHeaderSchema.ED_TransportMode, TransportModeList);
			}

			filters.AddTextFilter(FilterConstants.ManifestType, ExportCustomsManifestHeaderSchema.ED_ManifestType, ManifestTypeList);
			filters.AddNkFilter(FilterConstants.PortOfDeparture, ExportCustomsManifestHeaderSchema.ED_RL_NKPortOfDeparture, ModuleIDs.RefUNLOCO, PortList);
			filters.AddNkFilter(FilterConstants.PortOfDestination, ExportCustomsManifestHeaderSchema.ED_RL_NKPortOfDestination, ModuleIDs.RefUNLOCO, PortList);
			if (allowAir)
			{
				filters.AddTextFilter(FilterConstants.AirWayBill, GetAWBQuery).MaxLength = ExportCustomsManifestLinesSchema.EL_AirWayBill.MaxLength;
			}

			filters.AddFountainFilter(FilterConstants.JobNo, ExportCustomsManifestHeaderSchema.ED_BGMReference, "K");
			filters.AddTextFilter(FilterConstants.FolioReference, ExportCustomsManifestHeaderSchema.ED_FolioReference);
			filters.AddTextFilter(FilterConstants.CAN, CANFilter).MaxLength = ExportCustomsManifestLinesSchema.EL_CAN.MaxLength;

			return filters;
		}

		ZQuery GetAWBQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZString masterBillNumber = value.Replace("-", "").Replace(" ", "");
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ExportCustomsManifestLines), ExportCustomsManifestLinesSchema.EL_ED);
			subQuery.AddToFilter(ExportCustomsManifestLinesSchema.EL_AirWayBill, @operator, masterBillNumber);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ExportCustomsManifestHeader));
			if (masterBillNumber.Length <= ExportCustomsManifestHeaderSchema.ED_AirWayBill.MaxLength)
			{
				query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_AirWayBill, @operator, masterBillNumber);
			}
			query.AddSubQuery(subQuery, JoinCondition.Or);
			return query;
		}

		protected ZQuery CANFilter(SQLComparisonOperator op, ZString searchString)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ExportCustomsManifestLines), ExportCustomsManifestLinesSchema.EL_ED);
			subQuery.AddToFilter(ExportCustomsManifestLinesSchema.EL_CAN, op, searchString);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ExportCustomsManifestHeader));
			if (searchString.Length <= ExportCustomsManifestHeaderSchema.ED_CAN.MaxLength)
			{
				query.AddToFilter(ExportCustomsManifestHeaderSchema.ED_CAN, op, searchString);
			}
			query.AddSubQuery(subQuery, JoinCondition.Or);
			return query;
		}

		readonly bool allowAir;
		readonly bool allowSea;

		#endregion

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;

				if (!(allowAir && allowSea))
				{
					if (allowSea)
					{
						result.AddToFilter(ExportCustomsManifestHeaderSchema.ED_TransportMode, Core.Constants.TransportModes.Sea);
					}

					if (allowAir)
					{
						result.AddToFilter(ExportCustomsManifestHeaderSchema.ED_TransportMode, Core.Constants.TransportModes.Air);
					}
				}

				return result;
			}
		}

		#endregion
	}
}
