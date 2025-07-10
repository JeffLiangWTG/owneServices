using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseCartageAdviceWrapper : WarehouseOrderWrapper
	{
		#region Constructor

		public WarehouseCartageAdviceWrapper(WhsOrder order, BusinessObjectFactory factoryToWrap)
			: base(order, factoryToWrap)
		{
		}

		#endregion

		#region Static Memebers

		public static WarehouseCartageAdviceWrapper New(WhsOrder order, BusinessObjectFactory factoryToWrap)
		{
			WarehouseCartageAdviceWrapper result = null;
			if (order != null)
			{
				result = new WarehouseCartageAdviceWrapper(order, factoryToWrap);
			}
			return result;
		}

		#endregion

		#region Bizo Header Properties

		#region CartageAdviceOpeningText

		public override MultilingualString CartageAdviceOpeningText => DocumentsDataRegistry.Instance.WarehouseCartageAdviceOpeningText.Value;

		#endregion

		#region CartageAdviceClosingText

		public override MultilingualString CartageAdviceClosingText => DocumentsDataRegistry.Instance.WarehouseCartageAdviceClosingText.Value;

		#endregion

		#region CartageDropMode

		public override ZString CartageDropMode
		{
			get { return PickableDocketHelper.GetCartageDropModeFromWhsOrder(); }
		}

		WhsPickableDocketHelper PickableDocketHelper
		{
			get { return pickableDocketHelper ?? (pickableDocketHelper = new WhsPickableDocketHelper(Order)); }
		}

		WhsPickableDocketHelper pickableDocketHelper;

		#endregion

		#endregion
	}
}
