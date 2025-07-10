using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusEntryHeader : CusEntryHeader
	{
		public UPECusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString CH_Status
		{
			get
			{
				return base.CH_Status;
			}
			set
			{
				amendmentCleared = IsStatusChangingFromAmendmentPendingToCleared(CH_Status, value);
				base.CH_Status = value;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded &&
				IsInDatabase &&
				amendmentCleared)
			{
				EmailElectronicCreditNoteAndRequiredDocuments();
				amendmentCleared = false;
			}
			base.OnSaved(saveSucceeded);
		}

		void EmailElectronicCreditNoteAndRequiredDocuments()
		{
			new UPEElectronicCreditNoteAutoDelivery((UPEJobDeclaration)Declaration).Deliver();
		}
		ZBool amendmentCleared = false;
	}
}
