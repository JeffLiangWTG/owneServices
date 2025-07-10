using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("CompletedTime"), WrapperTypeName("PackageAuditWrapper")]
	public class PackageAuditWrapper : GenericWrapper
	{
		public PackageAuditWrapper(WhsPackageAudit auditToWrap, BusinessObjectFactory factory)
			: base(auditToWrap, factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		#region Properties

		public ZString Auditor
		{
			get { return PackageAudit?.WPA_GS_NKAuditor ?? ZString.Empty; }
		}

		public ZDateTimeOffset CompletedTime
		{
			get { return PackageAudit?.WPA_AuditCompleteTime ?? ZDateTimeOffset.Empty; }
		}

		public ZString DockDoorLocation
		{
			get { return PackageAudit?.Order?.Pick?.DockDoorLocation?.WLV_LocationString ?? ZString.Empty; }
		}

		public PackageAuditFailureWrapperCollection Failures
		{
			get { return new PackageAuditFailureWrapperCollection(PackageAudit, factory); }
		}

		#endregion

		#region PackageAudit

		WhsPackageAudit PackageAudit
		{
			get { return (WhsPackageAudit)WrappedBO; }
		}

		#endregion
	}
}
