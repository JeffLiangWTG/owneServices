using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCProcessingIndicatorDescriptionCodeList : ProcessingIndicatorDescriptionCodeList
	{
		protected AUCProcessingIndicatorDescriptionCodeList(string codeValue) : base(codeValue) { }
		public static AUCProcessingIndicatorDescriptionCodeList NonConfirmingDeclaration
		{
			get
			{
				return new AUCProcessingIndicatorDescriptionCodeList("N");
			}
		}

		public static AUCProcessingIndicatorDescriptionCodeList ConfirmingDeclaration
		{
			get
			{
				return new AUCProcessingIndicatorDescriptionCodeList("Y");
			}
		}

		public static AUCProcessingIndicatorDescriptionCodeList ConfirmedDeclaration
		{
			get
			{
				return new AUCProcessingIndicatorDescriptionCodeList("C");
			}
		}

		public static AUCProcessingIndicatorDescriptionCodeList NilCargoReportIndicator
		{
			get
			{
				return new AUCProcessingIndicatorDescriptionCodeList("NIL");
			}
		}

		public static AUCProcessingIndicatorDescriptionCodeList SlotSubManifest
		{
			get
			{
				return new AUCProcessingIndicatorDescriptionCodeList("S");
			}
		}

		public static AUCProcessingIndicatorDescriptionCodeList ConsolidationSubManifest
		{
			get
			{
				return new AUCProcessingIndicatorDescriptionCodeList("C");
			}
		}

		public static AUCProcessingIndicatorDescriptionCodeList Yes
		{
			get
			{
				return new AUCProcessingIndicatorDescriptionCodeList("Y");
			}
		}

		public static AUCProcessingIndicatorDescriptionCodeList No
		{
			get
			{
				return new AUCProcessingIndicatorDescriptionCodeList("N");
			}
		}

		public static AUCProcessingIndicatorDescriptionCodeList PayeeDeclarationIndicator
		{
			get
			{
				return new AUCProcessingIndicatorDescriptionCodeList("PAD");
			}
		}
	}
}
