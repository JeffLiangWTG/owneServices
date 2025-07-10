using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class BaseCusSeaManOBLHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateBO_RL_NKLoadPort()
		{
			AssertListValidation(Header.BO_RL_NKLoadPortInfo, "NZAKL");
		}

		public void TestValidateBO_RL_NKDestinationPort()
		{
			AssertListValidation(Header.BO_RL_NKDestinationPortInfo, "AUSYD");
		}

		#region Implementation

		protected abstract BaseCusSeaManOBLHeader GetNewOBLHeader();

		BaseCusSeaManOBLHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = GetNewOBLHeader();
				}
				return fHeader;
			}
		}
		BaseCusSeaManOBLHeader fHeader;

		protected void AssertListValidation(ZPropertyInfo info, ZGuid validCode, bool required)
		{
			AssertListValidation(info, ZGuid.Invalid, validCode, ZGuid.Empty, required);
		}

		protected void AssertListValidation(ZPropertyInfo info, ZString validCode)
		{
			AssertListValidation(info, new ZString("~~"), validCode, ZString.Empty, true);
		}

		protected void AssertListValidation(ZPropertyInfo info, IZType invalidCode, IZType validCode, IZType emptyCode, bool required)
		{
			info.Value = emptyCode;
			if (required)
			{
				AssertHasMessageErrors("when empty", info);
			}
			else
			{
				AssertNoMessageErrors("when empty", info);
			}

			info.Value = invalidCode;
			AssertHasMessageErrors("when invalid code entered", info);

			info.Value = validCode;
			AssertNoMessageErrors("when valid code entered", info);
			AssertNoErrors("when valid code entered", info);
		}

		#endregion
	}
}
