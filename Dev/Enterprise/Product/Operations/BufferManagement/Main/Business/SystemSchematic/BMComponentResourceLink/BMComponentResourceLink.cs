using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentResourceLink : AutoBMComponentResourceLink,
		IBMComponentResourceLink,
		IAuditParent
	{
		public BMComponentResourceLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new BMComponentResourceLinkFetchStrategy(this);
		}

		class BMComponentResourceLinkFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			internal BMComponentResourceLinkFetchStrategy(BMComponentResourceLink bmComponentResourceLink)
				: base(bmComponentResourceLink)
			{
				this.link = bmComponentResourceLink;
			}

			readonly BMComponentResourceLink link;

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();

				Factory.AddFetchHint(BMComponentResourceLinkSchema.FD_GS_NKResource, link.FD_GS_NKResource);
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();

				var mainQuery = new ZQuery(BMComponentResourceLinkSchema.PK, SQLComparisonOperator.NotEqual, link.PK);

				var subQuery = new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, link.FD_FC_Component);
				subQuery.AddToFilter(BMComponentResourceLinkSchema.FD_GS_NKResource, link.FD_GS_NKResource);

				Factory.AddFetchHint(BMComponentResourceLinkSchema.Instance, mainQuery, subQuery);
			}
		}

		#endregion

		#region Properties

		#region FD_FC_Component

		[List("Lookups.Components")]
		[RelatedBusinessObject("Component")]
		public override ZGuid FD_FC_Component
		{
			get { return base.FD_FC_Component; }
			set { base.FD_FC_Component = value; }
		}

		#endregion

		#region FD_CapacityConstraintDetectedUtc

		[ReadOnly(true)]
		public override ZDateTime FD_CapacityConstraintDetectedUtc
		{
			get { return base.FD_CapacityConstraintDetectedUtc; }
			set
			{
				base.FD_CapacityConstraintDetectedUtc = value;

				if (value.IsEmpty)
				{
					FD_IsPersistentlyOverloaded = ZBool.False;
				}
			}
		}

		[ResourceStringData("BMComponentResourceLink.CapacityConstraintDetectedLocal", Caption = "CCR Candidate Since", FullDescription = "The time this resource was first considered Capacity Constrained.")]
		public ZDateTime CapacityConstraintDetectedLocal
		{
			get { return FD_CapacityConstraintDetectedUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region FD_IsCapacityConstrained

		protected bool FD_IsCapacityConstrained_ReadOnly
		{
			get { return !FD_CapacityConstraintDetectedUtc.IsValid; }
		}

		#endregion

		#endregion

		#region New Properties

		#region SystemName

		[ResourceStringData("BMComponentResourceLink.SystemName", Caption = "System")]
		public ZString SystemName
		{
			get
			{
				var component = Component;
				var system = component != null ? component.System : null;
				return system != null ? system.FS_Name : ZString.Empty;
			}
		}

		#endregion

		#region TimeConsideredCCR

		[ResourceStringData("BMComponentResourceLink.TimeConsideredCCR", Caption = "Detected as CCR Candidate When", ShortCaption = "Detected as CCR", FullDescription = "The length of time this resource has been considered a Capacity Constrained Resource for the Release Gate into this Buffer.")]
		public ZString TimeConsideredCCR => FD_CapacityConstraintDetectedUtc.ToFriendlyTimeOverAgoString();

		#endregion

		#region MarkedAsCapacityConstrainedByFullName

		[ResourceStringData("BMComponentResourceLink.MarkedAsCapacityConstrainedByFullName", Caption = "Designated as CCR By (Full Name)", FullDescription = "The staff member who designated this staff as a CCR")]
		public ZString MarkedAsCapacityConstrainedByFullName
		{
			get
			{
				if (FD_GS_NKDesignatedAsCapacityConstrainedBy != string.Empty)
				{
					var query = new ZQuery(GlbStaffSchema.GS_Code, FD_GS_NKDesignatedAsCapacityConstrainedBy);
					return Factory.LoadTop1<GlbStaff>(query) != null ? Factory.LoadTop1<GlbStaff>(query).GS_FullName : ZString.Empty;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		#endregion

		#region CapacityReservationDetails

		[ReadOnly(true)]
		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		[ResourceStringData("BMComponentResourceLink.CapacityReservationDetails", Caption = "Capacity Reservation Details", FullDescription = "A listing of the workflows from the most recent run of the Release Gate which consumed capacity of this resource.")]
		public ZString CapacityReservationDetails
		{
			get
			{
				if (!capacityReservationDetails.HasValue)
				{
					var note = GetCapacityNote();
					capacityReservationDetails = note != null ? note.ST_NoteText : ZString.Empty;
				}

				return capacityReservationDetails.Value;
			}
			set
			{
				SetCapacityNoteText(value);
				capacityReservationDetails = value;
			}
		}

		ZString? capacityReservationDetails;

		internal void AddCapacityNoteFetchHint()
		{
			Factory.AddFetchHint(StmNoteSchema.Instance, GetCapacityNoteQuery());
		}

		ZQuery GetCapacityNoteQuery()
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
			query.AddToFilter(StmNoteSchema.ST_NoteType, NoteType);
			query.AddToFilter(StmNoteSchema.ST_Table, Schema.TableName);

			return query;
		}

		StmNote GetCapacityNote()
		{
			return Factory.LoadTop1<StmNote>(GetCapacityNoteQuery());
		}

		void SetCapacityNoteText(ZString value)
		{
			var note = GetCapacityNote();

			if (note == null && !value.IsEmpty)
			{
				note = Factory.New<StmNote>();
				note.ST_NoteType = NoteType;
				note.ST_ParentID = PK;
				note.ST_IsCustomDescription = true;
				note.ST_Table = Schema.TableName;
			}

			if (value.IsEmpty)
			{
				if (note != null)
				{
					note.Delete();
				}
			}
			else
			{
				note.IsLoggingEnabled = false;
				note.ST_NoteText = value;
			}
		}

		const string NoteType = "CAP";

		#endregion

		#endregion

		#region Related business objects

		public BMComponent Component
		{
			get { return Factory.Load<BMComponent>(FD_FC_Component); }
		}

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
