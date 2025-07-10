using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.PAVE.MENT.Business
{
	public class AdditionalExtractionCollection : NonPersistentBusinessObjectCollection<AdditionalExtractionLink>
	{
		public AdditionalExtractionCollection(MENTAgedScoreExtraction extraction)
			: base(extraction.Factory)
		{
			Argument.NotNull(extraction, nameof(extraction));
			this.extraction = extraction;
		}

		readonly MENTAgedScoreExtraction extraction;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AdditionalExtractionLink(Factory, extraction);
		}
	}
}
