using System;
using System.Text;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class MockRegistryItemDependentNumericRegistryDataType : RegistryItemDependentNumericRegistryDataType<int>
	{
		public MockRegistryItemDependentNumericRegistryDataType(IRegistryItem lowerBoundRegistryItem, IRegistryItem upperBoundRegistryItem)
			: this(lowerBoundRegistryItem, upperBoundRegistryItem, true, true)
		{
		}

		public MockRegistryItemDependentNumericRegistryDataType(IRegistryItem lowerBoundRegistryItem, IRegistryItem upperBoundRegistryItem, bool lowerBoundInclusive, bool upperBoundInclusive)
			: base(RegistryDataTypes.Codes.Int, lowerBoundRegistryItem, upperBoundRegistryItem, lowerBoundInclusive, upperBoundInclusive, 0)
		{
		}

		public new int GetLowerBoundRegistryItemValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return base.GetLowerBoundRegistryItemValue(companyPK, branchPK, departmentPK);
		}

		public new int GetUpperBoundRegistryItemValue(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return base.GetUpperBoundRegistryItemValue(companyPK, branchPK, departmentPK);
		}

		public new string GetErrorMessage(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return base.GetErrorMessage(companyPK, branchPK, departmentPK);
		}
		public override bool IsDefaultValueImmutable => true;

		#region RegistryDataType Overrides

		protected override byte[] SerialiseCore(int value)
		{
			return Encoding.Unicode.GetBytes(value.ToString());
		}

		protected override int DeserialiseCore(byte[] value)
		{
			return Convert.ToInt32(Encoding.Unicode.GetString(value));
		}

		#endregion
	}
}
