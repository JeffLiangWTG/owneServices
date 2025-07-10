using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	/// <summary>
	/// Class to help copy containers-for-invoices pivot information from one declaration to another. 
	/// Use this after you clone a Dec to ensure your new one has the same containers-for-invoices info.
	/// </summary>
	public class InvoicesForContainersPivotCloner
	{
		readonly JobDeclaration oldDec;
		readonly JobDeclaration newDec;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="oldDecFromWhichToCopyPivots">Your source declaration, the one you are cloning</param>
		/// <param name="newDecToAddPivotsTo">Target dec, the new one you have just made, to which you want to add pivot info.</param>
		public InvoicesForContainersPivotCloner(JobDeclaration oldDecFromWhichToCopyPivots, JobDeclaration newDecToAddPivotsTo)
		{
			this.oldDec = oldDecFromWhichToCopyPivots;
			this.newDec = newDecToAddPivotsTo;
		}

		/// <summary>
		/// Main cloning method. Call this to actually do the copying. It will modify the dclaration to which "newDecToAddPivotsTo" is a reference.
		/// </summary>
		public void CloneContainerInvoicePivots()
		{
			foreach (JobComInvoiceLine oldInvLine in oldDec.InvoiceLines)
			{
				foreach (NonPersistentCusContainer contForInvoice in oldInvLine.ContainersForInvoiceLinesForBindingOnly)
				{
					if (contForInvoice.IsForInvoiceLine)
					{
						GetNewContIdAndInvIdFromOldAndMakeNewPivotOnNewDecInvLine(oldInvLine, (CusContainer)(contForInvoice.Container), newDec);
					}
				}
			}
		}

		void GetNewContIdAndInvIdFromOldAndMakeNewPivotOnNewDecInvLine(JobComInvoiceLine oldInv, CusContainer oldCont, JobDeclaration newDec)
		{
			ZGuid newContId = ZGuid.Empty;

			foreach (CusContainer newCont in newDec.CusContainers)
			{
				if (newCont.CO_ContainerNumber == oldCont.CO_ContainerNumber)
				{
					newContId = newCont.PK;
					break;
				}
			}

			if (newContId == ZGuid.Empty)
			{
				return;  // Only create a new pivot if we have a container to pivot upon
			}

			foreach (JobComInvoiceLine newInvLine in newDec.InvoiceLines)
			{
				if (newInvLine.JI_LineNo == oldInv.JI_LineNo && newInvLine.InvoiceNumber == oldInv.InvoiceNumber)
				{
					CusContainerInvoiceLinePivot newPivot = newDec.Factory.New<CusContainerInvoiceLinePivot>();
					newPivot.C2_CO = newContId;
					newPivot.C2_JI = newInvLine.PK;
					newPivot.ClearHasChanges();
					newInvLine.ContainersPivot.Add(newPivot);
				}
			}
		}
	}
}
