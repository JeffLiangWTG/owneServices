using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	[SingleObjectAroundARow]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[DependentBusinessObject(typeof(CusExitItem), "Packages")]
	public class CusExitItemPackage : CusInvPack<CusExitItem, CusExitItemPackageLookups, CusExitItemPackageValidation>, ISynchroniserReadOnlyMembersProvider, Integration.Customs.EU.ICusExitItemPackage
	{
		public CusExitItemPackage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new static readonly CusExitItemPackageTypeDecider TypeDecider = new CusExitItemPackageTypeDecider();

		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}

		[ResourceStringData("7B3D73B5-937D-4C41-A538-87BEED8D7526", Caption = "Marks & Numbers")]
		public override ZString B5_MarksAndNumbers { get => base.B5_MarksAndNumbers; set => base.B5_MarksAndNumbers = value; }

		[ResourceStringData("DADE9B6E-8D02-48CC-A05A-19F82ACE515B", Caption = "Pack Type")]
		[List(nameof(Lookups) + "." + nameof(CusExitItemPackageLookups.UnitTypeList))]
		public override ZString B5_UnitType { get => base.B5_UnitType; set => base.B5_UnitType = value; }

		[ResourceStringData("9DFCCFE9-17F7-479B-9D43-ECE74C0A0523", Caption = "Pack Qty")]
		public override ZLong B5_UnitCount { get => base.B5_UnitCount; set => base.B5_UnitCount = value; }

		public new CusExitItemPackageLookups Lookups => new CusExitItemPackageLookups(this);
		protected override CusInvPackLookups GetNewLookups() => new CusExitItemPackageLookups(this);
	}
}
