namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkScaleDescriptor
	{
		bool IsScaleDescriptorInvalidated { get; }
		bool IsBranchOrDepartmentInvalidated { get; }

		INetworkScaleSet GetScaleSetForColumns(int columnAmount);

		void ClearCachedData();
	}
}
