using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[CodeProperty("MessageDescriptionForEdocs")]  // For eDocs when child		
	[DescriptionProperty("MessageDescriptionForEdocs")]  // For eDocs when child
	public class NctsEdiMessage : EDIMessage, IDocumentSupportable, IDocManagerSupport
	{
		public NctsEdiMessage(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		public NctsHeader Header => (EM_LinkedObject as NctsHeader) ?? (EM_LinkedObject as NctsDepartureMovementHeader)?.Header;

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new NctsEdiMessageDocumentSupporter(this); }
		}

		#endregion

		#region IDocManagerSupport Members

		public new DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new NctsEdiMessageDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		public ZString MessageCodeForEdocs
		{
			get { return "TAD/TSAD"; }
		}

		public ZString MessageDescriptionForEdocs
		{
			get { return HumanReadableNameCore; } //// TODO Revisit
		}

		protected override ZString HumanReadableNameCore
		{
			get { return (NoResString)"NCTS Message"; } // TODO Revisit
		}

		public ZBool IsSecurityDeclaration => Header.IsSecurityDeclaration;

		public ZString CountryCode => Header.CountryCode;
	}
}
