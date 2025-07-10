using CargoWise.Integration;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IMergeByListReflectionRetriever
		{
			ICodeDescriptionPairList GetMergeByListByCountryCode(string countryCode);

			ICodeDescriptionPairList GetMergeByListByCompanyCode(string companyCode);
		}
	}
}

