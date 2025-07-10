using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DataTransferSwitchRegistryBusinessObject))]
	sealed class DataTransferSwitchRegistryBusinessObjectTest : DataTransferRegistryBusinessObjectTest
	{
		public override void TestIsGoodToGo()
		{
			AssertEquals(false, BizObj.IsGoodToGo);

			BizObj.Directory = Env.TempPath;
			AssertEquals(false, BizObj.IsGoodToGo);

			BizObj.NextRunDateTime = ZDateTime.Now;
			BizObj.UpdateRuns(ZDateTime.Now);
			AssertEquals(false, BizObj.IsGoodToGo);

			BizObj.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			AssertEquals(false, BizObj.IsGoodToGo);

			BizObj.EnableInterface = true;
			AssertEquals(true, BizObj.IsGoodToGo);
		}

		#region Implementation

		#region Overrides
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			base.GetBusinessObjectToSerialise();
			BizObj.EnableInterface = true;
			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		new DataTransferSwitchRegistryBusinessObject BizObj
		{
			get
			{
				return (DataTransferSwitchRegistryBusinessObject)base.BizObj;
			}
		}
		#endregion

		#endregion
	}
}
