using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CommonPreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
	{
		public CommonPreviousDocumentValidation(PreviousDocument parent) : base(parent) { }

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

		const string RuleBR2017DateFormat = "yyyyMMdd";

		protected override void CheckCSI_ReferenceNumber()
		{
			var parent = Parent;

			if (parent.CSI_Code == PreviousDocumentTypeList.Codes.ReferenceDateOfEntryInTheDeclarantRecords)
			{
				if (!(ZDateTime.TryParseExact(parent.CSI_ReferenceNumber.Left(8), out var parsedDateTime, RuleBR2017DateFormat))
					|| parsedDateTime.IsInTheFutureDatePartOnly
					|| string.IsNullOrWhiteSpace(parent.CSI_ReferenceNumber.SubstringSafe(8, 1))
				)
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("45C74B7B-8E32-4F62-A21E-12C26BCCEA82", "[BR2017] Please enter a valid Reference Number in the format of '{0}' + 'EIDR Reference' with no space in between the two components. The entered date must not be a future date.", RuleBR2017DateFormat));
				}
			}

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();
			if (Parent.CSI_Code == PreviousDocumentTypeList.Codes.ReferenceDateOfEntryInTheDeclarantRecords && Parent.CSI_DateOfIssue.IsInThePastDatePartOnly)
			{
				Parent.CSI_DateOfIssueInfo.AddMessageError(Res.GetString("fe14c1ef-a0c2-4aab-8569-55fed2711b2b", "Date of Issue Cannot be earlier than Today."));
			}
		}

		protected override void CheckCSI_SubType() { }
	}
}
