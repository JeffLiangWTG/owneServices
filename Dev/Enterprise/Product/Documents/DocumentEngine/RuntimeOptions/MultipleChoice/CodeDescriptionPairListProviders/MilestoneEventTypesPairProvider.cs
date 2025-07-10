using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class MilestoneEventTypesPairProvider : ICodeDescriptionPairListProvider
	{
		#region Constructors

		public MilestoneEventTypesPairProvider()
			: this(ZString.Empty)
		{
		}

		public MilestoneEventTypesPairProvider(ZString jobType)
		{
			this.jobType = jobType;
		}

		#endregion

		#region Methods

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList milestoneEventTypesList = new CodeDescriptionPairList();

			milestoneEventTypesList.AddPair("");

			if (!this.jobType.IsEmpty)
			{
				AddMilestoneTemplateEvents(milestoneEventTypesList);
				if (milestoneEventTypesList.Count > 1)
				{
					milestoneEventTypesList.Add(new CategoryCodeDescriptionPair((NoResString)"Other Events", ""));
				}
			}

			AddRemainingEvents(milestoneEventTypesList);

			return milestoneEventTypesList;
		}

		#endregion

		#region Implementation

		void AddMilestoneTemplateEvents(CodeDescriptionPairList list)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ZQuery templateQuery = new ZQuery();
			templateQuery.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, jobType);
			ProcessTaskTemplate[] templates = factory.Load<ProcessTaskTemplate>(templateQuery);

			int templateToNotUseIndex = GetSystemTemplateIndexIfUserTemplateIsAbsent(templates);

			for (int i = 0; i < templates.Length; i++)
			{
				if (i != templateToNotUseIndex)
				{
					ZQuery milestoneQuery = new ZQuery();
					milestoneQuery.AddToFilter(ProcessTasksSchema.P9_ParentID, templates[i].PK);
					milestoneQuery.OrderBy = ProcessTasksSchema.P9_Sequence.Name;

					foreach (ProcessTask milestone in factory.Load<ProcessTask>(milestoneQuery))
					{
						list.AddPairIfNotExist(milestone.P9_SE_NKMilestoneEvent, milestone.P9_Description);
					}
				}
			}

			list.SortByDescription();

			list.AddPair("");
		}

		int GetSystemTemplateIndexIfUserTemplateIsAbsent(ProcessTaskTemplate[] templates)
		{
			bool systemTemplateOverridden = false;
			systemTemplateIndex = -1;
			for (int i = 0; i < templates.Length; i++)
			{
				if (templates[i].P0_IsSystem)
				{
					systemTemplateIndex = i;
				}
				else if (IsSystemTemplateOverridden(templates[i]))
				{
					systemTemplateOverridden = true;
				}
			}
			return systemTemplateOverridden ? systemTemplateIndex : -1;
		}

		bool IsSystemTemplateOverridden(ProcessTaskTemplate template)
		{
			return
				template.P0_SubType1.IsEmpty &&
				template.P0_SubType2.IsEmpty &&
				template.P0_SubType3.IsEmpty &&
				template.P0_SubType4.IsEmpty &&
				template.P0_SubType5.IsEmpty &&
				template.P0_LoadPortCountry.IsEmpty &&
				template.P0_DischargePortCountry.IsEmpty &&
				template.P0_GB.IsEmpty &&
				template.P0_GE.IsEmpty &&
				template.P0_OH_Client.IsEmpty;
		}

		int systemTemplateIndex;

		void AddRemainingEvents(CodeDescriptionPairList list)
		{
			var remainingEventList = new StmEventCodeDescriptionPairList();
			foreach (Event type in Events.All)
			{
				if (Events.ChangeLogs.Contains(type) || list.ContainsCode(type.Code))
				{
					remainingEventList.RemoveCode(type.Code);
				}
			}
			remainingEventList.SortByDescription();
			list.AddRange(remainingEventList);
		}

		readonly ZString jobType;

		#endregion
	}
}
