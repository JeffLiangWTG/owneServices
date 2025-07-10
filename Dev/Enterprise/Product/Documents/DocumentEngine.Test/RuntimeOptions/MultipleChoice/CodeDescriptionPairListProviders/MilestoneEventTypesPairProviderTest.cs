using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	class MilestoneEventTypesPairProviderTest : CodeDescriptionPairListProviderTest
	{
		#region Test Cases

		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), GeExpectedtCodeDescriptionPairList());
		}

		public void TestIsReturningCorrectDescriptionAfterCutomizingEventNames()
		{
			var factory = new BusinessObjectFactory();
			var z00Event = factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, Events.CustomisableEvent00.Code);
			var customDescription = "TestCustomDesc";
			z00Event.SE_Desc = customDescription;
			factory.Save();

			var provider = CreateCodeDescriptionPairListProvider();
			AssertEquals(customDescription, provider.GetCodeDescriptionPairList().GetDescriptionFromCode(Events.CustomisableEvent00.Code));
		}

		#endregion

		#region Overrides

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new MilestoneEventTypesPairProvider();
		}

		#endregion

		#region Implementation

		protected virtual ZString GetJobType()
		{
			return ZString.Empty;
		}

		ReadOnlyCodeDescriptionPairList GeExpectedtCodeDescriptionPairList()
		{
			CodeDescriptionPairList milestoneEventTypesList = new CodeDescriptionPairList();

			milestoneEventTypesList.AddPair("");

			if (!GetJobType().IsEmpty)
			{
				AddMilestoneTemplateEvents(milestoneEventTypesList);
				if (milestoneEventTypesList.Count > 1)
				{
					milestoneEventTypesList.Add(new CategoryCodeDescriptionPair("Other Events", ""));
				}
			}

			AddRemainingEvents(milestoneEventTypesList);

			return milestoneEventTypesList;
		}

		void AddMilestoneTemplateEvents(CodeDescriptionPairList list)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ZQuery templateQuery = new ZQuery();
			templateQuery.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, GetJobType());
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
			CodeDescriptionPairList remainingEventList = new CodeDescriptionPairList();
			foreach (Event type in Events.All)
			{
				if (!Events.ChangeLogs.Contains(type))
				{
					if (!list.ContainsCode(type.Code))
					{
						remainingEventList.AddPair(type.Code, type.Description);
					}
				}
			}
			remainingEventList.SortByDescription();
			list.AddRange(remainingEventList);
		}

		#endregion
	}
}
