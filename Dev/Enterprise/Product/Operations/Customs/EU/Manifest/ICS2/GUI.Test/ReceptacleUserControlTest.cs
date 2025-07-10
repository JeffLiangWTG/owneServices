using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(ReceptacleUserControl))]
	sealed class ReceptacleUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(AsycudaManifestHeader), control.BindingSource.DataSourceType);
		}

		public void TestReceptacleIdTextBox()
		{
			var receptacleIdTextBox = control.ReceptacleIdTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(receptacleIdTextBox);
				AssertEquals("BindTo", nameof(AsycudaManifestHeader.ReceptacleId), receptacleIdTextBox.BindTo);
				AssertNull("CaptionResourceString", receptacleIdTextBox.CaptionResourceString.Caption);
				var labelCaptionRenderProvider = new LabelCaptionRenderProvider();
				AssertEquals("ReceptacleIdTextBox label is not visible", false, labelCaptionRenderProvider.GetLabelCaptionVisible(receptacleIdTextBox));
			});
		}

		public void TestReceptacleEditButton() => CombineAssertions(() =>
		{
			var button = control.ReceptacleEditButton;
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm(manifestHeader);
			form.Controls.Add(control);
			form.Show();
			AssertReceptacleEditButton(button);
		});

		public void TestImplementIExtendedControl()
		{
			var receptacleUserControlExtendedControl = control as IExtendedControl;

			AssertNotNull("ReceptacleUserControl implements IExtendedControl?", receptacleUserControlExtendedControl);

			AssertEquals("Host", receptacleUserControlExtendedControl, receptacleUserControlExtendedControl.Host);
			AssertNotNull("Extensions", receptacleUserControlExtendedControl.Extensions);
		}

		public void TestImplementResourceStringBindingMember()
		{
			var receptacleUserControlBindingMember = control as IResourceStringBindingMember;

			AssertNotNull("ReceptacleUserControl implements IResourceStringBindingMember?", receptacleUserControlBindingMember);
			AssertEquals("ResourceStringBindingMember", "ReceptacleId", receptacleUserControlBindingMember.ResourceStringBindingMember);
		}

		void AssertReceptacleEditButton(ZButton button)
		{
			AssertEquals("Caption", "Receptacle ID(s)", button.GetExtension<ILabelCaptionRenderer>().Caption);
			AssertNoExceptionThrown(button.PerformClick);
			AssertType<ReceptacleForm>(ZFormModaliser.LastFormShownDialogForTest);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ReceptacleUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ReceptacleUserControl control;
	}
}
