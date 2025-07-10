using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(InlandTransport))]
	class InlandTransportTest : Customs.Business.Testing.CusCodeDataTest<InlandTransport>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Inland Transport", inlandTransport.HumanReadableName);
		}

		public void TestCY_Code_Caption()
		{
			NCTSTestHelper.AssertCaptions(inlandTransport.CY_CodeInfo, "Nationality", string.Empty, string.Empty);
		}

		public void TestCY_Data_MaxLength()
		{
			AssertEquals(35, inlandTransport.CY_DataInfo.MaxLength);
		}

		public void TestCY_Data_Caption()
		{
			NCTSTestHelper.AssertCaptions(inlandTransport.CY_DataInfo, "Wagon Number", "Wagon No.", string.Empty);
		}

		public void TestLookups()
		{
			AssertType<InlandTransportLookups>(inlandTransport.Lookups);
		}

		public void TestValidation()
		{
			AssertType<InlandTransportValidation>(inlandTransport.Validation);
		}

		public void TestDefaultValues()
		{
			var transport = Factory.New<InlandTransport>();
			AssertEquals("CY_Type", NctsConstants.CusCodeDataTypes.TransportInland, transport.CY_Type);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override IEnumerable<InlandTransport> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (InlandTransport)GetNewBusinessObject(factory);
		}

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var inlandTransport = nctsHeader.MovementHeader.InlandTransportList.AddNew();
			return inlandTransport;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			inlandTransport = nctsHeader.MovementHeader.InlandTransportList.AddNew();
		}
		InlandTransport inlandTransport;
	}
}
