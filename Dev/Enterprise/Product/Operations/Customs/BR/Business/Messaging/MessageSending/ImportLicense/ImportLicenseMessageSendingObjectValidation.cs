using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public ImportLicenseMessageSendingObjectValidation(ImportLicenseMessageSendingObject parent) : base(parent)
		{
		}

		public new ImportLicenseMessageSendingObject Parent => base.Parent as ImportLicenseMessageSendingObject;

		protected override void CheckShouldSend()
		{
			if (Parent.ShouldSend)
			{
				var parentEntryLines = Parent.Header.MergedLines;

				if (!parentEntryLines.Any())
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("92A1D649-8FC2-4D39-B765-2FEA4551651B", "Message should only be sent if the Entry Header contains at least one Entry Line"));
				}

				if (Parent.CustomsStatus == BRMessageStatusList.Codes.Accepted)
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("3E485702-F80E-4E0B-B1F1-471801769126", "Original Message has already been sent. License already contains a License number"));
				}
			}
		}
	}
}
