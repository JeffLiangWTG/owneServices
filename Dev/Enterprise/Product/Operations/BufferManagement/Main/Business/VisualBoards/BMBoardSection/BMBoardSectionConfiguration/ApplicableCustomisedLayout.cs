using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.BufferManagement.Business
{
	public class ApplicableCustomisedLayout : NonPersistentBusinessObject
	{
		public ApplicableCustomisedLayout(BMControlCustomisationLink link)
		{
			var layout = link.CustomisedLayout;

			LayoutPK = link.FML_FM_ControlCustomisation;
			ControlType = layout.Lookups.ControlTypes.GetDescriptionFromCode(layout.FM_ControlType);

			if (!link.FML_JobType.IsEmpty)
			{
				JobType = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", link.FML_JobType, link.Lookups.JobTypes.GetDescriptionFromCode(link.FML_JobType));
			}

			LayoutName = layout.FM_Name;
			Source = link.UsageDescription;
		}

		public ZGuid LayoutPK { get; private set; }

		[ResourceStringData("ApplicableCustomisedLayout.ControlType", Caption = "Control Type", FullDescription = "The type of graphical element this layout will be used for.")]
		public ZString ControlType { get; private set; }

		[ResourceStringData("ApplicableCustomisedLayout.JobType", Caption = "Job Type", FullDescription = "The type of job this layout will be used for. An empty job type implies the layout will be used for all tickets, except those whose job type has a separate layout configured.")]
		public ZString JobType { get; private set; }

		[ResourceStringData("ApplicableCustomisedLayout.LayoutName", Caption = "Layout Name")]
		public ZString LayoutName { get; private set; }

		[ResourceStringData("ApplicableCustomisedLayout.Source", Caption = "Source", FullDescription = "Indicates where the layout has been configured.")]
		public ZString Source { get; private set; }
	}
}
