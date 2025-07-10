using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MX;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(Clearance))]
	class ClearanceTest : Customs.Business.Testing.CusSupportingInfoTest<Clearance>
	{
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<Clearance>();
			CombineAssertions(() =>
			{
				AssertEquals(CusSupportingInfoTypeList.Codes.Clearance, supporting.CSI_Type);
				AssertEquals(CusEntryInstructionSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
			});
		}

		protected override IEnumerable<Clearance> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var clearance = declaration.CustomsEntryInstructions.AddNew().Clearances.AddNew();
			clearance.CSI_Code = "1";
			yield return clearance;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var entryInstruction = factory.NewWithValidTestData<CusEntryInstruction>();
			return entryInstruction.Clearances.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			return entryInstruction.Clearances.AddNew();
		}
	}
}
