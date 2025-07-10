using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMBufferTimespanForm))]
	public class BMBufferTimespanFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new BMBufferTimespanForm(Factory.New<BMBufferTimespan>());
		}
	}
}
