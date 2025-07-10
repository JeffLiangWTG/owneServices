using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Business
{
	public interface IBusinessObjectCollectionNotificationsViewerProvider : IBusinessObjectCollection
	{
		List<(ZString FieldName, ResourceStringData Caption, ZInt ColumnWidth)> HumanReadableColumns { get; }
	}
}
