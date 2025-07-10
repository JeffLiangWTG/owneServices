using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EdiOrgMembershipImporter
	{
		public EdiOrgMembershipImporter() { }

		public void Import()
		{
			var importInfoProvider = new Provider();
			var importInfo = importInfoProvider.ImportCollectionInfo;
			var importFactory = new BusinessObjectFactory();
			var processor = new EdiOrgMembershipFlattenedDataTransferProcessor(EdiOrgMembershipCollection.CreateAdhocCollection(importFactory), importInfo);

			bool isCancelled = false;
			var form = new MultistepDataImportWizardForm(importInfo, importInfoProvider.ContextKey, new DataTransferProcessor[] { processor }, "Memberships");
			form.Cancelled += (s, e) => { processor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayResult(processor, isCancelled); };
			form.Show();
		}

		void DisplayResult(EdiOrgMembershipFlattenedDataTransferProcessor processor, bool isCancelled)
		{
			string mesgs;
			string caption;

			if (isCancelled)
			{
				mesgs = Res.GetString("D4A8CB76-5BC9-4C45-AD52-85E7CE15333B", "No membership was created.");
				caption = Res.GetString("BEE22CC7-3AB6-4CD5-AE67-CF7888357502", "Import canceled");
			}
			else
			{
				mesgs = Res.GetString("EC5A8700-3020-46A7-974F-46BF32B235C5", "Memberships to Import = {0}", processor.HeadersToCreate) + "\r\n";
				mesgs += processor.Log;
				mesgs += "\r\n" + Res.GetString("46EE8DA6-1423-4460-B788-FA32CD242F98", "TOTAL: Memberships created = {0}, Memberships excluded = {1}",
					processor.HeadersCreated, processor.HeadersExcluded) + "\r\n";

				caption = Res.GetString("D57B2A88-BDA7-4CC4-95E1-8BF1E01838A6", "Import completed");
			}

			using (ZMessageBox notification = new ZMessageBox(mesgs, caption, MessageBoxButtons.OK, MessageBoxIcon.Information))
			{
				notification.ShowDialog();
			}
		}

		class Provider : IImportCollectionInfoProvider
		{
			public IImportCollectionInfo ImportCollectionInfo
			{
				get
				{
					if (collectionInfo == null)
					{
						var collectionFactory = new BusinessObjectFactory();
						collection = collection ?? new EdiOrgMembershipFlattenedCollection(collectionFactory);
						collectionInfo = new EdiOrgMembershipImportInfo(collection);
					}
					return collectionInfo;
				}
			}
			ImportCollectionInfoImpl collectionInfo;
			protected EdiOrgMembershipFlattenedCollection collection;

			public string ContextKey
			{
				get { return "{3E55912E-1CE3-4398-8ACF-EC1C3E9C7349}"; }
			}
		}
	}
}
