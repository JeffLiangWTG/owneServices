using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.GUI.Testing
{
	sealed class DataImporterFormForTest : DataImporterForm
	{
		public DataImporterFormForTest(string formCaption)
			: base(formCaption, BillingInterfaceName.Test)
		{
		}

		public DataImporterFormForTest(DataImporterBusinessObject businessEntity, string formCaption)
			: base(businessEntity, formCaption, BillingInterfaceName.Test)
		{
		}

		public new static DataImporterFormForTest Create(BillingInterfaceName interfaceName)
		{
			return new DataImporterFormForTest(new DataImporterBusinessObject(new BusinessObjectFactory()), null);
		}

		public new ZString ImportFileFilter
		{
			get { return base.ImportFileFilter; }
		}

		public new ZButton CloseButton
		{
			get { return base.CloseButton; }
		}

		public new ZButton ImportFromFileButton
		{
			get { return base.ImportFromFileButton; }
		}

		public new ZCheckBox OnlySaveDataWhenNoRecordsHaveErrorsCheckBox
		{
			get { return base.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox; }
		}

		public event CancelEventHandler BeforeImport;

		protected override bool OnBeforeImport()
		{
			bool result = false;
			if (base.OnBeforeImport())
			{
				if (BeforeImport != null)
				{
					CancelEventArgs e = new CancelEventArgs();
					BeforeImport(this, e);
					result = !e.Cancel;
				}
			}
			return result;
		}
	}
}
