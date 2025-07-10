using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	[ModuleID("LicenceDatabase")]
	public class FeatureSetDatabaseCollection : DependentBusinessObjectCollection<LicenceDatabase, FeatureControlSet>
	{
		public FeatureSetDatabaseCollection(FeatureControlSet featureSet, BusinessObjectFactory factory)
			: base(featureSet, factory)
		{
		}

		#region Implementation

		protected override SchemaGuidColumn FKSchemaColumnInDependent => LicenceDatabaseSchema.LD_FCS_FeatureSet;

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			((LicenceDatabase)child).LD_FCS_FeatureSet = Master.PK;
		}

		#endregion
	}
}
