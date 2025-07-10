using System;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseAttachingObjectValidation : ZValidation
	{
		public ImportLicenseAttachingObjectValidation(ImportLicenseAttachingObject parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly ImportLicenseAttachingObject parent;

		public override Type AutoValidationType => typeof(ImportLicenseAttachingObjectValidation);

		public override void ValidateAll()
		{
			ValidateShouldAttach();
		}

		public void ValidateShouldAttach()
		{
			ValidateCalculatedProperty(parent.ShouldAttachInfo);
		}

		protected void CheckShouldAttach()
		{
			if (parent.ShouldAttach && parent.ImportLicenseEntryMRN.Length > 10)
			{
				parent.ShouldAttachInfo.AddError(Res.GetString("D9361B74-8500-479C-9AA7-E23AAAFF8FB8", "The Import License Number cannot exceed the length of 10."));
			}
		}
	}
}
