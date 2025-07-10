using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class CusEntryNumSearchCriteriaTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestParse()
		{
			CusEntryNumSearchCriteria searchCriteria = new CusEntryNumSearchCriteria(ZString.Empty);
			NUnit.Framework.Assert.That(searchCriteria.EntryType, Is.EqualTo(string.Empty));
			NUnit.Framework.Assert.That(searchCriteria.Country, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(searchCriteria.Category, Is.EqualTo(default(string)));

			searchCriteria = new CusEntryNumSearchCriteria("AAA");
			NUnit.Framework.Assert.That(searchCriteria.EntryType, Is.EqualTo("AAA"));
			NUnit.Framework.Assert.That(searchCriteria.Country, Is.EqualTo(default(string)));
			NUnit.Framework.Assert.That(searchCriteria.Category, Is.EqualTo(default(string)));

			searchCriteria = new CusEntryNumSearchCriteria("AU:AAA");
			NUnit.Framework.Assert.That(searchCriteria.EntryType, Is.EqualTo("AAA"));
			NUnit.Framework.Assert.That(searchCriteria.Country, Is.EqualTo("AU"));
			NUnit.Framework.Assert.That(searchCriteria.Category, Is.EqualTo(default(string)), "string.Empty - should be [null]");

			searchCriteria = new CusEntryNumSearchCriteria("OTH:AU:AAA");
			NUnit.Framework.Assert.That(searchCriteria.EntryType, Is.EqualTo("AAA"));
			NUnit.Framework.Assert.That(searchCriteria.Country, Is.EqualTo("AU"));
			NUnit.Framework.Assert.That(searchCriteria.Category, Is.EqualTo("OTH"));
		}

		[ExpectNoExceptions]
		public void TestFind()
		{
			List<CusEntryNumber> cusEntryNumbers = new List<CusEntryNumber>();

			CusEntryNumber number1 = Factory.New<CusEntryNumber>();
			number1.CE_EntryType = "AAA";
			number1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			number1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			cusEntryNumbers.Add(number1);

			CusEntryNumber number2 = Factory.New<CusEntryNumber>();
			number2.CE_EntryType = "BBB";
			number2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Zimbabwe;
			cusEntryNumbers.Add(number2);

			CusEntryNumSearchCriteria searchCriteria = new CusEntryNumSearchCriteria("AAA");
			NUnit.Framework.Assert.That(searchCriteria.Find(null), Is.EqualTo(default(ISearcheableCusEntryNumber)));
			NUnit.Framework.Assert.That(searchCriteria.Find(Array.Empty<CusEntryNumber>()), Is.EqualTo(default(ISearcheableCusEntryNumber)));
			NUnit.Framework.Assert.That(searchCriteria.Find(cusEntryNumbers.ToArray()), Is.EqualTo(number1).Using(CustomComparers.TypeComparison));

			searchCriteria = new CusEntryNumSearchCriteria("AU:AAA");
			NUnit.Framework.Assert.That(searchCriteria.Find(cusEntryNumbers.ToArray()), Is.EqualTo(number1).Using(CustomComparers.TypeComparison));

			searchCriteria = new CusEntryNumSearchCriteria("ZW:AAA");
			NUnit.Framework.Assert.That(searchCriteria.Find(cusEntryNumbers.ToArray()), Is.EqualTo(default(ISearcheableCusEntryNumber)));

			searchCriteria = new CusEntryNumSearchCriteria("OTH:AU:AAA");
			NUnit.Framework.Assert.That(searchCriteria.Find(cusEntryNumbers.ToArray()), Is.EqualTo(number1).Using(CustomComparers.TypeComparison));

			searchCriteria = new CusEntryNumSearchCriteria("CUS:AU:AAA");
			NUnit.Framework.Assert.That(searchCriteria.Find(cusEntryNumbers.ToArray()), Is.EqualTo(default(ISearcheableCusEntryNumber)));

			searchCriteria = new CusEntryNumSearchCriteria("CUS:ZW:BBB");
			NUnit.Framework.Assert.That(searchCriteria.Find(cusEntryNumbers.ToArray()), Is.EqualTo(number2).Using(CustomComparers.TypeComparison));

			searchCriteria = new CusEntryNumSearchCriteria("ZW:BBB");
			NUnit.Framework.Assert.That(searchCriteria.Find(cusEntryNumbers.ToArray()), Is.EqualTo(number2).Using(CustomComparers.TypeComparison));

			searchCriteria = new CusEntryNumSearchCriteria("BBB");
			NUnit.Framework.Assert.That(searchCriteria.Find(cusEntryNumbers.ToArray()), Is.EqualTo(number2).Using(CustomComparers.TypeComparison));

			searchCriteria = new CusEntryNumSearchCriteria("CCC");
			NUnit.Framework.Assert.That(searchCriteria.Find(cusEntryNumbers.ToArray()), Is.EqualTo(default(ISearcheableCusEntryNumber)));
		}
	}
}
