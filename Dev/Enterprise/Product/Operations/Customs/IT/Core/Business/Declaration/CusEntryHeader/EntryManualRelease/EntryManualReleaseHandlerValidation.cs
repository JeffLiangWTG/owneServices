using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryManualReleaseHandlerValidation : ZValidation
{
	public EntryManualReleaseHandlerValidation(EntryManualReleaseHandler parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		ValidateReleaseCode();
		ValidateReleaseDate();
	}

	public override Type AutoValidationType => typeof(EntryManualReleaseHandlerValidation);

	public void ValidateReleaseCode()
	{
		ValidateCalculatedProperty(Parent.ReleaseCodeInfo);
	}

	public void ValidateReleaseDate()
	{
		ValidateCalculatedProperty(Parent.ReleaseDateInfo);
	}

	protected virtual void CheckReleaseCode()
	{
		MandatoryValidation.CheckEntered(Parent.ReleaseCodeInfo);
	}

	protected virtual void CheckReleaseDate()
	{
		MandatoryValidation.CheckEntered(Parent.ReleaseDateInfo);

		if (!Parent.ReleaseDate.IsValid)
		{
			Parent.ReleaseDateInfo.AddError(ValidationCaptions.EntryManualReleaseCaptions.InvalidDate);
		}
	}

	EntryManualReleaseHandler Parent => (EntryManualReleaseHandler)ParentFilter;
}
