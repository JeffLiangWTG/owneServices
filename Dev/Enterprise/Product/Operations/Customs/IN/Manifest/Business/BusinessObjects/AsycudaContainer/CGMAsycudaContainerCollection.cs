using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IN.Manifest.Business;

public class CGMAsycudaContainerCollection : AsycudaContainerCollection<CGMAsycudaContainer, CGMAsycudaManifestHeader>
{
	public CGMAsycudaContainerCollection(CGMAsycudaManifestHeader master) : base(master)
	{ }

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);

		if (Count > 0)
		{
			using (child.GetValidationSuspender())
			{
				var container = (CGMAsycudaContainer)child;
				SetDefaultContainerAgentCode(container);
			}
		}
	}

	void SetDefaultContainerAgentCode(CGMAsycudaContainer container)
	{
		var previousContainer = this[Count - 1];
		container.ContainerAgentCode = previousContainer.ContainerAgentCode;
	}
}
