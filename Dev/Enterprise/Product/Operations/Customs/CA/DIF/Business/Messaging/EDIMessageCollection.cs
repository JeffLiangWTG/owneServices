using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DIF.Business
{
	public class EDIMessageCollection : BusinessObjectCollection<EDIMessage>
	{
		public EDIMessageCollection(DIFDocument difDocument)
			: base(difDocument.Factory)
		{
			this.difDocument = difDocument;
		}

		readonly DIFDocument difDocument;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			if (difDocument.RequiredDocumentAddInfo != null)
			{
				result.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, difDocument.RequiredDocumentAddInfo.PK);
				result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}
	}
}
