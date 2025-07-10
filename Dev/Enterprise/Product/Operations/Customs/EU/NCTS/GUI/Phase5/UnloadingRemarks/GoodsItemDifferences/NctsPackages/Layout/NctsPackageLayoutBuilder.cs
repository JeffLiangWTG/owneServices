using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class NctsPackageLayoutBuilder<T> : ColumnLayoutBuilder<T, NctsPackageControlBag> where T : NctsPackage
	{
		public override NctsPackageControlBag CommonBag => NctsPackageControlBag.Instance;

		protected override int MaxColumns => 2;

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();
			SetCaption(CommonBag.DeclaredValueLabel, GetStatusLabelCaption, nctsPackage => nctsPackage.B5_TypeOfDifferenceInfo);
		}

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.DifModelTextBox, nctsPackage => nctsPackage.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF, nctsPackage => nctsPackage.B5_TypeOfDifferenceInfo);
			SetVisibility(CommonBag.DifBrandTextBox, nctsPackage => nctsPackage.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF, nctsPackage => nctsPackage.B5_TypeOfDifferenceInfo);
			SetVisibility(CommonBag.DifPackageIDTextBox, nctsPackage => nctsPackage.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF, nctsPackage => nctsPackage.B5_TypeOfDifferenceInfo);
			SetVisibility(CommonBag.DifMarksAndNumbersTextBox, nctsPackage => nctsPackage.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF, nctsPackage => nctsPackage.B5_TypeOfDifferenceInfo);
			SetVisibility(CommonBag.DifUnitCountCalcEdit, nctsPackage => nctsPackage.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF, nctsPackage => nctsPackage.B5_TypeOfDifferenceInfo);
			SetVisibility(CommonBag.DifUnitTypeDropEdit, nctsPackage => nctsPackage.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF, nctsPackage => nctsPackage.B5_TypeOfDifferenceInfo);
			SetVisibility(CommonBag.UnloadedValueLabel, nctsPackage => nctsPackage.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF, nctsPackage => nctsPackage.B5_TypeOfDifferenceInfo);
		}

		ResourceStringData GetStatusLabelCaption(T package)
		{
			switch (package.B5_TypeOfDifference)
			{
				case NctsUnloadedStateList.Codes.DEC:
				case NctsUnloadedStateList.Codes.DIF:
				case NctsUnloadedStateList.Codes.MIS:
					return Res.GetData("F5CA9FCB-4CE3-499E-87D2-6CCC210BECEF", "Declared Value");
				case NctsUnloadedStateList.Codes.NEW:
					return Res.GetData("DCC94D15-E03B-49C7-99AC-29A8E1E39074", "New Value");
				default:
					return new ResourceStringData();
			}
		}
	}
}
