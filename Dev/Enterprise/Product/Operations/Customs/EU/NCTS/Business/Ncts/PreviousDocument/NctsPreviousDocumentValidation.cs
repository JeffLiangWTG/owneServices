using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsPreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
	{
		public NctsPreviousDocumentValidation(NctsPreviousDocument parent)
			: base(parent)
		{
		}

		protected override bool IsSubTypeMandatory => false;

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();

			var parent = Parent;
			CheckValidIntegerQtyForNumberUnit(parent.CSI_QuantityInfo, parent.CSI_Quantity, parent.CSI_UnitOfQuantity);
		}

		protected override void CheckCSI_QuantityIsValidZDecimal()
		{
			CheckValidQtyDecimalRange(Parent.CSI_QuantityInfo);
		}

		protected override void CheckCSI_Quantity2()
		{
			base.CheckCSI_Quantity2();

			var parent = Parent;
			CheckValidIntegerQtyForNumberUnit(parent.CSI_Quantity2Info, parent.CSI_Quantity2, parent.CSI_UnitOfQuantity2);
		}

		protected override void CheckCSI_Quantity2IsValidZDecimal()
		{
			CheckValidQtyDecimalRange(Parent.CSI_Quantity2Info);
		}

		protected virtual int QuantityPrecision => NctsPreviousDocument.Schema.CSI_QuantityPrecision;

		protected virtual int QuantityScale => NctsPreviousDocument.Schema.CSI_QuantityScale;

		void CheckValidQtyDecimalRange(ZPropertyInfo info) => TypeValidation.CheckValidDecimal(info, QuantityPrecision, QuantityScale);

		void CheckValidIntegerQtyForNumberUnit(ZPropertyInfo info, ZDecimal quantity, ZString unit)
		{
			if (Parent.Factory.IsIntegerRequiredUnitOfQuantity(unit) && !quantity.IsInteger)
			{
				var errorField = info.HasHumanReadableName ? info.HumanReadableName.ToString() : Res.GetString("F71CE594-3222-48BA-9C0F-466F8B3B41DC", "Qty.");
				info.AddMessageError(Res.GetString("A191992D-6804-4455-BA6B-7FC3A77C54FB", "Only integer values are allowed for this {0} Unit", errorField));
			}
		}
	}
}
