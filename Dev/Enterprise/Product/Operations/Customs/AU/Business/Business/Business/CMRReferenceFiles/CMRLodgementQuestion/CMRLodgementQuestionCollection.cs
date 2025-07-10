
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRLodgementQuestionCollection : BusinessObjectCollection<CMRLodgementQuestion>
	{
		public CMRLodgementQuestionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IFindBoxListProvider FindBoxListProvider => new CMRLodgementQuestionFindBoxListProvider(this);

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(CMRLodgementQuestionSchema.CQ_LodgementQuestionType, SQLComparisonOperator.NotEqual, CMRCusEntryCPDec.LodgementQuestionTypes.GeneralLodgementQuestion);
			return result;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			errors.Add("This question cannot be selected because it is a General Lodgement Question not a Community Protection Question.");
		}
	}
}
