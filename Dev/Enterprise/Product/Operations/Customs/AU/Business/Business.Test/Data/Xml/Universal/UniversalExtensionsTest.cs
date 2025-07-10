using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UniversalExtensionsTest : TestCaseWithFactory
	{
		public void TestGetTILV4WarehouseForBO()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceLine = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_TILV = "100.00AUD";
			AssertEquals("100.00", invoiceLine.GetTILV4Warehouse().Value.Value);
		}

		public void TestGetTILV4WarehouseForUniversalXML()
		{
			var addInfos = new List<UniversalAddInfo>();
			var tilv = new UniversalAddInfo();
			tilv.Key = UniversalExtensions.TILV4Warehouse;
			tilv.Value = "100.00";
			addInfos.Add(tilv);
			AssertEquals(tilv, addInfos.GetTILV4Warehouse());
		}

		public void TestGetEntryNumberForWarehouse()
		{
			var addInfos = new List<UniversalAddInfo>();
			var tilv = new UniversalAddInfo();
			tilv.Key = UniversalExtensions.TILV4Warehouse;
			tilv.Value = "100.00";
			addInfos.Add(tilv);
			var wrn = new UniversalAddInfo();
			wrn.Key = "WRN";
			wrn.Value = "ENT1";
			addInfos.Add(wrn);
			AssertEquals(wrn, addInfos.GetEntryNumberForWarehouse());
		}

		public void TestGetEntryLineNumberForWarehouse()
		{
			var addInfos = new List<UniversalAddInfo>();
			var tilv = new UniversalAddInfo();
			tilv.Key = UniversalExtensions.TILV4Warehouse;
			tilv.Value = "100.00";
			addInfos.Add(tilv);
			var wrl = new UniversalAddInfo();
			wrl.Key = "WRL";
			wrl.Value = "1";
			addInfos.Add(wrl);
			AssertEquals(wrl, addInfos.GetEntryLineNumberForWarehouse());
		}

		public void TestIsNEXDOCS()
		{
			var eventData = new UniversalDataBuss.DataObjects.Universal.Event();
			Assert(!eventData.IsNEXDOCS());

			var dataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11) as UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
			dataContext.DataProvider = Constants.DataProvider.NEXDOCS;
			eventData.DataContext = dataContext;
			Assert(eventData.IsNEXDOCS());
		}

		public void TestIsNEXDOCSCertificatePrint()
		{
			var eventData = new UniversalDataBuss.DataObjects.Universal.Event();
			Assert(!eventData.IsNEXDOCSCertificatePrint());

			var dataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11) as UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
			dataContext.ActionPurpose = new CodeDescriptionPair() { Code = Constants.ActionPurpose.ADD, Description = "ADD" };
			eventData.DataContext = dataContext;
			eventData.EventType = Events.DocumentImportedCode;
			Assert(eventData.IsNEXDOCSCertificatePrint());
		}

		public void TestIsNEXDOCSMessageReceived()
		{
			var eventData = new UniversalDataBuss.DataObjects.Universal.Event();
			Assert(!eventData.IsNEXDOCSMessageReceived());

			eventData.EventType = Events.MessageReceivedCode;
			Assert(eventData.IsNEXDOCSMessageReceived());
		}

		public void TestIsNEXDOCSNotification()
		{
			var eventData = new UniversalDataBuss.DataObjects.Universal.Event();
			Assert(!eventData.IsNEXDOCSNotification());

			eventData.EventReference = Constants.EventReference.Notify;
			Assert(eventData.IsNEXDOCSNotification());
		}
	}
}
