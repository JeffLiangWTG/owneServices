using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CO.Manifest.Module
{
	public class DocumentIDsModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CO.DocumentIDs;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CO.DocumentIDs);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new DocumentIDsFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new DocumentIDsFilterControl(GridCollection, (DocumentIDsFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new DocumentIDsCollection(Factory, new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.ColombiaManifest));

		public override bool AllowDelete => false;
		public override bool AllowEdit => false;
		public override bool AllowNew => false;
		public override bool AllowView => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem(Res.GetString("CAAF3E9C-BA05-4134-AD55-DDF0B481EFF8", "number's file"), new EventHandler(ImportNumbersFile));
		}

		void ImportNumbersFile(object sender, EventArgs e)
		{
			try
			{
				using (var dialog = new ZOpenFileDialog())
				{
					dialog.CheckFileExists = true;
					dialog.Title = (NoResString)"Upload txt file";
					dialog.DefaultExt = (NoResString)"txt";
					dialog.Filter = (NoResString)"Text Only (*.txt)|*.txt";

					if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
					{
						using (var sr = new StreamReader(dialog.OpenFile()))
						{
							var info = ZString.Empty;
							var docIds = sr.ReadToEnd().Trim().Replace("[", "").Replace("]", "").Replace(" ", "").Split(',');
							if (docIds[0] != null && docIds[0].Length > 0)
							{
								var existIDs = GetExistsDocumentIds();
								var needSaving = true;
								foreach (var docId in docIds)
								{
									if (existIDs.Contains(docId))
									{
										needSaving = false;
										break;
									}
									else
									{
										CreateDocumentID(docId);
									}
								}

								if (needSaving)
								{
									Factory.Save();
									info = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} Document ID(s) have been saved.", docIds.Length);
								}
								else
								{
									info = string.Format(CultureInfo.InvariantCulture, (NoResString)"We can not upload this file. We found at least one document id in the system.\nCheck if it has been uploaded before.");
								}
							}
							else
							{
								info = string.Format(CultureInfo.InvariantCulture, (NoResString)"The file is empty.");
							}
							Globals.Message.ShowInformation(info, (NoResString)"Import Document ID(s)");
						}
					}
				}
			}
			catch (Exception ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		List<ZString> GetExistsDocumentIds()
		{
			var query = new ZQuery(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.ColombiaManifest);
			query.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False);
			query.AddToFilter(CusTransactionNumberSchema.TN_GC_Company, GlbCompany.CurrentCompany.PK);
			return Factory.Load<CusTransactionNumber>(query).Select(x => x.TN_TransactionReference).ToList();
		}

		void CreateDocumentID(ZString docId)
		{
			var transactionNumber = Factory.New<CusTransactionNumber>();
			transactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			transactionNumber.TN_TransactionReference = docId;
			transactionNumber.TN_GC_Company = GlbCompany.CurrentCompany.PK;
		}
	}
}
