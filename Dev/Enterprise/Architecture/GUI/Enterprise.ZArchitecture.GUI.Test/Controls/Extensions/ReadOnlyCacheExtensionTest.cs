using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class ReadOnlyCacheExtensionTest : BaseExtensionTest<ReadOnlyCacheExtension>
	{
		public void TestBackupAndRestore()
		{
			using (var extension = new ReadOnlyCacheExtension(null))
			using (var textBox = new ZTextBox())
			{
				textBox.Extensions.Add(extension);
				textBox.DataBindings.Add("ReadOnlyForBinding", new BindParent(), "Name");

				var binding = textBox.DataBindings["ReadOnlyForBinding"];

				AssertNotNull("Precondition", binding);
				AssertEquals(false, textBox.ReadOnly);

				extension.Backup();
				binding = textBox.DataBindings["ReadOnlyForBinding"];

				AssertNull("Should is removed for avoiding any value changes on ReadOnlyForBinding.", binding);
				AssertEquals("Should is false.", false, textBox.ReadOnly);

				textBox.ReadOnly = true;
				extension.Restore();

				binding = textBox.DataBindings["ReadOnlyForBinding"];
				AssertNotNull("Should restore from the extension.", binding);
				AssertEquals("Should restore from the extension.", false, textBox.ReadOnly);
			}
		}

		protected override IControlExtension Create()
		{
			return new ReadOnlyCacheExtension(null);
		}

		sealed class BindParent
		{
			public string Name { get; set; }
		}
	}
}
