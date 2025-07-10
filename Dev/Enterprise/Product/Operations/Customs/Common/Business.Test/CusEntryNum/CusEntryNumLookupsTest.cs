using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class CusEntryNumLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestChinaAdditionalReferencesAreTranslatable()
		{
			NUnit.Framework.Assert.That(typeof(MultilingualString).IsAssignableFrom(ChinaAdditionalReferenceNumberTypes.Descriptions.ShippingOrderNumber.GetType()), Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestGetAdditionalReferenceNumberTypesHaveNoDuplicates()
		{
			AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(Core.Constants.CountryCodes.China, ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber, ChinaAdditionalReferenceNumberTypes.Descriptions.ShippingOrderNumber);

			AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(Core.Constants.CountryCodes.UnitedStates, UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, UnitedStatesAdditionalReferenceNumberTypes.Descriptions.IT);

			AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(Core.Constants.CountryCodes.UnitedStates, UnitedStatesAdditionalReferenceNumberTypes.Codes.RRN, UnitedStatesAdditionalReferenceNumberTypes.Descriptions.RRN);

			AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(Core.Constants.CountryCodes.UnitedArabEmirates, UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.BillOfEntryNumber, UnitedArabEmiratesAdditionalReferenceNumberTypes.Descriptions.BillOfEntryNumber);

			AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(Core.Constants.CountryCodes.UnitedArabEmirates, UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.DeliveryOrderNumber, UnitedArabEmiratesAdditionalReferenceNumberTypes.Descriptions.DeliveryOrderNumber);

			AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(Core.Constants.CountryCodes.UnitedArabEmirates, UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.ManifestRegistrationNumber, UnitedArabEmiratesAdditionalReferenceNumberTypes.Descriptions.ManifestRegistrationNumber);

			AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(Core.Constants.CountryCodes.UnitedArabEmirates, UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.NoObjectionsCertificateNumber, UnitedArabEmiratesAdditionalReferenceNumberTypes.Descriptions.NoObjectionsCertificateNumber);

			AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(Core.Constants.CountryCodes.UnitedArabEmirates, UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.UAEInstalmentNumber, UnitedArabEmiratesAdditionalReferenceNumberTypes.Descriptions.UAEInstalmentNumber);

			AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(Core.Constants.CountryCodes.UnitedArabEmirates, UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.RotationNumber, UnitedArabEmiratesAdditionalReferenceNumberTypes.Descriptions.RotationNumber);
		}

		[ExpectNoExceptions]
		void AssertGeneralGetAdditionalReferenceNumberTypesHaveNoDuplicates(ZString countryCode, string refNumTypeCode, string refNumTypeDescrip)
		{
			var customsReferenceNumberTypeCollection = new CustomsReferenceNumberTypeCollection();
			customsReferenceNumberTypeCollection.Add(refNumTypeCode, (NoResString)"Testing Description");

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customsReferenceNumberTypeCollection);

			var checkedList = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(countryCode);
			var filteredList = checkedList.Cast<ICodeDescription>().Where(pair => pair.Code == refNumTypeCode);

			NUnit.Framework.Assert.That(filteredList.Count(), Is.EqualTo(1));
			NUnit.Framework.Assert.That(filteredList.First().Description, Is.EqualTo(refNumTypeDescrip));
		}

		[ExpectNoExceptions]
		public void TestIMRAndHIRAreNotUsedAsCodes()
		{
			var num = Factory.New<CusEntryNumber>();

			num.CE_RN_NKCountryCode = "IS";
			AssertListDoesntUseCode(CusEntryNumLookups.IMR, num.Lookups.AdditionalReferenceNumberTypes);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertListDoesntUseCode(CusEntryNumLookups.IMR, num.Lookups.AdditionalReferenceNumberTypes);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertListDoesntUseCode(CusEntryNumLookups.IMR, num.Lookups.AdditionalReferenceNumberTypes);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
			AssertListDoesntUseCode(CusEntryNumLookups.IMR, num.Lookups.AdditionalReferenceNumberTypes);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			AssertListDoesntUseCode(CusEntryNumLookups.IMR, num.Lookups.AdditionalReferenceNumberTypes);

			num.CE_RN_NKCountryCode = ZString.Empty;
			AssertListDoesntUseCode(CusEntryNumLookups.IMR, num.Lookups.AdditionalReferenceNumberTypes);
		}

		[ExpectNoExceptions]
		void AssertListDoesntUseCode(string code, CodeDescriptionPairList list)
		{
			foreach (ICodeDescription pair in list)
			{
				NUnit.Framework.Assert.That(pair.Code, Is.Not.EqualTo(code), code + " shouldn't be used in AdditionalReferenceNumberTypes");
			}
		}

		[ExpectNoExceptions]
		public void TestAdditionalReferenceNumberTypes()
		{
			int defaultValuesCount = FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value.Count;

			CusEntryNumber num = Factory.New<CusEntryNumber>();
			var num2 = Factory.New<CusEntryNumber>();

			num.CE_RN_NKCountryCode = "IS";
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new IcelandAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = "IS";
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for IS");

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new UnitedStatesAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.UnitedStates);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.Australia);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new UnitedArabEmiratesAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.UnitedArabEmirates);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new CanadaAdditionalReferenceNumberTypes().Count + defaultValuesCount - 2), "PCN, CCN already added to default registry");
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.Canada);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new ChinaAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.China);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new ChinaAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.HongKong);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new ChinaAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.Taiwan);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new GermanyAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.Germany);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new ItalyAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.Italy);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new BrazilAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.Brazil);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(new FranceAdditionalReferenceNumberTypes().Count + defaultValuesCount));
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for " + Core.Constants.CountryCodes.France);

			num.CE_RN_NKCountryCode = ZString.Empty;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(defaultValuesCount));
			num2.CE_RN_NKCountryCode = ZString.Empty;
			NUnit.Framework.Assert.That(num2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(num.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached for Empty");
		}

		[ExpectNoExceptions]
		public void TestAdditionalReferenceNumberTypes_EconomicGroupingList()
		{
			int defaultValuesCount = FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value.Count;

			CusEntryNumber num = Factory.New<CusEntryNumber>();
			DummyWithAdditionalReferenceNumbers dummy = Factory.New<DummyWithAdditionalReferenceNumbers>();
			CusEntryNumber numWithSpecialCustomsItems = CusEntryNumber.New(dummy, ZString.Empty, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			num.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Austria;
			NUnit.Framework.Assert.That(num.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(defaultValuesCount));

			numWithSpecialCustomsItems.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Austria;
			NUnit.Framework.Assert.That(numWithSpecialCustomsItems.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(CusEntryNumberTypes.EU.EUCustomsEntryTypeList.Count + defaultValuesCount));

			var dummy2 = Factory.New<DummyWithAdditionalReferenceNumbers>();
			var numWithSpecialCustomsItems2 = CusEntryNumber.New(dummy2, ZString.Empty, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			numWithSpecialCustomsItems2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Austria;
			NUnit.Framework.Assert.That(numWithSpecialCustomsItems2.Lookups.AdditionalReferenceNumberTypes, Is.EqualTo(numWithSpecialCustomsItems.Lookups.AdditionalReferenceNumberTypes), "AdditionalReferenceNumberTypes should be cached");
		}

		[ExpectNoExceptions]
		public void TestAdditionalReferenceNumberTypes_ParentIsDeleted()
		{
			var baseNum = Factory.New<CusEntryNumber>();
			baseNum.CE_ParentTable = "CusEntryHeader";
			Factory.Save();

			var testNum = NewFactory().Load<CusEntryNumber>(baseNum.PK);
			testNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Austria;

			NUnit.Framework.Assert.That(!testNum.IsDeleted, Is.True, "CusEntryNumber is not deleted");
			NUnit.Framework.Assert.That(testNum.Lookups.AdditionalReferenceNumberTypes.Count, Is.GreaterThan(0), "AdditionalReferenceNumberTypes has items");

			testNum.Delete();
			NUnit.Framework.Assert.That(testNum.IsDeleted, Is.True, "CusEntryNumber is deleted");
			NUnit.Framework.Assert.That(testNum.Lookups.AdditionalReferenceNumberTypes.Count, Is.EqualTo(0), "AdditionalReferenceNumberTypes has no item");
		}

		#region CountriesAsCodeDescriptionList

		[ExpectNoExceptions]
		public void TestCountriesAsCodeDescriptionList_ParentIsDeleted_ReturnsEmptyList()
		{
			var testNum = Factory.New<CusEntryNumber>();
			testNum.Delete();

			NUnit.Framework.Assert.That(testNum.IsDeleted, Is.True, "CusEntryNumber is deleted");
			NUnit.Framework.Assert.That(testNum.Lookups.CountriesAsCodeDescriptionList.Count, Is.EqualTo(0), "CountriesAsCodeDescriptionList is Empty");
		}

		[ExpectNoExceptions]
		public void TestCountriesAsCodeDescriptionList_ParentIsNotNullAndNotDeleted_ReturnsListOfCountries()
		{
			var testNum = Factory.New<CusEntryNumber>();

			NUnit.Framework.Assert.That(testNum.Lookups.CountriesAsCodeDescriptionList.Count, Is.EqualTo(testNum.Lookups.Countries.Count), "CountriesAsCodeDescriptionList has List");
		}

		#endregion

		#region Test Classes

		public class DummyWithAdditionalReferenceNumbers : DummyBusinessObject, IAdditionalReferenceNumberSupporter
		{
			public DummyWithAdditionalReferenceNumbers(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, CargoWise.ComponentModel.INotifications notify)
			{
			}

			bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
			{
				get { return true; }
			}

			void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
			{
			}

			CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
			{
				get { throw new NotImplementedException(); }
			}

			void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
			{
			}
		}

		#endregion

		#region Test IAdditionalReferenceNumberTypeProvider
		[ExpectNoExceptions]
		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var dummy = Factory.New<DummyWithAdditionalReferenceNumberTypeProvider>();
			var number = CusEntryNumber.New(dummy, ZString.Empty, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			number.CE_Category = ZString.Empty;
			number.CE_RN_NKCountryCode = ZString.Empty;
			var additionalReferenceNumberTypes = number.Lookups.AdditionalReferenceNumberTypes;
			NUnit.Framework.Assert.That(additionalReferenceNumberTypes.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(additionalReferenceNumberTypes.GetDescriptionFromCode("ALOHA"), Is.EqualTo("ALOHA WORLD"));
			NUnit.Framework.Assert.That(additionalReferenceNumberTypes.GetDescriptionFromCode("CIAO"), Is.EqualTo("CIAO WORLD"));
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			additionalReferenceNumberTypes = number.Lookups.AdditionalReferenceNumberTypes;
			NUnit.Framework.Assert.That(additionalReferenceNumberTypes.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(additionalReferenceNumberTypes.GetDescriptionFromCode("GDAY"), Is.EqualTo("GDAY WORLD"));
			NUnit.Framework.Assert.That(additionalReferenceNumberTypes.GetDescriptionFromCode("CYA"), Is.EqualTo("CYA WORLD"));
			number.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			additionalReferenceNumberTypes = number.Lookups.AdditionalReferenceNumberTypes;
			NUnit.Framework.Assert.That(additionalReferenceNumberTypes.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(additionalReferenceNumberTypes.GetDescriptionFromCode("HI"), Is.EqualTo("HELLO WORLD"));
			NUnit.Framework.Assert.That(additionalReferenceNumberTypes.GetDescriptionFromCode("BYE"), Is.EqualTo("GOODBYE WORLD"));
		}

		class DummyWithAdditionalReferenceNumberTypeProvider : DummyBusinessObject, IAdditionalReferenceNumberTypeProvider
		{
			public DummyWithAdditionalReferenceNumberTypeProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IAdditionalReferenceNumberTypeProvider Members

			public CodeDescriptionPairList GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
			{
				var result = new CodeDescriptionPairList();
				if (category == CusEntryNumber.Categories.AdditionalReferenceNumber)
				{
					result.AddPair("HI", "HELLO WORLD");
					result.AddPair("BYE", "GOODBYE WORLD");
				}
				else if (countryCode == Core.Constants.CountryCodes.Australia)
				{
					result.AddPair("GDAY", "GDAY WORLD");
					result.AddPair("CYA", "CYA WORLD");
				}
				else
				{
					result.AddPair("ALOHA", "ALOHA WORLD");
					result.AddPair("CIAO", "CIAO WORLD");
				}
				return result;
			}

			#endregion
		}
		#endregion
	}
}
