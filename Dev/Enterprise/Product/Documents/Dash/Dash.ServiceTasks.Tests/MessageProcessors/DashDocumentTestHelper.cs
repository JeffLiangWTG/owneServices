using System;
using CargoWise.EntityFramework;
using Enterprise.Dash.Business;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.ServiceTasks.Tests.MessageProcessors
{
	public static class DashDocumentTestHelper
	{
		public static DashDocument CreateDashDocument(
			this BusinessObjectFactory factory,
			string parseType = SharedConstants.ParseType.Code.CommercialInvoice,
			string parseStatus = SharedConstants.ParseStatus.Code.Processing)
		{
			var dashDocument = factory.NewWithValidTestData<DashDocument>();
			dashDocument.DDD_DocID = Guid.NewGuid();
			dashDocument.DDD_ParseType = parseType;
			dashDocument.DDD_ParseStatus = parseStatus;

			return dashDocument;
		}
	}
}
