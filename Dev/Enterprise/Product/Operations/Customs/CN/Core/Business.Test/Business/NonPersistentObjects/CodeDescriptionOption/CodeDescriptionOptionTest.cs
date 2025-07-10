using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CodeDescriptionOption))]
	class CodeDescriptionOptionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CodeDescriptionOption(new CodeDescriptionOptionCollectionParent(Factory, Factory.New<JobComInvoiceLine>().CargoAttributes), new CodeDescriptionPair(CargoAttributeList.Codes._11, CargoAttributeList.Descriptions._11));
		}

		public void TestProperties()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var declaration = testItems.JobDeclaration;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			var instruction = testItems.EntryInstruction;
			instruction.CEI_CIQRequires = true;
			var cusCodeData1 = Factory.New<SpecialBusinessIdentifier>();
			cusCodeData1.CY_Code = "B01";
			cusCodeData1.CY_ParentTableCode = instruction.TablePrefix;
			cusCodeData1.CY_Type = "SBI";
			cusCodeData1.CY_ParentID = instruction.PK;
			var cusCodeData2 = Factory.New<SpecialBusinessIdentifier>();
			cusCodeData2.CY_Code = "B04";
			cusCodeData2.CY_ParentTableCode = instruction.TablePrefix;
			cusCodeData2.CY_Type = "SBI";
			cusCodeData2.CY_ParentID = instruction.PK;
			Factory.Save();

			var instructionNew = new BusinessObjectFactory() { NameForDebugging = "Enterprise.Customs.CN.Business.Testing.CodeDescriptionOptionTest.TestProperties" }.Load<CusEntryInstruction>(instruction.PK);
			var testParent = new CodeDescriptionOptionCollectionParent(Factory, instructionNew.SpecialBusinessIdentifiers);
			var collection = testParent.OptionCollection;
			var option = collection.Cast<CodeDescriptionOption>().FirstOrDefault(o => o.Code == "B01");
			Assert("Should be selected", option.Selected);
			Assert("Should not has changes", !option.HasChanges);
			AssertEquals("Description", "国际赛事", option.Description);
			option = collection.Cast<CodeDescriptionOption>().FirstOrDefault(o => o.Code == "B03");
			Assert("Should not be selected", !option.Selected);
			Assert("Should not has changes", !option.HasChanges);
			AssertEquals("Description", "国际援助物资", option.Description);
		}
	}
}
