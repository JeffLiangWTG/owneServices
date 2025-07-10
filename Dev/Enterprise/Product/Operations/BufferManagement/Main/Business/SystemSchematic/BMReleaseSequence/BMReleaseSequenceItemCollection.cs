using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMReleaseSequenceItemCollection : ActiveBusinessObjectCollection<BMReleaseSequenceItem>
	{
		public BMReleaseSequenceItemCollection(BMReleaseSequence parent)
			: base(parent.Factory, parent, new ZQuery(), BMReleaseSequenceItemSchema.BMI_BMR_Sequence)
		{
		}
	}
}
