using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	[DefaultField("Amount")]
	public class DocCustomsDisbursementCharge : GenericWrapper
	{
		protected DocCustomsDisbursementCharge(CustomsDisbursementCharge lCItem, BusinessObjectFactory factoryToWrap)
			: base(lCItem, factoryToWrap)
		{
		}

		internal CustomsDisbursementCharge LCItem
		{
			get { return (CustomsDisbursementCharge)WrappedObject; }
		}

		public static DocCustomsDisbursementCharge New(CustomsDisbursementCharge lCItem, BusinessObjectFactory factoryForWrapper)
		{
			return lCItem != null ? new DocCustomsDisbursementCharge(lCItem, factoryForWrapper) : null;
		}

		public ZString ChargeCode { get { return LCItem.CustomsChargeLCItemSetting?.CostType ?? ZString.Empty; } }

		public ZString DocumentCustomLabelCode { get { return LCItem.CustomsChargeLCItemSetting?.DocumentCustomLabelCode ?? ZString.Empty; } }

		public ZString Label { get { return LCItem.CustomsChargeLCItemSetting?.Description ?? ZString.Empty; } }

		public ZDecimal Amount { get { return LCItem.Amount; } }
	}

	public class CustomsDisbursementCharge : NonPersistentBusinessObject
	{
		public CustomsDisbursementCharge()
		{
		}
		public CustomsDisbursementCharge(ICustomsChargeLCItemSetting setting, ZDecimal amount)
		{
			CustomsChargeLCItemSetting = setting;
			Amount = amount;
		}

		public ICustomsChargeLCItemSetting CustomsChargeLCItemSetting { get; set; }
		public ZDecimal Amount { get; set; }
	}
}
