using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CountryDefaultLanguageRegistryDataType))]
	sealed class CountryDefaultLanguageRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CountryDefaultLanguageRegistryDataType>
	{
		protected override CountryDefaultLanguageRegistryDataType GetNewDataType()
		{
			return new CountryDefaultLanguageRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var firstCollection = new CountryDefaultLanguageBusinessObjectCollection();
			var bizoA = firstCollection.AddNew();
			var country1 = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.China));
			bizoA.CountryPk = country1.PK;
			bizoA.DefaultLanguage = Core.SharedConstants.Languages.ChineseSimplified;

			var secondCollection = new CountryDefaultLanguageBusinessObjectCollection();
			var country2 = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.China));
			var bizoB = secondCollection.AddNew();
			bizoB.CountryPk = country2.PK;
			bizoB.DefaultLanguage = Core.SharedConstants.Languages.EnglishAmerican;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(firstCollection, new CountryDefaultLanguageRegistryDataType().Serialise(firstCollection)),
				new ValidSampleAndBinaryValueInDB(secondCollection, new CountryDefaultLanguageRegistryDataType().Serialise(secondCollection))
			};
		}

		protected override string ExpectedEditorName => "CountryDefaultLanguageRegistryItemEditor";

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
