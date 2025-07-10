using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.ZClientCCP.Kawasaki
{
	public class KawasakiInvoiceDataImporter : AUFlatFileInvoiceDataImporter
	{
		public KawasakiInvoiceDataImporter(string fileName, BaseJobDeclaration toJobDec) : base(fileName, toJobDec)
		{
			toJobDec.MakeNonPersistent();
			toJobDec.JobComInvoiceGroupHeaders[0].MakeNonPersistent();
			toJobDec.HasChanges = false;
			toJobDec.Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
		}

		internal KawasakiInvoiceDataFileReader DataReader
		{
			get { return (KawasakiInvoiceDataFileReader)dataReader; }
		}

		protected override void SetDataReader()
		{
			base.dataReader = new KawasakiInvoiceDataFileReader(FileName);
		}

		protected override BaseJobComInvoiceHeader AddNewInvoice()
		{
			BaseJobComInvoiceHeader invHead = base.AddNewInvoice();
			if (invHead != null)
			{
				OrgHeader buyerOrg = OrgHeader.LoadFromCode(factory, "KAWMOTSYD");
				if (buyerOrg != null)
				{
					invHead.JZ_OH_Buyer = buyerOrg.PK;
				}

				invHead.JZ_RX_NKInvoice_Currency = "AUD";
				invHead.JZ_MessageType = "IMP";
			}
			return invHead;
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			foreach (BaseJobComInvoiceHeader invoice in toJobDec.Invoices)
			{
				using (invoice.SuspendSettingHasChanges())
				{
					invoice.JZ_JZ_GroupInvoiceFK = ZGuid.Empty;
					invoice.JZ_JE = ZGuid.Empty;
				}
			}
		}
	}
}

#region Setup
#endregion
