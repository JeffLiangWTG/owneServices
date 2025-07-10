using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.TIP.DocWrappers.Testing
{
	[TestedType(typeof(DocTIPBatchARInvoiceLineTransactionLine))]
	public class DocTIPBatchARInvoiceLineTransactionLineTest : DocumentWrapperTestCase
	{
		public void TestOtherReferenceForIsLocalCartage_WithoutContainerDetails()
		{
			RefContainer containerType1 = Factory.New<RefContainer>();
			RefContainer containerType2 = Factory.New<RefContainer>();
			containerType1.RC_Code = "20FT";
			containerType2.RC_Code = "40FT";
			CommonCartage localTransport = Factory.New<CommonCartage>();
			localTransport.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			localTransport.ContainerBookedMoves.AddNew();
			localTransport.ContainerBookedMoves.AddNew();
			localTransport.Containers.ElementAt(0).JC_RC = containerType1.PK;
			localTransport.Containers.ElementAt(0).JC_ContainerNum = "CON1";
			localTransport.Containers.ElementAt(1).JC_RC = containerType2.PK;
			localTransport.Containers.ElementAt(1).JC_ContainerNum = "CON2";
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			job.JH_ParentID = localTransport.PK;
			Line.AL_JH = job.PK;
			InvoiceLineWrapper = DocTIPBatchARInvoiceLineTransactionLine.New(Line, Factory);
			AssertNotEquals("CON1:20FT CON2:40FT ", InvoiceLineWrapper.OtherReference);
		}

		public void TestOtherReferenceForCustomsJob_WithoutContainerNo()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "OrderNo";
			shipment.AttachedOrders.Add(order);
			BaseJobDeclaration customs = Factory.New<BaseJobDeclaration>();
			customs.JE_OwnerRef = "Owner";
			customs.JE_JS = shipment.PK;
			customs.CusContainers.AddNew();
			customs.CusContainers.AddNew();
			customs.CusContainers[0].CO_ContainerNumber = "CONT1";
			customs.CusContainers[0].CO_FCL_LCL_AIR = "FCL";
			customs.CusContainers[1].CO_ContainerNumber = "CONT2";
			customs.CusContainers[1].CO_FCL_LCL_AIR = "LCL";
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_ParentID = customs.PK;
			Line.AL_JH = job.PK;
			InvoiceLineWrapper = DocTIPBatchARInvoiceLineTransactionLine.New(Line, Factory);
			AssertEquals("Owner,OrderNo", InvoiceLineWrapper.OtherReference);
		}

		#region Overrides
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { InvoiceLineWrapper };
		}

		#endregion
		#region Implementation
		protected InvoicingLineBase Line;
		protected ARInvoice Invoice;
		protected DocTIPBatchARInvoiceLineTransactionLine InvoiceLineWrapper;
		protected override void SetUp()
		{
			Line = Factory.New<ARInvoiceLine>();
			Line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Invoice = Factory.New<ARInvoice>();
			Invoice.Lines.Add(Line);
			InvoiceLineWrapper = DocTIPBatchARInvoiceLineTransactionLine.New(Line, Factory);
			Factory.Save();
			base.SetUp();
		}
		#endregion
	}
}
