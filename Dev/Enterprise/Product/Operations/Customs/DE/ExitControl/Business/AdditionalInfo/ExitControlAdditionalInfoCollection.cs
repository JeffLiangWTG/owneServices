using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class ExitControlAdditionalInfoCollection : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection
	{
		public ExitControlAdditionalInfoCollection(BusinessObject parent) : base(parent)
		{
			this.EnableMaxCountValidation(99, Res.GetString("80653639-7E4A-4A7B-97BE-930FE668FB04", "Maximum number of Additional Information is 99."), false);
		}

		public new ExitControlAdditionalInfo this[int i] => (ExitControlAdditionalInfo)base[i];

		public new ExitControlAdditionalInfo AddNew() => (ExitControlAdditionalInfo)base.AddNew();
	}
}
