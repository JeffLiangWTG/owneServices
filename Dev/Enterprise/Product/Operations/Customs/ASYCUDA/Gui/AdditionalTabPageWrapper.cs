using System;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI;

public class AdditionalTabPageWrapper
{
	public ZTabPage NewTabPage;
	public AdditionalTabPageVisibility Visibility;
	public AsycudaManifestHeader Header;

	public AdditionalTabPageWrapper(ZTabPage newTabPage, AdditionalTabPageVisibility visibility, AsycudaManifestHeader header)
	{
		this.NewTabPage = newTabPage;
		this.Visibility = visibility;
		this.Header = header;
	}

	public void UpdateVisibility(object sender, EventArgs e)
	{
		NewTabPage.TabVisible = Visibility.isVisible?.Invoke(Header) ?? false;
	}
}
