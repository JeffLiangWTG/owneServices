using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	sealed class USCustomsAttachedDeclarationDataCreator : ICustomsAttachedDeclarationDataCreator
	{
		public ZGuid CreateAttachedDeclarationData(ZGuid shipmentPk, bool isCancelled)
		{
			var factory = new BusinessObjectFactory();

			var jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_JS = shipmentPk;

			if (isCancelled)
			{
				jobDeclaration.IsCancelled = true;
			}

			var houseBill = jobDeclaration.Bills.AddNew();
			var jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
			jobComInvoiceHeader.JZ_CU_RelatedHouseBill = houseBill.PK;

			var cusEntryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();

			var orgAddress = factory.New<OrgAddress>();
			orgAddress.OA_OH = Env.CurrentCompany.OrganisationPK;
			orgAddress.OA_Address1 = "Eugene Leroy Street ";

			var jobOrderHeader = factory.New<Order>();
			jobOrderHeader.JD_JE = jobDeclaration.PK;
			jobOrderHeader.JD_OA_BuyerAddress = orgAddress.PK;

			_ = jobOrderHeader.OrderLines.AddNew();

			var jobOrderHeaders = factory.Load<Order>(new ZQuery(JobOrderHeaderSchema.JD_JS, shipmentPk));
			foreach (var order in jobOrderHeaders)
			{
				order.JD_JE = jobDeclaration.PK;

				foreach (var orderLine in order.OrderLines)
				{
					var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
					var invoiceLine = jobComInvoiceHeader.InvoiceLines.AddNew();
					invoiceLine.JI_JO = orderLine.PK;
					invoiceLine.JI_CL = cusEntryLine.PK;
				}
			}

			factory.Save();

			return jobDeclaration.PK;
		}
	}
}
