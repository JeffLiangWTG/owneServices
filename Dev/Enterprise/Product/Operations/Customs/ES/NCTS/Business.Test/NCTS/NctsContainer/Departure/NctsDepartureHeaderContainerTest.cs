using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureHeaderContainer))]
	public class NctsDepartureHeaderContainerTest : Customs.Business.Testing.BaseCusInBondContainerTest<NctsDepartureHeaderContainer>
	{
		public void TestValidationType_Phase4()
		{
			var (header, container) = GetNewHeaderAndContainer();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsDepartureHeaderContainerPhase4Validation>(container.Validation);
		}

		public void TestValidationType_Phase5()
		{
			var (header, container) = GetNewHeaderAndContainer();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsDepartureHeaderContainerPhase5Validation>(container.Validation);
		}

		public void TestAdditionalSealsLineNumberGenerator()
		{
			var (_, container) = GetNewHeaderAndContainer();
			AssertType<CusSealSequenceNumberGenerator>("Sequence generator for Additional Seals", container.AdditionalSealsLineNumberGenerator);
		}

		public void TestRecalculateSequenceOnModeChanged()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var containers = header.DepartureHeaderContainers;
			CombineAssertions(() =>
			{
				var first = containers.AddNew();
				first.BC_Mode = Constants.ContainerModes.NonContainerised;
				var second = containers.AddNew();
				second.BC_Mode = Constants.ContainerModes.Containerised;
				var third = containers.AddNew();
				third.BC_Mode = Constants.ContainerModes.Containerised;
				var fourth = containers.AddNew();
				fourth.BC_Mode = Constants.ContainerModes.NonContainerised;

				AssertEquals("Prereq: first.BC_SequenceNumber (with mode NCT)", (ZShort)3, first.BC_SequenceNumber);
				AssertEquals("Prereq: second.BC_SequenceNumber (with mode CNT)", (ZShort)1, second.BC_SequenceNumber);
				AssertEquals("Prereq: third.BC_SequenceNumber (with mode CNT)", (ZShort)2, third.BC_SequenceNumber);
				AssertEquals("Prereq: fourth.BC_SequenceNumber (with mode NCT)", (ZShort)4, fourth.BC_SequenceNumber);

				fourth.BC_Mode = Constants.ContainerModes.Containerised;

				AssertEquals("After changing fourth element's mode: first.BC_SequenceNumber (with mode NCT)", (ZShort)4, first.BC_SequenceNumber);
				AssertEquals("After changing fourth element's mode: second.BC_SequenceNumber (with mode CNT)", (ZShort)1, second.BC_SequenceNumber);
				AssertEquals("After changing fourth element's mode: third.BC_SequenceNumber (with mode CNT)", (ZShort)2, third.BC_SequenceNumber);
				AssertEquals("After changing fourth element's mode: fourth.BC_SequenceNumber (with mode CNT)", (ZShort)3, fourth.BC_SequenceNumber);
			});
		}

		(NctsHeader, NctsDepartureHeaderContainer) GetNewHeaderAndContainer()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var container = header.DepartureHeaderContainers.AddNew();
			return (header, container);
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return header.DepartureHeaderContainers.AddNew();
		}
	}
}
