using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsHeaderForTest : NctsHeader
	{
		public NctsHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new ZBool IsArrivalEventAvailable => base.IsArrivalEventAvailable;

		public new NctsDepartureMovementHeaderForGuaranteesTesting MovementHeader => (NctsDepartureMovementHeaderForGuaranteesTesting)base.MovementHeader;

		protected override EU.NCTS.Business.NctsDepartureMovementHeader GetNewDepartureMovementHeader() => NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeaderForGuaranteesTesting>(this, "D");
	}

	class NctsDepartureMovementHeaderForGuaranteesTesting : NctsDepartureMovementHeader
	{
		public NctsDepartureMovementHeaderForGuaranteesTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new INctsGuaranteeCollection<NctsGuaranteeForTest> Guarantees => (INctsGuaranteeCollection<NctsGuaranteeForTest>)base.Guarantees;

		protected override INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuaranteeForTest>(this);
	}

	class NctsGuaranteeForTest : Guarantee
	{
		public NctsGuaranteeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override GuaranteeApportionmentType GetApportionmentTypeCore() => _apportionmentType;

		public void SetupApportionmentType(GuaranteeApportionmentType apportionmentType)
		{
			_apportionmentType = apportionmentType;
		}

		GuaranteeApportionmentType _apportionmentType;
	}
}
