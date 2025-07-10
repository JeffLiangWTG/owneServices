using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(PostClearanceDetailsControlBag))]
	sealed class PostClearanceDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(PostClearanceDetailsControlBag.InstanceForDeclaration.PostClearanceYNDropEdit);
				yield return nameof(PostClearanceDetailsControlBag.InstanceForDeclaration.UseCodeDescriptionTextBox);
				yield return nameof(PostClearanceDetailsControlBag.InstanceForDeclaration.ProductTypeDropEdit);
				yield return nameof(PostClearanceDetailsControlBag.InstanceForDeclaration.SerialNumberTextBox);
				yield return nameof(PostClearanceDetailsControlBag.InstanceForDeclaration.CustomsOfficeCodeFindBox);
				yield return nameof(PostClearanceDetailsControlBag.InstanceForDeclaration.GoodsLocationAddressControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => PostClearanceDetailsControlBag.InstanceForDeclaration;
	}
}
