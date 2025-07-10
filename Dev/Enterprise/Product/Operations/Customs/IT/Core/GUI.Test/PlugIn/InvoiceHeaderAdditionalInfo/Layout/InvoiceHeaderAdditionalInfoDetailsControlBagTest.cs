using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(InvoiceHeaderAdditionalInfoDetailsControlBag))]
sealed class InvoiceHeaderAdditionalInfoDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InvoiceHeaderAdditionalInfoDetailsControlBag.DescriptionTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => InvoiceHeaderAdditionalInfoDetailsControlBag.Instance;

	public void TestTemplate()
	{
		using (var template = new InvoiceHeaderAdditionalInfoDetailsControlBagForTest().CreateTemplateExposed())
		{
			AssertType<InvoiceHeaderAdditionalInfoDetailsUserControl>("Template Type", template);
		}
	}

	class InvoiceHeaderAdditionalInfoDetailsControlBagForTest : InvoiceHeaderAdditionalInfoDetailsControlBag
	{
		public Control CreateTemplateExposed() => base.CreateTemplate();
	}
}
