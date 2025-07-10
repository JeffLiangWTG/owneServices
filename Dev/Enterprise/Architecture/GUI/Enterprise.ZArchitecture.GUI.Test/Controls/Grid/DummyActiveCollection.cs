using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Testing
{
	class DummyActiveCollection : ActiveBusinessObjectCollection<DummyWithActiveCollection>, IBusinessObjectCollectionNotificationsViewerProvider
	{
		public DummyActiveCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter) { }

		protected override bool AllowNew => allowNew;

		public bool SetAllowAddNewTo(bool canAdd) => allowNew = canAdd;

		bool allowNew;

		public bool AllowNew_Expose => AllowNew;

		List<(ZString FieldName, ResourceStringData Caption, ZInt ColumnWidth)> IBusinessObjectCollectionNotificationsViewerProvider.HumanReadableColumns
		{
			get => new List<(ZString, ResourceStringData, ZInt ColumnWidth)> { (DummyBusinessObject.Schema.Z0_Code, Res.GetData("DummyActiveCollection|fffb67fc-cb5a-4d41-bab2-3895887f1ed4", "~Dummy Column~"), 100) };
		}
	}
}
