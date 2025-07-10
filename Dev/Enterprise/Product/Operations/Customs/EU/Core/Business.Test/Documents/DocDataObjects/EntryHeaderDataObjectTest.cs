using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(EntryHeaderDataObject))]
	public class EntryHeaderDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new EntryHeaderDataObject(entryHeader);
		}

		void TrySetProperty(object obj, string property, object value)
		{
			var prop = obj.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
			if (prop != null && prop.CanWrite)
			{
				prop.SetValue(obj, value, null);
			}
		}

		object GetPropertyValue(object obj, string property)
		{
			Type t = obj.GetType();
			PropertyInfo prop = t.GetProperty(property);
			return prop.GetValue(obj);
		}

		[ExpectNoExceptions]
		void TestValueIndicator(string property, string testProperty, bool useBool)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			TrySetProperty(invoice1, property, useBool ? ZBool.True : (ZString)"Y");
			invoice1.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];

			var dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That((ZBool)GetPropertyValue(dataObject, testProperty), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			TrySetProperty(invoice1, property, useBool ? ZBool.False : (ZString)"N");
			dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That(!(ZBool)GetPropertyValue(dataObject, testProperty), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice2.InvoiceLines.AddNew();
			TrySetProperty(invoice1, property, useBool ? ZBool.True : (ZString)"Y");
			TrySetProperty(invoice2, property, useBool ? ZBool.True : (ZString)"Y");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That((ZBool)GetPropertyValue(dataObject, testProperty), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			TrySetProperty(invoice1, property, useBool ? ZBool.False : (ZString)"N");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = (CusEntryHeader)invoice1.FirstEntryHeader;

			dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That(!(ZBool)GetPropertyValue(dataObject, testProperty), NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestHasRelatedParty()
		{
			TestValueIndicator(JobComInvoiceHeader.Schema.JZ_RelatedIndicator, "HasRelatedParty", false);
		}

		[ExpectNoExceptions]
		public void TestHasRestrictions()
		{
			TestValueIndicator(JobComInvoiceHeader.Schema.ZG_RelatedIndicator2, "HasRestrictions", false);
		}

		[ExpectNoExceptions]
		public void TestHasSaleConditions()
		{
			TestValueIndicator(JobComInvoiceHeader.Schema.ZG_RelatedIndicator3, "HasSaleConditions", false);
		}

		[ExpectNoExceptions]
		public void TestHasDisposal()
		{
			TestValueIndicator(JobComInvoiceHeader.Schema.ZG_RelatedIndicator4, "HasDisposal", false);
		}

		[TestDate(2019, 10, 18)]
		[ExpectNoExceptions]
		public void TestPlaceAndDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_BranchName = "Branch 1";
			declaration.JE_GB = branch.PK;
			var invoice1 = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That(dataObject.PlaceAndDate.Trim(), NUnit.Framework.Is.EqualTo("Branch 1 18.10.2019").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestHasRoyaltyCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var inv1Charge = invoice1.Charges.AddNew();
			inv1Charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var dataObject = new EntryHeaderDataObject(entry);

			NUnit.Framework.Assert.That(!dataObject.HasRoyaltyCharges, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			inv1Charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge;
			dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That(dataObject.HasRoyaltyCharges, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			var invLine1 = invoice1.InvoiceLines.AddNew();
			var invLine1Charge = invLine1.Charges.AddNew();
			invLine1Charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge;
			dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That(dataObject.HasRoyaltyCharges, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			inv1Charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge;
			dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That(dataObject.HasRoyaltyCharges, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			invLine1Charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge;
			dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That(!dataObject.HasRoyaltyCharges, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			var groupCharge = declaration.TopGroupInvoice.Charges.AddNew();
			groupCharge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge;
			dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That(dataObject.HasRoyaltyCharges, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			groupCharge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge;
			dataObject = new EntryHeaderDataObject(entry);
			NUnit.Framework.Assert.That(!dataObject.HasRoyaltyCharges, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNumberOfCalculationSheets()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.AllEntryLines.AddNew(); //1
			entry1.AllEntryLines.AddNew(); //2
			entry1.AllEntryLines.AddNew(); //3

			var dataObject = new EntryHeaderDataObject(entry1);

			NUnit.Framework.Assert.That(dataObject.NumberOfCalculationSheets, NUnit.Framework.Is.EqualTo(1));

			entry1.AllEntryLines.AddNew(); //4
			NUnit.Framework.Assert.That(dataObject.NumberOfCalculationSheets, NUnit.Framework.Is.EqualTo(2));

			entry1.AllEntryLines.AddNew(); //5
			entry1.AllEntryLines.AddNew(); //6
			entry1.AllEntryLines.AddNew(); //7
			NUnit.Framework.Assert.That(dataObject.NumberOfCalculationSheets, NUnit.Framework.Is.EqualTo(3));
		}

		[ExpectNoExceptions]
		public void TestPrintDefaultFlag()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var dataObject = new EntryHeaderDataObject(entry);

			NUnit.Framework.Assert.That(dataObject.PrintDefaultFlag, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPrintQ7CFlag()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var dataObject = new EntryHeaderDataObject(entry);

			NUnit.Framework.Assert.That(dataObject.PrintQ7CFlag, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPreviousCustomsDecision()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var dataObject = new EntryHeaderDataObject(entry);

			NUnit.Framework.Assert.That(dataObject.PreviousCustomsDecisions, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestQ7cDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var dataObject = new EntryHeaderDataObject(entry);

			NUnit.Framework.Assert.That(dataObject.Q7cDetails, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestQ8bDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var dataObject = new EntryHeaderDataObject(entry);

			NUnit.Framework.Assert.That(dataObject.Q8bDetails, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestQ9bDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var dataObject = new EntryHeaderDataObject(entry);

			NUnit.Framework.Assert.That(dataObject.Q9bDetails, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		public void TestDV1CaptionsAndLogo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.AllEntryLines.AddNew();
			var dataObject = new EntryHeaderDataObject(entry1);
			var entryLineGroup = new EntryLineGroup(entry1.AllEntryLines.Cast<CusEntryLine>(), 1);
			NUnit.Framework.Assert.That(dataObject.DV1DocumentLogo, NUnit.Framework.Is.Not.EqualTo(default(System.Drawing.Image)), "The document logo must be defined even if transparent. - should not be [null]");
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1EuropeanCommunityCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1DocumentTitle);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box1Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box2aCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box2bCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box3Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box4Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box5Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box6Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box7aCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box7aLine2Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box7bCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box7cCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box7cLine2Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box8aCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box8aLine2Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box8aLine3Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box8aLine4Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box8bCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box8bLine2Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box8bLine3Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box9aCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box9bCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box9bLine2Caption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box10aCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Box10bCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1ForOfficialUseCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1CalculationSheetNoCaption);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1Yes);
			AssertNotNullOrEmpty("The caption must be defined", dataObject.DV1No);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1BoxACaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1ItemCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box11aCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box11bCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box11cCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box12Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1BoxBCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box13Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box13aCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box13bCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box13cCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box14Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box14aCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box14bCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box14cCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box14dCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box15Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box16Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box17Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box17abCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box17cCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1BoxCCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box18Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box19Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box20Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box21Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box22Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box23Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1Box24Caption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1ForOfficialUseCaption);
			AssertNotNullOrEmpty("The caption must be defined", entryLineGroup.DV1CalculationSheetNoCaption);
		}
	}
}
