using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	[CodeProperty(Schema.MEX_Name)]
	[DescriptionProperty(Schema.MEX_Name)]
	public class MENTAgedScoreExtraction : AutoMENTAgedScoreExtraction, IRelatedModuleFilterSupportable
	{
		public MENTAgedScoreExtraction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("RelatedQuery")]
		public override ZGuid MEX_MAQ
		{
			get { return base.MEX_MAQ; }
			set { base.MEX_MAQ = value; }
		}

		[ResourceStringData("MENTAgedScoreExtraction.MEX_Name", Caption = "Extraction Name")]
		public override ZString MEX_Name
		{
			get { return base.MEX_Name; }
			set
			{
				base.MEX_Name = value;
			}
		}

		public FilterRuleProvider FilterProvider => filterProvider ?? (filterProvider = new BMFilterRuleProvider(this, moduleID: ModuleIDs.MENTSeries));
		FilterRuleProvider filterProvider;

		public StmModuleFilter SeriesFilter => FilterProvider.GetOrCreateAndCacheFilter();

		#endregion

		#region XML Columns

		[XmlColumnProperty]
		[ChildEditable]
		public AdditionalExtractionCollection AdditionalExtractions
		{
			get
			{
				if (additionalExtractions == null)
				{
					additionalExtractions = new AdditionalExtractionCollection(this);
					RegisterEditableChildObject(additionalExtractions);
				}

				return additionalExtractions;
			}
		}

		AdditionalExtractionCollection additionalExtractions;

		[XmlColumnProperty]
		[ChildEditable]
		public ExtractionColumnCollection SeriesColumns
		{
			get
			{
				if (seriesColumns == null)
				{
					seriesColumns = new ExtractionColumnCollection(Factory);
					RegisterEditableChildObject(seriesColumns);
				}

				return seriesColumns;
			}
		}

		ExtractionColumnCollection seriesColumns;

		[XmlColumnProperty]
		[List("Lookups.ExtractionTypes")]
		[ResourceStringData("MENTAgedScoreExtraction.AggregationType", Caption = "Aggregation Type")]
		public ZString AggregationType
		{
			get { return GetXmlColumnPropertyValue<ZString>(AggregationTypeInfo); }
			set
			{
				SetXmlColumnPropertyValue(AggregationTypeInfo, value);

				ValidateAggregationCollectionAndInstantaity();
			}
		}

		public ZPropertyInfo AggregationTypeInfo
		{
			get { return GetZPropertyInfo(nameof(AggregationType)); }
		}

		[XmlColumnProperty]
		[List("Lookups.CollectionColumns")]
		[ResourceStringData("MENTAgedScoreExtraction.CollectionColumn", Caption = "Collection Column")]
		public ZString CollectionColumn
		{
			get { return GetXmlColumnPropertyValue<ZString>(CollectionColumnInfo); }
			set
			{
				SetXmlColumnPropertyValue(CollectionColumnInfo, value);

				ValidateAggregationCollectionAndInstantaity();
			}
		}

		public ZPropertyInfo CollectionColumnInfo
		{
			get { return GetZPropertyInfo(nameof(CollectionColumn)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreExtraction.IsInstantaneous", Caption = "Instantaneous")]
		public ZBool IsInstantaneous
		{
			get { return GetXmlColumnPropertyValue<ZBool>(IsInstantaneousInfo); }
			set
			{
				SetXmlColumnPropertyValue(IsInstantaneousInfo, value);

				ValidateAggregationCollectionAndInstantaity();
			}
		}

		public ZPropertyInfo IsInstantaneousInfo
		{
			get { return GetZPropertyInfo(nameof(IsInstantaneous)); }
		}

		[XmlColumnProperty]
		[ChildEditable]
		public ExtractionColumnCollection CategoryColumns
		{
			get
			{
				if (categoryColumns == null)
				{
					categoryColumns = new ExtractionColumnCollection(Factory);
					RegisterEditableChildObject(categoryColumns);
				}

				return categoryColumns;
			}
		}

		ExtractionColumnCollection categoryColumns;

		[ChildEditable]
		public MENTAgedScoreVisualisationCollection Visualisations
		{
			get
			{
				if (visualisations == null)
				{
					visualisations = new MENTAgedScoreVisualisationCollection(this);
					RegisterEditableChildObject(visualisations);
				}

				return visualisations;
			}
		}

		MENTAgedScoreVisualisationCollection visualisations;

		public MENTAgedScoreVisualisation DefaultVisualisation
		{
			get
			{
				if (defaultVisualisation == null)
				{
					var query = new ZQuery();
					query.AddToFilter(MENTAgedScoreVisualisationSchema.MVI_MEX, PK);
					query.AddToFilter(MENTAgedScoreVisualisationSchema.MVI_IsCustomised, false);
					defaultVisualisation = Factory.LoadTop1<MENTAgedScoreVisualisation>(query);

					if (defaultVisualisation == null)
					{
						defaultVisualisation = CreateDefaultVisualisation();
					}

					RegisterEditableChildObject(defaultVisualisation);
				}

				return defaultVisualisation;
			}
		}

		MENTAgedScoreVisualisation defaultVisualisation;

		#endregion

		#region New Properties

		public IEnumerable<ZString> HistoricAggregationTypes
		{
			get
			{
				yield return ExtractionTypes.Codes.Average;
				yield return ExtractionTypes.Codes.Count;
				yield return ExtractionTypes.Codes.Maximum;
				yield return ExtractionTypes.Codes.Minimum;
				yield return ExtractionTypes.Codes.Sum;
			}
		}

		public MENTAgedScoreQuery RelatedQuery
		{
			get { return Factory.Load<MENTAgedScoreQuery>(MEX_MAQ); }
		}

		[ResourceStringData("MENTAgedScoreExtraction.RelatedQueryCode", Caption = "Query Code")]
		public ZString RelatedQueryCode
		{
			get { return RelatedQuery != null ? RelatedQuery.MAQ_Code : ZString.Empty; }
		}

		#endregion

		public MENTAgedScoreVisualisation CreateNewRelatedVisualisation()
		{
			var newVisualisation = (MENTAgedScoreVisualisation)this.DefaultVisualisation.Clone();
			newVisualisation.MVI_IsCustomised = true;

			this.RegisterEditableChildObject(newVisualisation);

			return newVisualisation;
		}

		public IEnumerable<MENTAgedScoreExtraction> AllExtractionsForExtractor
		{
			get { return AdditionalExtractions.Cast<AdditionalExtractionLink>().Select(ae => ae.RelatedExtraction).WhereNotNull().Append(this); }
		}

		void ValidateAggregationCollectionAndInstantaity()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateCollectionColumn();
				Validation.ValidateAggregationType();
				Validation.ValidateIsInstantaneous();
			}
		}

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (MENTAgedScoreExtraction)base.CloneInternal(args);

			BMExtensionMethods.SetCopiedBizoNamePropertyComplyingWithMaxLength(clone.MEX_NameInfo);
			FilterProvider.CopyFilterStrips(() => clone.SeriesFilter);

			clone.DefaultVisualisation.CopyPersistentValuesFrom(DefaultVisualisation);
			DefaultVisualisation.CopyXmlColumns(clone.DefaultVisualisation);
			clone.DefaultVisualisation.MVI_MEX = clone.PK;

			return clone;
		}

		#endregion

		#region Business Object Overrides

		protected override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(MENTAgedScoreExtractionSchema.MEX_ExtractionData); }
		}

		public override void Delete()
		{
			Visualisations.DeleteAll();
			FilterProvider.DeleteFilter();

			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DefaultVisualisation.MVI_IsCustomised = false; // Cause evaluation of DefaultVisualisation
		}

		MENTAgedScoreVisualisation CreateDefaultVisualisation()
		{
			var visualisation = Factory.New<MENTAgedScoreVisualisation>();
			visualisation.MVI_MEX = PK;
			visualisation.MVI_IsCustomised = false;
			return visualisation;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("be9bfc6a-404a-4d64-a837-414a306b2c7e", "MENT Aged Score Query: {0}, Extraction: {1}", RelatedQuery?.MAQ_QueryDescription, MEX_Name);

		#endregion
	}
}
