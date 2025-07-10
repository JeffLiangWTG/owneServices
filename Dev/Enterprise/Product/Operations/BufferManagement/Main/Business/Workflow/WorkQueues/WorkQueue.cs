using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.DebuggerDisplay("{DisplayText}")]
	public class WorkQueue : TagMagnitude
	{
		public WorkQueue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			TGM_TGD_Tag = TagProvider.GetWorkQueuesTagGroup(Factory).PK;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (HasChanges)
			{
				InvalidateRunningTotalsCache();
			}
		}

		public override void Delete()
		{
			var exisitingTagRule = tagRule ?? GetExistingTagRule();
			if (exisitingTagRule != null)
			{
				exisitingTagRule.Delete();
			}

			base.Delete();
		}

		#endregion

		#region TagMagnitude Overrides

		protected override bool ShouldPreventDeletingTagsOnSystemGroups
		{
			get { return false; }
		}

		protected override bool IsReadOnlyForSystemDefinedTagGroup
		{
			get { return false; }
		}

		#endregion

		#region Properties

		public override ZString TGM_Description
		{
			get { return base.TGM_Description; }
			set
			{
				base.TGM_Description = value;

				var exisitingTagRule = tagRule ?? GetExistingTagRule();
				if (exisitingTagRule != null)
				{
					exisitingTagRule.TGR_Name = value.SubstringSafe(0, TagRuleSchema.TGR_Name.MaxLength);
				}
			}
		}

		[List("Lookups.AllStaff")]
		[RelatedBusinessObject("CreatingUser")]
		public override ZString TGM_SystemCreateUser
		{
			get { return base.TGM_SystemCreateUser; }
			set { base.TGM_SystemCreateUser = value; }
		}

		#endregion

		#region Related Business Objects

		public GlbStaff CreatingUser
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, TGM_SystemCreateUser); }
		}

		[ChildEditable]
		public WorkQueueMembershipLinkCollection Members
		{
			get
			{
				if (members == null)
				{
					members = new WorkQueueMembershipLinkCollection(this);
					members.ApplySort(TagLinkSchema.Constants.TGL_Sequence, ListSortDirection.Descending); // Property name
					RegisterEditableChildObject(members);
					AddFetchHintsForMembers(members);
				}

				return members;
			}
		}

		WorkQueueMembershipLinkCollection members;

		void AddFetchHintsForMembers(IEnumerable<WorkQueueMembershipLink> members)
		{
			var memberFKs = members
				.Where(m => m.TGL_ParentTableCode == ProcessHeaderSchema.Constants.Prefix)
				.Select(m => m.TGL_ParentId);
			var memberProcessHeaders = Factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, memberFKs));

			// ProcessJobHeader.IsReleased will load process headers for job headers related to members
			Factory.AddFetchHint(ProcessHeaderSchema.Instance, new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, memberProcessHeaders.Select(h => h.PK)));
		}

		public WorkQueueTagRule TagRule
		{
			get
			{
				if (tagRule == null)
				{
					tagRule = GetExistingTagRule();

					if (tagRule == null)
					{
						tagRule = Factory.New<WorkQueueTagRule>();
						using (tagRule.SuspendSettingHasChanges())
						{
							tagRule.TGR_Name = TGM_Description.SubstringSafe(0, TagRuleSchema.TGR_Name.MaxLength);

							var templateLink = tagRule.TagTemplate;
							using (templateLink.SuspendSettingHasChanges())
							{
								templateLink.TGL_TGM_Magnitude = PK;
							}
						}
					}

					RegisterEditableChildObject(tagRule);
				}

				return tagRule;
			}
		}

		WorkQueueTagRule tagRule;

		WorkQueueTagRule GetExistingTagRule()
		{
			if (IsInDatabase)
			{
				var query = new ZDBOnlyQuery(typeof(WorkQueueTagRule));
				query.AddToFilter(TagRuleSchema.TGR_IsSystem, true);
				var linkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
				linkSubQuery.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, PK);
				linkSubQuery.AddToFilter(TagLinkSchema.TGL_ParentTableCode, TagRuleSchema.Constants.Prefix);
				query.AddSubQuery(TagRuleSchema.PK, linkSubQuery, JoinCondition.And);

				return Factory.LoadTop1<WorkQueueTagRule>(query);
			}

			return null;
		}

		#endregion

		#region Membership

		public QueueMemberOperationResult AddMember(ProcessHeader processHeader)
		{
			Argument.NotNull(processHeader, "processHeader");

			string message;
			var newSequence = GetNextSequenceNumber();

			if (!WorkQueueMembershipValidator.IsSequenceWithinShortIntRange(newSequence))
			{
				newSequence = ResequenceQueue() + 1;
			}

			if (WorkQueueMembershipValidator.CanAdd(this, processHeader, out message, sequenceNumber: newSequence))
			{
				var membershipLink = Factory.New<WorkQueueMembershipLink>();

				using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
				{
					membershipLink.TGL_ParentId = processHeader.PK;
					membershipLink.TGL_ParentTableCode = processHeader.TablePrefix;
					membershipLink.TGL_Sequence = (ZShort)newSequence;

					InvalidateRunningTotalsCache();
					Members.Add(membershipLink);
				}

				membershipLink.RunPreSaveValidation();

				return QueueMemberOperationResult.Success(membershipLink);
			}
			else
			{
				return QueueMemberOperationResult.Failed(message);
			}
		}

		public bool ContainsMember(ProcessHeader processHeader)
		{
			return Members.Any(l => l.TGL_ParentId == processHeader.PK);
		}

		public WorkQueueMembershipLink GetMembershipLink(ProcessHeader processHeader)
		{
			return Members.FirstOrDefault(l => l.TGL_ParentId == processHeader.PK);
		}

		public IEnumerable<ProcessHeader> MembersInSequence
		{
			get { return Members.OrderBy(m => m.TGL_Sequence).Select(m => m.Parent); }
		}

		internal ZInt GetNextSequenceNumber()
		{
			return Members.Count == 0 ? 1 : (ZInt)Members.Max(l => l.TGL_Sequence) + 1;
		}

		#endregion

		#region Running Totals

		void PopulateRunningTotals()
		{
			var orderedMembers = Members.OrderBy(m => m.TGL_Sequence);
			var runningTotal = 0;

			foreach (var member in orderedMembers)
			{
				var parent = member.Parent;
				if (parent != null)
				{
					runningTotal += parent.FH_PlannedDurationInMinutes;
				}

				runningTotals.Add(member.PK, runningTotal);
			}
		}

		internal ZInt GetRunningTotal(WorkQueueMembershipLink member)
		{
			var key = member.PK;
			return RunningTotals.ContainsKey(key) ? RunningTotals[key] : ZInt.Zero;
		}

		void InvalidateRunningTotalsCache()
		{
			runningTotals = null;
		}

		Dictionary<ZGuid, ZInt> RunningTotals
		{
			get
			{
				if (runningTotals == null)
				{
					runningTotals = new Dictionary<ZGuid, ZInt>();
					PopulateRunningTotals();
				}

				return runningTotals;
			}
		}

		Dictionary<ZGuid, ZInt> runningTotals;

		#endregion

		#region Resequence Queue

		ZShort ResequenceQueue()
		{
			var orderedMembers = Members.OrderBy(m => m.TGL_Sequence).ToList();
			var originalLastMemberSequence = orderedMembers.Last().TGL_Sequence;
			var newLastMemberSequence = AttemptToResequenceGracefully(orderedMembers);

			if (newLastMemberSequence == originalLastMemberSequence)
			{
				return ResequenceImpl(orderedMembers, 1, resequenceFrom1: true);
			}

			return newLastMemberSequence;
		}

		static ZShort AttemptToResequenceGracefully(List<WorkQueueMembershipLink> orderedMembers)
		{
			if (orderedMembers.Count > 2)
			{
				var sequenceDelta = orderedMembers[1].TGL_Sequence - orderedMembers.First().TGL_Sequence;
				var extrapolatedEndSequence = (int)sequenceDelta * (orderedMembers.Count - 1) + orderedMembers.First().TGL_Sequence;

				if (WorkQueueMembershipValidator.IsSequenceWithinShortIntRange(extrapolatedEndSequence))
				{
					return ResequenceImpl(orderedMembers, sequenceDelta);
				}
			}

			return orderedMembers.Last().TGL_Sequence;
		}

		static ZShort ResequenceImpl(List<WorkQueueMembershipLink> orderedMembers, ZShort delta, bool resequenceFrom1 = false)
		{
			if (orderedMembers.Count > 1 && !resequenceFrom1)
			{
				for (int i = 1; i < orderedMembers.Count; i++)
				{
					orderedMembers[i].TGL_Sequence = orderedMembers[i - 1].TGL_Sequence + delta;
				}
			}
			else
			{
				var runningSequence = (ZShort)1;
				foreach (var member in orderedMembers)
				{
					member.TGL_Sequence = runningSequence++;
				}
			}

			return orderedMembers.Last().TGL_Sequence;
		}

		#endregion
	}
}
