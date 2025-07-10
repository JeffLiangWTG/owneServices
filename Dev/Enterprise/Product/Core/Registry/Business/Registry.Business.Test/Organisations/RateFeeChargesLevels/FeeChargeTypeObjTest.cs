using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FeeChargeType))]
	sealed class FeeChargeTypeObjTest : RegistryBusinessObjectTemplateTestCase<FeeChargeType>
	{
		public void TestFeeChargeType()
		{
			BizObj.Code = ZString.Empty;
			AssertHasError(BizObj.CodeInfo, "Please enter a value.");

			BizObj.Code = "AAA";
			AssertNoErrors(BizObj.CodeInfo);
		}

		public void TestfeeChargeTypeDescription()
		{
			BizObj.Description = (NoResString)ZString.Empty;
			AssertHasError(BizObj.DescriptionInfo, "Please enter a value.");

			BizObj.Description = (NoResString)"Test Description";
			AssertNoErrors(BizObj.DescriptionInfo);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override FeeChargeType GetBusinessObjectToClone()
		{
			return (FeeChargeType)GetNewBusinessObject();
		}

		protected override FeeChargeType GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FeeChargeType();
		}
	}
}
