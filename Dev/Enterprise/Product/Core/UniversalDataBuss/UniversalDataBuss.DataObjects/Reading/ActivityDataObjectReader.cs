using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public abstract class ActivityDataObjectReader<TBusinessObject> : UniversalItemDataObjectReaderBase<Activity, TBusinessObject>
		where TBusinessObject : BusinessObject, IWorkflowProviderCore
	{
		protected ActivityDataObjectReader(Activity dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		protected override TBusinessObject GetExistingBusinessObject()
		{
			return GetBusinessObjectFromContextKey();
		}

		protected sealed override void PopulateBusinessObject(TBusinessObject targetBusinessObject)
		{
			PopulateBusinessObjectCore(targetBusinessObject);
			PopulateWorkflowCustomFields(targetBusinessObject, dataObject);
		}

		protected abstract void PopulateBusinessObjectCore(TBusinessObject targetBusinessObject);

		protected override bool LogChildTopLevelObjectsOnImport => true;
	}
}
