using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(ESH7ManifestControlBag))]
	sealed class ESH7ManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ESH7ManifestControlBag.CusAgentCodeFindBox);
				yield return nameof(ESH7ManifestControlBag.CertificateDropEdit);
				yield return nameof(ESH7ManifestControlBag.TrainingCheckBox);
				yield return nameof(ESH7ManifestControlBag.LocationOfGoodsUserControl);
				yield return nameof(ESH7ManifestControlBag.TransportDocumentTypeDropEdit);
				yield return nameof(ESH7ManifestControlBag.TransportDocumentReferenceTextBox);
				yield return nameof(ESH7ManifestControlBag.G3MRNToRevokeDropEdit);
				yield return nameof(ESH7ManifestControlBag.EntryLineNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ESH7ManifestControlBag.Instance;
	}
}
