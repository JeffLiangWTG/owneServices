using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(MiscOptionsControlBag))]
sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(MiscOptionsControlBag.AuthPerDeclarationCheckBox);
			yield return nameof(MiscOptionsControlBag.CertificateDropEdit);
			yield return nameof(MiscOptionsControlBag.DeclEmailAddrTextBox);
			yield return nameof(MiscOptionsControlBag.OtherEmailAddrTextBox);
			yield return nameof(MiscOptionsControlBag.SupportingInformationUserControl);
			yield return nameof(MiscOptionsControlBag.DontSendImporterIdCheckBox);
		}
	}
	protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
}
