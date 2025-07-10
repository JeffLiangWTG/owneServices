
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Matching
{
	/// <summary>
	/// Class to encapsulate creation of Transfer objects for purposes of Matching.
	/// </summary>
	public abstract class TransferCreator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TransferCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public abstract Transfer GetNewTransfer();

		public abstract ZString GetTransferDescription();
		public abstract ZByte GetTransferAH_NumberOfSupportingDocuments();

		// Given a primary organization and a sub-balance from a different organization, 
		// creates the necessary Transfer from the primary organization to the other org.  
		#region CreateTransfer

		public Transfer CreateTransfer(ZGuid primaryOrg, OrganizationSubBalance subBalance)
		{
			Transfer newTransfer = GetNewTransfer();
			OrgHeader fromOrg = Factory.Load<OrgHeader>(subBalance.Organization);
			OrgHeader toOrg = Factory.Load<OrgHeader>(primaryOrg);

			ZString orgDescription = ZString.Empty;
			ZString orgDescription1 = " " + Res.GetString("17af6f09-d7c4-4e77-9e8a-f675a6994b56", "(SYSTEM GENERATED)") + " ";
			ZByte orgAH_NumberOfSupporting = GetTransferAH_NumberOfSupportingDocuments();
			if (fromOrg != null && toOrg != null)
			{
				orgDescription = " " + Res.GetString("ed86df8b-782f-4354-9d8b-1c5f433fc55f", "FROM {0} TO {1}", fromOrg.OH_Code, toOrg.OH_Code);
			}

			newTransfer.TransferFrom.AH_OH = subBalance.Organization;
			newTransfer.TransferFrom.AH_InvoiceAmount = -subBalance.Amount;
			newTransfer.TransferFrom.AH_OSTotal = -subBalance.Amount;
			newTransfer.TransferFrom.AH_OutstandingAmount = -subBalance.Amount;
			newTransfer.TransferFrom.AH_Desc = GetTransferDescription();
			newTransfer.TransferFrom.AH_Desc += orgDescription + orgDescription1;
			newTransfer.TransferFrom.AH_TransactionCreatedByMatching = true;
			newTransfer.TransferFrom.AH_NumberOfSupportingDocuments = orgAH_NumberOfSupporting;

			newTransfer.TransferTo.AH_OH = primaryOrg;
			newTransfer.TransferTo.AH_InvoiceAmount = subBalance.Amount;
			newTransfer.TransferTo.AH_OSTotal = subBalance.Amount;
			newTransfer.TransferTo.AH_OutstandingAmount = subBalance.Amount;
			newTransfer.TransferTo.AH_Desc = GetTransferDescription();
			newTransfer.TransferTo.AH_Desc += orgDescription + orgDescription1;
			newTransfer.TransferTo.AH_NumberOfSupportingDocuments = orgAH_NumberOfSupporting;
			newTransfer.TransferTo.AH_TransactionCreatedByMatching = true;

			return newTransfer;
		}

		#endregion
	}
}