using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public class VoucherSection : FileSection
	{
		public VoucherSection(BusinessObjectFactory factory)
			: base(factory)
		{
			fSectionHeader = "[凭证]";
			fFileName = "VOUCHER.TXT";
		}

		protected override void Define()
		{
			base.Define();
			AccountNumberLine = new FileDefinitionLine(FileLineType.AlphaNumeric);
			VoucherDateLine = new FileDefinitionLine(FileLineType.Date);
			DebitAmountLine = new FileDefinitionLine(FileLineType.Numeric);
			CreditAmountLine = new FileDefinitionLine(FileLineType.Numeric);
			VoucherTypeLine = new FileDefinitionLine(FileLineType.AlphaNumeric);
			VoucherNumberLine = new FileDefinitionLine(FileLineType.AlphaNumeric);
			AttachmentLine = new FileDefinitionLine(FileLineType.Numeric);
			AdditionalDescirptionLine = new FileDefinitionLine(FileLineType.AlphaNumeric);
		}

		protected override void SetValue()
		{
			base.SetValue();

			AccountNumberLine.Field = "科目代码";
			AccountNumberLine.Length = 13;

			VoucherDateLine.Field = "凭证日期";

			DebitAmountLine.Field = "借方金额";
			DebitAmountLine.Length = 15;
			DebitAmountLine.Precision = 2;

			CreditAmountLine.Field = "贷方金额";
			CreditAmountLine.Length = 15;
			CreditAmountLine.Precision = 2;

			VoucherTypeLine.Field = "凭证类型";
			VoucherTypeLine.Length = 6;

			VoucherNumberLine.Field = "凭证编号";
			VoucherNumberLine.Length = 8;

			AttachmentLine.Field = "附件";
			AttachmentLine.Length = 3;
			AttachmentLine.Precision = 0;

			AdditionalDescirptionLine.Field = "摘要";
			AdditionalDescirptionLine.Length = 10;
		}

		protected override void AddLines()
		{
			base.AddLines();
			Lines.Add(AccountNumberLine);
			Lines.Add(VoucherDateLine);
			Lines.Add(DebitAmountLine);
			Lines.Add(CreditAmountLine);
			Lines.Add(VoucherTypeLine);
			Lines.Add(VoucherNumberLine);
			Lines.Add(AttachmentLine);
			Lines.Add(AdditionalDescirptionLine);
		}

		FileDefinitionLine AccountNumberLine;
		FileDefinitionLine VoucherDateLine;
		FileDefinitionLine VoucherTypeLine;
		FileDefinitionLine VoucherNumberLine;
		FileDefinitionLine DebitAmountLine;
		FileDefinitionLine CreditAmountLine;
		FileDefinitionLine AttachmentLine;
		FileDefinitionLine AdditionalDescirptionLine;
	}
}
