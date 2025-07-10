using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationLinkValidation : AutoBMControlCustomisationLinkValidation
	{
		public BMControlCustomisationLinkValidation(AutoBMControlCustomisationLink parent)
			: base(parent)
		{
		}

		protected new BMControlCustomisationLink Parent
		{
			get { return (BMControlCustomisationLink)base.Parent; }
		}

		protected override void CheckFML_JobType()
		{
			base.CheckFML_JobType();

			ListValidation.ErrorIfInvalidCode(Parent.FML_JobTypeInfo);
		}

		protected override void CheckFML_FM_ControlCustomisation()
		{
			base.CheckFML_FM_ControlCustomisation();

			CheckDuplicatesByJobType();
			CheckForLayoutsThatWillNotBeUsed();
		}

		void CheckDuplicatesByJobType()
		{
			var query = new ZQuery(BMControlCustomisationLinkSchema.FML_ParentId, Parent.FML_ParentId);
			query.AddToFilter(BMControlCustomisationLinkSchema.FML_ParentTableCode, Parent.FML_ParentTableCode);
			query.AddToFilter(BMControlCustomisationLinkSchema.FML_JobType, Parent.FML_JobType);

			var customisation = Parent.CustomisedLayout;
			var results = Parent.Factory.Load<BMControlCustomisationLink>(query)
				.Where(l => l.CustomisedLayout != null && customisation != null && l.CustomisedLayout.FM_ControlType == customisation.FM_ControlType)
				.Take(2).ToArray();

			if (results.Length > 1)
			{
				var controlType = results[0].CustomisedLayout.Lookups.ControlTypes.GetDescriptionFromCode(results[0].CustomisedLayout.FM_ControlType);

				Parent.FML_FM_ControlCustomisationInfo.AddError(Res.GetString("ff68eae1-c5dd-4d7c-9145-f0f68f9dab03", "There is already a {0} customization defined for this Job Type.", controlType));
			}
		}

		void CheckForLayoutsThatWillNotBeUsed()
		{
			if (Parent.FML_ParentTableCode == BMBoardSectionSchema.Constants.Prefix)
			{
				var section = Parent.Factory.Load<BMBoardSection>(Parent.FML_ParentId);
				var customisation = Parent.CustomisedLayout;

				if (section != null && customisation != null)
				{
					if (section.SectionConfiguration.ShowWorkflowOrJobWorkflowCards)
					{
						switch (customisation.FM_ControlType)
						{
							case CustomisedControlTypeList.Codes.TaskCard:
							case CustomisedControlTypeList.Codes.DetailedCard:
								Parent.FML_FM_ControlCustomisationInfo.AddError(Res.GetString("cf496f4d-4209-4d30-83d7-99b3ca7272a6", "This section shows tickets for workflows. This layout is for tasks only and will not be used."));
								break;
						}
					}
					else
					{
						switch (customisation.FM_ControlType)
						{
							case CustomisedControlTypeList.Codes.WorkflowSummaryCard:
							case CustomisedControlTypeList.Codes.WorkflowDetailedCard:
								Parent.FML_FM_ControlCustomisationInfo.AddError(Res.GetString("efe53ced-746a-4a4d-8827-97d95ec1b491", "This section shows tickets for tasks. This layout is for workflows only and will not be used."));
								break;
						}
					}
				}
			}
		}
	}
}
