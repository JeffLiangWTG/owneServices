using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(DogHitXRayForm))]
	internal class DogHitXRayFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DogHitXRayForm(new DogHitXRay(Factory));
		}
	}
}
