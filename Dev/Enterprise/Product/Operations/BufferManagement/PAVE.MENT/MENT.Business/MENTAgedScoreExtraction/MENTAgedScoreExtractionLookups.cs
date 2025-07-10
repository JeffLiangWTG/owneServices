using Enterprise.ZArchitecture.Core;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreExtractionLookups : AutoMENTAgedScoreExtractionLookups
	{
		public MENTAgedScoreExtractionLookups(AutoMENTAgedScoreExtraction parent)
			: base(parent)
		{
		}

		public ExtractionTypes ExtractionTypes
		{
			get { return Factory.GetCachedValue<ExtractionTypes>(); }
		}

		public CodeDescriptionPairList CollectionColumns
		{
			get
			{
				return Factory.GetCachedValue("MENTAgedScoreExtractionLookups.CollectionColumns", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(MENTColumns.Codes.Score, MENTColumns.Descriptions.Score);
					list.AddPair(MENTColumns.Codes.AttributeValue, MENTColumns.Descriptions.AttributeValue);
					list.AddPair(MENTColumns.Codes.None, MENTColumns.Descriptions.None);

					return list;
				});
			}
		}
	}
}
