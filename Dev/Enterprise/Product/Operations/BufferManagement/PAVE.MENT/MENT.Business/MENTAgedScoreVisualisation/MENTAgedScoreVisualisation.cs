using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreVisualisation : AutoMENTAgedScoreVisualisation
	{
		public MENTAgedScoreVisualisation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.GraphTypes")]
		[ResourceStringData("MENTAgedScoreVisualisation.MVI_GraphType", Caption = "Graph Type")]
		public override ZString MVI_GraphType
		{
			get { return base.MVI_GraphType; }
			set { base.MVI_GraphType = value; }
		}

		[RelatedBusinessObject("Extraction")]
		public override ZGuid MVI_MEX
		{
			get { return base.MVI_MEX; }
			set { base.MVI_MEX = value; }
		}

		#endregion

		#region XMLColumns

		#region Titles

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.GraphTitle", Caption = "Graph Title")]
		public ZString GraphTitle
		{
			get { return GetXmlColumnPropertyValue<ZString>(GraphTitleInfo); }
			set { SetXmlColumnPropertyValue(GraphTitleInfo, value); }
		}

		public ZPropertyInfo GraphTitleInfo
		{
			get { return GetZPropertyInfo(nameof(GraphTitle)); }
		}

		#region YAxis

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.YAxisLabel", Caption = "Y Axis Label")]
		public ZString YAxisLabel
		{
			get { return GetXmlColumnPropertyValue<ZString>(YAxisLabelInfo); }
			set { SetXmlColumnPropertyValue(YAxisLabelInfo, value); }
		}

		public ZPropertyInfo YAxisLabelInfo
		{
			get { return GetZPropertyInfo(nameof(YAxisLabel)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.YAxisUnits", Caption = "Y Axis Units")]
		public ZString YAxisUnits
		{
			get { return GetXmlColumnPropertyValue<ZString>(YAxisUnitsInfo); }
			set { SetXmlColumnPropertyValue(YAxisUnitsInfo, value); }
		}

		public ZPropertyInfo YAxisUnitsInfo
		{
			get { return GetZPropertyInfo(nameof(YAxisUnits)); }
		}

		#endregion

		#region XAxis

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.XAxisLabel", Caption = "X Axis Label")]
		public ZString XAxisLabel
		{
			get { return GetXmlColumnPropertyValue<ZString>(XAxisLabelInfo); }
			set { SetXmlColumnPropertyValue(XAxisLabelInfo, value); }
		}

		public ZPropertyInfo XAxisLabelInfo
		{
			get { return GetZPropertyInfo(nameof(XAxisLabel)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.XAxisUnits", Caption = "X Axis Units")]
		public ZString XAxisUnits
		{
			get { return GetXmlColumnPropertyValue<ZString>(XAxisUnitsInfo); }
			set { SetXmlColumnPropertyValue(XAxisUnitsInfo, value); }
		}

		public ZPropertyInfo XAxisUnitsInfo
		{
			get { return GetZPropertyInfo(nameof(XAxisUnits)); }
		}

		#endregion

		#endregion

		#region Flags

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.UseOverriddenCategorySequence", Caption = "Use Overridden Category Sequence")]
		public ZBool UseOverriddenCategorySequence
		{
			get { return GetXmlColumnPropertyValue<ZBool>(UseOverriddenCategorySequenceInfo); }
			set
			{
				SetXmlColumnPropertyValue(UseOverriddenCategorySequenceInfo, value);
				RefreshBinding();
			}
		}

		public ZPropertyInfo UseOverriddenCategorySequenceInfo
		{
			get { return GetZPropertyInfo(nameof(UseOverriddenCategorySequence)); }
		}

		[ChildEditable]
		[XmlColumnProperty]
		public VisualisationColumnSpecificationCollection CategorySequenceCollection
		{
			get
			{
				if (categorySequenceCollection == null)
				{
					categorySequenceCollection = new VisualisationColumnSpecificationCollection(this);
					RegisterEditableChildObject(categorySequenceCollection);
				}

				return categorySequenceCollection;
			}
		}

		VisualisationColumnSpecificationCollection categorySequenceCollection;

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.IsNormalised", Caption = "Normalized", FullDescription = "Select to normalize the data. Normalizing flattens the data onto a 0-100% Y axis instead of using discreet Y values.")]
		public ZBool IsNormalised
		{
			get { return GetXmlColumnPropertyValue<ZBool>(IsNormalisedInfo); }
			set { SetXmlColumnPropertyValue(IsNormalisedInfo, value); }
		}

		public ZPropertyInfo IsNormalisedInfo
		{
			get { return GetZPropertyInfo(nameof(IsNormalised)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.SmoothCurve", Caption = "Smooth Curve", FullDescription = "Smooth the line graph curve.")]
		public ZBool SmoothCurve
		{
			get { return GetXmlColumnPropertyValue<ZBool>(SmoothCurveInfo); }
			set { SetXmlColumnPropertyValue(SmoothCurveInfo, value); }
		}

		public ZPropertyInfo SmoothCurveInfo
		{
			get { return GetZPropertyInfo(nameof(SmoothCurve)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.ShowAcceptabilityBands", Caption = "Show Acceptability Bands", FullDescription = "Show the acceptability band bounds.")]
		public ZBool ShowAcceptabilityBands
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowAcceptabilityBandsInfo); }
			set { SetXmlColumnPropertyValue(ShowAcceptabilityBandsInfo, value); }
		}

		public ZPropertyInfo ShowAcceptabilityBandsInfo
		{
			get { return GetZPropertyInfo(nameof(ShowAcceptabilityBands)); }
		}

		#region Aesthetics

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		[ResourceStringData("MENTAgedScoreVisualisation.ShowLegend", Caption = "Show Legend", FullDescription = "Select to show the series legend.")]
		public ZBool ShowLegend
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowLegendInfo); }
			set { SetXmlColumnPropertyValue(ShowLegendInfo, value); }
		}

		public ZPropertyInfo ShowLegendInfo
		{
			get { return GetZPropertyInfo(nameof(ShowLegend)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.ShowHorizontalGridLines", Caption = "Show Horizontal Grid Lines", FullDescription = "Select to show horizontal grid lines.")]
		public ZBool ShowHorizontalGridLines
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowHorizontalGridLinesInfo); }
			set { SetXmlColumnPropertyValue(ShowHorizontalGridLinesInfo, value); }
		}

		public ZPropertyInfo ShowHorizontalGridLinesInfo
		{
			get { return GetZPropertyInfo(nameof(ShowHorizontalGridLines)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.ShowVerticalGridLines", Caption = "Show Vertical Grid Lines", FullDescription = "Select to show vertical grid lines.")]
		public ZBool ShowVerticalGridLines
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowVerticalGridLinesInfo); }
			set { SetXmlColumnPropertyValue(ShowVerticalGridLinesInfo, value); }
		}

		public ZPropertyInfo ShowVerticalGridLinesInfo
		{
			get { return GetZPropertyInfo(nameof(ShowVerticalGridLines)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.AllowZoom", Caption = "Allow Zoom")]
		public ZBool AllowZoom
		{
			get { return GetXmlColumnPropertyValue<ZBool>(AllowZoomInfo); }
			set { SetXmlColumnPropertyValue(AllowZoomInfo, value); }
		}

		public ZPropertyInfo AllowZoomInfo
		{
			get { return GetZPropertyInfo(nameof(AllowZoom)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.AllowArrowAnnotations", Caption = "Allow Arrows")]
		public ZBool AllowArrowAnnotations
		{
			get { return GetXmlColumnPropertyValue<ZBool>(AllowArrowAnnotationsInfo); }
			set { SetXmlColumnPropertyValue(AllowArrowAnnotationsInfo, value); }
		}

		public ZPropertyInfo AllowArrowAnnotationsInfo
		{
			get { return GetZPropertyInfo(nameof(AllowArrowAnnotations)); }
		}

		#endregion

		#endregion

		#region CategoryAggregation

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.PerformLowerBoundAggregation", Caption = "Perform Lower Bound Aggregation", FullDescription = "Perform Lower Bound Aggregation on the Categories provided. This will combine all columns below a sequence into a single column.")]
		public ZBool PerformLowerBoundAggregation
		{
			get { return GetXmlColumnPropertyValue<ZBool>(PerformLowerBoundAggregationInfo); }
			set
			{
				SetXmlColumnPropertyValue(PerformLowerBoundAggregationInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateUpperBoundAggregationSequence();
					Validation.ValidateLowerBoundAggregationSequence();
				}
			}
		}

		public ZPropertyInfo PerformLowerBoundAggregationInfo
		{
			get { return GetZPropertyInfo(nameof(PerformLowerBoundAggregation)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.PerformUpperBoundAggregation", Caption = "Perform Upper Bound Aggregation", FullDescription = "Perform Upper Bound Aggregation on the Categories provided. This will combine all columns above a sequence into a single column.")]
		public ZBool PerformUpperBoundAggregation
		{
			get { return GetXmlColumnPropertyValue<ZBool>(PerformUpperBoundAggregationInfo); }
			set
			{
				SetXmlColumnPropertyValue(PerformUpperBoundAggregationInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateUpperBoundAggregationSequence();
					Validation.ValidateLowerBoundAggregationSequence();
				}
			}
		}

		public ZPropertyInfo PerformUpperBoundAggregationInfo
		{
			get { return GetZPropertyInfo(nameof(PerformUpperBoundAggregation)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.LowerBoundAggregationSequence", Caption = "Lower Bound Aggregation Sequence", FullDescription = "The sequence value to be used as the boundary for lower bound aggregation. All sequences below this value will be aggregated together into the column with this sequence.")]
		public ZInt LowerBoundAggregationSequence
		{
			get { return GetXmlColumnPropertyValue<ZInt>(LowerBoundAggregationSequenceInfo); }
			set
			{
				SetXmlColumnPropertyValue(LowerBoundAggregationSequenceInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateUpperBoundAggregationSequence();
					Validation.ValidateLowerBoundAggregationSequence();
				}
			}
		}

		public ZPropertyInfo LowerBoundAggregationSequenceInfo
		{
			get { return GetZPropertyInfo(nameof(LowerBoundAggregationSequence)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("MENTAgedScoreVisualisation.UpperBoundAggregationSequence", Caption = "Upper Bound Aggregation Sequence", FullDescription = "The sequence value to be used as the boundary for upper bound aggregation. All sequences above this value will be aggregated together into the column with this sequence.")]
		public ZInt UpperBoundAggregationSequence
		{
			get { return GetXmlColumnPropertyValue<ZInt>(UpperBoundAggregationSequenceInfo); }
			set
			{
				SetXmlColumnPropertyValue(UpperBoundAggregationSequenceInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateUpperBoundAggregationSequence();
					Validation.ValidateLowerBoundAggregationSequence();
				}
			}
		}

		public ZPropertyInfo UpperBoundAggregationSequenceInfo
		{
			get { return GetZPropertyInfo(nameof(UpperBoundAggregationSequence)); }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public MENTAgedScoreExtraction Extraction
		{
			get { return Factory.Load<MENTAgedScoreExtraction>(this.MVI_MEX); }
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (MENTAgedScoreVisualisation)base.CloneInternal(args);

			return clone;
		}

		#endregion

		#region BusinessObject Overrides

		protected override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(MENTAgedScoreVisualisationSchema.MVI_VisualisationData); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MVI_IsCustomised = ZBool.True;
			MVI_GraphType = GraphTypes.Codes.Column;
			ShowLegend = ZBool.True;
			ShowHorizontalGridLines = ZBool.False;
			ShowVerticalGridLines = ZBool.False;
			AllowZoom = ZBool.False;
		}

		#endregion

		public ZBool AllowPopulateCategorySequenceCollection
		{
			get { return !UseOverriddenCategorySequence; }
		}

		public ZPropertyInfo AllowPopulateCategorySequenceCollectionInfo
		{
			get { return GetZPropertyInfo(nameof(AllowPopulateCategorySequenceCollection)); }
		}

		public void PopulateCategorySequenceCollection(IVisualisationFactoryProvider provider)
		{
			var extraction = Extraction;

			if (extraction != null)
			{
				var extractor = new MENTAgedScoreExtractor(extraction, provider);

				var result = extractor.Extract();

				if (result.ExtractionResults.Any(r => r.Results.Count > 0))
				{
					CategorySequenceCollection.RemoveAndDeleteAll();

					foreach (var categoryIndexMapping in result.ResultCategoryIndexMap)
					{
						CategorySequenceCollection.Add(new VisualisationColumnSpecification(this)
						{
							Column = categoryIndexMapping.ColumnsToMap.First(),
							ColumnDisplay = categoryIndexMapping.ColumnsToMap.First(),
							Sequence = categoryIndexMapping.Index,
							Selected = true
						});
					}
				}
			}
		}
	}
}
