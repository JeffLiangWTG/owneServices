using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyBusinessObjectWrapperWithInfo : DummyBusinessObjectWrapper
	{
		public DummyBusinessObjectWrapperWithInfo(DummyBusinessObject bizO)
			: base(bizO)
		{
			UnRegisterEditableChildObject(bizO);
		}

		public DummyBusinessObject WrappedDummy
		{
			get { return ((DummyBusinessObject)WrappedBusinessObject); }
		}

		public ZString WrappedZ0_Description
		{
			get { return WrappedDummy.Z0_Description; }
			set { WrappedDummy.Z0_Description = value; }
		}

		public ZPropertyInfo WrappedZ0_DescriptionInfo
		{
			get { return WrappedBusinessObject == null ? null : GetWrappedZPropertyInfo(nameof(WrappedZ0_Description), x => WrappedDummy.Z0_DescriptionInfo); }
		}

		public ZString WrappedZ0_Code
		{
			get { return WrappedDummy.Z0_Code; }
			set { WrappedDummy.Z0_Code = value; }
		}

		public ZPropertyInfo WrappedZ0_CodeInfo
		{
			get { return WrappedBusinessObject == null ? null : GetWrappedZPropertyInfo("WrappedDummy+Z0_CodeInfo", x => WrappedDummy.Z0_CodeInfo); }
		}
	}
}
