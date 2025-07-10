using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.GUI.Testing
{
	[TestedType(typeof(GVMSManifestControlBag))]
	sealed class GVMSManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(GVMSManifestControlBag.CarrierCodeDropEdit);
				yield return nameof(GVMSManifestControlBag.RouteIdDropEdit);
				yield return nameof(GVMSManifestControlBag.IsUnaccompaniedCheckBox);
				yield return nameof(GVMSManifestControlBag.EmptyVehicleDropEdit);
				yield return nameof(GVMSManifestControlBag.InspectionRequiredCheckBox);
				yield return nameof(GVMSManifestControlBag.InspectionLocationsGroupBox);
				yield return nameof(GVMSManifestControlBag.CustomsReferencesGroupBox);
				yield return nameof(GVMSManifestControlBag.TransitReferencesGroupBox);
				yield return nameof(GVMSManifestControlBag.OtherReferencesGroupBox);
				yield return nameof(GVMSManifestControlBag.HaulierTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => GVMSManifestControlBag.Instance;
	}
}
