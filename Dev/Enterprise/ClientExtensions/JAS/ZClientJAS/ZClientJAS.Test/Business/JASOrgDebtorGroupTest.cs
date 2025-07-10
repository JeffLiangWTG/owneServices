using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	[TestedType(typeof(JASOrgDebtorGroup))]
	class JASOrgDebtorGroupTest : OrgDebtorGroupTest
	{
		public void TestTypeDecided()
		{
			AssertEquals("Should be type decided", typeof(JASOrgDebtorGroup), Factory.New<OrgDebtorGroup>().GetType());
		}

		public void TestDelete()
		{
			JASOrgDebtorGroup debtorGroup = Factory.New<JASOrgDebtorGroup>();
			CreateNewMapping(debtorGroup);
			CreateNewMapping(debtorGroup);
			CreateNewMapping(debtorGroup);
			CreateNewMapping(debtorGroup);
			AssertEquals("Pre-condition. There should be 4 loaded", 4, Factory.Load<CognosDebtorMapping>(new ZQuery()).Length);
			debtorGroup.Delete();
			AssertEquals("Should be deleted when DebtorGroup is deleted", 0, Factory.Load<CognosDebtorMapping>(new ZQuery()).Length);
			ZQuery extraInfoFilter = new ZQuery();
			extraInfoFilter.FetchOnlyFromLocalCache = true;
			CognosAccGLAccountDescriptorExtraInfo[] extraInfoArray = Factory.Load<CognosAccGLAccountDescriptorExtraInfo>(extraInfoFilter);
			AssertEquals("ExtraInfo should not be deleted, only the mapping", 4, extraInfoArray.Length);
			foreach (CognosAccGLAccountDescriptorExtraInfo extraInfo in extraInfoArray)
			{
				Assert("ExtraInfo should not be deleted, only the mapping", !extraInfo.IsDeleted);
			}
		}

		void CreateNewMapping(JASOrgDebtorGroup debtorGroup)
		{
			CognosAccGLAccountDescriptorExtraInfo extraInfo = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			CognosDebtorMapping mapping = Factory.New<CognosDebtorMapping>();
			mapping.T8_OJ = debtorGroup.PK;
			mapping.T8_T9 = extraInfo.PK;
		}
	}
}
