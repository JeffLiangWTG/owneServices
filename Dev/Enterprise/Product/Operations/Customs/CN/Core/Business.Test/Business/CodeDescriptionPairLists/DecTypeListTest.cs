using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class DecTypeListTest : TestCaseWithFactory
	{
		public void TestGetDecTypeListDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			using (CNCustomsDataRegistry.Instance.CNBTHFunctionActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				AssertEquals("MessageSubTypeList should have 3 items", 3, declaration.Lookups.MessageSubTypeList.Count);
				AssertEquals("进口报关单", "进口报关单", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode("CUS"));
				AssertEquals("进境备案清单", "进境备案清单", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode("REC"));
				AssertEquals("进口报关单+出境备案清单", "进口报关单+出境备案清单", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode("BTH"));
				var anotherDeclaration = Factory.New<JobDeclaration>();
				anotherDeclaration.JE_MessageType = "IMP";
				AssertSame("Should have cached the result", declaration.Lookups.MessageSubTypeList, anotherDeclaration.Lookups.MessageSubTypeList);
				declaration.JE_MessageType = "EXP";
				AssertEquals("MessageSubTypeList should have 3 items", 3, declaration.Lookups.MessageSubTypeList.Count);
				AssertEquals("出口报关单", "出口报关单", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode("CUS"));
				AssertEquals("出境备案清单", "出境备案清单", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode("REC"));
				AssertEquals("进境备案清单+出口报关单", "进境备案清单+出口报关单", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode("BTH"));
			}

			using (CNCustomsDataRegistry.Instance.CNBTHFunctionActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), false))
			{
				declaration.JE_MessageType = "IMP";
				AssertEquals("MessageSubTypeList should have 2 items", 2, declaration.Lookups.MessageSubTypeList.Count);
				AssertEquals("进口报关单", "进口报关单", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode("CUS"));
				AssertEquals("进境备案清单", "进境备案清单", declaration.Lookups.MessageSubTypeList.GetDescriptionFromCode("REC"));
			}
		}

		public void TestGetDecTypeListFactory()
		{
			using (CNCustomsDataRegistry.Instance.CNBTHFunctionActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var testList = DecTypeList.GetDecTypeList(Factory, false, false);
				AssertEquals("MessageSubTypeList should have 3 items", 3, testList.Count);
				AssertEquals("报关单", "报关单", testList.GetDescriptionFromCode("CUS"));
				AssertEquals("备案清单", "备案清单", testList.GetDescriptionFromCode("REC"));
				AssertEquals("报关单+备案清单", "报关单+备案清单", testList.GetDescriptionFromCode("BTH"));
				Assert("Dec type should not be translatable", new DecTypeList() is UntranslatableCodeDescriptionPairList);
			}
		}
	}
}
