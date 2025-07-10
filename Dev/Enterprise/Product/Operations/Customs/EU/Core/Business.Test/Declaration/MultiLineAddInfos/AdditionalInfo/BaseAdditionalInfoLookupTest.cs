using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	sealed class BaseAdditionalInfoLookupTest : BusinessObjectLookupsTestCase
	{
		public void TestSubTypeList()
		{
			var subTypeList = additionalInfo.Lookups.SubTypeList;
			var expectedList = new AdditionalInfoSubTypeList();
			AssertContainsExactElementsInAnyOrder(expectedList.GetAllCodes(), subTypeList.GetAllCodes());
		}

		public void TestAdditonalInfoCodeList()
		{
			var config = new RefDataConfig(
			dataGroupings:
				[
					new(code: "EUN", description: "European Union", parent: ""),
					new(code: "FR", description: "France",  parent: "EUN")
				],
				cusCodeTypes:
				[
					new(typeCode: "ADDIN", description: "Additional Information", attributeTypes: [new(name: "Level", dataGrouping: "EUN"), new(name: "Direction", dataGrouping: "EUN")] ,
						cusCodes:
						[
							new(code: "CODE1", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item"), new(name: "Direction", value: "Import")]),
							new(code: "CODE2", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Item"), new(name: "Direction", value: "Export")]),
							new(code: "CODE3", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header"), new(name: "Direction", value: "Export")]),
						]
					),
					new(typeCode: "ADDIN", description: "Additional Information", attributeTypes: [new(name: "Level", dataGrouping: "FR"), new(name: "Direction", dataGrouping: "FR")] ,
						cusCodes:
						[
							new(code: "CODE4", dataGrouping: "FR", attributes: [new(name: "Level", value: "Item"), new(name: "Direction", value: "Export")]),
						]
					)
				]
			);
			EUUniversalTestDataHelper.SetUpTestRefData(Factory, config);

			importExportParent.IsImport = false;
			importExportParent.IsExport = true;
			importExportParent.DataGroupingCode = "EUN";
			importExportParent.Level = "Item";
			var codeList = additionalInfo.Lookups.GetAdditionalInformationList(importExportParent, "Export");
			AssertEquals(true, codeList.ContainsCode("CODE2"));

			importExportParent.IsImport = true;
			importExportParent.IsExport = false;
			codeList = additionalInfo.Lookups.GetAdditionalInformationList(importExportParent, "Import");
			AssertEquals(true, codeList.ContainsCode("CODE1"));

			importExportParent.IsImport = true;
			importExportParent.IsExport = true;
			codeList = additionalInfo.Lookups.GetAdditionalInformationList(importExportParent, "Both");
			AssertEquals(true, codeList.ContainsCode("CODE1"));
			AssertEquals(true, codeList.ContainsCode("CODE2"));

			importExportParent.IsExport = false;
			codeList = additionalInfo.Lookups.GetAdditionalInformationList(importExportParent, "Import");
			AssertEquals(true, codeList.ContainsCode("CODE1"));

			importExportParent.Level = "Header";
			codeList = additionalInfo.Lookups.GetAdditionalInformationList(importExportParent, "Both");
			AssertEquals(true, codeList.ContainsCode("CODE3"));

			importExportParent.Level = "Item";
			importExportParent.DataGroupingCode = "FR";
			codeList = additionalInfo.Lookups.GetAdditionalInformationList(importExportParent, "Both");
			AssertEquals(true, codeList.ContainsCode("CODE4"));
		}

		public void TestAdditonalInfoCodeListOnEmptyParent()
		{
			additionalInfo.CSI_ParentID = ZGuid.Empty;
			additionalInfo.CSI_ParentTableCode = ZString.Empty;

			var codeList = additionalInfo.Lookups.CodeList;
			AssertEquals(0, codeList.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			importExportParent = Factory.NewWithValidTestData<ImportExportParentForTest>();
			additionalInfo = Factory.NewWithValidTestData<AdditionalInfoForTest>();
		}

		ImportExportParentForTest importExportParent;
		AdditionalInfoForTest additionalInfo;
	}

	sealed class ImportExportParentForTest : DummyBusinessObject, ICanBeImportOrExport
	{
		public ImportExportParentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZBool IsImport { get; set; }

		public ZBool IsExport { get; set; }

		public string Level { get; set; }

		public string TrueCountryCode { get; set; }

		public string DataGroupingCode { get; set; }

		public void ValidatePreviousDocuments() { }
	}

	public class AdditionalInfoForTest : BaseAdditionalInfo
	{
		public AdditionalInfoForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new BaseAdditionalInfoLookups Lookups => (BaseAdditionalInfoLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new BaseAdditionalInfoLookups(this);
	}
}
