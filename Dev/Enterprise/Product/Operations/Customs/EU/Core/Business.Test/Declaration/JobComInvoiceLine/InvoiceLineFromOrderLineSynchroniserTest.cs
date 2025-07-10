using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class InvoiceLineFromOrderLineSynchroniserTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.AutoPopulateInvoicePackagingAndContainerDetailsFromOrderLines.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ComInvoiceReconciliatorTest.Helper = new TestHelper();
			ComInvoiceReconciliatorTest.CreateDummyDeclarationAndOrders(Factory);
			ComInvoiceReconciliatorTest.AssertPreConditionDummyDeclarationAndOrders(Factory);

			orderLine1 = ComInvoiceReconciliatorTest.SelectedOrders[0].OrderLines[0];
			orderLine1.JO_ActualVolume = 100;
			orderLine1.JO_UnitOfVolume = "M3";
			orderLine1.JO_OuterPacksUQ = "BAG";
			orderLine1.JO_OuterPacks = 11;
			orderLine1.JO_ContainerNumber = "C1";

			orderLine2 = ComInvoiceReconciliatorTest.SelectedOrders[1].OrderLines[0];
			orderLine2.JO_ActualVolume = 200;
			orderLine2.JO_UnitOfVolume = "M3";
			orderLine2.JO_OuterPacksUQ = "XXX";
			orderLine2.JO_OuterPacks = 22;
			orderLine2.JO_ContainerNumber = "DANU1223345";

			var declarationCusContainer = ComInvoiceReconciliatorTest.Declaration.CusContainers.AddNew();
			declarationCusContainer.CO_ContainerNumber = "C1";
			Factory.Save();
		}

		public void TestEuUpdateInvoiceLinesFromOrderLinesWithPackagesAndContainers_PlainSynch()
		{
			invLineA = ComInvoiceReconciliatorTest.Declaration.InvoiceLines[0];
			invLineB = ComInvoiceReconciliatorTest.Declaration.InvoiceLines[1];
			var synchroniser = new InvoiceLineFromOrderLineSynchroniser();
			synchroniser.UpdateInvoiceLineFromOrderLine(orderLine1, invLineA);
			synchroniser.UpdateInvoiceLineFromOrderLine(orderLine2, invLineB);
			MakeAssertionsA();
			MakeAssertionsB(false);
		}

		public void TestEuUpdateInvoiceLinesFromOrderLinesWithPackagesAndContainers_Reconcile()
		{
			CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var reconciliator = new ComInvoiceReconciliator(ComInvoiceReconciliatorTest.Declaration, ComInvoiceReconciliatorTest.SelectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			reconciliator.ImportInvoices();
			invLineA = ComInvoiceReconciliatorTest.Declaration.InvoiceLines[0];
			invLineB = ComInvoiceReconciliatorTest.Declaration.InvoiceLines[3];
			MakeAssertionsA();
			MakeAssertionsB(true);
		}

		public void TestEuUpdateInvoiceLinesFromOrderLinesWithPackagesAndContainers_CreateDeclarationFromOrder()
		{
			var order = ComInvoiceReconciliatorTest.SelectedOrders[0];
			order.PlannedContainers.AddNew().J1_ContainerNumber = "C1";
			order.Factory.Save();
			var notifierShutUp = new SilentNotifierForTest();
			var preAdvice = order.CreateAndLinkPreAdviceToOrder(notifierShutUp, Order.TargetObjectForCreate.Declaration);
			var declaration = (JobDeclaration)preAdvice.CreateStandAloneBrokerage(notifierShutUp);
			declaration.Reload();
			declaration.InvoiceLines.Load();
			invLineA = declaration.InvoiceLines[0];
			MakeAssertionsA();
		}

		void MakeAssertionsA()
		{
			AssertEquals("Volume should have been updated", ZDecimal.Parse("100"), invLineA.JI_Volume);
			AssertEquals("Volume UQ should have been updated", "M3", invLineA.JI_VolumeUQ);

			var pivot = (InvoiceLinePackagePivot)invLineA.PackagesPivot.ToArray()[0];

			AssertEquals("Package count should have been updated", 11, pivot.CHC_NumberOfPacks);
			AssertEquals("Package type should have been updated", "BG", pivot.Package.CW_PackType);
			AssertEquals("Marks and numbers should have been updated", "ORDER1 - 1", pivot.Package.CW_MarksAndNos);
			var containersForInvoiceLines = invLineA.ContainersForInvoiceLinesForBindingOnly;
			AssertEquals("containersForInvoiceLines", 1, containersForInvoiceLines.Count);
			AssertEquals("Is for invoice line checkbox has been updated for container C1", true, containersForInvoiceLines[0].IsForInvoiceLine);
			AssertEquals("Is for invoice line checkbox has been updated for container C1", "C1", containersForInvoiceLines[0].ContainerNumber);
		}

		void MakeAssertionsB(bool hasMatchedPackage)
		{
			AssertEquals("Volume should have been updated", 200m, invLineB.JI_Volume);

			var pivot = (InvoiceLinePackagePivot)invLineB.PackagesPivot.ToArray()[0];
			AssertEquals("Package count should have been updated", 22, pivot.CHC_NumberOfPacks);
			AssertEquals("Package type should have been updated", "PK", pivot.Package.CW_PackType);
			AssertEquals("Marks and numbers should have been updated", "ORDER2 - 1", pivot.Package.CW_MarksAndNos);
			AssertEquals("Is for invoice line checkbox has been updated for container C1", false, invLineB.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
		}

		public void TestUpdateContainerSelectionOnInvoiceLineDoesntDuplicateContainersForInvoiceLines()
		{
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_ContainerNumber = "DANU1111111";
			var declaration = Factory.New<JobDeclaration>();
			var cont1 = declaration.CusContainers.AddNew();
			var cont2 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "DANU1111111";
			cont2.CO_ContainerNumber = "DANU2222222";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var synchroniser = new InvoiceLineFromOrderLineSynchroniser();
			synchroniser.UpdateInvoiceLineFromOrderLine(orderLine, invoiceLine);
			AssertEquals("Synch'ing order should create two available containers on the invoice line, not 4 (as would happen if you loaded the CusContainers collection twice)", 2, invoiceLine.ContainersForInvoiceLinesForBindingOnly.Count);
			AssertEquals(true, invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(cont1).IsForInvoiceLine);
			AssertEquals(false, invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(cont2).IsForInvoiceLine);
		}

		public void TestGetTwoCharacterUnitType()
		{
			var synchroniser = new InvoiceLineFromOrderLineSynchroniser();
			AssertEquals((new ZString("BG"), true), synchroniser.GetTwoCharacterUnitType("BAG"));
			AssertEquals((new ZString("CS"), true), synchroniser.GetTwoCharacterUnitType("CS"));
			AssertEquals((new ZString("PK"), false), synchroniser.GetTwoCharacterUnitType("G"));
			AssertEquals((new ZString("PK"), false), synchroniser.GetTwoCharacterUnitType("AA"));
			AssertEquals((new ZString("PK"), false), synchroniser.GetTwoCharacterUnitType("AAA"));
			AssertEquals((new ZString("PK"), true), synchroniser.GetTwoCharacterUnitType("PKG"));
		}

		class SilentNotifierForTest : List<INotification>, INotifications, INotificationSubscriberQueryUser
		{
			public void QueryUser(IQueryUserEventArgs queryUserEventArgs)
			{
				var queryUserMsgBoxEventArgs = queryUserEventArgs as QueryUserMsgBoxEventArgs;
				if (queryUserMsgBoxEventArgs != null)
				{
					queryUserMsgBoxEventArgs.Response = true;
				}
			}
		}

		BaseJobComInvoiceLine invLineA;
		BaseJobComInvoiceLine invLineB;
		OrderLine orderLine1;
		OrderLine orderLine2;
	}
}
