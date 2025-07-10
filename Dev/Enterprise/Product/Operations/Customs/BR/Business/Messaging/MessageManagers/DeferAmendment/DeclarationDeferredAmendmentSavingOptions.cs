using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class DeclarationDeferredAmendmentSavingOptions : DeferredAmendmentSavingOptions, IDeferredAmendmentSavingOptions
	{
		public DeclarationDeferredAmendmentSavingOptions(JobDeclaration declaration) : base()
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		public bool QueueForSendAmendment => SaveWithEntryChanges && declaration.IsExport;

		ZBool IDeferredAmendmentSavingOptions.ShouldTakeReasonForSavingWithoutSendingSeparately => SendAmendment || QueueForSendAmendment;
	}
}
