using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DataConverters.Accounting
{
	public class AccountingImportController : ZSingletonController
	{
		public AccountingImportController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new MainForm(new Converter());
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ImportAccountingData; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DummyImporter); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleTemplateCopyNotSupportedException("Can't copy - this is a singleton controller");
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ImportAccountingData; }
		}
	}
}
