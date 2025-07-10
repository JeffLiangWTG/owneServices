using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	sealed class ADAWMappingUpdateHelperTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestWithNullUpdateInfo()
		{
			new ADAWMappingUpdateHelper().UpdateADAWTemplateMappingField(null);
		}

		public void TestUpdateADAWTemplateMappingField()
		{
			PrepareTestDataForADAW();

			var mappingUpdateInfo = new List<AdAWMappingUpdateInfo>()
			{
				new AdAWMappingUpdateInfo("ITest", "JI_Description", "JI_DescriptionNew", true, false, null),
				new AdAWMappingUpdateInfo("ITest", "RN_Code", "JI_CountryOfOrigin", true, false, null),
				new AdAWMappingUpdateInfo("ITest", "RN_Desc", "JI_CountryOfOrigin", true, false, "RN_Code"),
				new AdAWMappingUpdateInfo("ITest", "ChildBOCollection.ChildBO", "ChildBOCollection.ChildBONew", false, true, null),
			};

			new ADAWMappingUpdateHelper().UpdateADAWTemplateMappingField(mappingUpdateInfo);
			AssertITestInterfaceADAW();

			new ADAWMappingUpdateHelper().UpdateADAWTemplateMappingField(mappingUpdateInfo);
			AssertITestInterfaceADAW();
		}

		public void TestUpdateADAWTemplateMappingFieldReverseOrder()
		{
			PrepareTestDataForADAW();

			var mappingUpdateInfo = new List<AdAWMappingUpdateInfo>()
			{
				new AdAWMappingUpdateInfo("ITest", "ChildBOCollection.ChildBO", "ChildBOCollection.ChildBONew", false, true, null),
				new AdAWMappingUpdateInfo("ITest", "RN_Desc", "JI_CountryOfOrigin", true, false, "RN_Code"),
				new AdAWMappingUpdateInfo("ITest", "RN_Code", "JI_CountryOfOrigin", true, false, null),
				new AdAWMappingUpdateInfo("ITest", "JI_Description", "JI_DescriptionNew", true, false, null),
			};

			new ADAWMappingUpdateHelper().UpdateADAWTemplateMappingField(mappingUpdateInfo);
			AssertITestInterfaceADAW();

			new ADAWMappingUpdateHelper().UpdateADAWTemplateMappingField(mappingUpdateInfo);
			AssertITestInterfaceADAW();
		}

		public void TestMultipleInterfaces()
		{
			PrepareTestDataForADAW();

			var mappingUpdateInfo = new List<AdAWMappingUpdateInfo>()
			{
				new AdAWMappingUpdateInfo("ITestSomeOtherInterface", "RN_Desc", "JI_CountryOfOriginInOtherCountry", true, false, "JI_Description"),
				new AdAWMappingUpdateInfo("ITestSomeOtherInterface", "JI_Description", "JI_DescriptionInOtherCountry", true, false, "RN_Code"),
				new AdAWMappingUpdateInfo("ITest", "JI_Description", "JI_DescriptionNew", true, false, null),
				new AdAWMappingUpdateInfo("ITest", "RN_Code", "JI_CountryOfOrigin", true, false, null),
				new AdAWMappingUpdateInfo("ITest", "RN_Desc", "JI_CountryOfOrigin", true, false, "RN_Code"),
				new AdAWMappingUpdateInfo("ITest", "ChildBOCollection.ChildBO", "ChildBOCollection.ChildBONew", false, true, null),
			};

			new ADAWMappingUpdateHelper().UpdateADAWTemplateMappingField(mappingUpdateInfo);
			AssertMultiInterfaceADAW();

			new ADAWMappingUpdateHelper().UpdateADAWTemplateMappingField(mappingUpdateInfo);
			AssertMultiInterfaceADAW();
		}

		void PrepareTestDataForADAW()
		{
			var sqlText = @"
INSERT INTO [StmModuleFilter]([S9_PK],[S9_GC],[S9_ModuleID],[S9_FilterData],[S9_IsPublished],[S9_SaveColumnLayout],[S9_RelatedEntityID],[S9_IsSystem],[S9_ColumnLayoutData],[S9_FilterName],[S9_FilterType],[S9_ParentID],[S9_ParentTableCode],[S9_GridColourLayoutID],[S9_SaveGridColourLayout])
VALUES
           (newid()
           ,null
           ,'GLOWDataImportV2_ITest'
           ,dbo.CLRCompressStringAsBytes('
<MappingTables>
	<MappingTable>
		<FieldMappings>
			<FieldMapping>
				<SourceColumnIndex>1</SourceColumnIndex>
				<TargetColumnName>JI_Description</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>2</SourceColumnIndex>
				<TargetColumnName>RN_Code</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>false</ShouldMatch>
					<ShouldCreate>true</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>3</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection.ChildBO</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>4</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection2.ChildBO</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>5</SourceColumnIndex>
				<TargetColumnName>RN_Desc</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
		</FieldMappings>
	</MappingTable>
</MappingTables>')
           ,0
           ,0
           ,null
           ,0
           ,null
           ,'Test'
           ,''
           ,null
           ,''
           ,null
           ,0)

INSERT INTO [StmModuleFilter]([S9_PK],[S9_GC],[S9_ModuleID],[S9_FilterData],[S9_IsPublished],[S9_SaveColumnLayout],[S9_RelatedEntityID],[S9_IsSystem],[S9_ColumnLayoutData],[S9_FilterName],[S9_FilterType],[S9_ParentID],[S9_ParentTableCode],[S9_GridColourLayoutID],[S9_SaveGridColourLayout])
VALUES
           (newid()
           ,null
           ,'GLOWDataImportV2_ITestSomeOtherInterface'
           ,dbo.CLRCompressStringAsBytes('
<MappingTables>
	<MappingTable>
		<FieldMappings>
			<FieldMapping>
				<SourceColumnIndex>1</SourceColumnIndex>
				<TargetColumnName>JI_Description</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>2</SourceColumnIndex>
				<TargetColumnName>RN_Desc</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>false</ShouldMatch>
					<ShouldCreate>true</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>3</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection.ChildBO</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
		</FieldMappings>
	</MappingTable>
</MappingTables>')
           ,0
           ,0
           ,null
           ,0
           ,null
           ,'Test'
           ,''
           ,null
           ,''
           ,null
           ,0)
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void AssertITestInterfaceADAW()
		{
			AssertADAW("GLOWDataImportV2_ITest",
				@"<MappingTables>
	<MappingTable>
		<FieldMappings>
			<FieldMapping>
				<SourceColumnIndex>1</SourceColumnIndex>
				<TargetColumnName>JI_DescriptionNew</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>2</SourceColumnIndex>
				<TargetColumnName>JI_CountryOfOrigin</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>3</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection.ChildBONew</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>false</ShouldMatch>
					<ShouldCreate>true</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>4</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection2.ChildBO</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
		</FieldMappings>
	</MappingTable>
</MappingTables>");
		}

		void AssertMultiInterfaceADAW()
		{
			AssertITestInterfaceADAW();
			AssertADAW("GLOWDataImportV2_ITestSomeOtherInterface",
	@"<MappingTables>
	<MappingTable>
		<FieldMappings>
			<FieldMapping>
				<SourceColumnIndex>1</SourceColumnIndex>
				<TargetColumnName>JI_DescriptionInOtherCountry</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
			<FieldMapping>
				<SourceColumnIndex>3</SourceColumnIndex>
				<TargetColumnName>ChildBOCollection.ChildBO</TargetColumnName>
				<MatchingInfo>
					<ShouldMatch>true</ShouldMatch>
					<ShouldCreate>false</ShouldCreate>
				</MatchingInfo>
			</FieldMapping>
		</FieldMappings>
	</MappingTable>
</MappingTables>");
		}

		void AssertADAW(string interfaceName, string result)
		{
			var sqlText = $@"
Select dbo.CLRUncompressAsString(S9_FilterData)
from dbo.StmModuleFilter
WHERE S9_ModuleID = '{interfaceName}'
";

			using (var cmd = TestConnection.Command(sqlText))
			{
				var templatedata = (string)cmd.ExecuteScalar();
				AssertEquals(result.Replace("\t", ""), templatedata.Replace("\t", "").Replace(" ", ""));
			}
		}
	}
}
