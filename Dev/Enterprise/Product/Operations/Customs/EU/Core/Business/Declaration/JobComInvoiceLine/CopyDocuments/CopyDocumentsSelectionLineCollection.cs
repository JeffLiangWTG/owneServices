using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration;
public class CopyDocumentsSelectionLineCollection : NonPersistentBusinessObjectCollection<CopyDocumentsSelectionLine>
{
	public CopyDocumentsSelectionLineCollection(JobComInvoiceLine line) : base(line.Factory)
	{
		using (SuspendSettingHasChanges())
		{
			foreach (var supportingDocument in line.SupportingDocuments)
			{
				Add(new CopyDocumentsSelectionLine(supportingDocument));
			}

			foreach (var additionalInfo in line.AdditionalInfos)
			{
				Add(new CopyDocumentsSelectionLine(additionalInfo));
			}
		}
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException();

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;
}
