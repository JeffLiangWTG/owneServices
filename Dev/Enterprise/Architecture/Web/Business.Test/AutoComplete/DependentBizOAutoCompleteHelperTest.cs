using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	public abstract class DependentBizOAutoCompleteHelperTest : AutoCompleteHelperTest
	{
		#region Test Additional Parameters Serialization

		public virtual void TestAdditionalParamsSerialization()
		{
			ZGuid expectedPK = ZGuid.NewZGuid();
			((DependentBizOAutoCompleteHelper)Helper).ParentPK = expectedPK;

			string paramsStr = Helper.SerializeAdditionalParamsToString();

			((DependentBizOAutoCompleteHelper)Helper).ParentPK = ZGuid.NewZGuid();

			Helper.RestoreAdditionalParamsFromSerializedString(paramsStr);

			AssertEquals("ParentPK", expectedPK, ((DependentBizOAutoCompleteHelper)Helper).ParentPK);
		}

		#endregion

	}
}
