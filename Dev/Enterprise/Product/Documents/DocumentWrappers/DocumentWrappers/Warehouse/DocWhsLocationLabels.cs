using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsLocationLabels : DocBaseWrapper
	{
		#region Constructors

		protected DocWhsLocationLabels(WhsLocationLabelList locationLabelControl, BusinessObjectFactory factoryToWrap)
			: base(locationLabelControl, factoryToWrap)
		{
		}

		#endregion

		#region Static

		public static DocWhsLocationLabels New(WhsLocationLabelList locationLabelControl, BusinessObjectFactory factoryToWrap)
		{
			return (locationLabelControl == null) ? null : new DocWhsLocationLabels(locationLabelControl, factoryToWrap);
		}

		#endregion

		#region Properties

		#region Collections

		public DocWhsLocationCollection Labels
		{
			get
			{
				return new DocWhsLocationCollection(LocationLabelControl.Locations, Factory);
			}
		}

		#endregion

		#endregion

		#region Implementation

		WhsLocationLabelList LocationLabelControl
		{
			get { return (WhsLocationLabelList)base.WrappedObject; }
		}

		#endregion
	}
}
