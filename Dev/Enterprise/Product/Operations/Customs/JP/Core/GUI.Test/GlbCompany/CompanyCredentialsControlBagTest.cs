using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsControlBag))]
	sealed class CompanyCredentialsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames => new[] { nameof(CompanyCredentialsControlBag.NaccsMailboxGroupBox) };

		protected override ControlBag GetControlBagForTesting() => CompanyCredentialsControlBag.Instance;
	}
}
