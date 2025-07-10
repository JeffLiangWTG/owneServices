using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZDummyModuleButtonGrid : ZModuleButtonGrid
	{
		public ZDummyModuleButtonGrid()
		{
			InnerGrid.GridId = "iouwefr87094528907";
		}

		public void EditSelected()
		{
			EditButton_Click(this, EventArgs.Empty);
			Application.DoEvents();

			if (LastShownZForm != null)
			{
				LastShownZForm.Dispose();
			}
		}

		public new void Edit(BusinessObject selected, object sender)
		{
			base.Edit(selected, sender);
		}

		public new ZToolStripButton NewButton
		{
			get { return GetButton(ZModuleButtonGrid.Buttons.New); }
		}

		public new ZToolStripButton EditButton
		{
			get { return GetButton(ZModuleButtonGrid.Buttons.Edit); }
		}

		public new ZToolStripButton AttachButton
		{
			get { return GetButton(ZModuleButtonGrid.Buttons.Attach); }
		}

		public new ZToolStripButton DetachButton
		{
			get { return GetButton(ZModuleButtonGrid.Buttons.Detach); }
		}

		public new bool NeedsSaveToShowEditForm(BusinessObject selected)
		{
			return base.NeedsSaveToShowEditForm(selected);
		}

		internal override DialogResult ConfirmDetach(string message)
		{
			return DialogResult.Yes;
		}

		protected override IModuleDecisionProvider GetNewModuleDecisionProvider(IFindBox findBox)
		{
			return new ZDummyButtonGridModuleDecisionProvider(findBox);
		}
#if DEBUG
		internal
#endif
		ZToolStripButton GetButton(string name)
		{
			var toolStrip = (ZToolStrip)Controls.Find("toolStrip", true).First();
			return toolStrip.Items.Find(name, true).FirstOrDefault() as ZToolStripButton;
		}

		protected override BusinessObject GetObjectToEdit(BusinessObject selected)
		{
			if (!UseAlternativeObjectForEdit)
			{
				return base.GetObjectToEdit(selected);
			}
			else
			{
				return ((DummyBaseBusinessObject)selected).RelatedDummy;
			}
		}

		bool useAlternativeObjectForEdit;
		public bool UseAlternativeObjectForEdit
		{
			get { return useAlternativeObjectForEdit; }
			set { useAlternativeObjectForEdit = value; }
		}

		internal ZController GetController()
		{
			return GetNewControllerCore(null);
		}

		protected override ZController GetNewControllerCore(BusinessObject selected)
		{
			return ControllerForTest ?? base.GetNewControllerCore(selected);
		}

		public ZController ControllerForTest { get; set; }
	}
}
