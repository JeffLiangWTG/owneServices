using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Registry;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	using System.Reflection;
	using CargoWise.Common;
	using CargoWise.Data.Testing;
	using Enterprise.Accounting.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;
	using AccGenericCharge = GenericCharge.GenericCharge;

	[TestedType(typeof(AccPayableOrderHeader))]
	public class AccPayableOrderHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestAL_ATFieldEditabilityWhenPayableAllowUserToModifyGSTIdRegistryIsSetAtSystemLevel()
		{
			Order.APH_InvoiceNumber = "1111";
			Order.APH_InvoiceDate = ZDate.Today;
			Order.SupplierDocumentaryAddress.OrganisationPK = new TestObjectCreator(Factory).AALSHI.PK;

			var genericCharge = Factory.LoadTop1<AccGenericCharge>(new ZQuery(ViewGenericChargeSchema.VC_IsGLAccount, true));
			var line = Order.OrderLines.AddNew();
			line.GenericCharge = genericCharge.PK;
			line.APL_ItemPrice = 5;
			line.APL_Quantity = 10;
			line.APL_QtyReceived = 10;
			line.APL_QtyInvoiced = 10;
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var apInvoice1 = Order.PopulateInvoiceFromOrder();
				Assert("AL_AT should be editable", !apInvoice1.Lines[0].AL_ATInfo.ReadOnly);
			}

			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var apInvoice2 = Order.PopulateInvoiceFromOrder();
				Assert("AL_AT should be readonly", apInvoice2.Lines[0].AL_ATInfo.ReadOnly);
			}
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccPayableOrderHeader()
		{
			var header = Factory.New<AccPayableOrderHeader>();

			var localList = new List<string>
			{
				nameof(header.APH_Calc_TotalAmount)
			};

			var orderList = new List<string>
			{
				nameof(header.APH_Calc_TotalLinePrice),
				nameof(header.APH_Calc_TotalInvoicedPrice),
				nameof(header.APH_Calc_TotalQuantity),
				nameof(header.APH_Calc_TotalQuantityInvoiced),
				nameof(header.APH_Calc_TotalQuantityReceived),
				nameof(header.APH_Calc_TotalQuantityRemaining)
			};

			var exList = new List<string>
			{
				nameof(header.APH_EstimatedExchangeRate)
			};

			var tester = new DecimalPlacesAttributeTester(header, header.Company);
			tester.CheckLocalCurrency(localList, nameof(header.LocalDecimals));
			tester.CheckNonLocalCurrency(orderList, nameof(header.OrderCurrencyDecimals), nameof(header.APH_RX_NKOrderCurrency), header);
			tester.CheckExchangeRate(exList, nameof(header.ExchangeRateDecimals));
		}

		public void TestGetNoteTypes()
		{
			AssertCollectionContains(PredefinedNoteTypes.Instance.SpecialInstructions, Order.NoteTypes);
			AssertCollectionContains(PredefinedNoteTypes.Instance.InternalWorkNotes, Order.NoteTypes);
			AssertCollectionContains(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog, Order.NoteTypes);
			AssertEquals(3, Order.NoteTypes.Count);
		}

		public void TestConstructorSetsConcurrencyPolicy()
		{
			var order = (AccPayableOrderHeader)GetNewBusinessObject();
			AssertEquals(ConcurrencyPolicy.Strict, order.APH_AHInfo.ConcurrencyPolicy);
			AssertEquals(ConcurrencyPolicy.Strict, order.APH_InvoiceNumberInfo.ConcurrencyPolicy);
			AssertEquals(ConcurrencyPolicy.Strict, order.APH_InvoiceDateInfo.ConcurrencyPolicy);
		}

		public void TestSetDefault()
		{
			Order = Factory.New<AccPayableOrderHeader>();
			AssertEquals(Constants.PayableOrderStage.Request, Order.APH_Stage);
			AssertEquals(Constants.PayableOrderDisposition.OrderIncomplete, Order.APH_Disposition);
			AssertEquals(Constants.PayableOrderType.VariableOverhead, Order.APH_Type);
			AssertEquals(Constants.PayableOrderGoodsStatus.NotReceived, Order.APH_GoodsReceivedStatus);
			if (GlbBranch.CurrentBranch.OrgProxy != null && GlbBranch.CurrentBranch.OrgProxy.MainAddress != null)
			{
				AssertEquals(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, Order.APH_OA_Buyer);
			}

			AssertEquals(GlbCompany.CurrentCompany.PK, Order.APH_GC);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Order.APH_RX_NKOrderCurrency);
			AssertEquals(1m, Order.APH_EstimatedExchangeRate);
		}

		public void TestReadOnly()
		{
			Order.APH_Disposition = Core.Constants.PayableOrderDisposition.PendingGoodsReceivedAudit;
			Assert(!Order.ReadOnly);
			Order.IsCancelled = true;
			Assert(Order.ReadOnly);
			Order.IsCancelled = false;
			Assert(!Order.ReadOnly);
			Order.APH_Disposition = Core.Constants.PayableOrderDisposition.Complete;
			Assert(Order.ReadOnly);
		}

		public void TestSetAPH_Disposition_MilestoneCollectionReadOnlyIsFalse()
		{
			Order.APH_Disposition = Core.Constants.PayableOrderDisposition.Complete;
			Assert(Order.ReadOnly);
			Assert(!Order.WorkflowItems.MilestonesIncludingRelatedSortable.ReadOnly);
		}

		public void TestUpdatePropertyWithUpdateAttribute()
		{
			AssertEquals("Precondition: empty", ZDate.Empty, Order.APH_BookingConfDate);
			Assert("Order's Disposition was not Completed", Order.APH_Disposition != Constants.PayableOrderDisposition.Complete);
			var eventTime = ZDateTimeOffset.Now;
			Order.GetLogs().AddNew(Events.BookingConfirmed, eventTime);
			AssertEquals("updated", eventTime.Date, Order.APH_BookingConfDate);
			Order.APH_Disposition = Constants.PayableOrderDisposition.Complete;
			Order.GetLogs().AddNew(Events.BookingConfirmed, eventTime.AddDays(1));
			AssertEquals("not updated", eventTime.Date, Order.APH_BookingConfDate);
		}

		public void TestSetDisposition()
		{
			Order.APH_Disposition = Constants.PayableOrderDisposition.OrderIncomplete;
			AssertEquals(Constants.PayableOrderStage.Request, Order.APH_Stage);
			Order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			AssertEquals(Constants.PayableOrderStage.Request, Order.APH_Stage);
			Order.APH_Disposition = Constants.PayableOrderDisposition.OrderToBePlaced;
			AssertEquals(Constants.PayableOrderStage.Order, Order.APH_Stage);
			Order.APH_Disposition = Constants.PayableOrderDisposition.PendingConfirmation;
			AssertEquals(Constants.PayableOrderStage.Order, Order.APH_Stage);
			Order.APH_Disposition = Constants.PayableOrderDisposition.ExpectedDLVPending;
			AssertEquals(Constants.PayableOrderStage.Track, Order.APH_Stage);
			Order.APH_Disposition = Constants.PayableOrderDisposition.DeliveryInProgress;
			AssertEquals(Constants.PayableOrderStage.Track, Order.APH_Stage);
			Order.APH_Disposition = Constants.PayableOrderDisposition.APInvoiceToBePosted;
			AssertEquals(Constants.PayableOrderStage.Receive, Order.APH_Stage);
			Order.APH_Disposition = Constants.PayableOrderDisposition.PendingGoodsReceivedAudit;
			AssertEquals(Constants.PayableOrderStage.Receive, Order.APH_Stage);
			Order.APH_Disposition = Constants.PayableOrderDisposition.Complete;
			AssertEquals(Constants.PayableOrderStage.Receive, Order.APH_Stage);
		}

		public void TestStateChangeOnFactorySaving()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.OrderLines.Add(Factory.NewWithValidTestData<AccPayableOrderLine>());
			Factory.Save();
			AssertEquals(Constants.PayableOrderDisposition.PendingApproval, order.APH_Disposition);
			order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Disposition = Core.Constants.PayableOrderDisposition.DeliveryInProgress;
			var line = order.OrderLines.AddNew();
			line.APL_Quantity = 123m;
			AssertNotEquals(0m, order.APH_Calc_TotalQuantityRemaining);
			Factory.Save();
			AssertEquals(Constants.PayableOrderDisposition.PendingApproval, order.APH_Disposition);
			order.APH_Disposition = Core.Constants.PayableOrderDisposition.DeliveryInProgress;
			Factory.Save();
			AssertEquals(Constants.PayableOrderDisposition.DeliveryInProgress, order.APH_Disposition);
			line.APL_QtyInvoiced = 123m;
			line.APL_QtyReceived = 123m;
			AssertEquals(0m, order.APH_Calc_TotalQuantityRemaining);
			Factory.Save();
			AssertEquals(Constants.PayableOrderDisposition.APInvoiceToBePosted, order.APH_Disposition);
			order.OrderLines.DeleteAll();
			Factory.Save();
			AssertEquals(Constants.PayableOrderDisposition.OrderIncomplete, order.APH_Disposition);
			order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_InvoiceNumber = "1111";
			order.APH_InvoiceDate = ZDate.Today;
			order.OrderLines.Add(Factory.NewWithValidTestData<AccPayableOrderLine>());
			Factory.Save();
			var invoiceFromOrder = order.PopulateInvoiceFromOrder();
			invoiceFromOrder.Factory.Save();
			AssertEquals(Constants.PayableOrderDisposition.PendingGoodsReceivedAudit, order.APH_Disposition);
			order.Logs.AddNew(Events.RecordAudited);
			Factory.Save();
			AssertEquals(Constants.PayableOrderDisposition.Complete, order.APH_Disposition);
			order.Logs.GetAllLogs().Cast<StmALog>().ForEach(s =>
			{
				if (s.SL_SE_NKEvent == Events.RecordAuditedCode && !s.SL_IsCancelled)
				{
					s.Cancel();
				}
			}

			);
			Factory.Save();
			AssertEquals(Constants.PayableOrderDisposition.PendingGoodsReceivedAudit, order.APH_Disposition);
		}

		public void TestPopulateInvoiceFromOrderReadOnlyFieldsForInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var filter = new ZQuery(ViewGenericChargeSchema.VC_IsGLAccount, true);
			var charge = Factory.LoadTop1<AccGenericCharge>(filter);
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_InvoiceNumber = "1111";
			order.APH_InvoiceDate = ZDate.Today;
			order.SupplierDocumentaryAddress.OrganisationPK = testObjectCreator.AALSHI.PK;
			var line = Factory.NewWithValidTestData<AccPayableOrderLine>();
			line.GenericCharge = charge.PK;
			line.APL_ItemPrice = 5;
			line.APL_Quantity = 10;
			line.APL_QtyReceived = 10;
			line.APL_QtyInvoiced = 10;
			order.OrderLines.Add(line);
			Factory.Save();
			var invoiceFromOrder = order.PopulateInvoiceFromOrder();
			AssertEquals("Should be editable", false, invoiceFromOrder.AH_RequisitionDateInfo.ReadOnly);
			AssertEquals("Should be editable", false, invoiceFromOrder.AH_RequisitionStatusInfo.ReadOnly);
			var allPropertiesExceptRequsition = invoiceFromOrder.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name != InvoicingBase.Schema.AH_RequisitionDate && x.Name != InvoicingBase.Schema.AH_RequisitionStatus && invoiceFromOrder.ZPropertyInfoHash.ContainsKey(x.Name));
			var allZProperties = invoiceFromOrder.ZPropertyInfoHash.ToList<ZPropertyInfo>().Where(x => x.Name != InvoicingBase.Schema.AH_RequisitionDate && x.Name != InvoicingBase.Schema.AH_RequisitionStatus && allPropertiesExceptRequsition.Any(y => y.Name == x.Name));
			Assert("All property other than RequisitionDate and RequisitionStatus should be readonly", allZProperties.All(x => x.ReadOnly));
		}

		public void TestPopulateInvoiceFromOrderValidationErrorForAL_AT()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			ZQuery filter = new ZQuery(ViewGenericChargeSchema.VC_IsGLAccount, true);
			var charge = Factory.LoadTop1<AccGenericCharge>(filter);
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_InvoiceNumber = "1111";
			order.APH_InvoiceDate = ZDate.Today;
			order.SupplierDocumentaryAddress.OrganisationPK = testObjectCreator.AALSHI.PK;
			var line = Factory.NewWithValidTestData<AccPayableOrderLine>();
			line.GenericCharge = charge.PK;
			line.APL_ItemPrice = 5;
			line.APL_Quantity = 10;
			line.APL_QtyReceived = 10;
			line.APL_QtyInvoiced = 10;
			order.OrderLines.Add(line);
			Factory.Save();
			var invoiceFromOrder = order.PopulateInvoiceFromOrder();
			AssertEquals("PreCondition: AL_AT should be empty", ZGuid.Empty, invoiceFromOrder.Lines[0].AL_AT);
			AssertEquals("PreCondition: AL_AT should be editable", false, invoiceFromOrder.Lines[0].AL_ATInfo.ReadOnly);
			AssertHasErrors("AL_AT should have  error", invoiceFromOrder.Lines[0].AL_ATInfo);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Factory.Save();
			invoiceFromOrder = order.PopulateInvoiceFromOrder();
			invoiceFromOrder.Lines[0].AL_ExchangeRate = invoiceFromOrder.AH_ExchangeRate;
			AssertEquals("PreCondition: AL_AT should be empty", ZGuid.Empty, invoiceFromOrder.Lines[0].AL_AT);
			AssertEquals("PreCondition: AL_AT should be readonly", true, invoiceFromOrder.Lines[0].AL_ATInfo.ReadOnly);
			AssertNoErrors("AL_AT should not have any error", invoiceFromOrder.Lines[0].AL_ATInfo);
			invoiceFromOrder.Factory.Save();
			AssertEquals("Invoice should be posted", true, invoiceFromOrder.IsPosted);
		}

		public void TestPopulateInvoiceFromOrderWithValidExchangeRate()
		{
			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testObjectCreator = new TestObjectCreator(Factory);

				var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
				order.APH_InvoiceNumber = "1111";
				order.APH_InvoiceDate = ZDate.Today;
				order.SupplierDocumentaryAddress.OrganisationPK = testObjectCreator.AALSHI.PK;

				var orderLine = Factory.NewWithValidTestData<AccPayableOrderLine>();
				orderLine.GenericCharge = TestObjectCreator.CC3.PK;
				orderLine.APL_ItemPrice = 5;
				orderLine.APL_Quantity = 10;
				orderLine.APL_QtyReceived = 10;
				orderLine.APL_QtyInvoiced = 10;
				order.OrderLines.Add(orderLine);

				AssertEquals("Order should have local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, order.APH_RX_NKOrderCurrency);

				var invoice = order.PopulateInvoiceFromOrder();
				AssertNotNull("Invoice populated from Orde", invoice);
				Assert("Invoice should not be a reversal", !invoice.IsReversalTransaction);
				Assert("Invoice should use Job ExchangeRate", invoice.UseJobExchangeRate);

				AssertEquals("Have one line", 1, invoice.Lines.Count);
				var invoiceLine = invoice.Lines[0];
				Assert("Not related to a Job", invoiceLine.AL_JH.IsEmpty);
				AssertEquals("Has local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, invoiceLine.AL_RX_NKTransactionCurrency);
				AssertEquals("AL_ExchangeRate", 1M, invoiceLine.AL_ExchangeRate);
			}
		}

		public void TestCalcLineCounter()
		{
			AssertEquals("No Order Lines: LineCounter should be 0", 0, Order.APH_Calc_LineCount);
			AccPayableOrderLine line1 = Order.OrderLines.AddNew();
			AccPayableOrderLine line2 = Order.OrderLines.AddNew();
			AssertEquals(2, Order.APH_Calc_LineCount);
		}

		public void TestCalcInnerPacks()
		{
			AssertEquals("No Order Lines: InnerPacks should be 0", 0, Order.APH_Calc_InnerPacks);
			AccPayableOrderLine line1 = Order.OrderLines.AddNew();
			AccPayableOrderLine line2 = Order.OrderLines.AddNew();
			line1.APL_InnerPacks = 6;
			line1.APL_InnerPacksUQ = Constants.PkgUnit.Keg;
			line2.APL_InnerPacks = 7;
			line2.APL_InnerPacksUQ = "";
			AssertEquals(13, Order.APH_Calc_InnerPacks);
		}

		public void TestCalcOuterPacks()
		{
			AssertEquals("No Order Lines: OuterPacks should be 0", 0, Order.APH_Calc_OuterPacks);
			AccPayableOrderLine line1 = Order.OrderLines.AddNew();
			AccPayableOrderLine line2 = Order.OrderLines.AddNew();
			line1.APL_OuterPacks = 6;
			line1.APL_OuterPacksUQ = Constants.PkgUnit.Keg;
			line2.APL_OuterPacks = 7;
			line2.APL_OuterPacksUQ = "";
			AssertEquals(13, Order.APH_Calc_OuterPacks);
		}

		public void TestCalcTotalQuantity()
		{
			AssertEquals("No Order Lines: Total Quantity should be 0", 0m, Order.APH_Calc_TotalQuantity);
			AccPayableOrderLine line1 = Order.OrderLines.AddNew();
			AccPayableOrderLine line2 = Order.OrderLines.AddNew();
			line1.APL_Quantity = 6;
			line2.APL_Quantity = 7;
			AssertEquals(13m, Order.APH_Calc_TotalQuantity);
		}

		public void TestCalcTotalQuantityInvoiced()
		{
			AssertEquals("No Order Lines: Total Quantity should be 0", 0m, Order.APH_Calc_TotalQuantityInvoiced);
			AccPayableOrderLine line1 = Order.OrderLines.AddNew();
			AccPayableOrderLine line2 = Order.OrderLines.AddNew();
			line1.APL_QtyInvoiced = 6;
			line2.APL_QtyInvoiced = 7;
			AssertEquals(13m, Order.APH_Calc_TotalQuantityInvoiced);
		}

		public void TestCalcTotalQuantityReceived()
		{
			AssertEquals("No Order Lines: Total Quantity Received should be 0", 0m, Order.APH_Calc_TotalQuantityReceived);
			AccPayableOrderLine line1 = Order.OrderLines.AddNew();
			AccPayableOrderLine line2 = Order.OrderLines.AddNew();
			line1.APL_QtyReceived = 6;
			line2.APL_QtyReceived = 7;
			AssertEquals(13m, Order.APH_Calc_TotalQuantityReceived);
		}

		public void TestCalcTotalQuantityRemaining()
		{
			AssertEquals("No Order Lines: Total Quantity Remaining should be 0", 0m, Order.APH_Calc_TotalQuantityRemaining);
			AccPayableOrderLine line1 = Order.OrderLines.AddNew();
			AccPayableOrderLine line2 = Order.OrderLines.AddNew();
			line1.APL_Quantity = 10;
			line2.APL_Quantity = 20;
			line1.APL_QtyInvoiced = 3;
			line2.APL_QtyInvoiced = 5;
			line1.APL_QtyReceived = 4;
			line2.APL_QtyReceived = 6;
			AccountingConfigurationRegistry.Instance.APOrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(20m, Order.APH_Calc_TotalQuantityRemaining);
		}

		public void TestDateTimePropertiesFromLogs()
		{
			Order.Logs.AddNew(Events.SetToInactive, "deactivate order", new ZDateTimeOffset(2015, 6, 17));
			AssertEquals(new ZDateTime(2015, 6, 17), Order.APH_DeactivatedDate);
		}

		public void TestAPInvoicePostDate()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_InvoiceNumber = "1111";
			order.APH_InvoiceDate = ZDate.Today;
			AssertEquals("Invoice is not posted, Post Date should be empty", ZDateTime.Empty, order.APInvoicePostedDate);
			var invoice = order.PopulateInvoiceFromOrder();
			invoice.Factory.Save();
			var invoiceInCurrentFactory = Factory.Load<AccTransactionHeader>(invoice.PK);
			AssertEquals("Invoice is posted, Post Date should not be empty", invoiceInCurrentFactory.AH_PostDate, order.APInvoicePostedDate);
		}

		public void TestDocumentSupporter()
		{
			AssertNotNull("AccPayableOrderHeaderDocumentSupporter", Order.DocumentSupporter as AccPayableOrderHeaderDocumentSupporter);
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Code should be POD. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "POD", ((IDocManagerSupport)Order).DocManagerInfo.DocManagerCode);
		}

		public void TestCanBePosted()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			AssertEquals("Please save this form before posting the order", order.GetErrorMessageForNotAbleToPost());
			Factory.Save();
			AssertEquals(Constants.PayableOrderStage.Request, order.APH_Stage);
			AssertEquals("The Payable Invoice may not be posted for a Purchase Order that is pending approval", order.GetErrorMessageForNotAbleToPost());
			order.APH_Stage = Constants.PayableOrderStage.Order;
			order.APH_InvoiceNumber = "1111";
			order.APH_InvoiceDate = ZDate.Today;
			var invoice = order.PopulateInvoiceFromOrder();
			invoice.Factory.Save();
			Factory.Save();
			AssertEquals("A Payables Invoice has already been posted for this Purchase Order, please reverse this invoice to process any amendments", order.GetErrorMessageForNotAbleToPost());
			var splitOrder = order.SplitOrder(AccPayableOrderHeader.CreateOrderType.Split);
			Factory.Save();
			AssertEquals("The Payables Invoice for Split Orders should be posted on the primary Purchase Order which includes the order lines for all of the related split orders as well", splitOrder.GetErrorMessageForNotAbleToPost());
		}

		public void TestCanBePosted_InvoiceDateValidation()
		{
			using (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
				order.APH_Stage = Constants.PayableOrderStage.Order;
				order.APH_InvoiceNumber = "2222";
				order.APH_InvoiceDate = ZDate.Today.AddDays(1);
				var invoice = order.PopulateInvoiceFromOrder();
				invoice.AH_PostDate = ZDateTime.Now;
				Factory.Save();
				AssertEquals(@"Unable to post AP Invoice.
Invoice Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date.", order.GetErrorMessageForNotAbleToPost());
			}
		}

		public void TestShouldPromptSplitOnSave()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			var line = order.OrderLines.AddNew();
			line.APL_Quantity = 10;
			line.APL_QtyInvoiced = 10;
			AssertEquals(order.ShouldPromptSplitOnSave, false);
			line.APL_QtyInvoiced = 5;
			AssertEquals(order.ShouldPromptSplitOnSave, true);
			order.SplitOrder(AccPayableOrderHeader.CreateOrderType.Split);
			AssertEquals(order.ShouldPromptSplitOnSave, false);
		}

		public void TestCanSplitOrder()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Type = "BPE";
			AssertEquals("The order must be saved before splitting can occur", order.CanSplitOrder());
			Factory.Save();
			AssertEquals(true, string.IsNullOrEmpty(order.CanSplitOrder()));
			order.APH_OrderNumber = "1234";
			AssertEquals("The order must be saved before splitting can occur", order.CanSplitOrder());
		}

		public void TestIsAlreadySplit()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_OrderNumber = "1234";
			AssertEquals(false, order.IsAlreadySplit);
			order.SplitOrder(AccPayableOrderHeader.CreateOrderType.Split);
			AssertEquals(true, order.IsAlreadySplit);
		}

		public void TestSplitOrder()
		{
			//Split
			var order1 = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order1.APH_OrderNumber = "1111";
			AssertEquals("Before Split", (byte)0, order1.APH_OrderNumberSplit);
			var splitOrder = order1.SplitOrder(AccPayableOrderHeader.CreateOrderType.Split);
			AssertEquals("After Split", (byte)0, order1.APH_OrderNumberSplit);
			AssertEquals("After Split", (byte)1, splitOrder.APH_OrderNumberSplit);
			//New Order
			var order2 = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order2.APH_OrderNumber = "2222";
			splitOrder = order2.SplitOrder(AccPayableOrderHeader.CreateOrderType.New);
			AssertEquals("SplitNumber", (byte)0, order2.APH_OrderNumberSplit);
			AssertEquals("SplitNumber", (byte)0, splitOrder.APH_OrderNumberSplit);
		}

		public void TestSplitOrder_APHOrderNumber()
		{
			var factory = new BusinessObjectFactory();
			var order1 = factory.New<AccPayableOrderHeader>();
			factory.Save();
			AssertEquals("PO000001", order1.APH_OrderNumber);
			var order2 = order1.SplitOrder(AccPayableOrderHeader.CreateOrderType.Split);
			factory.Save();
			AssertEquals("PO000001", order1.APH_OrderNumber);
			AssertEquals("The order number should be same", order1.APH_OrderNumber, order2.APH_OrderNumber);
		}

		public void TestAPH_BookingConfDate()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Disposition = Core.Constants.PayableOrderDisposition.PendingConfirmation;
			order.APH_BookingConfDate = ZDate.Today.AddDays(5);
			Assert(order.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.BookingConfirmedCode));
			AssertEquals(Core.Constants.PayableOrderDisposition.ExpectedDLVPending, order.APH_Disposition);
		}

		public void TestAPH_ExpectedDelivery()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Disposition = Core.Constants.PayableOrderDisposition.PendingConfirmation;
			order.APH_ExpectedDelivery = ZDate.Today.AddDays(5);
			Assert(order.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.ArrivalCode));
			AssertEquals(0m, order.APH_Calc_TotalQuantityRemaining);
			AssertEquals(Core.Constants.PayableOrderDisposition.PendingConfirmation, order.APH_Disposition);
			var line = order.OrderLines.AddNew();
			line.APL_Quantity = 10;
			order.APH_ExpectedDelivery = ZDate.Today.AddDays(7);
			AssertNotEquals(0m, order.APH_Calc_TotalQuantityRemaining);
			AssertEquals(Core.Constants.PayableOrderDisposition.DeliveryInProgress, order.APH_Disposition);
		}

		public void TestAPH_Calc_TotalInvoicedPrice()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			var line1 = order.OrderLines.AddNew();
			line1.APL_ItemPrice = 5m;
			line1.APL_QtyInvoiced = 10m;
			var line2 = order.OrderLines.AddNew();
			line2.APL_ItemPrice = 6.609m;
			line2.APL_QtyInvoiced = 6.6m;
			AssertEquals(93.62m, order.APH_Calc_TotalInvoicedPrice);
		}

		public void TestAPH_Calc_TotalAmount()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			var line1 = order.OrderLines.AddNew();
			line1.APL_LinePrice = 100.111m;
			AssertEquals(100.11m, order.APH_Calc_TotalAmount);
			var line2 = order.OrderLines.AddNew();
			line2.APL_LinePrice = 200.225m;
			AssertEquals(300.34m, order.APH_Calc_TotalAmount);
		}

		public void TestAPH_Calc_TotalLinePrice()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			var line1 = order.OrderLines.AddNew();
			line1.APL_LinePrice = 7.111m;
			AssertEquals(7.11m, order.APH_Calc_TotalLinePrice);
			var line2 = order.OrderLines.AddNew();
			line2.APL_LinePrice = 8.225m;
			AssertEquals(15.34m, order.APH_Calc_TotalLinePrice);
		}

		public void TestAPH_Calc_CreatedUser()
		{
			var loginUser = Factory.NewWithValidTestData<GlbStaff>();
			loginUser.GS_LoginName = "David Park";
			loginUser.GS_Code = "DP";
			Factory.Save();
			using (Env.SetTemporaryUserContext(loginUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
				Factory.Save();
				AssertEquals(loginUser.PK, order.APH_Calc_CreatedUser);
			}
		}

		public void TestApprove()
		{
			var creator = new TestObjectCreator(Factory);
			var collection = new PaymentThreeLevelAuthorisationSettingsCollection();
			var newUpToSetting = collection.AddNew();
			newUpToSetting.Amount = 100m;
			newUpToSetting.AuthorisationRequirement = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newUpToSetting.Range = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			var newSetting = collection.AddNew();
			newSetting.Amount = 100m;
			newSetting.AuthorisationRequirement = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.PayableOrderAuthorizationSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			var line = order.OrderLines.AddNew();
			line.APL_LinePrice = 100m;
			Factory.Save();
			Env.Security.PayableOrderApprovalFirstLevelApproval.IsAllowed = false;
			AssertEquals(false, order.Approve());
			AssertEquals(0, order.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.AuthorisedCode));
			Env.Security.PayableOrderApprovalFirstLevelApproval.IsAllowed = true;
			AssertEquals(true, order.Approve());
			AssertEquals(1, order.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.AuthorisedCode));
			AssertEquals(Constants.PayableOrderDisposition.OrderToBePlaced, order.APH_Disposition);
			var order2 = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order2.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			line = order2.OrderLines.AddNew();
			line.APL_LinePrice = 100m;
			Factory.Save();
			AssertEquals(true, order2.Approve());
			AssertEquals(1, order2.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.AuthorisedCode));
			AssertEquals(Constants.PayableOrderDisposition.OrderToBePlaced, order2.APH_Disposition);
		}

		public void TestApproveOrderAlreadyApproved()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			var line = order.OrderLines.AddNew();
			line.APL_LinePrice = 100m;
			Factory.Save();
			AssertEquals(true, order.Approve());
			Factory.Save();
			System.Threading.Thread.Sleep(5000);
			order.Logs.AddNew(Events.Booked);
			AssertEquals(1, order.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.AuthorisedCode));
			AssertEquals("BKD event doesn't exist", Constants.PayableOrderDisposition.OrderToBePlaced, order.APH_Disposition);
			order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			Factory.Save();
			AssertEquals(true, order.Approve());
			AssertEquals(2, order.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.AuthorisedCode));
			AssertEquals("BKD event exists since the last approval", Constants.PayableOrderDisposition.PendingConfirmation, order.APH_Disposition);
			order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			Factory.Save();
			AssertEquals(true, order.Approve());
			AssertEquals(3, order.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.AuthorisedCode));
			AssertEquals("BKD event exists but not since the last approval", Constants.PayableOrderDisposition.OrderToBePlaced, order.APH_Disposition);
		}

		public void TestApproveOrderCheckArrivalEvent()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_BookingConfDate = ZDate.Today;
			order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			var line = order.OrderLines.AddNew();
			line.APL_LinePrice = 100m;
			order.Logs.AddNew(Events.Booked);
			Factory.Save();
			AssertEquals(true, order.Approve());
			AssertEquals(Constants.PayableOrderDisposition.ExpectedDLVPending, order.APH_Disposition);
			Factory.Save();
			System.Threading.Thread.Sleep(1000);
			order.Logs.AddNew(Events.Arrival);
			order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;
			order.Logs.AddNew(Events.Booked);
			Factory.Save();
			AssertEquals(true, order.Approve());
			AssertEquals(Constants.PayableOrderDisposition.DeliveryInProgress, order.APH_Disposition);
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestPopulateInvoiceFromOrder()
		{
			var creator = new TestObjectCreator(Factory);
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_GoodsDescription = "Goods Description";
			order.APH_InvoiceNumber = "OrderInv1";
			order.APH_InvoiceDate = ZDate.Today;
			order.APH_DueDate = ZDate.Today.AddDays(1);
			var lineForOrder = order.OrderLines.AddNew();
			lineForOrder.APL_GB = GlbBranch.CurrentBranch.PK;
			lineForOrder.APL_GE = GlbDepartment.CurrentDepartment.PK;
			lineForOrder.APL_AG = creator.CreateGLHeader().PK;
			lineForOrder.APL_AC = creator.OverheadChargeCode.PK;
			lineForOrder.APL_Quantity = 20;
			lineForOrder.APL_ItemPrice = 5m;
			lineForOrder.APL_QtyInvoiced = 10;
			AssertEquals(100m, lineForOrder.APL_LinePrice);
			Factory.Save();
			var splitOrder = order.SplitOrder(AccPayableOrderHeader.CreateOrderType.Split);
			var originalLineForSplitOrder = splitOrder.OrderLines[0];
			originalLineForSplitOrder.APL_Quantity = 10;
			originalLineForSplitOrder.APL_ItemPrice = 5m;
			originalLineForSplitOrder.APL_GB = GlbBranch.CurrentBranch.PK;
			originalLineForSplitOrder.APL_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals(50m, originalLineForSplitOrder.APL_LinePrice);
			var anotherLineForSplitOrder = splitOrder.OrderLines.AddNew();
			anotherLineForSplitOrder.APL_GB = GlbBranch.CurrentBranch.PK;
			anotherLineForSplitOrder.APL_GE = GlbDepartment.CurrentDepartment.PK;
			anotherLineForSplitOrder.APL_AG = creator.CreateGLHeader().PK;
			anotherLineForSplitOrder.APL_AC = creator.CC2.PK;
			anotherLineForSplitOrder.APL_Quantity = 5;
			anotherLineForSplitOrder.APL_ItemPrice = 5m;
			anotherLineForSplitOrder.APL_QtyInvoiced = 3;
			AssertEquals(25m, anotherLineForSplitOrder.APL_LinePrice);
			var invoicePopulated = order.PopulateInvoiceFromOrder();
			invoicePopulated.Lines.Cast<InvoiceLine>().ForEach(x => x.AL_ExchangeRate = invoicePopulated.AH_ExchangeRate);
			AssertEquals(order.APH_OrderNumber, invoicePopulated.AH_ChequeOrReference);
			AssertEquals("Goods Description", invoicePopulated.AH_Desc);
			AssertEquals("OrderInv1", invoicePopulated.AH_TransactionNum);
			AssertEquals(ZDate.Today, invoicePopulated.AH_InvoiceDate);
			AssertEquals(ZDate.Today.AddDays(1), invoicePopulated.AH_DueDate);
			AssertEquals("Lines with 0 Quantity Invoiced should not be populated to AP invoice", 2, invoicePopulated.Lines.Count);
			AssertEquals(true, invoicePopulated.Lines.Cast<InvoiceLine>().Any(x => x.AL_OSExTaxAmount == 50m));
			AssertEquals(true, invoicePopulated.Lines.Cast<InvoiceLine>().Any(x => x.AL_OSExTaxAmount == 15m));
			invoicePopulated.Factory.Save();
			AssertEquals(Constants.PayableOrderDisposition.PendingGoodsReceivedAudit, order.APH_Disposition);
		}

		public void TestInvoiceDateSetDueDate()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_DueDate = ZDate.Empty;
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_IsCreditor = ZBool.True;
			supplier.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			supplier.APSettlementGroupPK = Factory.New<OrgHeader>().PK;
			supplier.APSettlementGroup.CompanyData.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromInvoiceDate;
			supplier.APSettlementGroup.CompanyData.OB_APPaymentTermDays = 3;
			order.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			order.APH_InvoiceDate = ZDate.Today;
			AssertEquals("Due date", order.APH_InvoiceDate.AddDays(3), order.APH_DueDate);
		}

		public void TestGenerateOrderNumberOnSave()
		{
			Order.APH_OrderNumber = "1234";
			Assert(!Order.IsInDatabase);
			string expectedOrderNumber = Env.NumberFountains.PayableOrderNumber(Order.APH_GC.ToGuid()).PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertNotEquals("1234", Order.APH_OrderNumber);
			AssertEquals(expectedOrderNumber, Order.APH_OrderNumber);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Shouldn't be fired on this business object", true);
		}

		public void TestHasRegistryRestrictions()
		{
			//Blank Supplier
			SetOrderApprovalRegistryRestrictions(false, true, false, false);
			Order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10);
			AssertEquals(PayableOrderRegistryHelper.BlankSupplierRestrictionMessage, Order.HasRegistryRestrictions());
			// Overridden Supplier
			// a. Single supplier for both documentry as well as pickup/delivery address
			SetOrderApprovalRegistryRestrictions(false, false, true, false);
			Order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10);
			OverrideOrderSupplier();
			AssertEquals(PayableOrderRegistryHelper.OverriddenSupplierRestrictionMessage, Order.HasRegistryRestrictions());
			// b. Different supplier organization for documentry and pickup/delivery address
			Order.SupplierDocumentaryAddress.E2_AddressOverride = false;
			AssertEquals(string.Empty, Order.HasRegistryRestrictions());
			var pickupDeliveryOrg = Order.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.SupplierPickupDeliveryAddress);
			var newOrganization = Factory.NewWithValidTestData<OrgHeader>();
			pickupDeliveryOrg.OrganisationPK = newOrganization.PK;
			pickupDeliveryOrg.E2_AddressOverride = true;
			AssertEquals(PayableOrderRegistryHelper.OverriddenSupplierRestrictionMessage, Order.HasRegistryRestrictions());
			// Temporary Organization
			// a. Single org for documentry and pickup/delivery address
			SetOrderApprovalRegistryRestrictions(false, false, false, true);
			Order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10);
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_IsTempAccount = true;
			Order.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			AssertEquals(PayableOrderRegistryHelper.TemporaryOrganizationRestrictionMessage, Order.HasRegistryRestrictions());
			// b. Different org's for documentry and pickup/delivery address
			supplier.OH_IsTempAccount = false;
			AssertEquals(string.Empty, Order.HasRegistryRestrictions());
			pickupDeliveryOrg = Order.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.SupplierPickupDeliveryAddress);
			newOrganization = Factory.NewWithValidTestData<OrgHeader>();
			newOrganization.OH_IsTempAccount = true;
			pickupDeliveryOrg.OrganisationPK = newOrganization.PK;
			AssertEquals(PayableOrderRegistryHelper.TemporaryOrganizationRestrictionMessage, Order.HasRegistryRestrictions());
			// No restriction
			SetOrderApprovalRegistryRestrictions(false, false, false, false);
			Order = TestObjectCreator.CreateNewPendingApprovalPurchaseOrder(10);
			AssertEquals(string.Empty, Order.HasRegistryRestrictions());
		}

		AccPayableOrderHeader Order;
		protected override void SetUp()
		{
			base.SetUp();
			Order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
		}

		void SetOrderApprovalRegistryRestrictions(bool enableCAR, bool enableBSR, bool enableOSD, bool enableTOR)
		{
			var codeDescriptionList = AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.Value;
			codeDescriptionList.Cast<CodeDescriptionBool>().ForEach(x =>
			{
				if (x.Code == PayableOrderRestrictionList.Codes.CreatorApproval)
				{
					x.Bool = enableCAR;
				}

				if (x.Code == PayableOrderRestrictionList.Codes.BlankSuppliers)
				{
					x.Bool = enableBSR;
				}

				if (x.Code == PayableOrderRestrictionList.Codes.OverriddenSupplier)
				{
					x.Bool = enableOSD;
				}

				if (x.Code == PayableOrderRestrictionList.Codes.SupplierIsTempOrg)
				{
					x.Bool = enableTOR;
				}
			}

			);
			AccountingConfigurationRegistry.Instance.PayableOrderApprovalRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeDescriptionList);
		}

		TestObjectCreator testObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}

		void OverrideOrderSupplier()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			Order.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			Order.SupplierDocumentaryAddress.E2_AddressOverride = true;
			Order.SupplierDocumentaryAddress.E2_CompanyName = "FAKE COMPANY";
			Order.SupplierDocumentaryAddress.E2_Address1 = "I DON'T KNOW";
			Order.SupplierDocumentaryAddress.E2_City = "Sydney";
			Order.SupplierDocumentaryAddress.E2_Postcode = "2020";
			Order.SupplierDocumentaryAddress.E2_State = "NSW";
		}
	}

	[UseSnapshotProtection]
	public class AccPayableOrderHeaderNonTransactionalTest : TestCase
	{
		public void TestDontSkipFountainNumbersForAccPayableOrderHeader()
		{
			var factory1 = new BusinessObjectFactory();
			var order1 = factory1.New<AccPayableOrderHeader>();
			factory1.Save();
			AssertEquals("APH_OrderNumber", "PO000001", order1.APH_OrderNumber);
			var order2 = factory1.New<AccPayableOrderHeader>();
			var order3 = factory1.New<AccPayableOrderHeaderWithSaveException>();
			try
			{
				factory1.Save();
				Fail("Exception should be thrown to test unsuccessful saving.");
			}
			catch (NotImplementedException)
			{
			}

			var factory2 = new BusinessObjectFactory();
			var order4 = factory2.New<AccPayableOrderHeader>();
			factory2.Save();
			AssertEquals("APH_OrderNumber", "PO000002", order4.APH_OrderNumber);
			var order5 = factory2.New<AccPayableOrderHeader>();
			factory2.Save();
			AssertEquals("APH_OrderNumber", "PO000003", order5.APH_OrderNumber);
			order3.Delete();
			factory1.Save();
			AssertEquals("APH_OrderNumber", "PO000004", order2.APH_OrderNumber);
		}

		protected class AccPayableOrderHeaderWithSaveException : AccPayableOrderHeader
		{
			public AccPayableOrderHeaderWithSaveException(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaving()
			{
				throw new NotImplementedException();
			}
		}
	}

	[TestedType(typeof(AccPayableOrderHeader))]
	class AccPayableOrderHeaderWorkflowProviderTest : WorkflowProviderTest<AccPayableOrderHeader, AccPayableOrderHeaderProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get
			{
				return new AccPayableOrderHeaderWorkflowDescriptor().Code;
			}
		}
	}
}
