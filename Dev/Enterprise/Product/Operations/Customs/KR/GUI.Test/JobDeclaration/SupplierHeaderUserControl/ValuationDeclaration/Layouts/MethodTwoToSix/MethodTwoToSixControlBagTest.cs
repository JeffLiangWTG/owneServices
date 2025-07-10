using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MethodTwoToSixControlBag))]
	sealed class MethodTwoToSixControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MethodTwoToSixControlBag.ExpectedCustomsValueCalcEdit);
				yield return nameof(MethodTwoToSixControlBag.SupportingDocument1TextBox);
				yield return nameof(MethodTwoToSixControlBag.SupportingDocument2TextBox);
				yield return nameof(MethodTwoToSixControlBag.SampleItemCheckBox);
				yield return nameof(MethodTwoToSixControlBag.AdvertisingUseCheckBox);
				yield return nameof(MethodTwoToSixControlBag.UseOfDefectiveRepairCheckBox);
				yield return nameof(MethodTwoToSixControlBag.ReplacementItemCheckBox);
				yield return nameof(MethodTwoToSixControlBag.GiftOrFreeDonationCheckBox);
				yield return nameof(MethodTwoToSixControlBag.ForProductionAndManufactureCheckBox);
				yield return nameof(MethodTwoToSixControlBag.ItemUseCodeOtherReasonTextBox);
				yield return nameof(MethodTwoToSixControlBag.PerformancePriceOfPaidTransactionCheckBox);
				yield return nameof(MethodTwoToSixControlBag.PriceListCheckBox);
				yield return nameof(MethodTwoToSixControlBag.ManufacturingCostCheckBox);
				yield return nameof(MethodTwoToSixControlBag.InvoiceCheckBox);
				yield return nameof(MethodTwoToSixControlBag.GoodsPricingBasisOtherReasonTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MethodTwoToSixControlBag.InstanceForDeclaration;
	}
}
