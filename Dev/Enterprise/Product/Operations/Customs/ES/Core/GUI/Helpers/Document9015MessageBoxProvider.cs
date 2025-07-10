using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.GUI
{
	public class Document9015MessageBoxProvider : IDocument9015MessageBoxProvider
	{
		public Document9015MessageBoxProvider()
		{
		}

		public bool AskIfShouldRemove9015Documents(ZString entryNumber)
		{
			return Globals.Message.Show(
				Res.GetString("D3A1246A-95AB-4B20-BE08-E8607806320B", @"For Entry {0} there is a document 9015 in Supporting Documents to request a 50% VAT guaranteed amount reduction, but this declaration doesn't match criteria for this request. Do you want to remove 9015 documents?", entryNumber),
				Res.GetString("79542DC9-410E-48F3-8ADE-F40FC866942F", "9015 Documents"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				DialogResult.No) == DialogResult.Yes;
		}
	}
}
