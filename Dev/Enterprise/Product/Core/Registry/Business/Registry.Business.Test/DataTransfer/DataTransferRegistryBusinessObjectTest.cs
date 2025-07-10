using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DataTransferRegistryBusinessObject))]
	public class DataTransferRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDirectory()
		{
			BizObj.Directory = "ABC";
			Assert("Directory has no error", !BizObj.DirectoryInfo.HasErrors());
			BizObj.Directory = ZString.Empty;
			AssertEquals("Error when not entered:", true, BizObj.DirectoryInfo.HasErrors());
		}

		public virtual void TestIsGoodToGo()
		{
			AssertEquals(false, BizObj.IsGoodToGo);

			BizObj.Directory = Env.TempPath;
			AssertEquals(false, BizObj.IsGoodToGo);

			BizObj.NextRunDateTime = ZDateTime.Now;
			BizObj.UpdateRuns(ZDateTime.Now);
			AssertEquals(false, BizObj.IsGoodToGo);

			BizObj.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			AssertEquals(true, BizObj.IsGoodToGo);
		}

		public void TestGroupPK()
		{
			AssertEquals("Initial value", true, BizObj.GroupPK.IsEmpty);
			BizObj.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			AssertEquals(Core.Constants.Groups.PostMastersGroupPK, BizObj.GroupPK);
		}

		#region Implementation

		#region Overrides
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.Directory = Env.TempPath;
			BizObj.NextRunDateTime = ZDateTime.Now.AddDays(1);
			BizObj.UpdateRuns(ZDateTime.Now);
			BizObj.Interval = 1;

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

		new DataTransferRegistryBusinessObject BizObj
		{
			get
			{
				return (DataTransferRegistryBusinessObject)base.BizObj;
			}
		}
		#endregion

		#endregion
	}
}
