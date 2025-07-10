using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class EntryHeader : IDataObject,
		IAddInfoCollectionParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent
	{
		public EntryHeader()
		{
		}

		public EntryHeader(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[CandidateKey, Mandatory]
		public EntryType Type { get; set; }
		[MaxLength(35), CandidateKey]
		public ZString? Reference { get; set; }
		public ZInt? EntryInstructionLink { get; set; }
		public EntryStatus EntryStatus { get; set; }
		public ZDateTime? EntrySubmittedDate { get; set; }
		public ZDateTime? EntryReleaseDate { get; set; }
		public ZDateTime? BondValidToDate { get; set; }
		public CodeDescriptionPair MessageStatus { get; set; }
		public ZDecimal? TotalAmountPaid { get; set; }

		public List<AddInfo> AddInfoCollection { get; set; }
		public List<EntryHeaderCharge> EntryHeaderChargeCollection { get; set; }
		public List<EntryLine> EntryLineCollection { get; set; }
		public List<EntryNumber> EntryNumberCollection { get; set; }
		public List<EntryHeader> RelatedEntryHeaderCollection { get; set; }
		public List<AddInfoGroup> AddInfoGroupCollection { get; set; }
		public List<CustomsReference> CustomsReferenceCollection { get; set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; set; }
		public List<EntryHeaderPaymentInformation> PaymentInformationCollection { get; set; }
	}
}

