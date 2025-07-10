using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Customs.US;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ExportEntryFilerIDControl))]
	sealed class ExportEntryFilerIDControlTest : RegistryZUserControlTestCase
	{
		public void TestFilerCodeCharacterCase()
		{
			using (var control = new ExportEntryFilerIDControl())
			{
				AssertEquals(CharacterCasing.Upper, control.EntryFilerIDTextBox.CharacterCasing);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new ExportEntryFilerID();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			bool result = false;
			var filer = control.CurrentDataItem as ExportEntryFilerID;
			if (filer != null)
			{
				result = filer.ReadOnly;
			}
			return result;
		}

		#endregion
	}
}
