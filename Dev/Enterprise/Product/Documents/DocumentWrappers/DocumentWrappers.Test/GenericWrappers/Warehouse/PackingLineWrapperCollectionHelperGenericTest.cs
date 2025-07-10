using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class PackingLineWrapperCollectionHelperGenericTest : PackingSlipLineWrapperCollectionHelperTest<WarehousePackingSlipLineWrapper, WarehousePackingSlipLineWrapperCollection>
	{
		#region TestPackingLines_RollingUp

		protected override ZString GetPartAttrib1(WarehousePackingSlipLineWrapper wrapper)
		{
			return wrapper.PartAttribute1;
		}

		protected override ZString GetPartAttrib2(WarehousePackingSlipLineWrapper wrapper)
		{
			return wrapper.PartAttribute2;
		}

		protected override ZString GetPartAttrib3(WarehousePackingSlipLineWrapper wrapper)
		{
			return wrapper.PartAttribute3;
		}

		protected override ZString GetSerialNumber(WarehousePackingSlipLineWrapper wrapper)
		{
			return wrapper.TrackedSerialNumber;
		}

		protected override ZDateTime GetExpiryDate(WarehousePackingSlipLineWrapper wrapper)
		{
			return wrapper.ExpiryDate;
		}

		protected override ZDateTime GetPackingDate(WarehousePackingSlipLineWrapper wrapper)
		{
			return wrapper.PackingDate;
		}

		protected override ZDecimal GetUnitsMet(WarehousePackingSlipLineWrapper wrapper)
		{
			return (ZDecimal)wrapper.UnitsMet.NativeValue;
		}

		#endregion

		#region Implementation

		protected override PackingSlipLineWrapperCollectionHelper<WarehousePackingSlipLineWrapper, WarehousePackingSlipLineWrapperCollection> GetNewHelper()
		{
			return new PackingLineWrapperCollectionHelperGeneric();
		}

		#endregion
	}
}
