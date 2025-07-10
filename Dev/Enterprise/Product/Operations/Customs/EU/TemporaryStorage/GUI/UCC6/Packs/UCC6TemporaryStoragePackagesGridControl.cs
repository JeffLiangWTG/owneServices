using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStoragePackagesGridControl : ZUserControl
	{
		public UCC6TemporaryStoragePackagesGridControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (Header != null)
			{
				Header.AMA_MessageTypeInfo.ValueChanged -= TemporaryStorageHeader_AMA_MessageTypeChanged;
				Header.AMA_MessageTypeInfo.ValueChanged += TemporaryStorageHeader_AMA_MessageTypeChanged;
				TemporaryStorageHeader_AMA_MessageTypeChanged(this, EventArgs.Empty);
			}
		}

		protected virtual void TemporaryStorageHeader_AMA_MessageTypeChanged(object sender, EventArgs args)
		{
			if (Header != null)
			{
				GridPacks.SetAvailability(!Header.IsTransfer, nameof(TemporaryStoragePack.ContainerPK));
				GridPacks.SetAvailability(!Header.IsTransfer, AsycudaPackSchema.Constants.APA_MarksAndNumbers);
			}
		}
		protected override void Dispose(bool disposing)
		{
			if (Header != null)
			{
				Header.AMA_MessageTypeInfo.ValueChanged -= TemporaryStorageHeader_AMA_MessageTypeChanged;
			}
			base.Dispose(disposing);
		}

		TemporaryStorageHeader Header => CurrentDataItem as TemporaryStorageHeader;
	}
}
