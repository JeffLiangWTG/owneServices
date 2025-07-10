using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	[CodeProperty(AccConsolidationGroup.Schema.YR_Code), DescriptionProperty(AccConsolidationGroup.Schema.YR_Description)]
	public class AccConsolidationGroup : AutoAccConsolidationGroup
	{
		public AccConsolidationGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("8656e632-5a70-4696-8c6b-b81dedd71703", "Consolidation Group"); }
		}

		[ChildEditable]
		public AccConsolidationMemberCollection GroupMembers
		{
			get
			{
				if (groupMembers == null)
				{
					var relationship = new DependentRelationship(
						 this,
						 typeof(AccConsolidationMember),
						 new ZQuery(),
						 AccConsolidationMemberSchema.YM_YR_ConsolidationGroup);
					groupMembers = new AccConsolidationMemberCollection(Factory, relationship);
					RegisterEditableChildObject(groupMembers);
				}
				return groupMembers;
			}
		}
		AccConsolidationMemberCollection groupMembers;

		public AccConsolidationGroup ParentGroup
		{
			get { return Factory.Load<AccConsolidationGroup>(YR_YR_ConsolidationGroup); }
		}

		[RelatedBusinessObject("ParentGroup")]
		[List("Lookups.ConsolidationGroups")]
		public override ZGuid YR_YR_ConsolidationGroup
		{
			get { return base.YR_YR_ConsolidationGroup; }
			set { base.YR_YR_ConsolidationGroup = value; }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void Delete()
		{
			base.Delete();
			GroupMembers.DeleteAll();
		}

		public override bool CanDelete
		{
			get { return string.IsNullOrEmpty(ReasonForNotAbleToDelete); }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			YR_YR_ConsolidationGroup = ZGuid.Empty;
		}
#endif

		public IEnumerable<ZGuid> GetChildCompanies()
		{
			var companies = new List<ZGuid>();

			foreach (var member in GroupMembers)
			{
				if (member.Company != null)
				{
					companies.Add(member.YM_GC_Company);
				}
			}

			return companies;
		}

		public IEnumerable<AccConsolidationGroup> GetAllDirectChildrenGroups()
		{
			return Factory.Load<AccConsolidationGroup>(new ZQuery(AccConsolidationGroupSchema.YR_YR_ConsolidationGroup, PK));
		}

		public IEnumerable<AccConsolidationGroup> GetAllGetChildGroupsIncludingDescendents()
		{
			var result = new List<AccConsolidationGroup>();
			var queue = new Queue<AccConsolidationGroup>();
			queue.Enqueue(this);

			while (queue.Count > 0)
			{
				var currentGroup = queue.Dequeue();
				result.Add(currentGroup);

				var childGroups = Factory.Load<AccConsolidationGroup>(new ZQuery(AccConsolidationGroupSchema.YR_YR_ConsolidationGroup, currentGroup.PK));

				foreach (var childGroup in childGroups)
				{
					queue.Enqueue(childGroup);
				}
			}

			result.Remove(this);

			return result;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				UsageCollector.Report(UsageFeatures.Codes.AccConsolidationGroup,
					(UsageProperties.ConsolidationGroupClientID, GlbCompany.CurrentCompany.LicenceKeyIdentifier),
					(UsageProperties.ConsolidationGroupCreatedTime, YR_SystemCreateTimeUtc.ToString("yyyy-MM-dd HH:mm")),
					(UsageProperties.ConsolidationGroupLastEditTime, YR_SystemLastEditTimeUtc.ToString("yyyy-MM-dd HH:mm")));
			}
		}
	}
}
