using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public abstract class UniversalItemDataObjectReaderBase<TDataObject, TBusinessObject>  : TopLevelDataObjectReader<TDataObject, TBusinessObject>
		where TDataObject : TopLevelDataObject
		where TBusinessObject : BusinessObject
	{
		protected UniversalItemDataObjectReaderBase(TDataObject dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		protected TBusinessObject GetBusinessObjectFromContextKey()
		{
			if (dataObject.DataContext.DataTargetCollection != null && dataObject.DataContext.DataTargetCollection.Any())
			{
				var dataTarget =
					dataObject.DataContext.DataTargetCollection.FirstOrDefault(
						t => t.Type.HasValue && t.Type.Value == DataContextType.ToString());

				if (dataTarget != null)
				{
					BusinessObject foundBusinessObject;
					if (TryGetExistingBusinessObjectMatchedOnDataTargetKey(dataObject, logger, dataTarget, factory.BOFactory, out foundBusinessObject))
					{
						if (foundBusinessObject == null)
						{
							throw new DataObjectReadFailureException(GetReasonForFailedToGetBusinessObjectFromContextKey(dataTarget));
						}

						return (TBusinessObject)foundBusinessObject;
					}
				}
			}

			return null;
		}

		public bool TryGetExistingBusinessObjectMatchedOnDataTargetKey(ITopLevelDataObject universalItem,
			IXmlImportLogger logger,
			IDataTargetDataObject dataTarget,
			BusinessObjectFactory factory,
			out BusinessObject foundBusinessObject) => TryGetExistingBusinessObjectMatchedOnDataTargetKeyCore(universalItem, logger, dataTarget, factory, out foundBusinessObject);

		protected virtual bool TryGetExistingBusinessObjectMatchedOnDataTargetKeyCore(
			ITopLevelDataObject universalItem,
			IXmlImportLogger logger,
			IDataTargetDataObject dataTarget,
			BusinessObjectFactory factory,
			out BusinessObject foundBusinessObject
		)
		{
			foundBusinessObject = null;
			if (dataTarget == null || !dataTarget.Key.HasValue || dataTarget.Key.Value.IsEmpty)
			{
				return false;
			}

			foundBusinessObject = DataContextManager.LoadBusinessObjectFromDataTarget(universalItem, dataTarget, factory, logger);
			return true;
		}

		protected virtual string GetReasonForFailedToGetBusinessObjectFromContextKey(IDataTargetDataObject dataTarget)
			=> Res.GetString("4DA510CE-90C5-4466-A841-8899238FC4C2", "Match couldn't be found for {0} with Key {1}", DataContextType, dataTarget.Key);
	}
}
