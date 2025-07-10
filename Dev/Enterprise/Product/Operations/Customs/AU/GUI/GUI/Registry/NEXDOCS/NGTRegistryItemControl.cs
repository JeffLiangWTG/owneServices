using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class NGTRegistryItemControl : RegistryZUserControl
	{
		public NGTRegistryItemControl()
		{
			InitializeComponent();
		}

		public new NGT CurrentDataItem => (NGT)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetBusinessEntityReadOnly();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			SetBusinessEntityReadOnly();
		}

		void SetBusinessEntityReadOnly()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.ReadOnly = ReadOnly;
			}
		}
	}
}
