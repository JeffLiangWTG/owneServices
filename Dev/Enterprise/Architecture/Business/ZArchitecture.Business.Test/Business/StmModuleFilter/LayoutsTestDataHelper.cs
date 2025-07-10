using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class LayoutsTestDataHelper
	{
		public LayoutsTestDataHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public const string TestModuleID = "testModuleID";
		readonly BusinessObjectFactory Factory;

		#region Creating a Layout with User Data

		public StmModuleFilter NewLayoutWithUserData(ZString layoutName)
		{
			return NewLayoutDataWithLayout(layoutName).Layout;
		}

		public StmModuleFilter NewLayoutWithUserData(ZString layoutName, ZString moduleID)
		{
			return NewLayoutDataWithLayout(layoutName, moduleID).Layout;
		}

		public StmModuleFilter NewLayoutWithUserData(ZString layoutName, ZGuid userPk, ZString userTablePrefix)
		{
			return NewLayoutDataWithLayout(layoutName, userPk, userTablePrefix).Layout;
		}

		#endregion

		#region Creating a Layout

		public StmModuleFilter NewLayout(ZString layoutName)
		{
			return NewLayout(layoutName, TestModuleID);
		}

		public T NewLayout<T>(ZString layoutName) where T : StmModuleFilter
		{
			return NewLayout<T>(layoutName, TestModuleID);
		}

		public StmModuleFilter NewLayout(ZString layoutName, ZString moduleID)
		{
			return NewLayout<StmModuleFilter>(layoutName, moduleID);
		}

		public T NewLayout<T>(ZString layoutName, ZString moduleID) where T : StmModuleFilter
		{
			T result = Factory.New<T>();
			result.S9_FilterName = layoutName;
			result.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			result.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			result.S9_ModuleID = moduleID;

			return result;
		}

		#endregion

		#region Creating a Published Layout

		public StmModuleFilter NewPublishedOrganisationLayoutForWeb(ZString layoutName, ZGuid organisationPK)
		{
			var layout = NewPublishedLayout(layoutName, TestModuleID);
			layout.SetIsPublishedForOrganisationInWeb(true, organisationPK);
			return layout;
		}

		public StmModuleFilter NewPublishedCompanyLayoutForWeb(ZString layoutName, ZGuid companyPK)
		{
			var layout = NewPublishedLayout(layoutName, TestModuleID);
			layout.SetIsPublishedForCompanyInWeb(true, companyPK);
			return layout;
		}

		public StmModuleFilter NewPublishedLayout(ZString layoutName)
		{
			return NewPublishedLayout(layoutName, TestModuleID);
		}

		public StmModuleFilter NewPublishedLayout(ZString layoutName, ZString moduleID)
		{
			StmModuleFilter result = NewLayout(layoutName, moduleID);
			result.S9_IsPublished = true;
			return result;
		}

		#endregion

		#region Creating a Layout User Data

		public StmModuleFilterUserData NewLayoutData(StmModuleFilter layout)
		{
			return NewLayoutData(layout, EnvProxy.Instance.CurrentUser.PK, GlbStaffSchema.Constants.Prefix);
		}

		public StmModuleFilterUserData NewLayoutData(StmModuleFilter layout, ZGuid userPk, ZString userTablePrefix)
		{
			StmModuleFilterUserData result = Factory.New<StmModuleFilterUserData>();
			result.S0_RelatedEntityID = userPk;
			result.S0_RelatedEntityTableCode = userTablePrefix;
			result.S0_S9 = layout.PK;

			if (userPk != layout.S9_RelatedEntityID)
			{
				layout.S9_RelatedEntityID = userPk;
			}

			return result;
		}

		#endregion

		#region Creating Layout User Data attached to a new Layout

		public StmModuleFilterUserData NewLayoutDataWithLayout(ZString layoutName)
		{
			return NewLayoutDataWithLayout(layoutName, TestModuleID);
		}

		public StmModuleFilterUserData NewLayoutDataWithLayout(ZString layoutName, ZString moduleID)
		{
			return NewLayoutData(NewLayout(layoutName, moduleID));
		}

		public StmModuleFilterUserData NewLayoutDataWithLayout(ZString layoutName, ZGuid userPk, ZString userTablePrefix)
		{
			return NewLayoutData(NewLayout(layoutName), userPk, userTablePrefix);
		}

		#endregion

		#region Creating the 'last used layout' StmData record

		public StmData NewLastUsedLayout(StmModuleFilter layout)
		{
			return NewLastUsedLayout(layout, EnvProxy.Instance.CurrentUser.PK);
		}

		public StmData NewLastUsedLayout(StmModuleFilter layout, ZGuid userPk)
		{
			StmData result = Factory.New<StmData>();
			result.SD_Owner = userPk;
			result.SD_DepartmentGuid = EnvProxy.Instance.CurrentCompany.PK;
			result.SD_GuidValue = layout.PK;
			result.SD_Name = TestModuleID;

			return result;
		}

		#endregion
	}
}
