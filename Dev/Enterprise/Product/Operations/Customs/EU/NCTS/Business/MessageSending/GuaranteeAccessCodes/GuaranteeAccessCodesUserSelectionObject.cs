using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeAccessCodesUserSelectionObject : NonPersistentBusinessObject
	{
		// DO NOT DELETE - needed by ZFormBasherTest due to password field
		public GuaranteeAccessCodesUserSelectionObject() { }

		public GuaranteeAccessCodesUserSelectionObject(NctsGuarantee guarantee)
		{
			this.Guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}
		public readonly NctsGuarantee Guarantee;

		[ResourceStringData("3AAE2FBF-87D3-460F-823E-ED9930E385AD", Caption = "Type")]
		public ZString GuaranteeType => Guarantee.PW_BondType;

		[ResourceStringData("16F9D89F-725D-43B3-A5D4-F487ED476823", Caption = "GRN")]
		public ZString GuaranteeReferenceNumber => Guarantee.PW_BondNumber;

		public ZString OtherGuaranteeReference => Guarantee.PW_BondNumber2;

		[ResourceStringData("9F0172FA-7B66-4E6F-869C-8232F2BC65BB", Caption = "Access Code")]
		[Password]
		public ZString AccessCode => Guarantee.PW_Password;

		[ResourceStringData("1BBFCCB2-9DFC-4767-8E68-7AE7B50F8565", Caption = "Select")]
		public ZBool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				SetNonPersistentPropertyValue(IsSelectedInfo, ref isSelected, value);
			}
		}
		ZBool isSelected;

		public ZPropertyInfo IsSelectedInfo => GetZPropertyInfo(nameof(IsSelected));
	}
}
