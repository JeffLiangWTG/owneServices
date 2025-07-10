using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class WeightBusinessObject : NonPersistentBusinessObject, ITotalValueAndUnits
	{
		public ZDecimal Value
		{
			get { return value; }
			set { this.value = value; }
		}
		ZDecimal value;

		public ZString Unit
		{
			get { return unit; }
			set { unit = value; }
		}
		ZString unit;

		#region ITotalValueAndUnits Members

		public ValueAndUnitSelfTotaller GetNewForTotalling()
		{
			return new ValueAndUnitSelfTotaller(Value, Unit);
		}

		public void AddSelfToResult(ValueAndUnitSelfTotaller result)
		{
			result.Value += Value;
		}

		#endregion
	}
}
