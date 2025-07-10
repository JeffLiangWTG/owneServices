using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class DocumentTracking : IDataObject
	{
		public DocumentTracking()
		{
		}

		public DocumentTracking(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(15)]
		public ZString? DocumentNumber { get; set; }
		public ZDateTime? ReceivedDate { get; set; }
		public ZDateTime? ValidToDate { get; set; }
		[MaxLength(200), AllowLineControlWhiteSpace]
		public ZString? DocumentNote { get; set; }
		public CodeDescriptionPair Category { get; set; }
		[Mandatory]
		public CodeDescriptionPair DocumentType { get; set; }
		[Mandatory]
		public CodeDescriptionPair DocumentPeriod { get; set; }
		[Mandatory]
		public CodeDescriptionPair DocumentUsage { get; set; }
		public Country Country { get; set; }
		public ZBool? IsOriginalDocumentRequired { get; set; }
		public ZBool? IsDocumentCreditControl { get; set; }
		public ZDateTime? SentToCustomsBroker { get; set; }
		public ZDateTime? ReceivedFromCustomsBroker { get; set; }
		public ZDateTime? ReturnToShipper { get; set; }
		public OrganizationAddress DocumentOwner { get; set; }
		public List<DocumentTrackingAttribute> DocumentTrackingAttributeCollection { get; private set; }
	}
}
