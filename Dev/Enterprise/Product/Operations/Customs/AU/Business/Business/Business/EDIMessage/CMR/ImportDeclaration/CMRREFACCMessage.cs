using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRREFACCMessage : CMRImportDeclarationMessage
	{
		public CMRREFACCMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.REFACC;
		}

		protected override ZString GetStatusCore()
		{
			return CMRMessageStatusDescription.CLEAR;
		}

		public override ZString GetReport()
		{
			ZStringBuilder result = new ZStringBuilder();

			if (EM_LinkedObject != null)
			{
				result.Append(GetEM_LinkedObjectDetails(false));
				result.Append("\r\n");
			}

			result.Append("\r\nEntry Number: " + REFACCInfoProvider.EntryNumber);
			result.Append("\r\nEFT Run Number: " + REFACCInfoProvider.EFTRunNumber);
			result.Append("\r\nClaim Number: " + REFACCInfoProvider.ClaimNumber);

			return result.ToString();
		}

		public REFACCInfoProvider REFACCInfoProvider
		{
			get
			{
				if (fREFACCInfoProvider == null)
				{
					fREFACCInfoProvider = new REFACCInfoProvider(CUSRES, this);
				}
				return fREFACCInfoProvider;
			}
		}
		REFACCInfoProvider fREFACCInfoProvider;

		public CusEntryHeader EntryHeader
		{
			get
			{
				var consolidatedDeclaration = EM_LinkedObject as ConsolidatedDeclaration;
				var entryHeader = consolidatedDeclaration != null ? ((JobDeclaration)consolidatedDeclaration.LeadDeclaration).EntryHeader : EM_LinkedObject as CusEntryHeader;
				return entryHeader;
			}
		}
	}
}
