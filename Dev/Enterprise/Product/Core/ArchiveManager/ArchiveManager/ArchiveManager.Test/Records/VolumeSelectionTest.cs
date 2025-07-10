using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.GUI.Records;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Records
{
	[TestedType(typeof(VolumeSelection))]
	class VolumeSelectionTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
			=> new VolumeSelection(1);

		#endregion
	}

	[TestedType(typeof(VolumeSelectionForm))]
	class VolumeSelectionFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var selection = new VolumeSelection(1);
			selection.HasChanges = false;
			return new VolumeSelectionForm(selection);
		}
	}

	class VolumeSelectionValidationTest : BusinessObjectValidationTestCase
	{ }
}
