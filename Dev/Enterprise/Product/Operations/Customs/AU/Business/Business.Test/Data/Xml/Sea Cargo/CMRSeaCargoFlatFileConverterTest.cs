namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRSeaCargoFlatFileConverterTest : SeaCargoFlatFileConverterTest
	{
		protected override SeaCargoFlatFileDataImporter GetNewImporter() => new CMRSeaCargoFlatFileDataImporter();
	}
}
