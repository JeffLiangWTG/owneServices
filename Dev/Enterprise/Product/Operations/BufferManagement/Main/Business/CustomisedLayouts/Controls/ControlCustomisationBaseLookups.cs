using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ControlCustomisationBaseLookups : ZLookups
	{
		public ControlCustomisationBaseLookups(ControlCustomisationBase parent)
			: base(parent)
		{
		}

		new ControlCustomisationBase Parent
		{
			get { return (ControlCustomisationBase)base.Parent; }
		}

		public CodeDescriptionPairList ControlTypes
		{
			get { return Parent.ControlTypes; }
		}

		public ColorList ColorList
		{
			get { return Factory.GetCachedValue<ColorList>(); }
		}

		public FontList FontList
		{
			get { return Factory.GetCachedValue<FontList>(); }
		}

		public ControlAlignmentList AlignmentList
		{
			get { return Factory.GetCachedValue<ControlAlignmentList>(); }
		}

		public OrientationList OrientationList
		{
			get { return Factory.GetCachedValue<OrientationList>(); }
		}

		public StatusButtonBehaviorOptionsList StatusButtonBehaviorOptionsList => Factory.GetCachedValue<StatusButtonBehaviorOptionsList>();
	}
}
