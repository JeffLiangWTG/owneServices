using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class AdditionalArrivalNotificationDetailsUserControl : ZUserControl, ISupportMultipleResourceStringDataSupporter, ISupportMultipleResourceStringData
	{
		public AdditionalArrivalNotificationDetailsUserControl()
		{
			InitializeComponent();
		}

		public ISupportMultipleResourceStringData SupportMultipleResourceStringData => this;

		public IReadOnlyList<string> MultipleKeysToUse => new[] { NctsHeader.Phase5CaptionKey };

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			TIRPartialUnloadingCheckBox.DataBindings.RemoveBinding(nameof(ZCheckBox.IsVisibleForBinding));
			TIRCarnetPageIntEdit.DataBindings.RemoveBinding(nameof(ZIntEdit.IsVisibleForBinding));

			if (dataSource != null)
			{
				var isShowTIRArrivalDetailsDataMemberName = $"{dataMember}.{nameof(NctsHeader.ESNctsHeader)}.{nameof(CusESNctsHeader.IsShowTIRArrivalDetails)}";
				TIRPartialUnloadingCheckBox.DataBindings.Add(new KBinding(nameof(ZCheckBox.IsVisibleForBinding), BindingSource.DataSource, isShowTIRArrivalDetailsDataMemberName, false, DataSourceUpdateMode.Never));
				TIRCarnetPageIntEdit.DataBindings.Add(new KBinding(nameof(ZIntEdit.IsVisibleForBinding), BindingSource.DataSource, isShowTIRArrivalDetailsDataMemberName, false, DataSourceUpdateMode.Never));
			}
		}
	}
}	
