using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI;

public abstract class AdditionalTabPageUserControlBase : ManifestSpecificProviderUserControl
{
	public ZUserControl CurrentControl { get; set; }
}
