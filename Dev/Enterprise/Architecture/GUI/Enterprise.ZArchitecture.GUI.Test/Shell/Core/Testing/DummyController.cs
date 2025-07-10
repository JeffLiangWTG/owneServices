using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestExcludeZControllersAllHaveSecurityCheckpoints]
	public class DummyController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => DummyControllerIDs.Dummy;

		public override ModuleIdentifier ModuleID => DummyModuleIDs.Dummy;

		protected override string GetIDForFormCache(IBusiness businessEntity) => string.IsNullOrEmpty(IDForFormCache) ? base.GetIDForFormCache(businessEntity) : IDForFormCache;

		public string IDForFormCache;

		public override Type TypeOfTopLevelBusinessObject => typeof(DummyEnterpriseBusinessObject);

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			LastCreatedBizObject = (BusinessObject)base.GetNewBusinessEntityInLocalFactory();
			return LastCreatedBizObject;
		}

		protected internal override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			BusinessObject result = factory.Load<DummyBusinessObject>(sourceEntityPK);
			if (result == null)
			{
				var dummyChild = factory.Load<DummyDependantBusinessObject>(sourceEntityPK);
				result = (dummyChild == null) ? null : dummyChild.Parent;
			}

			return result;
		}

		Dictionary<IBusiness, ZString> dependentObjects;
		public Dictionary<IBusiness, ZString> DependentObjects
		{
			get
			{
				if (dependentObjects == null)
				{
					dependentObjects = new Dictionary<IBusiness, ZString>();
				}
				return dependentObjects;
			}
		}

		protected override IZForm GetForm(IBusiness businessEntity) => GetFormCore(businessEntity);

		public virtual IZForm GetFormCore(IBusiness businessEntity)
		{
			LastFormCreated = new ZDummyForm(businessEntity);
			foreach (var obj in DependentObjects)
			{
				(LastFormCreated as ZDummyForm).DependentObjects.Add(obj.Key, obj.Value);
			}
			return LastFormCreated;
		}

		protected override SecurityCheckpoint CheckPointForView => (SecurityCheckpoint)EnvProxy.Instance.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => (SecurityCheckpoint)EnvProxy.Instance.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => (SecurityCheckpoint)EnvProxy.Instance.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => (SecurityCheckpoint)EnvProxy.Instance.Security.None;

		public void SetInitialTabPageNameForForm_Exposed(string tabPageName)
		{
			SetInitialTabPageNameToSelectWhenAFormIsShown(tabPageName);
		}

		public override bool MakeUrlOnlyOpenableForCurrentCompany(ZGuid guid) => !guid.Equals(ZGuid.BrettsGuid) && base.MakeUrlOnlyOpenableForCurrentCompany(guid);

		internal ZForm LastFormCreated;
		internal BusinessObject LastCreatedBizObject;
	}
}
