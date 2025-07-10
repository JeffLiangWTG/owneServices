using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Messaging.Business
{
	public class EDIMessageAttach : AutoEDIMessageAttach
	{
		public EDIMessageAttach(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("EDIMessage")]
		public override ZGuid EG_EM
		{
			get { return base.EG_EM; }
			set { base.EG_EM = value; }
		}

		public EDIMessage EDIMessage
		{
			get { return Factory.Load<EDIMessage>(EG_EM); }
		}

		public IeDoc GetAttachment()
		{
			var docManagerSupport = EDIMessage.EM_LinkedObject as IDocManagerSupport;

			IeDoc result = null;

			if (docManagerSupport != null)
			{
				result = docManagerSupport.DocManagerInfo.AllEDocs.GetFromUniqueKey(EG_StorageDocsGuid.ToGuid());
			}

			if (result == null)
			{
				var docManagerSupportProvider = EDIMessage.EM_LinkedObject as IDocManagerSupportProvider;
				if (docManagerSupportProvider != null)
				{
					foreach (var support in docManagerSupportProvider.DocManagerSupports)
					{
						result = support?.DocManagerInfo.AllEDocs.GetFromUniqueKey(EG_StorageDocsGuid.ToGuid());
						if (result != null)
						{
							break;
						}
					}
				}
			}

			if (result == null)
			{
				var documentFactory = docManagerSupport == null ? null : docManagerSupport.DocManagerInfo.MasterFactory;
				if (documentFactory == null)
				{
					IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
					documentFactory = documentFactoryProvider.GetFactory(Factory);
				}
				result = documentFactory.FindEDocsFromAllSDDatabasesByPK(EG_StorageDocsGuid);
			}

			return result;
		}
	}
}
