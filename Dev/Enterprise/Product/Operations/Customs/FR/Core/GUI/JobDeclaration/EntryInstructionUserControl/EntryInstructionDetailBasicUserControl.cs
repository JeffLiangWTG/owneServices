using System;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class EntryInstructionDetailBasicUserControl : ZUserControl
	{
		public EntryInstructionDetailBasicUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (declaration != null)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged += SetLocationOfGoodsVisibility;
				declaration.JE_MessageTypeInfo.ValueChanged += SetLocationOfGoodsVisibility;
				LocationOfGoodsPanel.Visible = declaration.IsUCC6AndIsImport;
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (declaration != null)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged -= SetLocationOfGoodsVisibility;
				declaration.JE_MessageTypeInfo.ValueChanged -= SetLocationOfGoodsVisibility;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (declaration != null)
				{
					declaration.JE_ApplicationCodeInfo.ValueChanged -= SetLocationOfGoodsVisibility;
					declaration.JE_MessageTypeInfo.ValueChanged -= SetLocationOfGoodsVisibility;
					foreach (var instruction in declaration.CustomsEntryInstructions)
					{
						instruction.DisposeMutex();
					}
				}
			}

			base.Dispose(disposing);
		}

		void SetLocationOfGoodsVisibility(object sender, EventArgs e)
		{
			LocationOfGoodsPanel.Visible = declaration?.IsUCC6AndIsImport ?? false;
		}

		internal void HandleDeclarationControlVisibilityChangedCore(bool isVisible)
		{
			ValuationBypassCodeDropEdit.Visible = isVisible && !declaration.IsUCC6;
			ValuationBypassReasonTextBox.Visible = isVisible && !declaration.IsUCC6;
		}

		JobDeclaration declaration => (JobDeclaration)CurrentDataItem;
	}
}
