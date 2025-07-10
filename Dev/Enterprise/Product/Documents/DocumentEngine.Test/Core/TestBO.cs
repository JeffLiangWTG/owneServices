using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TestBO : NonPersistentBusinessObject
	{
		public ZString TextField
		{
			get { return textField; }
			set { textField = value; }
		}
		ZString textField;

		public ZInt IntField
		{
			get { return intField; }
			set { intField = value; }
		}
		ZInt intField;

		public ZLong LongField
		{
			get { return longField; }
			set { longField = value; }
		}
		ZLong longField;

		public ZDecimal DecimalField
		{
			get { return decimalField; }
			set { decimalField = value; }
		}
		ZDecimal decimalField;

		public WeightBusinessObject Weight
		{
			get { return weight ?? (weight = new WeightBusinessObject()); }
		}
		WeightBusinessObject weight;
	}
}
