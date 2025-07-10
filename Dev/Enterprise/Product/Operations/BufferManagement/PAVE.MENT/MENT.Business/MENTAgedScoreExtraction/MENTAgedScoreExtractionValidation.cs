using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreExtractionValidation : AutoMENTAgedScoreExtractionValidation
	{
		public MENTAgedScoreExtractionValidation(AutoMENTAgedScoreExtraction parent)
			: base(parent)
		{
		}

		new MENTAgedScoreExtraction Parent
		{
			get { return (MENTAgedScoreExtraction)base.Parent; }
		}

		public override void ValidateAll()
		{
			ValidateCollectionColumn();
			ValidateAggregationType();

			base.ValidateAll();

			ValidateFilterStrips();
		}

		#region MEX_Name

		protected override void CheckMEX_Name()
		{
			base.CheckMEX_Name();

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.MEX_NameInfo, Parent.RelatedQuery.Extractions, false);
		}

		#endregion

		#region AggregationTypes

		public void ValidateAggregationType()
		{
			ValidateCalculatedProperty(Parent.AggregationTypeInfo);
		}

		protected void CheckAggregationType()
		{
			MandatoryValidation.CheckEntered(Parent.AggregationTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AggregationTypeInfo);

			if (IsAggregationTypeValidForNone())
			{
				if (Parent.IsInstantaneous)
				{
					Parent.AggregationTypeInfo.AddError(Res.GetString("461540ef-79c2-4a58-bdef-cff614491cb0", "Instantaneous Extractions must use {0} as their Aggregation Type. Instantaneous extractions provide a single result so an aggregation function is not valid.", ExtractionTypes.Codes.None));
				}
				else
				{
					Parent.AggregationTypeInfo.AddError(Res.GetString("3771fb1c-f311-4729-8d85-c25e8555d37b", "Non-Instantaneous Extractions must specify an Aggregation Type other than {0} that can be used when more than one result is returned for a grouping.", ExtractionTypes.Codes.None));
				}
			}

			if (Parent.CollectionColumn == MENTColumns.Codes.Score && !Parent.HistoricAggregationTypes.Any(x => x == Parent.AggregationType))
			{
				Parent.AggregationTypeInfo.AddError(Res.GetString("9e38ca14-3d85-41f0-9e29-4755a4d79c8d", "Non-Numeric Aggregation Types can not be used on defined numeric columns."));
			}

			if (Parent.CollectionColumn == MENTColumns.Codes.AttributeValue && Parent.AggregationType != ExtractionTypes.Codes.Count)
			{
				Parent.AggregationTypeInfo.AddError(Res.GetString("e58045ed-cc27-4cc9-b874-4263f00902a0", "Attribute Value is a text based column, Aggregation Types which are Numeric function can only be used on defined numeric columns."));
			}
		}

		bool IsAggregationTypeValidForNone()
		{
			return (!Parent.IsInstantaneous && Parent.AggregationType == ExtractionTypes.Codes.None) || (Parent.IsInstantaneous && Parent.AggregationType != ExtractionTypes.Codes.None);
		}

		#endregion

		#region CollectionColumn

		public void ValidateCollectionColumn()
		{
			ValidateCalculatedProperty(Parent.CollectionColumnInfo);
		}

		protected void CheckCollectionColumn()
		{
			MandatoryValidation.CheckEntered(Parent.CollectionColumnInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CollectionColumnInfo);

			if (!Parent.IsInstantaneous && Parent.CollectionColumn == MENTColumns.Codes.None)
			{
				Parent.CollectionColumnInfo.AddError(Res.GetString("40fca9eb-1645-47ac-ae83-ea2a81348121", "Only Instantaneous Extractions can use {0} Collection Columns.", MENTColumns.Codes.None));
			}
		}

		#endregion

		#region IsInstantaneous

		public void ValidateIsInstantaneous()
		{
			ValidateCalculatedProperty(Parent.IsInstantaneousInfo);
		}

		protected void CheckIsInstantaneous()
		{
			if (Parent.IsInstantaneous && (Parent.AggregationType != ExtractionTypes.Codes.None || Parent.CollectionColumn != MENTColumns.Codes.None))
			{
				Parent.IsInstantaneousInfo.AddError(Res.GetString("9a540f41-f2db-4cc2-a408-104ae51d3af9", "Instantaneous Extractions require {0} Aggregation and {1} Collection Columns. No Aggregation is required on the rows returned.", ExtractionTypes.Codes.None, MENTColumns.Codes.None));
			}
		}

		#endregion

		#region SeriesFilter

		void ValidateFilterStrips()
		{
			RelatedModuleFiltersHelper.ValidateFilterStrips(Parent.SeriesFilter);
		}

		#endregion
	}
}
