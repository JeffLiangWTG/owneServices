using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(AdditionalInformationDetailsControlBag))]
	sealed class AdditionalInformationDetailsControlBagTest : ControlBagAbstractTest
	{
		public void TestTemplate()
		{
			using (var template = new AddInfoDetailsControlBagForTesting().CreateTemplateExposed())
			{
				AssertType<AdditionalInformationDetailsUserControl>(template);
			}
		}

		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(AdditionalInformationDetailsControlBag.KindDropEdit);
				yield return nameof(AdditionalInformationDetailsControlBag.FullTypeCodeFindBox);
				yield return nameof(AdditionalInformationDetailsControlBag.ReferenceTextBox);
				yield return nameof(AdditionalInformationDetailsControlBag.DescriptionTextBox);
				yield return nameof(AdditionalInformationDetailsControlBag.DetailTextBox);
				yield return nameof(AdditionalInformationDetailsControlBag.CurrencyDropEdit);
				yield return nameof(AdditionalInformationDetailsControlBag.AmountCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => AdditionalInformationDetailsControlBag.Instance;

		sealed class AddInfoDetailsControlBagForTesting : AdditionalInformationDetailsControlBag
		{
			internal Control CreateTemplateExposed() => base.CreateTemplate();
		}
	}
}
