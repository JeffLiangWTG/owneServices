using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(EUICS2ManifestControlBag))]
	sealed class EUICS2ManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUICS2ManifestControlBag.SpecificCircumstanceIndicatorDropEdit);
				yield return nameof(EUICS2ManifestControlBag.ReEntryIndicatorCheckBox);
				yield return nameof(EUICS2ManifestControlBag.PreviousMRNTextBox);
				yield return nameof(EUICS2ManifestControlBag.BranchGuidFindBox);
				yield return nameof(EUICS2ManifestControlBag.LocalReferenceNumberTextBox);
				yield return nameof(EUICS2ManifestControlBag.TransportDocumentTypeDropEdit);
				yield return nameof(EUICS2ManifestControlBag.MOTIdentifierTextBox);
				yield return nameof(EUICS2ManifestControlBag.MOTIdentifierTypeDropEdit);
				yield return nameof(EUICS2ManifestControlBag.RegistrationNumberTextBox);
				yield return nameof(EUICS2ManifestControlBag.AddressedMemberStateDropEdit);
				yield return nameof(EUICS2ManifestControlBag.CustomsProfileDropEdit);
				yield return nameof(EUICS2ManifestControlBag.ActualDepartureDateEdit);
				yield return nameof(EUICS2ManifestControlBag.MeansOfTransportTypeDropEdit);
				yield return nameof(EUICS2ManifestControlBag.VehicleRegistrationAndNationalityUserControl);
				yield return nameof(EUICS2ManifestControlBag.ReceptacleUserControl);
				yield return nameof(EUICS2ManifestControlBag.SplitConsignmentIndicatorCheckBox);
				yield return nameof(EUICS2ManifestControlBag.OriginCodeFindBox);
				yield return nameof(EUICS2ManifestControlBag.FinalDestinationCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUICS2ManifestControlBag.Instance;
	}
}
