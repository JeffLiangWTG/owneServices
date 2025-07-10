using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	[TestedType(typeof(JASOrgCreditorGroup))]
	class JASOrgCreditorGroupTest : OrgCreditorGroupTest
	{
		public void TestTypeDecided()
		{
			AssertEquals("Should be type decided", typeof(JASOrgCreditorGroup), Factory.New<OrgCreditorGroup>().GetType());
		}

		public void TestDelete()
		{
			JASOrgCreditorGroup creditorGroup = Factory.New<JASOrgCreditorGroup>();
			CreateNewMapping(creditorGroup);
			CreateNewMapping(creditorGroup);
			CreateNewMapping(creditorGroup);
			CreateNewMapping(creditorGroup);
			AssertEquals("Pre-condition. There should be 4 loaded", 4, Factory.Load<CognosCreditorMapping>(new ZQuery()).Length);
			creditorGroup.Delete();
			AssertEquals("Should be deleted when CreditorGroup is deleted", 0, Factory.Load<CognosCreditorMapping>(new ZQuery()).Length);
			ZQuery extraInfoFilter = new ZQuery();
			extraInfoFilter.FetchOnlyFromLocalCache = true;
			CognosAccGLAccountDescriptorExtraInfo[] extraInfoArray = Factory.Load<CognosAccGLAccountDescriptorExtraInfo>(extraInfoFilter);
			AssertEquals("ExtraInfo should not be deleted, only the mapping", 4, extraInfoArray.Length);
			foreach (CognosAccGLAccountDescriptorExtraInfo extraInfo in extraInfoArray)
			{
				Assert("ExtraInfo should not be deleted, only the mapping", !extraInfo.IsDeleted);
			}
		}

		void CreateNewMapping(JASOrgCreditorGroup creditorGroup)
		{
			CognosAccGLAccountDescriptorExtraInfo extraInfo = Factory.New<CognosAccGLAccountDescriptorExtraInfo>();
			CognosCreditorMapping mapping = Factory.New<CognosCreditorMapping>();
			mapping.T7_OG = creditorGroup.PK;
			mapping.T7_T9 = extraInfo.PK;
		}
	}
}
