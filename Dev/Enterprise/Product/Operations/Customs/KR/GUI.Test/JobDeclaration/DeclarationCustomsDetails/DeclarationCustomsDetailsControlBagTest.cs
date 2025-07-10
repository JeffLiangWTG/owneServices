using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DeclarationCustomsDetailsControlBag))]
	sealed class DeclarationCustomsDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DeclarationCustomsDetailsControlBag.CustomsOfficeCodeFindBox);
				yield return nameof(DeclarationCustomsDetailsControlBag.DepartmentCodeFindBox);
				yield return nameof(DeclarationCustomsDetailsControlBag.DepartureCountryCodeFindBox);
				yield return nameof(DeclarationCustomsDetailsControlBag.ContainerPackDropEdit);
				yield return nameof(DeclarationCustomsDetailsControlBag.BondedAreaCodeFindBox);
				yield return nameof(DeclarationCustomsDetailsControlBag.LocationIDInBondedAreaTextBox);
				yield return nameof(DeclarationCustomsDetailsControlBag.UnderbondMovementArrivalDateEdit);
				yield return nameof(DeclarationCustomsDetailsControlBag.CustomsBrokerCommentUserControl);
				yield return nameof(DeclarationCustomsDetailsControlBag.SouthNorthTradeTypeDropEdit);
				yield return nameof(DeclarationCustomsDetailsControlBag.GoldTradeTransactionYNDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DeclarationCustomsDetailsControlBag.Instance;
	}
}
