using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(HeaderDetailsControlBag))]
	sealed class HeaderDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(HeaderDetailsTemplateUserControl.IsDeclarantImporterCheckBox);
				yield return nameof(HeaderDetailsTemplateUserControl.RegistrationNumberTextBox);
				yield return nameof(HeaderDetailsTemplateUserControl.IsFinalizedCheckBox);
				yield return nameof(HeaderDetailsTemplateUserControl.BranchGuidFindBox);
				yield return nameof(HeaderDetailsTemplateUserControl.UnlinkedDeclarationsNumberLabel);
			}
		}

		protected override ControlBag GetControlBagForTesting() => HeaderDetailsControlBag.Instance;
	}
}
