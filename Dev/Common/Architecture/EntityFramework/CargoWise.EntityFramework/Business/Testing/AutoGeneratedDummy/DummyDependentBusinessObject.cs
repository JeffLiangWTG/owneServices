#if DEBUG

using System;
using System.Data;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	public interface IDummyDependant
	{
	}

	public class DummyWithDependentsBusinessObject : DummyBaseBusinessObject
	{
		public DummyWithDependentsBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Dependents

		[ChildEditable]
		public DummyDependentBusinessObjectCollection Dependents
		{
			get
			{
				if (fDependents == null)
				{
					fDependents = new DummyDependentBusinessObjectCollection(this, Factory);
					fDependents.Load();

					RegisterEditableChildObject(fDependents);
				}

				return fDependents;
			}
		}
		DummyDependentBusinessObjectCollection fDependents;

		public ActiveBusinessObjectCollection<DummyDependantBusinessObject> ActiveDependents
		{
			get
			{
				DependentRelationship relationship = new DependentRelationship(this, typeof(DummyDependantBusinessObject));
				return new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory, relationship);
			}
		}

		#endregion
	}

	public class DummyWithDependentsAndClusterKeyBusinessObject : DummyWithDependentsBusinessObject, IClusterKeyEntity
	{
		public DummyWithDependentsAndClusterKeyBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)Z0_NumberInfo;
	}

	sealed class DummyWithDependentsAndTypeDeciderContextBusinessObject : DummyWithDependentsBusinessObject, ITypeDeciderContext
	{
		public DummyWithDependentsAndTypeDeciderContextBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		string ITypeDeciderContext.Country => "XX";
	}

	[AllowAllObjectsToBeLoaded]
	[DescriptionProperty(DummyDependantBusinessObject.Schema.ZD1_NumberUnitCode)]
	public class DummyDependantBusinessObject : AutoDummyDependentBizo, IDummyDependant
	{
		public new abstract class Schema : AutoDummyDependentBizo.Schema
		{
			public const string ZD1_WrappedNumberProperty = "ZD1_WrappedNumberProperty";
		}

		public DummyDependantBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static DummyDependantBusinessObject New(BusinessObjectFactory factory)
		{
			return (DummyDependantBusinessObject)factory.New(typeof(DummyDependantBusinessObject));
		}

		#region TypeDecider

		public readonly static DummyDependantTypeDecider TypeDecider = new DummyDependantTypeDecider();

		public class DummyDependantTypeDecider : TypeDecider
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "For unit testing only, remove for static resetter rule")]
			public Type TypeForLoadOverride;

			public override Type GetTypeForNew()
			{
				return typeof(DummyDependantBusinessObject);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return TypeForLoadOverride ?? typeof(DummyDependantBusinessObject);
			}

			public override Type GetTypeForBinding()
			{
				return GetTypeForNew();
			}

			protected override Type GetTypeForNewCore(ITypeDeciderContext context)
			{
				if (context?.Country == "XX")
				{
					return typeof(DummyDependantBusinessObjectXXCountry);
				}
				else
				{
					return typeof(DummyDependantBusinessObject);
				}
			}
		}

		public class DummyDependantBusinessObjectXXCountry : DummyDependantBusinessObject
		{
			public DummyDependantBusinessObjectXXCountry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		#endregion

		public DummyBusinessObjectCollection PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett
		{
			get
			{
				if (fPotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett == null)
				{
					fPotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett = new DummyBusinessObjectCollection(Factory);
				}
				return fPotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett;
			}
		}
		DummyBusinessObjectCollection fPotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett;

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(DummyDependentBizoSchema.ZD1_Number, SQLComparisonOperator.NotEqual, ZD1_Number_IgnoreInActiveFilter); }
		}

		public const int ZD1_Number_WhenInError = 92;
		public const int ZD1_Number_IgnoreInActiveFilter = -321;

		public ZString NonPersistentProperty
		{
			get { return nonPersistentProperty; }
			set
			{
				nonPersistentProperty = value;
				Factory.InvalidateCachedProperties();
				NonPersistentPropertyInfo.RefreshBinding();
			}
		}
		ZString nonPersistentProperty;

		public ZPropertyInfo NonPersistentPropertyInfo
		{
			get { return GetZPropertyInfo(nameof(NonPersistentProperty)); }
		}

		public ZInt ZD1_WrappedNumberProperty
		{
			get { return ZD1_Number; }
			set { ZD1_Number = value; }
		}

		public ZPropertyInfo ZD1_WrappedNumberPropertyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZD1_WrappedNumberProperty, x => ZD1_NumberInfo); }
		}

		#region Business Object Overrides

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new BusinessObjectFetchStrategyForTest(this);
		}

		protected override void SetDefaultValues()
		{
			Row[Schema.ZD1_Number] = 0;
			Row[Schema.ZD1_Code] = "";
		}

		public int RunPreSaveValidationCount;
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			RunPreSaveValidationCount++;
		}

		protected override ZString GetAdditionalInfoForZSaveExceptionCore() => string.Format("Related Business Object PK: {0}", ZD1_Z0);

		protected override bool SupportsCloneCore() => true;
		#endregion

		#region Related Business Objects

		public DummyBusinessObject Parent
		{
			get { return Factory.Load<DummyBusinessObject>(ZD1_Z0); }
		}

		#endregion
	}

	class DummyDependantWithClusterKeyBusinessObject : DummyDependantBusinessObject, IClusterKeyEntity
	{
		public DummyDependantWithClusterKeyBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)ZD1_NumberInfo;

		public event EventHandler OnDeleting;

		public override void Delete()
		{
			OnDeleting?.Invoke(this, EventArgs.Empty);
			base.Delete();
		}
	}
}

#endif
