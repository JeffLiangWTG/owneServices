
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceConnectionDependentCollection : DependentBusinessObjectCollection<LicenceConnection, LicenceDatabase>
	{
		public LicenceConnectionDependentCollection(LicenceDatabase parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		#region Implementation

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return LicenceConnectionSchema.LK_LD; }
		}

		protected override bool AllowNewCore
		{
			get { return EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed; }
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			alternativeAdditionalFilter.OrderBy = LicenceConnectionSchema.LK_ConnectionOrder.Name;
			base.Load(alternativeAdditionalFilter);
		}

		#endregion

		#region Defaults

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((LicenceConnection)child).LK_ConnectionOrder = (ZByte)(Count + 1);
		}

		#endregion
	}
}

