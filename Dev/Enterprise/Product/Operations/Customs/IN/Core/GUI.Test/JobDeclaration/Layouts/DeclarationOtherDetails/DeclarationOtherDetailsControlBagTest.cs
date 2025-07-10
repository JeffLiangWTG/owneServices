using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(DeclarationOtherDetailsControlBag))]
sealed class DeclarationOtherDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(DeclarationOtherDetailsControlBag.IECCodeTextBox);
			yield return nameof(DeclarationOtherDetailsControlBag.OriginStateDropEdit);
			yield return nameof(DeclarationOtherDetailsControlBag.ExporterClassTextBox);
			yield return nameof(DeclarationOtherDetailsControlBag.EPZCodeDropEdit);
			yield return nameof(DeclarationOtherDetailsControlBag.BranchSerialNumberTextBox);
			yield return nameof(DeclarationOtherDetailsControlBag.AuthorizedDealerCodeTextBox);
			yield return nameof(DeclarationOtherDetailsControlBag.TypeOfExporterDropEdit);
			yield return nameof(DeclarationOtherDetailsControlBag.SealByDropEdit);
			yield return nameof(DeclarationOtherDetailsControlBag.RotationNumberTextBox);
			yield return nameof(DeclarationOtherDetailsControlBag.RotationDateDateEdit);
			yield return nameof(DeclarationOtherDetailsControlBag.StuffingAtDropEdit);
			yield return nameof(DeclarationOtherDetailsControlBag.SampleAccompaniedDropEdit);
			yield return nameof(DeclarationOtherDetailsControlBag.GoodsRegistrationSeparatorUserControl);
			yield return nameof(DeclarationOtherDetailsControlBag.TranshipperGuidFindBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => DeclarationOtherDetailsControlBag.Instance;
}
