using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class BusinessObjectCollectionNotificationsViewerProvider : DummyBusinessObjectCollection, IBusinessObjectCollectionNotificationsViewerProvider
	{
		public BusinessObjectCollectionNotificationsViewerProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		List<(ZString FieldName, ResourceStringData Caption, ZInt ColumnWidth)> IBusinessObjectCollectionNotificationsViewerProvider.HumanReadableColumns
		{
			get => new List<(ZString, ResourceStringData, ZInt ColumnWidth)> { (DummyBusinessObject.Schema.Z0_Code, Res.GetData("DummyActiveCollection|fffb67fc-cb5a-4d41-bab2-3895887f1ed4", "~Dummy Column~"), 100) };
		}
	}
}
