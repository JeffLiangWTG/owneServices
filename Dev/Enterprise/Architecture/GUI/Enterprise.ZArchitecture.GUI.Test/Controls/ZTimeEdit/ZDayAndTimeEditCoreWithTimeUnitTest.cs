using Enterprise.ZArchitecture.GUI.Internal;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDayAndTimeEditCoreWithTimeUnitTest : ZTimeEditCoreTest
	{
		#region Implementation

		protected override ZTimeEditCore NewCore()
		{
			return new ZDayAndTimeEditCore(ContainerPenaltyTimeUnit.Codes.Hours, Control);
		}

		protected override ZTimeEdit NewEditor()
		{
			return new ZDayAndTimeEdit();
		}

		new ZDayAndTimeEdit Control
		{
			get { return (ZDayAndTimeEdit)base.Control; }
		}

		#endregion
	}
}
