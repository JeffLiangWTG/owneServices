using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsHeaderContainerSequenceNumberGeneratorTest : TestCaseWithFactory
	{
		public void TestRecalculateWhenAdded_AllOneMode()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var containers = header.DepartureHeaderContainers;
			CombineAssertions(() =>
			{
				var first = containers.AddNew();
				first.BC_Mode = Constants.ContainerModes.Containerised;
				var second = containers.AddNew();
				second.BC_Mode = Constants.ContainerModes.Containerised;
				var third = containers.AddNew();
				third.BC_Mode = Constants.ContainerModes.Containerised;

				AssertEquals("Prereq: first.BC_SequenceNumber", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("Prereq: second.BC_SequenceNumber", (ZShort)2, second.BC_SequenceNumber);
				AssertEquals("Prereq: third.BC_SequenceNumber", (ZShort)3, third.BC_SequenceNumber);

				var fourth = containers.AddNew();
				AssertEquals("After adding fourth element: first.BC_SequenceNumber", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("After adding fourth element: second.BC_SequenceNumber", (ZShort)2, second.BC_SequenceNumber);
				AssertEquals("After adding fourth element: third.BC_SequenceNumber", (ZShort)3, third.BC_SequenceNumber);
				AssertEquals("After adding fourth element: fourth.BC_SequenceNumber", (ZShort)4, fourth.BC_SequenceNumber);
			});
		}

		public void TestRecalculateWhenAdded_DifferentModes()
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

				AssertEquals("Prereq: first.BC_SequenceNumber (with mode NCT)", (ZShort)3, first.BC_SequenceNumber);
				AssertEquals("Prereq: second.BC_SequenceNumber (with mode CNT)", (ZShort)1, second.BC_SequenceNumber);
				AssertEquals("Prereq: third.BC_SequenceNumber (with mode CNT)", (ZShort)2, third.BC_SequenceNumber);

				var fourth = containers.AddNew();
				AssertEquals("Fourth element has BC_Mode the same as the first element (NCT)", first.BC_Mode, fourth.BC_Mode);
				AssertEquals("After adding fourth element: first.BC_SequenceNumber (with mode NCT)", (ZShort)3, first.BC_SequenceNumber);
				AssertEquals("After adding fourth element: second.BC_SequenceNumber (with mode CNT)", (ZShort)1, second.BC_SequenceNumber);
				AssertEquals("After adding fourth element: third.BC_SequenceNumber (with mode CNT)", (ZShort)2, third.BC_SequenceNumber);
				AssertEquals("After adding fourth element: fourth.BC_SequenceNumber (with mode NCT)", (ZShort)4, fourth.BC_SequenceNumber);
			});
		}

		public void TestReCalculateAllOnDelete_AllOneMode()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var containers = header.DepartureHeaderContainers;
			CombineAssertions(() =>
			{
				var first = containers.AddNew();
				first.BC_Mode = Constants.ContainerModes.Containerised;
				var second = containers.AddNew();
				second.BC_Mode = Constants.ContainerModes.Containerised;
				var third = containers.AddNew();
				third.BC_Mode = Constants.ContainerModes.Containerised;
				var fourth = containers.AddNew();
				fourth.BC_Mode = Constants.ContainerModes.Containerised;

				AssertEquals("Prereq: first.BC_SequenceNumber", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("Prereq: second.BC_SequenceNumber", (ZShort)2, second.BC_SequenceNumber);
				AssertEquals("Prereq: third.BC_SequenceNumber", (ZShort)3, third.BC_SequenceNumber);
				AssertEquals("Prereq: fourth.BC_SequenceNumber", (ZShort)4, fourth.BC_SequenceNumber);

				second.Delete();
				AssertEquals("After removing second element: first.BC_SequenceNumber", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("After removing second element: third.BC_SequenceNumber", (ZShort)2, third.BC_SequenceNumber);
				AssertEquals("After removing second element: fourth.BC_SequenceNumber", (ZShort)3, fourth.BC_SequenceNumber);
				
				first.Delete();
				AssertEquals("After removing first element: third.BC_SequenceNumber", (ZShort)1, third.BC_SequenceNumber);
				AssertEquals("After removing first element: fourth.BC_SequenceNumber", (ZShort)2, fourth.BC_SequenceNumber);
			});
		}

		public void TestReCalculateAllOnDelete_DifferentModes()
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

				first.Delete();
				AssertEquals("After removing first element: first.BC_SequenceNumber (with mode CNT)", (ZShort)1, second.BC_SequenceNumber);
				AssertEquals("After removing first element: third.BC_SequenceNumber (with mode CNT)", (ZShort)2, third.BC_SequenceNumber);
				AssertEquals("After removing first element: fourth.BC_SequenceNumber (with mode NCT)", (ZShort)3, fourth.BC_SequenceNumber);

				second.Delete();
				AssertEquals("After removing second element: third.BC_SequenceNumber (with mode CNT)", (ZShort)1, third.BC_SequenceNumber);
				AssertEquals("After removing second element: fourth.BC_SequenceNumber (with mode NCT)", (ZShort)2, fourth.BC_SequenceNumber);
			});
		}

		public void TestReCalculateAll_AllOneMode()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var containers = header.DepartureHeaderContainers;
			CombineAssertions(() =>
			{
				var first = containers.AddNew();
				first.BC_Mode = Constants.ContainerModes.Containerised;
				var second = containers.AddNew();
				second.BC_Mode = Constants.ContainerModes.Containerised;
				var third = containers.AddNew();
				third.BC_Mode = Constants.ContainerModes.Containerised;
				var fourth = containers.AddNew();
				fourth.BC_Mode = Constants.ContainerModes.Containerised;
				first.BC_SequenceNumber = 5;
				second.BC_SequenceNumber = 6;
				third.BC_SequenceNumber = 8;
				fourth.BC_SequenceNumber = 10;

				AssertEquals("Prereq: First element BC_SequenceNumbe", (ZShort)5, first.BC_SequenceNumber);
				AssertEquals("Prereq: Second element BC_SequenceNumber", (ZShort)6, second.BC_SequenceNumber);
				AssertEquals("Prereq: Third element BC_SequenceNumber", (ZShort)8, third.BC_SequenceNumber);
				AssertEquals("Prereq: Fourth element BC_SequenceNumber", (ZShort)10, fourth.BC_SequenceNumber);

				var generator = new NctsHeaderContainerSequenceNumberGenerator(() => containers.Cast<NctsDepartureHeaderContainer>());
				generator.ReCalculateAll();
				AssertEquals("After calling ReCalculateAll: First element BC_SequenceNumber", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("After calling ReCalculateAll: Second element BC_SequenceNumber", (ZShort)2, second.BC_SequenceNumber);
				AssertEquals("After calling ReCalculateAll: Third element BC_SequenceNumber", (ZShort)3, third.BC_SequenceNumber);
				AssertEquals("After calling ReCalculateAll: Fourth element BC_SequenceNumber", (ZShort)4, fourth.BC_SequenceNumber);
			});
		}

		public void TestReCalculateAll_DifferentModes()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var containers = header.DepartureHeaderContainers;

			CombineAssertions(() =>
			{
				var first = containers.AddNew();
				first.BC_Mode = Constants.ContainerModes.Containerised;
				var second = containers.AddNew();
				second.BC_Mode = Constants.ContainerModes.NonContainerised;
				var third = containers.AddNew();
				third.BC_Mode = Constants.ContainerModes.NonContainerised;
				var fourth = containers.AddNew();
				fourth.BC_Mode = Constants.ContainerModes.Containerised;
				first.BC_SequenceNumber = 5;
				second.BC_SequenceNumber = 6;
				third.BC_SequenceNumber = 8;
				fourth.BC_SequenceNumber = 10;

				AssertEquals("Prereq: First element BC_SequenceNumbe", (ZShort)5, first.BC_SequenceNumber);
				AssertEquals("Prereq: Second element BC_SequenceNumber", (ZShort)6, second.BC_SequenceNumber);
				AssertEquals("Prereq: Third element BC_SequenceNumber", (ZShort)8, third.BC_SequenceNumber);
				AssertEquals("Prereq: Fourth element BC_SequenceNumber", (ZShort)10, fourth.BC_SequenceNumber);

				var generator = new NctsHeaderContainerSequenceNumberGenerator(() => containers.Cast<NctsDepartureHeaderContainer>());
				generator.ReCalculateAll();
				AssertEquals("After calling ReCalculateAll: First element (with mode CNT) BC_SequenceNumber", (ZShort)1, first.BC_SequenceNumber);
				AssertEquals("After calling ReCalculateAll: Second element (with mode NCT) BC_SequenceNumber", (ZShort)3, second.BC_SequenceNumber);
				AssertEquals("After calling ReCalculateAll: Third element (with mode NCT) BC_SequenceNumber", (ZShort)4, third.BC_SequenceNumber);
				AssertEquals("After calling ReCalculateAll: Fourth element (with mode CNT) BC_SequenceNumber", (ZShort)2, fourth.BC_SequenceNumber);
			});
		}
	}
}
