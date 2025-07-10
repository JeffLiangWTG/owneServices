using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI
{
	[TestedType(typeof(FinalPriceExtensionRequestNewControlBag))]
	sealed class FinalPriceExtensionRequestNewControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(FinalPriceExtensionRequestNewControlBag.MessageTypeDropEdit);
				yield return nameof(FinalPriceExtensionRequestNewControlBag.CustomsOfficeCodeFindBox);
				yield return nameof(FinalPriceExtensionRequestNewControlBag.BranchGuidFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => FinalPriceExtensionRequestNewControlBag.Instance;
	}
}
