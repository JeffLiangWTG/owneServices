using CargoWise.Application;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationLookups : AutoBMControlCustomisationLookups
	{
		public BMControlCustomisationLookups(AutoBMControlCustomisation parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ControlTypes
		{
			get { return Factory.GetCachedValue<CustomisedControlTypeList>(); }
		}

		public CodeDescriptionPairList JobTypes
		{
			get => Factory.GetCachedValue("IWorkflowDescriptorList", () => (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>());
		}

		public ColorList ColorList
		{
			get { return Factory.GetCachedValue<ColorList>(); }
		}
	}
}
