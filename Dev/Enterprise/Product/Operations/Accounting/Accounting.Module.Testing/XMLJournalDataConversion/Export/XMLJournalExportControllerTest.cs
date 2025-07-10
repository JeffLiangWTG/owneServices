using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(XMLJournalExportController))]
	public class XMLJournalExportControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.XmlJournalExport;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return null;
		}

		public override void TestNewForm()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = new TestObjectCreator(Factory).GLHeader1.PK;
			line.FillWithValidTestData();
			Factory.Save();
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			XMLJournalExportController testController = (XMLJournalExportController)ZControllerFactory.Create(GetControllerID());

			AccGLHeader gLHeader = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "1000.00.00");
			AssertNotNull(gLHeader);
			AccountingConfigurationRegistry.Instance.APJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid());

			using (testController.ShowNewForm())
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AccountingConfigurationRegistry.Instance.APJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			using (testController.ShowNewForm())
			{
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AccountingConfigurationRegistry.Instance.APJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid());

			using (testController.ShowNewForm())
			{
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AccountingConfigurationRegistry.Instance.APJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			using (testController.ShowNewForm())
			{
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
