using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	class EUCustomsAttachedDeclarationDataCreator : ICustomsAttachedDeclarationDataCreator
	{
		public ZGuid CreateAttachedDeclarationData(ZGuid shipmentPk, bool isCancelled)
		{
			var factory = new BusinessObjectFactory();

			var jobDeclaration = factory.New<Enterprise.Integration.Customs.EU.IJobDeclaration>();
			jobDeclaration.JE_JS = shipmentPk;

			if (isCancelled)
			{
				jobDeclaration.IsCancelled = true;
			}

			var jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
			_ = jobComInvoiceHeader.AddNewInvoiceLine();

			factory.Save();

			return jobDeclaration.PK;
		}
	}
}
