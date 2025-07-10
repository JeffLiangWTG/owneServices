using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundDeclarationImportEntriesControlBag))]
	sealed class RefundDeclarationImportEntriesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(RefundDeclarationImportEntriesControlBag.SequenceNumberCalcEdit);
				yield return nameof(RefundDeclarationImportEntriesControlBag.ImportEntryNumberCodeFindBox);
				yield return nameof(RefundDeclarationImportEntriesControlBag.ImportEntryLineNumCodeFindBox);
				yield return nameof(RefundDeclarationImportEntriesControlBag.CustomsDisbursementBillCodeFindBox);
				yield return nameof(RefundDeclarationImportEntriesControlBag.AmendSequenceNumber5WNDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => RefundDeclarationImportEntriesControlBag.Instance;
	}
}
