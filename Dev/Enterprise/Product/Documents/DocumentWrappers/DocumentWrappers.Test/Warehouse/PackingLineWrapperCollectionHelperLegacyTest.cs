using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class PackingLineWrapperCollectionHelperLegacyTest : PackingSlipLineWrapperCollectionHelperTest<DocWhsPackingSlipLine, DocWhsPackingSlipLineCollection>
	{
		#region TestPackingLines_RollingUp

		protected override ZString GetPartAttrib1(DocWhsPackingSlipLine wrapper)
		{
			return wrapper.PartAttrib1;
		}

		protected override ZString GetPartAttrib2(DocWhsPackingSlipLine wrapper)
		{
			return wrapper.PartAttrib2;
		}

		protected override ZString GetPartAttrib3(DocWhsPackingSlipLine wrapper)
		{
			return wrapper.PartAttrib3;
		}

		protected override ZString GetSerialNumber(DocWhsPackingSlipLine wrapper)
		{
			return wrapper.TrackedSerialNumber;
		}

		protected override ZDateTime GetExpiryDate(DocWhsPackingSlipLine wrapper)
		{
			return wrapper.ExpiryDate;
		}

		protected override ZDateTime GetPackingDate(DocWhsPackingSlipLine wrapper)
		{
			return wrapper.PackingDate;
		}

		protected override ZDecimal GetUnitsMet(DocWhsPackingSlipLine wrapper)
		{
			return wrapper.LineUnitsMet;
		}

		#endregion

		#region Implementation

		protected override PackingSlipLineWrapperCollectionHelper<DocWhsPackingSlipLine, DocWhsPackingSlipLineCollection> GetNewHelper()
		{
			return new PackingLineWrapperCollectionHelperLegacy();
		}

		#endregion
	}
}
