using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsControlBag))]
	sealed class CompanyCredentialsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(CompanyCredentialsControlBag.UnipassCertificateGroupBox));
			}
		}

		protected override ControlBag GetControlBagForTesting() => CompanyCredentialsControlBag.Instance;
	}
}
