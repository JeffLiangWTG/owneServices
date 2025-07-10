using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(AmendedItem))]
	sealed class AmendedItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAmendedItemID()
		{
			var itemHeader = new AmendedItem();
			AssertEquals("Header", itemHeader.ID);

			var itemInDictionary = new AmendedItem();
			itemInDictionary.IDsInList = new List<IDInList>() { new IDInList() { IDType = nameof(IImport5BALine), IDValue = "2" } };
			AssertEquals("Entry Line : 2", itemInDictionary.ID);

			var itemNotInDictionary = new AmendedItem();
			itemNotInDictionary.IDsInList = new List<IDInList>() { new IDInList() { IDType = "IImportRandomLine", IDValue = "3" } };
			AssertExceptionThrown<Exception>(() => { var id = itemNotInDictionary.ID; });

			var itemWithMultipleIDs = new AmendedItem();
			itemWithMultipleIDs.IDsInList = new List<IDInList>()
											{
												new IDInList() { IDType = nameof(IExportEntryLine), IDValue = "2" },
												new IDInList() { IDType = nameof(IExportInvoiceLine), IDValue = "4" },
												new IDInList() { IDType = nameof(IExportVehicleNo), IDValue = "5" }
											};
			AssertEquals("Entry Line : 2, Invoice Line : 4, Vehicle No. : 5", itemWithMultipleIDs.ID);

			var itemOrganization = new AmendedItem();
			itemOrganization.IDsInList = new List<IDInList>() { new IDInList() { IDType = nameof(IOrganization), IDValue = nameof(RoleType.Supplier) } };
			AssertEquals("Organization : Supplier", itemOrganization.ID);

			var itemHeaderHasNotIDValue = new AmendedItem();
			itemHeaderHasNotIDValue.IDsInList = new List<IDInList>() { new IDInList() { IDType = nameof(IExportEntryHeader), IDValue = "" } };
			AssertEquals("Header", itemHeaderHasNotIDValue.ID);

			var itemHeaderHasIDValue = new AmendedItem();
			itemHeaderHasIDValue.IDsInList = new List<IDInList>() { new IDInList() { IDType = nameof(IExportEntryHeader), IDValue = "1" } };
			AssertEquals("Header : 1", itemHeaderHasIDValue.ID);
		}

		public void TestAmendmentTypeDescription()
		{
			var item = new AmendedItem();
			item.AmendType = Customs.Business.AccumulativeAmendment.EntityAmendType.Update;
			AssertEquals("Update", item.AmendTypeDescription);

			item.AmendType = Customs.Business.AccumulativeAmendment.EntityAmendType.Delete;
			AssertEquals("Delete", item.AmendTypeDescription);

			item.AmendType = Customs.Business.AccumulativeAmendment.EntityAmendType.Add;
			AssertEquals("Add", item.AmendTypeDescription);
		}
	}
}
