using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public class DummyAsycudaManifestHeader : AsycudaManifestHeader
	{
		public DummyAsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			AMA_RN_NKCountry = "US";
		}

		public sealed override ZString AMA_RN_NKCountry
		{
			get => base.AMA_RN_NKCountry;
			set
			{
				using (GetCheckBusinessObjectTypeSuspender())
				{
					base.AMA_RN_NKCountry = value;
				}
			}
		}

		public override ZString CustomsSystem => "US";
	}
}
