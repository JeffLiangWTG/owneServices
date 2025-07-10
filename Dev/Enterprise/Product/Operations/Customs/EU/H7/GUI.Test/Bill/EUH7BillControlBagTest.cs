using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7BillControlBag))]

	sealed class EUH7BillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUH7BillControlBag.LocalReferenceNumberTextBox);
				yield return nameof(EUH7BillControlBag.MovementReferenceNumberTextBox);
				yield return nameof(EUH7BillControlBag.LocationOfGoodsUserControl);
				yield return nameof(EUH7BillControlBag.GoodsValueConvertToLocalCurrencyControl);
				yield return nameof(EUH7BillControlBag.AdditionalProcedureDropEdit);
				yield return nameof(EUH7BillControlBag.MessageStatusDropEdit);
				yield return nameof(EUH7BillControlBag.AdditionalProcedureCodesUserControl);
				yield return nameof(EUH7BillControlBag.StandAloneDeclarationUserControl);
				yield return nameof(EUH7BillControlBag.ContainerUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUH7BillControlBag.Instance;
	}
}
