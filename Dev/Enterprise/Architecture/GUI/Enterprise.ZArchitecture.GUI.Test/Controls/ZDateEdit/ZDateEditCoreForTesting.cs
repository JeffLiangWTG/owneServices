using CargoWise.Types;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZDateEditCoreForTesting : ZDateEditCore
	{
		public ZDateEditCoreForTesting(IDateInputControl control)
			: base(control)
		{
		}

		public new ZDateTime StringToDate(string inputText)
		{
			return base.StringToDate(inputText);
		}
	}
}
