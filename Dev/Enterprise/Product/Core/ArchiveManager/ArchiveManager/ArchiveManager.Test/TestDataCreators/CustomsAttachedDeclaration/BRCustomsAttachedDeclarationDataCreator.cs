using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Customs.BR.Business;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	class BRCustomsAttachedDeclarationDataCreator : ICustomsAttachedDeclarationDataCreator
	{
		public ZGuid CreateAttachedDeclarationData(ZGuid shipmentPk, bool isCancelled)
		{
			var factory = new BusinessObjectFactory();

			var jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_JS = shipmentPk;
			jobDeclaration.BoardingOfficeIsCustomsEnclosure = false;

			if (isCancelled)
			{
				jobDeclaration.IsCancelled = true;
			}

			var jobComInvoiceHeader = jobDeclaration.Invoices.AddNew();
			_ = jobComInvoiceHeader.InvoiceLines.AddNew();

			factory.Save();

			return jobDeclaration.PK;
		}
	}
}
