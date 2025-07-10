using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InwardProcessingPlace))]
	public class InwardProcessingPlaceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Inward Processing Place", place.HumanReadableName);
		}

		public void TestE2_AddressSequence()
		{
			var place2 = instruction.InwardProcessingPlaces.AddNew();
			var place3 = instruction.InwardProcessingPlaces.AddNew();
			var place4 = instruction.InwardProcessingPlaces.AddNew();
			AssertEquals((ZByte)0, place.E2_AddressSequence);
			AssertEquals((ZByte)1, place2.E2_AddressSequence);
			AssertEquals((ZByte)2, place3.E2_AddressSequence);
			AssertEquals((ZByte)3, place4.E2_AddressSequence);

			instruction.InwardProcessingPlaces.RemoveAndDelete(place3);
			AssertEquals((ZByte)0, place.E2_AddressSequence);
			AssertEquals((ZByte)1, place2.E2_AddressSequence);
			AssertEquals((ZByte)2, place4.E2_AddressSequence);

			var place5 = instruction.InwardProcessingPlaces.AddNew();
			AssertEquals((ZByte)3, place5.E2_AddressSequence);
		}

		public void TestISequenceNumberLine()
		{
			CombineAssertions(() =>
			{
				var sequenceLine = (IShortSequenceNumberLine)place;
				AssertEquals("FKToHeader", instruction.PK, sequenceLine.FKToHeader);
				AssertEquals("SequenceNumber", new ZShort(0), sequenceLine.SequenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			instruction = Factory.CreateInwardProcessingInstruction();
			place = instruction.InwardProcessingPlaces.AddNew();
		}

		CusEntryInstruction instruction;
		InwardProcessingPlace place;
	}
}
