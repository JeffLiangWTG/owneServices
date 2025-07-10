using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	[CodeProperty(Schema.MAQ_Code)]
	[DescriptionProperty(Schema.MAQ_AttributeDescription)]
	public class MENTAgedScoreQuery : AutoMENTAgedScoreQuery, IDocManagerSupport, IMENTQueryable
	{
		public MENTAgedScoreQuery(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ReadOnly(true)]
		public override ZBool MAQ_IsSystem
		{
			get { return base.MAQ_IsSystem; }
			set { base.MAQ_IsSystem = value; }
		}

		[ReadOnlyMember(nameof(MAQ_SqlText_Readonly))]
		public override ZString MAQ_SqlText
		{
			get { return base.MAQ_SqlText; }
			set { base.MAQ_SqlText = value; }
		}

		protected bool MAQ_SqlText_Readonly
		{
			get { return Linked || base.MAQ_IsSystem; }
		}

		[ReadOnlyMember(nameof(MAQ_IsSystem))]
		public override ZString MAQ_AttributeDescription
		{
			get { return base.MAQ_AttributeDescription; }
			set { base.MAQ_AttributeDescription = value; }
		}

		[MaxLength(10)]
		[ReadOnlyMember(nameof(MAQ_Code_Readonly))]
		public override ZString MAQ_Code
		{
			get { return base.MAQ_Code; }
			set { base.MAQ_Code = value; }
		}

		protected bool MAQ_Code_Readonly
		{
			get { return IsInDatabase; }
		}

		[ReadOnlyMember(nameof(MAQ_IsFaulty_Readonly))]
		public override ZBool MAQ_IsFaulty
		{
			get { return base.MAQ_IsFaulty; }
			set { base.MAQ_IsFaulty = value; }
		}

		protected bool MAQ_IsFaulty_Readonly
		{
			get { return !MAQ_IsFaulty; }
		}

		[ReadOnlyMember(nameof(MAQ_IsSystem))]
		public override ZBool MAQ_AutoArchive
		{
			get { return base.MAQ_AutoArchive; }
			set { base.MAQ_AutoArchive = value; }
		}

		[ReadOnlyMember(nameof(MAQ_IsSystem))]
		public override ZBool MAQ_AutoPurge
		{
			get { return base.MAQ_AutoPurge; }
			set { base.MAQ_AutoPurge = value; }
		}

		[ChildEditable]
		public MENTAgedScoreExtractionCollection Extractions
		{
			get
			{
				if (extractions == null)
				{
					extractions = new MENTAgedScoreExtractionCollection(this);
					RegisterEditableChildObject(extractions);
				}

				return extractions;
			}
		}

		MENTAgedScoreExtractionCollection extractions;

		public MENTStmScheduleTask QuerySchedule
		{
			get
			{
				if (querySchedule == null)
				{
					var query = new ZQuery(StmScheduleTaskSchema.S5_ParentID, this.PK);
					query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, MENTAgedScoreQuerySchema.Constants.Prefix);
					querySchedule = Factory.LoadTop1<MENTStmScheduleTask>(query);

					if (querySchedule == null)
					{
						querySchedule = Factory.New<MENTStmScheduleTask>();
						using (querySchedule.SuspendSettingHasChanges())
						{
							querySchedule.S5_ParentTableCode = MENTAgedScoreQuerySchema.Constants.Prefix;
							querySchedule.S5_ParentID = this.PK;
						}
					}

					RegisterEditableChildObject(querySchedule);
				}

				return querySchedule;
			}
		}

		MENTStmScheduleTask querySchedule;

		public BMComponentAcceptabilityBand RelatedAcceptabilityBand
		{
			get
			{
				if (!MAQ_BAB_RelatedAcceptabilityBand.IsEmpty && relatedAcceptabilityBand == null)
				{
					relatedAcceptabilityBand = Factory.Load<BMComponentAcceptabilityBand>(MAQ_BAB_RelatedAcceptabilityBand);
				}

				return relatedAcceptabilityBand;
			}
		}

		BMComponentAcceptabilityBand relatedAcceptabilityBand;

		[ReadOnly(true)]
		[ResourceStringData("MENTAgedScoreQuery.Linked", Caption = "Linked")]
		public ZBool Linked
		{
			get { return RelatedAcceptabilityBand != null; }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void Delete()
		{
			Extractions.DeleteAll();
			QuerySchedule.Delete();
			base.Delete();
		}

		public ZString SQLLabelDescription
		{
			get { return ResString.GetMultilingualString("94ba49d3-9b29-4895-833a-711d13466704", "Enter a SQL statement that generates a result set containing five fields: \'{0}\', \'{1}\', \'{2}\', \'{3}\' and \'{4}\'.", "Score", "ReleaseGroup", "Component", "AttributeValue", "Staff"); }
		}

		public IMENTQueryable GetQueryable()
		{
			return RelatedAcceptabilityBand ?? (IMENTQueryable)this;
		}

		#region IMENTQueryable

		DataCollectionStrategy IMENTQueryable.CollectionStrategy
		{
			get { return new QueryCollectionStrategy(this); }
		}

		#region QueryCollectionStrategy

		class QueryCollectionStrategy : DataCollectionStrategy
		{
			public QueryCollectionStrategy(MENTAgedScoreQuery query)
			{
				this.query = query;
			}

			readonly MENTAgedScoreQuery query;

			protected override string QueryText
			{
				get { return query.MAQ_SqlText; }
			}
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.MENTAgedScoreQuery)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region BusinessObject Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return this.MAQ_Code; }
		}

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && !ShouldNotBeSavedByFactory; }
		}

		internal bool ShouldNotBeSavedByFactory { get; set; }

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (this.ShouldNotBeSavedByFactory)
			{
				QuerySchedule.Delete();
				querySchedule = null;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			MAQ_PurgeDays = 365;
		}

		#endregion
	}
}
