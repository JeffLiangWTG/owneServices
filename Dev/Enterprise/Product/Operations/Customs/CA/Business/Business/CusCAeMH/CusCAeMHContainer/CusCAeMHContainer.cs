using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[UserDefinedValues]
	[CodeProperty(CusCAeMHContainer.Schema.BQ_ContainerNumber), DescriptionProperty(CusCAeMHContainer.Schema.BQ_ContainerNumber)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusCAeMHContainer : AutoCusCAeMHContainer
		, Customs.Business.ISynchroniserReadOnlyMembersProvider, Integration.Customs.CA.ICusCAeMHContainer
	{
		#region Schema

		public new class Schema : AutoCusCAeMHContainer.Schema
		{
			public const string IsNonContainerized = "IsNonContainerized";
		}

		#endregion

		public CusCAeMHContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overriden

		public override ZString BQ_ContainerNumber
		{
			get { return IsNonContainerized ? (ZString)"NCT" : base.BQ_ContainerNumber; }
			set { base.BQ_ContainerNumber = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusCAeMHContainerLookups.ContainerTypes))]
		public override ZString BQ_RC_NKContainerType
		{
			get { return base.BQ_RC_NKContainerType; }
			set { base.BQ_RC_NKContainerType = value; }
		}

		[RelatedBusinessObject("MasterBill")]
		public override ZGuid BQ_BP_Master
		{
			get { return base.BQ_BP_Master; }
			set { base.BQ_BP_Master = value; }
		}

		public CusCAeMHMaster MasterBill
		{
			get { return Factory.Load<CusCAeMHMaster>(this.BQ_BP_Master); }
		}

		#endregion

		#region New Properties

		public CusCAeMHHouseContainerPivotCollectionForContainer Pivots
		{
			get { return fPivots ?? (fPivots = new CusCAeMHHouseContainerPivotCollectionForContainer(this)); }
		}
		CusCAeMHHouseContainerPivotCollectionForContainer fPivots;

		public ZBool IsNonContainerized
		{
			get { return this.GetUserDefinedValue<ZBool>(Schema.IsNonContainerized); }
			internal set { this.SetUserDefinedValue(Schema.IsNonContainerized, value); }
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region Implementation

		protected override CusCAeMHContainerValidation GetNewValidation()
		{
			return IsNonContainerized ? new CusCAeMHContainerValidation(this) : new CusCAeMHContainerizedContainerValidation(this);
		}

		public override bool ReadOnly
		{
			get { return IsNonContainerized || base.ReadOnly; }
			set { base.ReadOnly = value; }
		}

		public override bool CanDelete
		{
			get { return !IsNonContainerized; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("52E13455-DA87-4CC6-AC4C-88C213C77611", "Non-containerized container cannot be deleted."); }
		}

		public override void Delete()
		{
			Pivots.DeleteAll();
			base.Delete();
		}

		#endregion
	}
}
