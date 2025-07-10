using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	[TestedType(typeof(CompanyConfigurationControlBag))]
	sealed class CompanyBrokerageControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(CompanyConfigurationControlBag.CustomsConfigurationGroupBox));
			}
		}

		protected override ControlBag GetControlBagForTesting() => CompanyConfigurationControlBag.Instance;
	}
}
