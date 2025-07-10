using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyBusinessObjectSupportMultipleResourceStringData3 : DummyBusinessObjectSupportMultipleResourceStringData1
	{
		public DummyBusinessObjectSupportMultipleResourceStringData3(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool IsECCCompliant
		{
			get
			{
				throw new DeletedRowInaccessibleException();
			}
			set
			{
				base.IsECCCompliant = value;
			}
		}
	}
}
