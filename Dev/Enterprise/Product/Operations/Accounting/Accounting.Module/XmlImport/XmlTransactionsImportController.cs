using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class XmlTransactionsImportController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.XmlTransactionsImport; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override IZForm ShowNewForm()
		{
			var	transferDirector = new MultipleInvoiceXmlDataTransferDirector(true);
			transferDirector.PromptUserAndImport(BillingInterfaceName.TransactionsXmlImport);
			return null;
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleTemplateCopyNotSupportedException("You cannot Show a Template CopyForm for a Transaction Import");
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ImportXmlAccountingTransactions; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
