using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Query that loads only StorageMains from the current country that we are in
	/// </summary>
	public class CountrySpecificRefTypesQuery : ZQuery
	{
		public CountrySpecificRefTypesQuery()
		{
			AddToFilter(StorageMainSchema.SM_Type, AssemblyDataLookup.GetDocManagerCodes());
		}
	}
}
