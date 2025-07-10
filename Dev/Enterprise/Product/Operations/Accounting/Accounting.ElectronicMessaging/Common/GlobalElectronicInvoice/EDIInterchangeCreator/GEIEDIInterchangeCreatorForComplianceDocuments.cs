using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class GEIEDIInterchangeCreatorForComplianceDocuments : GEIEDIInterchangeCreator
	{
		protected GEIEDIInterchangeCreatorForComplianceDocuments(GlbCompany company)
			: base(company)
		{ }

		protected override string ParentTableCode => AccComplianceDocumentHeaderSchema.Constants.Prefix;

		protected override GEIEmailNotificationCreator GetEmailCreator(EDIMessage ediMessage, ZGuid transactionPK, IEnumerable<ZString> errors, ILogger logger)
		{
			var complianceDocumentHeader = new BusinessObjectFactory().Load<AccComplianceDocumentHeader>(transactionPK);
			return new GEIEmailNotificationCreator(ediMessage, complianceDocumentHeader, errors, logger);
		}
	}
}
