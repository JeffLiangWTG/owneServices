using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.NZP.Testing
{
	[TestedType(typeof(NZPDataRegistry))]
	public class NZPDataRegistryTest : RegistryItemSetTestCase<NZPDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("5 items should be int the collection", 5, AllItems.Count);
			AssertVisible(ItemSet.CMSExportDirectoryRaw);
			AssertVisible(ItemSet.CMSLastDateExportedRaw);
			AssertVisible(ItemSet.CMSWarehouseCodeRaw);
			AssertVisible(ItemSet.ActualLastDateExportedRaw, true);
			AssertVisible(ItemSet.InvoiceTermsInTFileRaw);
		}

		public void TestCMSExportDirectory()
		{
			ItemSet.CMSExportDirectory = "abc";
			AssertEquals("Export Directory should be 'abc'", "abc", ItemSet.CMSExportDirectory);
		}

		public void TestCMSLastDateExported()
		{
			ItemSet.CMSLastDateExported = ZDateTime.Invalid;
			AssertEquals("Last Date Exported should " + DateTime.MinValue, DateTime.MinValue, ItemSet.CMSLastDateExported);
			AssertEquals("Last Date Exported should " + ZDateTime.Invalid, ZDateTime.Invalid, ItemSet.CMSLastDateExported);
			ZDateTime lastDateExported = new ZDateTime(2005, 10, 17, 15, 52, 23);
			ItemSet.CMSLastDateExported = lastDateExported;
			AssertEquals("Last Date Exported should be " + lastDateExported.ToString(), lastDateExported, ItemSet.CMSLastDateExported);
		}

		public void TestCMSWarehouseCode()
		{
			ItemSet.CurrentCMSWarehouseCode = "xyz";
			AssertEquals("Warehouse Code should be 'xyz'", "xyz", ItemSet.CurrentCMSWarehouseCode);
		}

		public void TestGetCMSWarehouseCode()
		{
			GlbBranch testBranch = CreateUpBranch();
			ItemSet.CMSWarehouseCodeRaw.SetValue(Guid.Empty, testBranch.PK.ToGuid(), Guid.Empty, "ABCD");
			AssertEquals("Warehouse code should be 'ABCD'", "ABCD", ItemSet.GetCMSWarehouseCode(testBranch.GB_Code));
			AssertEquals("Warehouse code should be empty", "", ItemSet.GetCMSWarehouseCode(""));
			AssertEquals("Warehouse code should be empty, because branch code was does not exists", "", ItemSet.GetCMSWarehouseCode("blah"));
		}

		public void TestActualLastDateExported()
		{
			AssertEquals("Default value should be 1/4/06", new ZDateTime(2006, 4, 1), ItemSet.ActualLastDateExportedRaw.DefaultValue);
			AssertEquals("Should only be visible to developers and not cached.", RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached, ItemSet.ActualLastDateExportedRaw.Options);
			AssertExceptionThrown<RegistryValidationException>(() => ItemSet.ActualLastDateExported = ZDateTime.Invalid);
			ZDateTime actualLastDate = new ZDateTime(2006, 5, 10, 16, 00, 23);
			ItemSet.ActualLastDateExported = actualLastDate;
			AssertEquals("Actual Last Date Exported should be " + actualLastDate, actualLastDate, ItemSet.ActualLastDateExported);
		}

		public void TestInvoiceTermsInTFile()
		{
			ReadOnlyCodeDescriptionPairList pairList = ItemSet.InvoiceTermsInTFileRaw.DefaultValue;
			AssertEquals(2, pairList.Count);
			Assert(pairList.ContainsCode(Core.Constants.InvoiceTerms.CashOnDelivery));
			Assert(pairList.ContainsCode(Core.Constants.InvoiceTerms.PaymentInAdvance));
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("FOO");
			ItemSet.InvoiceTermsInTFileRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("Value.Count", 1, ItemSet.InvoiceTermsInTFileRaw.Value.Count);
			AssertEquals("Code", "FOO", ItemSet.InvoiceTermsInTFileRaw.Value[0].Code);
		}

		GlbBranch CreateUpBranch()
		{
			GlbBranch testBranch = Factory.New<GlbBranch>();
			testBranch.GB_Code = "XYZ";
			testBranch.GB_BranchName = "XYZ Branch";
			testBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			return testBranch;
		}

		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}

		BusinessObjectFactory factory;
	}
}
