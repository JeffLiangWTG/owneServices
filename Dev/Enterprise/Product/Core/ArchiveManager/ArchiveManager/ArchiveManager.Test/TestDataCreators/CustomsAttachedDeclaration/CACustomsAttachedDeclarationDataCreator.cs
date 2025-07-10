using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CA;
using ConfigCategories = Enterprise.Customs.Universal.RefCusRulingConfigCategories.Codes;
using ConfigTypes = Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	sealed class CACustomsAttachedDeclarationDataCreator : ICustomsAttachedDeclarationDataCreator
	{
		public ZGuid CreateAttachedDeclarationData(ZGuid shipmentPk, bool isCancelled)
		{
			var factory = new BusinessObjectFactory();

			var jobDeclaration = factory.New<IJobDeclaration>();
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

					var invoiceLine = jobComInvoiceHeader.AddNewInvoiceLine();
					invoiceLine.JI_JO = orderLine.PK;
					invoiceLine.JI_CL = cusEntryLine.PK;

					var cusRuling = factory.New<ICusRuling>();
					cusRuling.ZZX_RulingType = "A";
					cusRuling.ZZX_Description = "B";
					cusRuling.ZZX_RulingNumber = "1";

					var cusRulingConfig = cusRuling.Configurations.AddNew();
					cusRulingConfig.ZZY_Value = "1";
					cusRulingConfig.ZZY_JI_InvoiceLine = invoiceLine.PK;
					cusRulingConfig.ZZY_ZZX_CusRuling = ZGuid.Empty;
					cusRulingConfig.ZZY_Category = ConfigCategories.GST;
					cusRulingConfig.ZZY_Type = ConfigTypes.TreatmentCode;
				}
			}

			factory.Save();

			return jobDeclaration.PK;
		}
	}
}
