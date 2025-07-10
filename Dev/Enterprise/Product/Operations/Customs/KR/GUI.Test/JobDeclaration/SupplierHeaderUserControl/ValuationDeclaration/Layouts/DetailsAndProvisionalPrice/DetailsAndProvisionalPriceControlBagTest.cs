using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DetailsAndProvisionalPriceControlBag))]
	sealed class DetailsAndProvisionalPriceControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.InvoiceNoTextBox);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.InvoiceDateEdit);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.PurchaseOrderNoTextBox);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.PurchaseOrderDateEdit);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.ContractNoTextBox);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.ContractDateEdit);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.TotalCustomsValueCalcEdit);

				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.ProvisionalPricingDropEdit);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.ProvisionalAdditionRateCalcEdit);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.ProvisionalAdditionAmountCalcEdit);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.EstimatedDateOfFinalPriceDateEdit);
				yield return nameof(DetailsAndProvisionalPriceControlBag.InstanceForDeclaration.ContractExpirationDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DetailsAndProvisionalPriceControlBag.InstanceForDeclaration;
	}
}
