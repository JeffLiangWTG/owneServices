using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(AllocationForm))]
	internal class AllocationFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new AllocationForm(new Allocation(Factory));
		}
	}
}
