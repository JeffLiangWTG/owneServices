using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(NationalAdditionalCodeHelper))]
	class NationalAdditionalCodeHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadWithOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = new NationalAdditionalCodeCollection(invoiceLine.JI_NationalAdditionalCodesInfo, 8);
			var code = collection.AddNew();

			var count = 0;
			invoiceLine.JI_DescriptionInfo.ValueChanged += (s, e) =>
			{
				if (e is ValueChangedEventArgs ve)
				{
					count += ve.OldValue.Equals("S001") ? 2 : 1;
				}
			};

			NationalAdditionalCodeHelper.LoadOrCreate("S001", invoiceLine, 1, invoiceLine.JI_DescriptionInfo, code);

			var loadedCode = NationalAdditionalCodeHelper.LoadWithOrder(invoiceLine, 1);
			NUnit.Framework.Assert.That(loadedCode, NUnit.Framework.Is.Not.EqualTo(default(NationalAdditionalCode)), "Code should not be null - should not be [null]");

			CombineAssertions("Testing NationalAdditionalCode property", () =>
			{
				NUnit.Framework.Assert.That(loadedCode.CY_Code, NUnit.Framework.Is.EqualTo("S001").Using(CustomComparers.TypeComparison), "CY_Code");
				NUnit.Framework.Assert.That(invoiceLine.PK, NUnit.Framework.Is.EqualTo(loadedCode.CY_ParentID), "CY_ParentID");
				NUnit.Framework.Assert.That(invoiceLine.TablePrefix, NUnit.Framework.Is.EqualTo(loadedCode.CY_ParentTableCode).Using(CustomComparers.TypeComparison), "CY_ParentTableCode");
			});

			NUnit.Framework.Assert.That(count, NUnit.Framework.Is.EqualTo(1), "refreshBinding Called on JI_DescriptionInfo");

			NationalAdditionalCodeHelper.LoadOrCreate("S002", invoiceLine, 1, invoiceLine.JI_DescriptionInfo, code);
			NUnit.Framework.Assert.That(loadedCode.CY_Code, NUnit.Framework.Is.EqualTo("S002").Using(CustomComparers.TypeComparison), "CY_Code");
			NUnit.Framework.Assert.That(count, NUnit.Framework.Is.EqualTo(3), "refreshBinding Called on JI_DescriptionInfo");

			NationalAdditionalCodeHelper.LoadOrCreate("", invoiceLine, 1, invoiceLine.JI_DescriptionInfo, code);
			NUnit.Framework.Assert.That(count, NUnit.Framework.Is.EqualTo(4), "refreshBinding Called on JI_DescriptionInfo");
		}
	}
}
