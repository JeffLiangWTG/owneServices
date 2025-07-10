using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(IdentificationMeansCodeCollection))]
	public class IdentificationMeansCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		public void TestMaxCount()
		{
			NUnit.Framework.Assert.That(EntryInstruction.IdentificationMeanCodes.MaxCount, NUnit.Framework.Is.EqualTo(7));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return EntryInstruction.IdentificationMeanCodes;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusCode = Factory.New<IdentificationMeansCode>();
			cusCode.CY_ParentTableCode = EntryInstruction.TablePrefix;
			return cusCode;
		}

		CusEntryInstruction EntryInstruction
		{
			get
			{
				if (entryInstruction == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
					entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
				}

				return entryInstruction;
			}
		}
		CusEntryInstruction entryInstruction;
	}
}
