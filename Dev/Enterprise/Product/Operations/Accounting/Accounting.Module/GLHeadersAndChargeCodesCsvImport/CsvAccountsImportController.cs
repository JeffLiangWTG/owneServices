using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.Accounting.GUI.ImportAccountsControllerForm;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CsvAccountsChartImportController : ZSingletonController
	{
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ImportCsvChartOfAccounts; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CsvAccountsImport; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override IZForm ShowNewForm()
		{
			ZFormModaliser.ShowDialogAndDispose(GetNewForm());
			return null;
		}

		static ImportAccountsControllerForm GetNewForm()
		{
			var businessEntity = new AccountsImportBusinessObject(new BusinessObjectFactory());
			return new ImportAccountsControllerForm(businessEntity);
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleTemplateCopyNotSupportedException("You cannot Show a Template CopyForm for an Account Import");
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#region Test
#if DEBUG

		public Form GetNewForm_ForTest()
		{
			return GetNewForm();
		}

#endif
		#endregion

	}
}
