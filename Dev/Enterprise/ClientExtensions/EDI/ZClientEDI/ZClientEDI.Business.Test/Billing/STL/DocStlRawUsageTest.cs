using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(DocStlRawUsage))]
	internal class DocStlRawUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			var docUsage = GetNewBusinessObject() as DocStlRawUsage;
			AssertNotNull(docUsage.UsageLines1);
			AssertNotNull(docUsage.UsageLines2);
			AssertNotNull(docUsage.UsageLines3);
			AssertNotNull(docUsage.UsageLines4);
			AssertNotNull(docUsage.UsageLines5);
			AssertNotNull(docUsage.UsageLines6);
			AssertNotNull(docUsage.UsageLines7);
			AssertNotNull(docUsage.UsageLines8);
			AssertNotNull(docUsage.UsageLines9);
			AssertNotNull(docUsage.UsageLines10);
		}

		public void TestAddUsageSummarySections()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 9, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = new RawStlUsageForTesting(context);
			rawUsage.Section.Lines.AddNew();
			rawUsage.Section.Lines.AddNew();

			var docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 0, 0, 0, 0, 0);

			rawUsage.Section.Header.Column1 = "hello";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 2, 0, 0, 0, 0, 0);

			rawUsage.Section.Header.Column2 = "world";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 2, 0, 0, 0, 0);

			rawUsage.Section.Header.Column3 = "I'm out";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 0, 2, 0, 0, 0);

			rawUsage.Section.Header.Column4 = "of";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 0, 0, 2, 0, 0);

			rawUsage.Section.Header.Column5 = "words *cry*";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 0, 0, 0, 2, 0);

			rawUsage.Section.Header.Column6 = "------------";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 0, 0, 0, 0, 2);

			rawUsage.Section.Header.Column7 = "Column7";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 0, 0, 0, 0, 0, 2);

			rawUsage.Section.Header.Column8 = "Column8";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 0, 0, 0, 0, 0, 0, 2);

			rawUsage.Section.Header.Column9 = "Column9";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 0, 0, 0, 0, 0, 0, 0, 2);

			rawUsage.Section.Header.Column10 = "Column10";
			docUsage = DocStlRawUsage.New(rawUsage, Factory);
			AssertUsageLines(docUsage, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
		}

		void AssertUsageLines(DocStlRawUsage docUsage, int count1, int count2, int count3, int count4, int count5, int count6, int count7 = 0, int count8 = 0, int count9 = 0, int count10 = 0)
		{
			AssertEquals(count1, docUsage.UsageLines1.Count);
			AssertEquals(count2, docUsage.UsageLines2.Count);
			AssertEquals(count3, docUsage.UsageLines3.Count);
			AssertEquals(count4, docUsage.UsageLines4.Count);
			AssertEquals(count5, docUsage.UsageLines5.Count);
			AssertEquals(count6, docUsage.UsageLines6.Count);
			AssertEquals(count7, docUsage.UsageLines7.Count);
			AssertEquals(count8, docUsage.UsageLines8.Count);
			AssertEquals(count9, docUsage.UsageLines9.Count);
			AssertEquals(count10, docUsage.UsageLines10.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 9, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var dummyUsage = new DummyStlRawUsage(context);

			return DocStlRawUsage.New(dummyUsage, Factory);
		}

		class RawStlUsageForTesting : DummyStlRawUsage
		{
			public RawStlUsageForTesting(BillingLoadRawUsageContext context)
				: base(context)
			{
				Section = new SummarySection(Factory);
			}

			public override SummarySection[] GetRawUsageSummarySections()
			{
				return new SummarySection[] { Section };
			}

			public SummarySection Section;
		}

		#endregion
	}
}
