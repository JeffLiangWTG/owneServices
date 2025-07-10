using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business
{
	public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader
		, Integration.Customs.GB.IDepartureMovementHeader, IAllowPermitProcessing
	{
		public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

		protected override NctsDepartureMovementHeaderPhase4Validation GetNewPhase4Validation() => new NctsDepartureMovementHeaderValidation(this);

		protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

		public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<Guarantee>)base.Guarantees;

		protected override INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<Guarantee>(this);

		protected override Type CusInBondCargoDescTypeCore => typeof(NctsDepartureCargoDesc);

		protected override ZBool AllowMixedCaseAuthorisationNumbersCore => true;
		public ZString GetPermitReference() => Header.GetPermitReference();
		public ZInt GetPermitReferenceNumberLine() => Header.GetPermitReferenceNumberLine();
		public IList<PermitRecord> GetPermitRecords() => Header.GetPermitRecords();
		public ZString GetPermitComment(PermitRecord permitRecord) => Header.GetPermitComment(permitRecord);
		public ZInt PermitValueDecimalPlaceCount => Header.PermitQuantityDecimalPlaceCount;
		public ZInt PermitQuantityDecimalPlaceCount => Header.PermitQuantityDecimalPlaceCount;
		public ZInt PackageCount => TotalNumberOfPackages.ToZInt();

		public void PrepareForLrnRegeneration()
		{
			NeedsRegenerateLocalReferenceNumber = true;
			UpdateLocalReferenceNumberForRetransmission();
		}
	}
}
