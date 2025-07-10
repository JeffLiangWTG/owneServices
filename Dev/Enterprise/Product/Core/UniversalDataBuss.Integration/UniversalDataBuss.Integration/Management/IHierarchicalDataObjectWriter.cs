
namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IHierarchicalDataObjectWriter
	{
		/// <summary>
		///		Gets or sets a flag indicating whether to include parent into data object.
		/// </summary>
		bool IncludeParent { get; set; }

		/// <summary>
		///		Gets or sets a flag indicating whether to include сhildren into data object.
		/// </summary>
		bool IncludeChildren { get; set; }
	}
}
