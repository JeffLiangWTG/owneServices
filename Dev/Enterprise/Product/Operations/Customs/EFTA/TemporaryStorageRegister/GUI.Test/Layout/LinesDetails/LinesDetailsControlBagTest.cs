using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(LinesDetailsControlBag))]
sealed class LinesDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(LinesDetailsControlBag.UnionStatusDropEdit);
			yield return nameof(LinesDetailsControlBag.PackagesRemainingCalcEdit);
			yield return nameof(LinesDetailsControlBag.LineNumberCalcEdit);
			yield return nameof(LinesDetailsControlBag.CustomsStatusDropEdit);
			yield return nameof(LinesDetailsControlBag.PackageTypeDropEdit);
			yield return nameof(LinesDetailsControlBag.OwnerReferenceTypeDropEdit);
			yield return nameof(LinesDetailsControlBag.LimitDateEdit);
			yield return nameof(LinesDetailsControlBag.CustodianEORIBranchUserControl);
			yield return nameof(LinesDetailsControlBag.DisposalEntitledTraderEORIBranchUserControl);
			yield return nameof(LinesDetailsControlBag.GoodsDescriptionTextBox);
			yield return nameof(LinesDetailsControlBag.LocationofGoodsTextBox);
			yield return nameof(LinesDetailsControlBag.OwnerReferenceNumberTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => LinesDetailsControlBag.Instance;
}
