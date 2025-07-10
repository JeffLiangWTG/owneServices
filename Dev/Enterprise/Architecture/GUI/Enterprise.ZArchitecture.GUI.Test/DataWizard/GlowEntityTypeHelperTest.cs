using System;
using CargoWise.Application;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GlowEntityTypeHelperTest : TestCaseWithDummy
	{
		public void TestGetImportMappings_GlowInterfaceIsNull()
		{
			var mapping = Factory.NewWithValidTestData<StmModuleFilter>();
			mapping.S9_ModuleID = "GLOWDataImportV2_IDummyLogged";
			mapping.S9_RelatedEntityID = Guid.Empty;

			Factory.Save();

			var mappings = GlowEntityTypeHelper.GetImportMappings(Factory, typeof(DummyLogged));
			AssertEquals(mappings.Length, 0);
		}

		public void TestGetImportMappings_GlowInterfaceExists()
		{
			var mapping1 = Factory.NewWithValidTestData<StmModuleFilter>();
			mapping1.S9_FilterName = "mapping name1";
			mapping1.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping1.S9_RelatedEntityID = Guid.Empty;

			var mapping2 = Factory.NewWithValidTestData<StmModuleFilter>();
			mapping2.S9_FilterName = "mapping name2";
			mapping2.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping2.S9_RelatedEntityID = Guid.Empty;

			var mapping3 = Factory.NewWithValidTestData<StmModuleFilter>();
			mapping3.S9_FilterName = "mapping name3";
			mapping3.S9_ModuleID = "GLOWDataImportV2_ISomethingElse";
			mapping3.S9_RelatedEntityID = Guid.Empty;

			var mapping4 = Factory.NewWithValidTestData<StmModuleFilter>();
			mapping4.S9_FilterName = "mapping name4";
			mapping4.S9_ModuleID = "GLOWDataImportV2_IDummyBizo";
			mapping4.S9_RelatedEntityID = Guid.Empty;

			Factory.Save();
			try
			{
				var parserMock = new Mock<IDataTransferMappingParser>();
				parserMock.Setup(p => p.FromModuleFilterInfo(It.IsAny<IModuleFilterInfo>())).Returns((IModuleFilterInfo filterInfo) =>
				{
					var mappingMock = new Mock<IDataTransferMapping>();
					mappingMock.Setup(m => m.Name).Returns(filterInfo.FilterName);
					mappingMock.Setup(m => m.PK).Returns(filterInfo.PK);
					return mappingMock.Object;
				});

				ObjectFactory.Substitute(parserMock.Object);
				var mappings = GlowEntityTypeHelper.GetImportMappings(Factory, typeof(DummyBusinessObject));
				AssertEquals(mappings.Length, 3);
				AssertEquals(mappings[0].Name, "mapping name1");
				AssertEquals(mappings[0].PK, mapping1.PK.ToGuid());
				AssertEquals(mappings[1].Name, "mapping name2");
				AssertEquals(mappings[1].PK, mapping2.PK.ToGuid());
				AssertEquals(mappings[2].Name, "mapping name4");
				AssertEquals(mappings[2].PK, mapping4.PK.ToGuid());
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
			}
		}
	}
}
