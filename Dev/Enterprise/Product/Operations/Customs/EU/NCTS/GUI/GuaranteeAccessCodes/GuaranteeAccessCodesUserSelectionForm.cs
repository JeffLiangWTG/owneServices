using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class GuaranteeAccessCodesUserSelectionForm : ZChildForm
	{
		public GuaranteeAccessCodesUserSelectionForm()
		{
			InitialiseForm();
		}

		public GuaranteeAccessCodesUserSelectionForm(GuaranteeAccessCodesUserSelectionObjectCollection guarantees) : base(guarantees)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			GuaranteesGrid.AfterBind += GuaranteesGrid_AfterBind;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var listManager = GuaranteesGrid.ListManager;
				if (listManager != null)
				{
					listManager.CurrentChanged -= GuaranteesGrid_AfterBind;
					listManager.CurrentItemChanged -= ItemsGrid_ListManager_CurrentItemChanged;
					listManager = null;
				}
			}
			base.Dispose(disposing);
		}

		void GuaranteesGrid_AfterBind(object sender, EventArgs e)
		{
			var listManager = GuaranteesGrid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentItemChanged += ItemsGrid_ListManager_CurrentItemChanged;
				ItemsGrid_ListManager_CurrentItemChanged(null, null);
			}
		}

		void ItemsGrid_ListManager_CurrentItemChanged(object sender, EventArgs e)
		{
			SelectOnlyOneGuarantee();
			SetConfirmButtonReadOnly();
		}

		void SetConfirmButtonReadOnly()
		{
			ConfirmButton.ReadOnly = BusinessEntity != null && ((GuaranteeAccessCodesUserSelectionObjectCollection)BusinessEntity).OfType<GuaranteeAccessCodesUserSelectionObject>().Count(guarantee => guarantee.IsSelected) != 1;
		}

		void SelectOnlyOneGuarantee()
		{
			if (GuaranteesGrid.ListManager?.GetCurrent() is GuaranteeAccessCodesUserSelectionObject guaranteeCurrent && guaranteeCurrent.IsSelected)
			{
				foreach (GuaranteeAccessCodesUserSelectionObject guarantee in (GuaranteeAccessCodesUserSelectionObjectCollection)BusinessEntity)
				{
					if (guarantee.PK != guaranteeCurrent.PK)
					{
						guarantee.IsSelected = false;
					}
				}
			}
			GuaranteesGrid.Refresh();
		}

		public override string FormHeading => Res.GetString("E41E8BC7-AD16-41F5-9A8F-6E64390B41C8", "Guarantee Access Codes");

		void ConfirmButton_Click(object sender, EventArgs e)
		{
			if (GuaranteesGrid.ListManager?.GetCurrent() is GuaranteeAccessCodesUserSelectionObject selectionObject && selectionObject.IsSelected)
			{
				ConfirmButton_ClickCore(selectionObject.Guarantee.CusGuarantee);
			}
		}

		protected virtual void ConfirmButton_ClickCore(CusGuaranteeHeader guarantee)
		{
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
