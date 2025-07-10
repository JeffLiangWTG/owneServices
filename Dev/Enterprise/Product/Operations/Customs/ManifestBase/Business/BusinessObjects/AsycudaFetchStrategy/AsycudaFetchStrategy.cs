using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ManifestBase
{
	public abstract class AsycudaFetchStrategy : EnterpriseBusinessObjectFetchStrategy, IAsycudaFetchStrategy
	{
		protected AsycudaFetchStrategy(EnterpriseBusinessObject businessObject)
			: base(businessObject)
		{
		}

		public const string FetchForDeleteKey = "AsycudaFetchForDelete";
		public const string FetchForLoadChildEditableObjectsKey = "AsycudaFetchForLoadChildEditableObjects";
		public const string FetchForValidateKey = "AsycudaFetchForValidate";
		public const string FetchForTreeTableStrategyKey = "AsycudaFetchForTreeTableStrategy";

		protected virtual void AddCommonFetchHints()
		{
		}

		protected virtual IEnumerable<BusinessObject> LoadChildren() => Enumerable.Empty<BusinessObject>();

		#region Delete
		protected sealed override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			AsycudaFetchStrategyHelper.FetchForDelete(BusinessObject);
		}

		public void AddFetchHintsForDelete() => AddFetchHintsForDeleteCore();
		protected virtual void AddFetchHintsForDeleteCore() => AddCommonFetchHints();
		protected virtual IEnumerable<BusinessObject> LoadChildrenForDelete() => LoadChildren();

		IEnumerable<BusinessObject> IAsycudaFetchStrategy.FetchForDelete()
		{
			SetBusinessObjectFetched(FetchForDeleteKey);
			FetchForDelete();
			return LoadChildrenForDelete();
		}
		#endregion

		#region LoadChildEditableObjects
		protected sealed override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			AsycudaFetchStrategyHelper.FetchForLoadChildEditableObjects(BusinessObject);
		}

		public void AddFetchHintsForLoadChildEditableObjects() => AddFetchHintsForLoadChildEditableObjectsCore();
		protected virtual void AddFetchHintsForLoadChildEditableObjectsCore() => AddCommonFetchHints();
		protected virtual IEnumerable<BusinessObject> LoadChildrenForLoadChildEditableObjects() => LoadChildren();

		IEnumerable<BusinessObject> IAsycudaFetchStrategy.FetchForLoadChildEditableObjects()
		{
			SetBusinessObjectFetched(FetchForLoadChildEditableObjectsKey);
			FetchForLoadChildEditableObjects();
			return LoadChildrenForLoadChildEditableObjects();
		}
		#endregion

		#region Validate
		protected sealed override void FetchForValidateCore()
		{
			AsycudaFetchStrategyHelper.FetchForValidate(BusinessObject);
		}

		public void AddFetchHintsForValidate() => AddFetchHintsForValidateCore();
		protected virtual void AddFetchHintsForValidateCore() => AddCommonFetchHints();
		protected virtual IEnumerable<BusinessObject> LoadChildrenForValidate() => LoadChildren();

		IEnumerable<BusinessObject> IAsycudaFetchStrategy.FetchForValidate()
		{
			SetBusinessObjectFetched(FetchForValidateKey);
			base.FetchForValidateCore();
			return LoadChildrenForValidate();
		}
		#endregion

		#region FetchForTreeTableStrategy
		public void FetchForTreeTableStrategy(IExternalFetchHintSupporter externalFetchHintSupporter)
		{
			if (!GetBusinessObjectFetched(FetchForTreeTableStrategyKey))
			{
				SetBusinessObjectFetched(FetchForTreeTableStrategyKey);
				AddFetchHintsForTreeTableStrategyCore(externalFetchHintSupporter);
			}
		}

		protected virtual void AddFetchHintsForTreeTableStrategyCore(IExternalFetchHintSupporter externalFetchHintSupporter)
		{
		}
		#endregion

		protected void SetBusinessObjectFetched(string key)
		{
			var alreadyFetched = BusinessObject.Factory.GetCachedValue(key, () => new HashSet<ZGuid>(), CacheStalenessPolicy.NeverStale);
			alreadyFetched.Add(BusinessObject.PK);
		}

		protected bool GetBusinessObjectFetched(string key)
		{
			var alreadyFetched = BusinessObject.Factory.GetCachedValue(key, () => new HashSet<ZGuid>(), CacheStalenessPolicy.NeverStale);
			return alreadyFetched.Contains(BusinessObject.PK);
		}
	}
}
