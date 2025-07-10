#if DEBUG

using System.IO;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class DirectDebitBatchHeader
	{
		public bool IsValidatingBatchForFileGeneration_ForTestOnly
		{
			get { return IsValidatingBatchForFileGeneration; }
			set { IsValidatingBatchForFileGeneration = value; }
		}

		public DirectDebitBatchHeaderValidation DDRValidation_ForTestOnly => DDRValidation;

		public DDRFileGenerator GetDDRFileGenerator_ForTestOnly(TextWriter writer, string format)
		{
			return GetDDRFileGenerator(writer, format);
		}

		public bool FIsSettingLineBatchNo_ForTestOnly
		{
			get { return fIsSettingLineBatchNo; }
			set { fIsSettingLineBatchNo = value; }
		}
	}
}

#endif
