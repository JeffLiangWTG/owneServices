using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class SupportingDocumentWrapperWithoutDocument : ISupportingDocumentOnly
	{
		public SupportingDocumentWrapperWithoutDocument(ZString code, ZString referenceNumber, ZDateTime dateOfIssue)
		{
			this.code = code;
			this.referenceNumber = referenceNumber;
			this.dateOfIssue = dateOfIssue;
		}
		readonly ZString code;
		readonly ZString referenceNumber;
		readonly ZDateTime dateOfIssue;

		public ZBool IsD48AndNotClosed => false;

		public ZDecimal D48Amount => ZDecimal.Zero;

		public sbyte D48Deadline => 0;

		public ZBool IsUnderInvoiceLine => false;

		public IEnumerable<IImputationSheet> ImputationsSheets => Enumerable.Empty<IImputationSheet>();

		public ZString Code => code;

		public ZString Description => ZString.Empty;

		public ZString Type => ZString.Empty;

		public ZString RefNumber => referenceNumber;

		public ZDateTime DateIssue => dateOfIssue;

		public ZString PFAIdentification => ZString.Empty;

		public ZString PFADocument => ZString.Empty;
	}
}
