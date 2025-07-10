using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPAYEXCMessage : CMRImportDeclarationMessage
	{
		public CMRPAYEXCMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.PAYEXC;
		}

		public override ZString GetReport()
		{
			ZStringBuilder reportBuilder = new ZStringBuilder();

			reportBuilder.Append(base.GetReport());

			CusEntryHeader entryHeader = EM_LinkedObject as CusEntryHeader;
			if (entryHeader != null && entryHeader.Declaration != null && entryHeader.Declaration.Importer != null)
			{
				reportBuilder.Append("\r\nThe Electronic Funds Transfer (EFT) payment for this entry has failed as the EFT limit for the Importer ");
				reportBuilder.Append(entryHeader.Declaration.Importer.OH_FullNameTruncated);
				reportBuilder.Append(" has been exceeded.");
				reportBuilder.Append("\r\nPayment may be made alternatively by making the payment party on the Misc Options tab 'Broker'.");
				reportBuilder.Append("\r\nPayment may be made by resending the payment on the next working day when the Importer's limit commences again.");
				reportBuilder.Append("\r\nOr the Importer may consider increasing their limit by applying directly to Australian Customs.");
			}
			return reportBuilder.ToString();
		}
	}
}
