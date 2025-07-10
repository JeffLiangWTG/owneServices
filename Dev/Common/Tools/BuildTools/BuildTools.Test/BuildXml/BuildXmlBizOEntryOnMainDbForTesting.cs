namespace CargoWise.BuildTools.Testing
{
	sealed class BuildXmlBizOEntryOnMainDbForTesting : BuildXmlBizOEntry
	{
		public BuildXmlBizOEntryOnMainDbForTesting(string tableName, string solutionName, bool masterFileReference, bool preventDelete, bool convertZStringToWesternEuropeanCharacters = false)
			: base(null, null, tableName, solutionName, masterFileReference, preventDelete, convertZStringToWesternEuropeanCharacters)
		{
		}
	}
}
