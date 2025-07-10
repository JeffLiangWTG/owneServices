using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.IE.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsDepartureCargoDesc : EU.NCTS.Business.NctsDepartureCargoDesc, Integration.Customs.IENCTS.IDepartureCargoDesc, ICusInBondCargoDescTypeProvider
	{
		public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsHeader Header => (NctsHeader)base.Header;

		public new NctsDepartureCargoDescPhase5Validation Validation => (NctsDepartureCargoDescPhase5Validation)base.Validation;

		[ChildEditable(true)]
		public new EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => (EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument>)base.PreviousDocuments;
		protected override EU.NCTS.Business.INctsPreviousDocumentCollection<EU.NCTS.Business.NctsPreviousDocument> GetPreviousDocuments() => new EU.NCTS.Business.NctsPreviousDocumentCollection<NctsPreviousDocument>(this);

		protected override EU.NCTS.Business.NctsDepartureCargoDescPhase5Validation GetNewPhase5Validation() => new NctsDepartureCargoDescPhase5Validation(this);

		protected override Type PreviousDocumentType => typeof(NctsPreviousDocument);

		public new EU.NCTS.Business.INctsPackageCollection<NctsPackage, NctsDepartureCargoDesc> Packages => (EU.NCTS.Business.INctsPackageCollection<NctsPackage, NctsDepartureCargoDesc>)base.Packages;
		protected override EU.NCTS.Business.INctsPackageCollection<EU.NCTS.Business.NctsPackage, EU.NCTS.Business.NctsCommonCargoDesc> GetNctsPackageCollection() => new EU.NCTS.Business.NctsPackageCollection<NctsPackage, NctsDepartureCargoDesc>(this);

		public Type CusInBondCargoDescType => typeof(NctsDepartureCargoDesc);
	}
}
