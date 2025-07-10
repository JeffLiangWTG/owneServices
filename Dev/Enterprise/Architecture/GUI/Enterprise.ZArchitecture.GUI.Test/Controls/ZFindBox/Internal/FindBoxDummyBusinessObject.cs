using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	public class FindBoxDummyBusinessObject : SuperDummyBusinessObject
	{
		public new class Schema : SuperDummyBusinessObject.Schema
		{
			public const string SS_DummyGuid = "SS_DummyGuid";
			public const string SS_Dummy = "SS_Dummy";
		}

		public FindBoxDummyBusinessObject(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			((INeedRow)this).Row[Schema.SS_Name] = "";
		}

		#endregion

		#region SS_DummyGuid

		public virtual ZGuid SS_DummyGuid
		{
			get { return new ZGuid(((INeedRow)this).Row["Z0_Guid"]); }
			set
			{
				if (value != SS_DummyGuid)
				{
					HasChanges = true;

					((INeedRow)this).Row["Z0_Guid"] = ((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
					SS_DummyGuidInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SS_DummyGuidInfo
		{
			get { return GetZPropertyInfo(nameof(SS_DummyGuid)); }
		}

		bool fSS_DummyGuid_ReadOnly;
		public bool SS_DummyGuid_ReadOnly
		{
			get
			{
				return fSS_DummyGuid_ReadOnly;
			}
			set
			{
				fSS_DummyGuid_ReadOnly = value;
				SS_DummyGuidInfo.RefreshBinding();
			}
		}

		#endregion

		#region SS_Dummy

		public virtual ZString SS_Dummy
		{
			get { return new ZString(((INeedRow)this).Row[DummyBusinessObject.Schema.Z0_FK_Code]); }
			set
			{
				if (value != SS_Dummy)
				{
					HasChanges = true;

					((INeedRow)this).Row[DummyBusinessObject.Schema.Z0_FK_Code] = ((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
					SS_DummyInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SS_DummyInfo
		{
			get { return GetZPropertyInfo(nameof(SS_Dummy)); }
		}

		public int SS_Dummy_MaxLength
		{
			get { return sS_Dummy_MaxLength; }
			set { sS_Dummy_MaxLength = value; }
		}
		int sS_Dummy_MaxLength = -1;

		bool fSS_Dummy_ReadOnly;
		public bool SS_Dummy_ReadOnly
		{
			get
			{
				return fSS_Dummy_ReadOnly;
			}
			set
			{
				fSS_Dummy_ReadOnly = value;
				SS_DummyInfo.RefreshBinding();
			}
		}

		#endregion

		#region Dummies

		DummyBusinessObjectCollection fDummies;
		public DummyBusinessObjectCollection Dummies
		{
			get
			{
				if (fDummies == null)
				{
					fDummies = new DummyBusinessObjectCollection(Factory, DummiesFilter);
				}
				return fDummies;
			}
		}

		public ZQuery fDummiesFilter = new ZQuery();
		public ZQuery DummiesFilter
		{
			get { return fDummiesFilter; }
			set { fDummiesFilter = value; }
		}

		#endregion

		#region Dummies For BindToList

		public DummyBusinessObjectCollection DummiesForBindToListChange => SS_Dummy == "AAXXX" ? DummyCollectionA : DummyCollectionB;

		DummyBusinessObjectCollection dummyCollectionA;
		DummyBusinessObjectCollection DummyCollectionA
		{
			get
			{
				if (dummyCollectionA == null)
				{
					dummyCollectionA = new DummyBusinessObjectCollection(Factory);
					dummyCollectionA.FindBoxListProviderOverride = new DummyBusinessObjectFindBoxListProvider(dummyCollectionA);
					var dummy = dummyCollectionA.AddNew();
					dummy.Z0_Code = "AAXXX";
					dummy.Z0_Description = "AAXXX Description";
				}
				return dummyCollectionA;
			}
		}

		DummyBusinessObjectCollection dummyCollectionB;
		DummyBusinessObjectCollection DummyCollectionB
		{
			get
			{
				if (dummyCollectionB == null)
				{
					dummyCollectionB = new DummyBusinessObjectCollection(Factory);
					dummyCollectionB.FindBoxListProviderOverride = new DummyBusinessObjectFindBoxListProvider(dummyCollectionB);
					var dummy = dummyCollectionB.AddNew();
					dummy.Z0_Code = "BBXXX";
					dummy.Z0_Description = "BBXXX Description";
				}
				return dummyCollectionB;
			}
		}

		class DummyBusinessObjectFindBoxListProvider : FindBoxListProvider
		{
			internal DummyBusinessObjectFindBoxListProvider(IBusinessObjectCollection collection)
				: base(collection)
			{
			}

			public override bool AutoCompleteOnCommit => true;
		}

		#endregion
	}
}
