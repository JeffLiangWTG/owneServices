using System;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class ContainerCountrySpecificUserControl : ZUserControl
	{
		protected ContainerCountrySpecificUserControl()
		{
			InitializeComponent();
		}

		public new AsycudaContainer CurrentDataItem => (AsycudaContainer)base.CurrentDataItem;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (CurrentHeader != null)
			{
				UnHookManifestHeaderEvents(CurrentHeader);
				CurrentHeader = null;
			}
			if (dataSource is AsycudaManifestHeader header)
			{
				CurrentHeader = header;
				dataMember = "Containers";
			}
			base.SetDataBinding(dataSource, dataMember);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var currentContainer = CurrentDataItem;
			if (currentContainer != null)
			{
				UnHookContainerEvents(currentContainer);
			}
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var containerItem = CurrentDataItem;
			if (containerItem != null && !containerItem.IsDeleted)
			{
				UnHookContainerEvents(containerItem);
				HookContainerEvents(containerItem);
			}
			ManifestTypeInfo_ValueChanged(null, null);
		}

		protected AsycudaManifestHeader CurrentHeader { get; private set; }

		protected virtual void ManifestTypeInfo_ValueChanged(object sender, EventArgs e)
		{
		}

		protected virtual void UnHookManifestHeaderEvents(AsycudaManifestHeader header)
		{
			header.AMA_ManifestTypeInfo.ValueChanged -= ManifestTypeInfo_ValueChanged;
		}

		protected virtual void HookManifestHeaderEvents(AsycudaManifestHeader header)
		{
			header.AMA_ManifestTypeInfo.ValueChanged += ManifestTypeInfo_ValueChanged;
		}

		protected virtual void UnHookContainerEvents(AsycudaContainer containerItem)
		{
		}

		protected virtual void HookContainerEvents(AsycudaContainer containerItem)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var containerItem = CurrentDataItem;
				if (containerItem != null)
				{
					UnHookContainerEvents(containerItem);
				}
				if (CurrentHeader != null)
				{
					UnHookManifestHeaderEvents(CurrentHeader);
					CurrentHeader = null;
				}
			}
			base.Dispose(disposing);
		}
	}
}
