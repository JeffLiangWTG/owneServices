using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.ASYCUDA.GUI;

public class AdditionalTabPageVisibility
{
	public Func<AsycudaManifestHeader, bool> isVisible;
	public Func<AsycudaManifestHeader, ZPropertyInfo>[] dependencies;

	public AdditionalTabPageVisibility(Func<AsycudaManifestHeader, bool> isVisible, params Func<AsycudaManifestHeader, ZPropertyInfo>[] dependencies)
	{
		this.isVisible = isVisible;
		this.dependencies = dependencies;
	}
}
