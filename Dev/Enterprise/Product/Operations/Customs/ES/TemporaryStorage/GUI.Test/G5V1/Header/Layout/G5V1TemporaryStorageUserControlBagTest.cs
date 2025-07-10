using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStorageUserControlBag))]
	sealed class G5V1TemporaryStorageUserControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(G5V1TemporaryStorageUserControl.DeclarationDetailsGroupBox);
				yield return nameof(G5V1TemporaryStorageUserControlBag.DocumentsTabControl);
				yield return nameof(G5V1TemporaryStorageUserControlBag.DestinationCustomsOfficeCodeFindBox);
				yield return nameof(G5V1TemporaryStorageUserControlBag.DestinationLocationOfGoodsUserControl);
				yield return nameof(G5V1TemporaryStorageUserControlBag.ManualLocationOfGoodsCodeFindBox);
				yield return nameof(G5V1TemporaryStorageUserControlBag.CertificateDropEdit);
				yield return nameof(G5V1TemporaryStorageUserControlBag.TransportDocumentTypeDropEdit);
				yield return nameof(G5V1TemporaryStorageUserControlBag.TransportDocumentTextBox);
				yield return nameof(G5V1TemporaryStorageUserControlBag.TrainingCheckBox);
				yield return nameof(G5V1TemporaryStorageUserControlBag.IsSimplifiedCheckBox);
				yield return nameof(G5V1TemporaryStorageUserControlBag.GuaranteeGroupBox);
				yield return nameof(G5V1TemporaryStorageUserControlBag.MovementOfContainersOnlyCheckBox);
				yield return nameof(G5V1TemporaryStorageUserControlBag.UnionGoodsCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => G5V1TemporaryStorageUserControlBag.Instance;
	}
}
