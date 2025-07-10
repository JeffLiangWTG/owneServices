namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryHeaderValidation : EU.Business.Declaration.CusEntryHeaderValidation
{
	public CusEntryHeaderValidation(CusEntryHeader parent) : base(parent)
	{
	}

	public new CusEntryHeader Parent => (CusEntryHeader)base.Parent;

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		CheckLinesCount();
	}

	public void CheckLinesCount() => CheckLinesCountCore();

	protected virtual void CheckLinesCountCore()
	{
		var linesCountLimit = GetEntryLinesCountLimit();
		if (Parent.MergedLines.Count > linesCountLimit)
		{
			Parent.AddRowMessageError(ValidationCaptions.CusEntryHeader.TheMaximumNumberOfEntryLinesHasBeenExceeded(linesCountLimit));
		}
	}

	protected virtual int GetEntryLinesCountLimit()
	{
		var parentDeclaration = Parent?.Declaration;
		if (parentDeclaration?.IsImport ?? false)
		{
			return Ucc6XmlConstants.EntryHeader.ImportEntryLinesCountLimit;
		}

		if (parentDeclaration?.IsUCC6AndIsExport ?? false)
		{
			return Ucc6XmlConstants.EntryHeader.ExportEntryLinesCountLimit;
		}

		return SADConstants.CustomsFieldMaxLength.EntryHeader.EntryLinesCountLimit;
	}
}
