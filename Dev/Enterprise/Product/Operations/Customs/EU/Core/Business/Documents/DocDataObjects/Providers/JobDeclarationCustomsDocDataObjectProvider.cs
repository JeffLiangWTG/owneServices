using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CMR;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	sealed class JobDeclarationCustomsDocDataObjectProvider : ICustomsDocDataObjectProvider
	{
		public object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			if (parent is JobDeclaration declaration && dataContext == DataContext.CMRWayBill)
			{
				return new CMRConsignmentNoteDocDataObject(new CMRWayBillWrapper(declaration));
			}

			return null;
		}
	}
}
