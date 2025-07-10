using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public abstract partial class ComplianceDocumentModule : ZFilterGridModule
	{
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowNew => false;

		public override bool SupportsWorkflow => true;

		#region GetNewAdditionalMenuItems

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> menus = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			menus.Insert(0, new ZMenuItem(ResString.GetMultilingualString("Accounting.ComplianceDocument.Void", "Void"), new EventHandler(HandleVoid)));
			return menus.ToArray();
		}

		#endregion

#if DEBUG
		protected ZForm LastShownComplianceDocumentForm_ForTest;
#endif

		protected virtual void HandleVoid(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				ShowNoSelectedMessage();
				return;
			}

			var complianceDocument = SelectedBusinessObjects[0] as AccComplianceDocumentHeader;
			if (complianceDocument != null)
			{
				complianceDocument.Reload();
				IZForm form = null;
				if (complianceDocument.IsAdded)
				{
					string caption = Res.GetString("FF50CDB5-D4D2-4CDD-8461-E82365216A6B", "Void Compliance Document");
					var msg = Res.GetString("9B53D2F1-A58D-4ADC-8CE4-290AAAD991EC", "This compliance document does not have document number set. Do you want to delete this document record instead?");
					var result = Globals.Message.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					switch (result)
					{
						case DialogResult.Yes:
							form = ShowDeleteForm(complianceDocument);
#if DEBUG
							LastShownComplianceDocumentForm_ForTest = (ZForm)form;
#endif
							return;
						case DialogResult.No:
							break;
					}
				}

				var controller = GetNewController(complianceDocument) as ComplianceDocumentController;
				form = controller.ShowVoidForm(complianceDocument);
#if DEBUG
				LastShownComplianceDocumentForm_ForTest = (ZForm)form;
#endif
			}
		}
	}
}
