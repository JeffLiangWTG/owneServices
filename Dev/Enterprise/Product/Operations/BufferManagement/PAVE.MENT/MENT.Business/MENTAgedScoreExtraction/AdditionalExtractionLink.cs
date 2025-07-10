using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.PAVE.MENT.Business
{
	public class AdditionalExtractionLink : NonPersistentBusinessObject<AdditionalExtractionLinkValidation>
	{
		public AdditionalExtractionLink(BusinessObjectFactory factory, MENTAgedScoreExtraction extraction)
			: base(factory)
		{
			this.BaseExtraction = extraction;
		}

		public readonly MENTAgedScoreExtraction BaseExtraction;

		#region New Properties

		[List("Queries")]
		[ResourceStringData("AdditionalExtractionLink.QueryPK", Caption = "Query")]
		public ZGuid QueryPK
		{
			get
			{
				EnsureQueryPKIsSet();

				return queryPK;
			}
			set
			{
				SetNonPersistentPropertyValue(QueryPKInfo, ref queryPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateQueryPK();
				}
			}
		}
		ZGuid queryPK;

		public ZPropertyInfo QueryPKInfo
		{
			get { return GetZPropertyInfo(nameof(QueryPK)); }
		}

		void EnsureQueryPKIsSet()
		{
			if (queryPK.IsEmpty)
			{
				var extraction = RelatedExtraction;
				if (extraction != null)
				{
					queryPK = extraction.RelatedQuery.PK;
				}
			}
		}

		#endregion

		#region XML Properties

		[XmlColumnProperty]
		[List("Extractions")]
		[ResourceStringData("AdditionalExtractionLink.ExtractionPK", Caption = "Extraction")]
		public ZGuid ExtractionPK
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(ExtractionPKInfo); }
			set
			{
				SetXmlColumnPropertyValue(ExtractionPKInfo, value);

				EnsureQueryPKIsSet();

				if (!IsValidationSuspended)
				{
					Validation.ValidateExtractionPK();
				}
			}
		}

		public ZPropertyInfo ExtractionPKInfo
		{
			get { return GetZPropertyInfo(nameof(ExtractionPK)); }
		}

		#endregion

		#region Related Business Objects

		public MENTAgedScoreQuery RelatedQuery
		{
			get { return Factory.Load<MENTAgedScoreQuery>(QueryPK); }
		}

		public MENTAgedScoreExtraction RelatedExtraction
		{
			get { return Factory.Load<MENTAgedScoreExtraction>(ExtractionPK); }
		}

		public MENTAgedScoreQueryCollection Queries
		{
			get { return new MENTAgedScoreQueryCollection(Factory); }
		}

		public MENTAgedScoreExtractionCollection Extractions
		{
			get { return RelatedQuery == null ? new MENTAgedScoreExtractionCollection(Factory, ZQuery.NoResultQuery) : RelatedQuery.Extractions; }
		}

		public override AdditionalExtractionLinkValidation GetNewValidation()
		{
			return new AdditionalExtractionLinkValidation(this);
		}

		#endregion
	}
}
