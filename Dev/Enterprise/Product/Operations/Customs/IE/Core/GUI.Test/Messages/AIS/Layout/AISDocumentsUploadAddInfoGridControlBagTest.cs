using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(AISDocumentsUploadAddInfoGridControlBag))]
	sealed class AISDocumentsUploadAddInfoGridControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(AISDocumentsUploadAddInfoGridControlBag.AddInfosGrid));
				yield return (nameof(AISDocumentsUploadAddInfoGridControlBag.AddInfosIM483Grid));
			}
		}

		protected override ControlBag GetControlBagForTesting() => AISDocumentsUploadAddInfoGridControlBag.Instance;

		public new void TestNoBindingMemberDuplicates()
		{
			AssertNull("We should not be testing this, we need a duplicate binding", null);
		}
	}
}
