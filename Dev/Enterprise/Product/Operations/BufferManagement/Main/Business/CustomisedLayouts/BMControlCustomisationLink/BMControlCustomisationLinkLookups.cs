using CargoWise.Application;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationLinkLookups : AutoBMControlCustomisationLinkLookups
	{
		public BMControlCustomisationLinkLookups(AutoBMControlCustomisationLink parent)
			: base(parent)
		{
		}

		public BMControlCustomisationCollection CustomisedLayouts
		{
			get { return Factory.GetCachedValue("BMControlCustomisationLinkLookups.CustomisedLayouts", () => new BMControlCustomisationCollection(Factory)); }
		}

		public CodeDescriptionPairList JobTypes
		{
			get => Factory.GetCachedValue("IWorkflowDescriptorList", () => (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>());
		}
	}
}
