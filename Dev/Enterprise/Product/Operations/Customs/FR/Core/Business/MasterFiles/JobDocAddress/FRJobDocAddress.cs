using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class FRJobDocAddress : JobDocAddress
	{
		public FRJobDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZGuid E2_OA_Address
		{
			get => base.E2_OA_Address;
			set
			{
				if (base.E2_OA_Address != value)
				{
					base.E2_OA_Address = value;

					var bizObj = Parent as BusinessObject;
					if (bizObj != null)
					{
						bizObj.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString E2_RN_NKCountryCode
		{
			get => base.E2_RN_NKCountryCode;
			set
			{
				if (base.E2_RN_NKCountryCode != value)
				{
					base.E2_RN_NKCountryCode = value;

					var bizObj = Parent as BusinessObject;
					if (bizObj != null)
					{
						bizObj.MarkAsNeedingValidation();
					}
				}
			}
		}
	}
}
