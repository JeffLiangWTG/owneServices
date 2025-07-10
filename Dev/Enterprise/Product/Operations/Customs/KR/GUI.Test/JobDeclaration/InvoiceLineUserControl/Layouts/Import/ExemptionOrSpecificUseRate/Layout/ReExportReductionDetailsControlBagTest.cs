using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ReExportReductionDetailsControlBag))]
	sealed class ReExportReductionDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ReExportReductionDetailsControlBag.InstanceForDeclaration.CustomsOfficeCodeFindBox);
				yield return nameof(ReExportReductionDetailsControlBag.InstanceForDeclaration.DestCountryCodeFindBox);
				yield return nameof(ReExportReductionDetailsControlBag.InstanceForDeclaration.EstimateDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ReExportReductionDetailsControlBag.InstanceForDeclaration;
	}
}
