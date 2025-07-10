using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	sealed class OverrideRevokeReasonUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestRevokeReasonControlsReadOnly()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var sendingObjectParent = new G3MessageSendingObjectParent(manifest);

			using (var form = new ZForm(sendingObjectParent))
			using (var control = new OverrideRevokeReasonUserControl())
			{
				control.SetDataBinding(sendingObjectParent, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var overrideRevokeReasonCheckBox = form.Controls.Find("overrideRevokeReasonCheckBox", searchAllChildren: true).Single() as ZCheckBox;
				var revokeReasonDropEdit = form.Controls.Find("revokeReasonDropEdit", searchAllChildren: true).Single() as ZDropEdit;
				var revokeReasonDescriptionTextBox = form.Controls.Find("revokeReasonDescriptionTextBox", searchAllChildren: true).Single() as ZTextBox;

				AssertNotNull("Checkbox is not null", overrideRevokeReasonCheckBox);
				AssertNotNull("Revoke Reason Drop Edit is not null", revokeReasonDropEdit);
				AssertNotNull("Revoke Reason Description Text Box is not null", revokeReasonDescriptionTextBox);

				Assert("DropEdit allows drop", revokeReasonDropEdit.AllowDrop);
				overrideRevokeReasonCheckBox.Checked = true;
				Assert(sendingObjectParent.OverrideDefaultRevokeReason);
				Assert(!revokeReasonDropEdit.ReadOnly);
				Assert(!revokeReasonDescriptionTextBox.ReadOnly);
				overrideRevokeReasonCheckBox.Checked = false;
				Assert(!sendingObjectParent.OverrideDefaultRevokeReason);
				Assert(revokeReasonDropEdit.ReadOnly);
				Assert(revokeReasonDescriptionTextBox.ReadOnly);
			}
		}
	}
}
