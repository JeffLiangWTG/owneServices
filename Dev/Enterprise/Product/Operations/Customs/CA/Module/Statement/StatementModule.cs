using System;
using Enterprise.Customs.CA.GUI;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public abstract class StatementModule : ZFilterGridModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new StatementFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override void AddExtraImportExportMenuItems()
		{
			base.AddExtraImportExportMenuItems();

			if (CACustomsDataRegistry.Instance.EnableImportUniversalTransactionBatchXMLFiles.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AddImportDataMenuItem(Res.GetString("50E96B41-60BC-4709-B96B-1C16BE892851", "Universal Transaction Batch XML"), ImportXmlFromFiles, false);
			}
		}

		void ImportXmlFromFiles(object sender, EventArgs eventArgs)
		{
			using (var frm = new TransactionBatchXmlDataImportForm())
			{
				ZFormModaliser.ShowDialogAndDispose(frm);
			}
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		public override bool SupportsWorkflow => true;

		public override bool AllowEdit => true;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;
	}
}
