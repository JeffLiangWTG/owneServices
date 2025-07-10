using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class GEIEDIInterchangeCreatorForTransactions : GEIEDIInterchangeCreator
	{
		protected GEIEDIInterchangeCreatorForTransactions(GlbCompany company)
			: base(company)
		{ }

		protected override string ParentTableCode => AccTransactionHeaderSchema.Constants.Prefix;

		protected override GEIEmailNotificationCreator GetEmailCreator(EDIMessage ediMessage, ZGuid transactionPK, IEnumerable<ZString> errors, ILogger logger)
		{
			var invoicingBase = new BusinessObjectFactory().Load<InvoicingBase>(transactionPK);
			return new GEIEmailNotificationCreator(ediMessage, invoicingBase, errors, logger);
		}
	}
}
