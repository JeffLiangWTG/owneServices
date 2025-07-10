using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCMRREFACCMessage : DocBaseWrapper
	{
		DocCMRREFACCMessage(CMRREFACCMessage cMRREFACCMessage, BusinessObjectFactory factory)
			: base(cMRREFACCMessage, factory)
		{
		}

		public static DocCMRREFACCMessage New(CMRREFACCMessage cMRREFACCMessage, BusinessObjectFactory factory)
		{
			return (cMRREFACCMessage == null) ? null : new DocCMRREFACCMessage(cMRREFACCMessage, factory);
		}

		CMRREFACCMessage CMRREFACCMessage
		{
			get { return (CMRREFACCMessage)WrappedObject; }
		}

		public DocREFACCInfoProvider InfoProvider
		{
			get
			{
				if (fREFACCInfoProvider == null)
				{
					fREFACCInfoProvider = DocREFACCInfoProvider.New(CMRREFACCMessage.REFACCInfoProvider, Factory);
				}

				return fREFACCInfoProvider;
			}
		}

		protected DocREFACCInfoProvider fREFACCInfoProvider;

		public DocCusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = DocCusEntryHeader.New(CMRREFACCMessage.EntryHeader, Factory);
				}

				return fEntryHeader;
			}
		}

		protected DocCusEntryHeader fEntryHeader;
	}
}
