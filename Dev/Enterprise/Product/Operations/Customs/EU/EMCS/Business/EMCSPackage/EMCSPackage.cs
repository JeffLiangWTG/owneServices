using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	[SingleObjectAroundARow]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class EMCSPackage : CusInvPack<EMCSJobDeclaration, EMCSPackageLookups, EMCSPackageValidation>
		, ISynchroniserReadOnlyMembersProvider
	{
		public EMCSPackage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new EMCSPackageLookups Lookups => base.Lookups;

		public new EMCSPackageValidation Validation => base.Validation;

		[List(nameof(Lookups) + "." + nameof(EMCSPackageLookups.PackTypeList))]
		public override ZString B5_UnitType { get => base.B5_UnitType; set => base.B5_UnitType = value; }

		[MaxLength(999)]
		public override ZString B5_MarksAndNumbers { get => base.B5_MarksAndNumbers; set => base.B5_MarksAndNumbers = value; }

		public override bool ReadOnly
		{
			get => base.ReadOnly || IsMessageStatusSentOrAcknowledgedOnParent;
			set => base.ReadOnly = value;
		}

		public ZGuid FKToHeader => B5_ParentID;

		public override bool CanDelete => base.CanDelete && !IsMessageStatusSentOrAcknowledgedOnParent;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B5_UnitCount = ZLong.Zero;
		}

		protected override CusInvPackLookups GetNewLookups() => new EMCSPackageLookups(this);

		protected override CusInvPackValidation GetNewValidation() => new EMCSPackageValidation(this);

		bool IsMessageStatusSentOrAcknowledgedOnParent => Parent?.IsMessageStatusSentOrAcknowledged ?? false;

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion
	}
}
