using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5TWLineMessageData : NonPersistentBusinessObject
	{
		public GOVCBR5TWLineMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ZInt ImportEntryLineNo { get; set; }
		public ZDate ExamineStartDate { get; set; }
		public ZDate ExamineEndDate { get; set; }
		public ZDate IssueDate { get; set; }
		public ZString ContentDescription { get; set; }
		public ZString CorrectionResult { get; set; }
		public ZString RequestDocumentNumber { get; set; }
		public ZString AttachedDeclarationNumber { get; set; }
		public ZString FormattedAttachedDeclarationNumber { get; set; }

		public ZString ContentDescriptionShort { get => ContentDescription.SubstringSafe(0, 100); }
		public ZString CorrectionResultShort { get => CorrectionResult.SubstringSafe(0, 100); }
	}
	public class GOVCBR5TWLineMessageDataCollection : NonPersistentBusinessObjectCollection<GOVCBR5TWLineMessageData>
	{
		public GOVCBR5TWLineMessageDataCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new GOVCBR5TWLineMessageData(Factory);
		protected override bool AllowNewCore => false;
	}
}
