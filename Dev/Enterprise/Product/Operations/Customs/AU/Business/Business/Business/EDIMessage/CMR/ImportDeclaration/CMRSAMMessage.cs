using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSAMMessage : CMRImportDeclarationMessage
	{
		public CMRSAMMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString GetReport()
		{
			StringBuilder reportBuilder = new StringBuilder();
			reportBuilder.Append(base.GetReport());
			reportBuilder.Append(GetMessageAdviceErrors());
			return reportBuilder.ToString();
		}

		public override ZString StatusOfLinesReport
		{
			get { return ZString.Empty; }
		}

		protected override ZString AdditionalLineReference(ZString lineNumber)
		{
			return !lineNumber.IsEmpty && lineNumber != "0" ? "LINE " + lineNumber : string.Empty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.SAM;
		}
	}
}
