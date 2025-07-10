using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitHeader : EU.ExitControl.Business.CusExitHeader
		, Integration.Customs.IEExitControl.ICusExitHeader
	{
		public CusExitHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusExitHeaderValidation Validation => (CusExitHeaderValidation)base.Validation;

		public new ExitControlBase.Business.ICusExitConsignmentCollection<CusExitConsignment> CusExitConsignments => (ExitControlBase.Business.ICusExitConsignmentCollection<CusExitConsignment>)base.CusExitConsignments;

		public new ExitControlBase.Business.ICusExitReportCollection<CusExitReport> CusExitReports => (ExitControlBase.Business.ICusExitReportCollection<CusExitReport>)base.CusExitReports;
		protected override ExitControlBase.Business.ICusExitReportCollection<ExitControlBase.Business.CusExitReport> CreateNewCusExitReportCollection() => new ExitControlBase.Business.CusExitReportCollection<CusExitReport>(this);

		protected override ExitControlBase.Business.ICusExitConsignmentPackageCollection<ExitControlBase.Business.CusExitConsignmentPackage> CreateNewCusExitConsignmentPackageCollection() => new ExitControlBase.Business.CusExitConsignmentPackageCollection<CusExitConsignmentPackage>(this);

		protected override ExitControlBase.Business.CusExitHeaderValidation GetNewValidation() => new CusExitHeaderValidation(this);

		protected override ExitControlBase.Business.ICusExitConsignmentCollection<ExitControlBase.Business.CusExitConsignment> CreateNewCusExitConsignmentCollection() => new ExitControlBase.Business.CusExitConsignmentCollection<CusExitConsignment>(this);

		public new ExitControlBase.Business.ICusExitContainerCollection<CusExitContainer> CusExitContainers => (ExitControlBase.Business.ICusExitContainerCollection<CusExitContainer>)base.CusExitContainers;

		protected override ExitControlBase.Business.ICusExitContainerCollection<ExitControlBase.Business.CusExitContainer> CreateNewCusExitContainerCollection() => new ExitControlBase.Business.CusExitContainerCollection<CusExitContainer>(this);

		protected override EU.ExitControl.Business.CusExitConsignment CreateConsignmentFromEntryHeader(EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			var consignment = base.CreateConsignmentFromEntryHeader(entryHeader);
			if (consignment != null)
			{
				if (entryHeader.Declaration is JobDeclaration declaration && !declaration.JE_OwnerRef.IsEmpty)
				{
					consignment.CXC_UniqueConsignmentReference = declaration.JE_OwnerRef;
				}
				else
				{
					consignment.CXC_UniqueConsignmentReference = entryHeader.CH_BGMReference;
				}
			}
			return consignment;
		}
	}
}
