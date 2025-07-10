using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDAManifest.GUI.Testing
{
	[TestedType(typeof(ASYCUDABillControlBag))]
	sealed class ASYCUDABillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ASYCUDABillControlBag.BDEGMSeparatorUserControl);
				yield return nameof(ASYCUDABillControlBag.SADOfficeCodeDropEdit);
				yield return nameof(ASYCUDABillControlBag.SADRegistrationSerialTextBox);
				yield return nameof(ASYCUDABillControlBag.SADRegistrationNumberTextBox);
				yield return nameof(ASYCUDABillControlBag.SADRegistrationDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ASYCUDABillControlBag.Instance;
	}
}
