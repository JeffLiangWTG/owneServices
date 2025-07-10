using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceDetailsControlBag))]
	sealed class CommercialInvoiceDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommercialInvoiceDetailsControlBag.IncoTermCodeDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.NoOfPacksCalcDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.PaymentTermsCodeDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.LetterOfCreditNumberTextBox);
				yield return nameof(CommercialInvoiceDetailsControlBag.NoteTextLongTextControl);

				yield return nameof(CommercialInvoiceDetailsControlBag.DRWApplicantDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.SupportingDocumentTypeDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.SupportingDocumentNoTextBox);
				yield return nameof(CommercialInvoiceDetailsControlBag.InboundDateEdit);

				yield return nameof(CommercialInvoiceDetailsControlBag.ValuationCodeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommercialInvoiceDetailsControlBag.Instance;
	}
}
