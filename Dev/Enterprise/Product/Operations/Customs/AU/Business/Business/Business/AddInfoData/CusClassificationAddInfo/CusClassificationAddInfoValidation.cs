using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusClassificationAddInfoValidation : AUAddInfoValidation
	{
		public CusClassificationAddInfoValidation(CusClassificationAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckZA_TreatmentCode_Hidden()
		{
			base.CheckZA_TreatmentCode_Hidden();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_TreatmentCode_HiddenInfo, Parent.Lookups.TreatmentCodeList);
		}

		protected override void CheckZA_InstrumentType_Hidden()
		{
			base.CheckZA_InstrumentType_Hidden();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_InstrumentType_HiddenInfo, Parent.Lookups.ZA_InstrumentType_List);
		}

		protected override void CheckZA_InstrumentCode_Hidden()
		{
			base.CheckZA_InstrumentCode_Hidden();
			ListValidation.WarnIfInvalidCode(Parent.ZA_InstrumentCode_HiddenInfo, Parent.Lookups.CMRInstrumentNumberList);
		}

		protected new CusClassificationAddInfo Parent
		{
			get { return (CusClassificationAddInfo)base.Parent; }
		}
	}
}
