using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(DetailsHeaderControlBag))]
sealed class DetailsHeaderControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(DetailsHeaderControlBag.StatusDropEdit);
			yield return nameof(DetailsHeaderControlBag.CustomsOfficeCodeFindBox);
			yield return nameof(DetailsHeaderControlBag.PreviousReferenceTypeDropEdit);
			yield return nameof(DetailsHeaderControlBag.PresentationDateEdit);
			yield return nameof(DetailsHeaderControlBag.ArrvialDateEdit);
			yield return nameof(DetailsHeaderControlBag.ATBNumberTextBox);
			yield return nameof(DetailsHeaderControlBag.PreviousReferenceNumberTextBox);
			yield return nameof(DetailsHeaderControlBag.CustomerReferenceTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => DetailsHeaderControlBag.Instance;
}
