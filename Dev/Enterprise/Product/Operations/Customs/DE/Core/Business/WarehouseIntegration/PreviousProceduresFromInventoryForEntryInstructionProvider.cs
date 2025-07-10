using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.WarehouseIntegration;

public abstract class PreviousProceduresFromInventoryForEntryInstructionProvider : NonPersistentBusinessObject
{
	public CusEntryInstruction Instruction { get; }

	public PreviousProceduresFromInventoryForEntryInstructionProvider(CusEntryInstruction instruction) : base(instruction.Factory)
	{
		Instruction = instruction;
	}

	public PreviousDocumentMaster PreviousDocumentsMaster => Instruction.PreviousDocumentMaster;

	public bool IsApplicable => IsApplicableCore;

	protected virtual bool IsApplicableCore => Instruction.Warehouse != null && Instruction.JobDeclaration.IsImport && ApplicableLines.Count > 0;

	protected IReadOnlyCollection<JobComInvoiceLine> ApplicableLines => applicableLines ??= GetApplicableLines();
	IReadOnlyCollection<JobComInvoiceLine> applicableLines;
	protected abstract IReadOnlyCollection<JobComInvoiceLine> GetApplicableLines();

	public void CreatePreviousProcedures()
	{
		var previousDocumentMaster = Instruction.PreviousDocumentMaster;
		var authorizationNumberBefore = previousDocumentMaster.AuthorizationNumber;

		var previousDocuments = Instruction.PreviousDocuments;
		previousDocuments.RemoveAndDeleteAll();

		CreatePreviousProceduresCore();

		var result = authorizationNumberBefore.FallbackTo(previousDocumentMaster.GetAuthorizationNumberIfOnlyOneExists());
		previousDocumentMaster.AuthorizationNumber = result;
	}

	protected abstract void CreatePreviousProceduresCore();

	protected PreviousDocument GetPreviousDocumentToPopulate(ZString previousDocumentMasterProcedure)
	{
		PreviousDocument previousDocument;
		var previousDocuments = Instruction.PreviousDocuments;
		if (!previousDocuments.Any())
		{
			PreviousDocumentsMaster.CSI_Procedure = previousDocumentMasterProcedure;
			previousDocument = previousDocuments[0];
		}
		else
		{
			previousDocument = previousDocuments.AddNew();
		}

		return previousDocument;
	}
}
