using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCMRPAYRECMessage : DocBaseWrapper
	{
		DocCMRPAYRECMessage(CMRPAYRECMessage cMRPAYRECMessage, BusinessObjectFactory factory)
			: base(cMRPAYRECMessage, factory)
		{
		}

		public static DocCMRPAYRECMessage New(CMRPAYRECMessage cMRPAYRECMessage, BusinessObjectFactory factory)
		{
			return (cMRPAYRECMessage == null) ? null : new DocCMRPAYRECMessage(cMRPAYRECMessage, factory);
		}

		CMRPAYRECMessage CMRPAYRECMessage
		{
			get { return (CMRPAYRECMessage)WrappedObject; }
		}

		public DocPAYRECInfoProvider InfoProvider
		{
			get
			{
				if (fPAYRECInfoProvider == null)
				{
					fPAYRECInfoProvider = DocPAYRECInfoProvider.New(CMRPAYRECMessage.PAYRECInfoProvider, Factory);
				}
				return fPAYRECInfoProvider;
			}
		}

		public DocCusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = DocCusEntryHeader.New(CMRPAYRECMessage.EntryHeader, Factory);
				}

				return fEntryHeader;
			}
		}

		#region Implementation
		DocPAYRECInfoProvider fPAYRECInfoProvider;
		DocCusEntryHeader fEntryHeader;
		#endregion
	}
}
