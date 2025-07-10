using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ProvisionalPricingReasonsControlBag))]
	sealed class ProvisionalPricingReasonsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason101CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason102CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason103CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason104CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason105CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason106CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason107CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason108CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason109CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason110CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason111CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason112CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason113CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason114CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason115CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason116CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason117CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.ProvisionalPricingReason120CheckBox);
				yield return nameof(ProvisionalPricingReasonsControlBag.InstanceForDeclaration.OtherReasonTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ProvisionalPricingReasonsControlBag.InstanceForDeclaration;
	}
}
