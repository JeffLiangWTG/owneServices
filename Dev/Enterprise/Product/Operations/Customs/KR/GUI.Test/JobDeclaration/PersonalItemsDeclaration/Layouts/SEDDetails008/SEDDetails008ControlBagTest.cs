using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(SEDDetails008ControlBag))]
	sealed class SEDDetails008ControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SEDDetails008ControlBag.Instance.StayPeriodDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.CustomsOfficeCodeFindBox);
				yield return nameof(SEDDetails008ControlBag.Instance.DepartmentCodeFindBox);
				yield return nameof(SEDDetails008ControlBag.Instance.HasItemsDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.WeaponDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.DrugDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.AnimalsDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.EndangeredItemsDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.CounterfeitDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.CommercialUseItemsDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.ExcessTimeLimitItemsDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.PornographyDropEdit);
				yield return nameof(SEDDetails008ControlBag.Instance.BranchCodeGuidFindBox);
				yield return nameof(SEDDetails008ControlBag.Instance.BrokerCodeFindBox);
				yield return nameof(SEDDetails008ControlBag.Instance.ServiceCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SEDDetails008ControlBag.Instance;
	}
}
