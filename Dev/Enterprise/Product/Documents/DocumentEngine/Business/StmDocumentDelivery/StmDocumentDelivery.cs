using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine.Business
{
	public sealed class StmDocumentDelivery : AutoStmDocumentDelivery, IStmDocumentDelivery
	{
		public StmDocumentDelivery(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
