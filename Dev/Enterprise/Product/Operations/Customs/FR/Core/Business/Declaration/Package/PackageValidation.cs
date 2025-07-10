using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class PackageValidation : EU.Business.Declaration.PackageValidation
	{
		public PackageValidation(AutoCusDecHouseContainerPack parent) : base(parent)
		{
		}
		protected new Package Parent => (Package)base.Parent;
		protected override void CheckCW_MarksAndNos()
		{
			base.CheckCW_MarksAndNos();
			var marksAndNos = Parent.CW_MarksAndNos;
			if (marksAndNos.Length > Declaration.Package.Schema.CW_MarksAndNosMaxLengthForMessage)
			{
				Parent.CW_MarksAndNosInfo.AddWarning(Res.GetString("de54c548-5b2a-480c-8727-286747c4ffqa", "This field is recommended to be no more than 42 characters long."));
			}
		}
	}
}
