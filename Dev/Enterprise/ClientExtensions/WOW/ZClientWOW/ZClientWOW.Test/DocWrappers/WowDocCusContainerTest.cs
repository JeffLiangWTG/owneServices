using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowDocCusContainer))]
	class WowDocCusContainerTest : DocBaseCusContainerAbstractTest<WoolworthsCusContainer, WowDocCusContainer>
	{
		public void TestWareHouseLocation()
		{
			var jobDec = DeclarationInternal;
			jobDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			jobDec.JE_RL_NKOrigin = "HKHKG";
			jobDec.JE_RL_NKFinalDestination = "AUSYD";
			jobDec.JE_MasterBill = "MAWB";
			var invoice = jobDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomAttrib4 = ContainerInternal.CO_ContainerNumber;
			invoiceLine.JI_PartNo = "product";
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "order1";
			var orderline = order.OrderLines.AddNew();
			orderline.JO_Partno = invoiceLine.JI_PartNo;
			invoiceLine.JI_OrderNumber = order.JD_OrderNumber;
			invoiceLine.JI_CustomDecimal1 = new ZDecimal(orderline.JO_LineNo);
			var delivery = orderline.Deliveries.AddNew();
			delivery.J4_OA_NKDeliveryPoint = "1000";
			delivery.J4_RL_NKDestinationPort = jobDec.JE_RL_NKFinalDestination;
			var deliveryCont = delivery.Containers.AddNew();
			deliveryCont.J5_ContainerNum = ContainerInternal.CO_ContainerNumber;
			deliveryCont.J5_MasterBill = "MAWB";
			AssertEquals("WareHouseLocation", "1000", ContainerWrapperInternal.WareHouseLocation);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override WowDocCusContainer CreateContainerWrapper(WoolworthsCusContainer containerInternal, Enterprise.Customs.Business.BaseJobDeclaration declarationInternal)
		{
			return (WowDocCusContainer)WowDocCusContainer.New(ContainerInternal, (JobDeclaration)declarationInternal, Factory);
		}

		#endregion
	}
}
