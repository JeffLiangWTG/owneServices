using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(DataTransferForm))]
	public sealed class DataTransferFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DataTransferForm();
		}
	}
}
