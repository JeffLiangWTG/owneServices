using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class NctsCusInBondContainerPackageGenPivot : CustomsGenPivot, Integration.Customs.EU.NCTS.INctsCusInBondContainerPackageGenPivot, ISynchroniserReadOnlyMembersProvider
	{
		public NctsCusInBondContainerPackageGenPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsPackage Relation1Object
		{
			get => (NctsPackage)base.Relation1Object;
			set => base.Relation1Object = value;
		}

		public NctsCusInBondContainer Container => Relation2Object as NctsCusInBondContainer;

		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property) => MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = GenPivotTypeDecider.Types.CusNctsContainer;
			XX_Relation1TableCode = CusInvPackSchema.Constants.Prefix;
			XX_Relation2TableCode = CusInBondContainerSchema.Constants.Prefix;
		}
	}
}
