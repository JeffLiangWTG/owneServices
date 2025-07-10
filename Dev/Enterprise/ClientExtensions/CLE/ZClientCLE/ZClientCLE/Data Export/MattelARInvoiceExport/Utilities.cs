
using CargoWise.Types;

namespace Enterprise.Client.CLE.MattelARInvoiceExport
{
	public class Utilities
	{
		#region OutputFileName

		public ZString GetFileName(ZString jobNumber)
		{
			return ("INV" + Separator + GetDateForFileName() + Separator + jobNumber + FileExtension);
		}

		public ZString FileExtension
		{
			get { return ".EDI"; }
		}

		ZString GetDateForFileName()
		{
			return ZDateTime.Now.ToString("yyyyMMddHHmm");
		}

		#endregion

		const string Separator = "_";
	}
}
