using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Testing;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	sealed class DummyDocDataObjectWithFindBoxWithoutDescription : DocDataObject
	{
		public DummyDocDataObjectWithFindBoxWithoutDescription(object id = default)
			: base(id)
		{
		}

		#region Code

		[CargoWise.ComponentModel.List(nameof(CodeList))]
		[CustomFindBoxPopup(typeof(DummyCustomFindBoxPopup), false)]
		public ZString Code
		{
			get => code;
			set
			{
				if (SetNonPersistentPropertyValue(CodeInfo, ref code, value))
				{
					Validate(CodeInfo);
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		public object CodeList { get; set; }

		#endregion
	}
}
