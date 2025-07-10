namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public class NctsMessageGeneratorFactoryChooser : NctsChooser<INctsMessageGeneratorFactory>
	{
		readonly NctsHeader header;

		public NctsMessageGeneratorFactoryChooser(NctsHeader header, NctsMessageFunctionSet messageFunction)
			: base(typeof(INctsMessageGeneratorFactory).FullName, GetNctsCountryFromNctsHeader(header, messageFunction))
		{
			this.header = header;
		}

		public INctsMessageGeneratorFactory GeneratorFactory
		{
			get => DictionaryOfAvailableChoosers.TryGetValue(header.CountryCode, out var generatorFactory)
				? generatorFactory
				: null;
		}
	}
}
