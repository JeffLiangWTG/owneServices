using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class FormatterForBillTypeLayoutTestForShipmentTest : TestCaseWithFactory
	{
		public void TestMarksAndNumbers()
		{
			ZString expectedResultForMarksAndNumbers =
				"          \n" +
				"          \n" +
				"          \n" +
				"          \n" +
				"This is   \n" +
				"the marks \n" +
				"and       \n" +
				"numbers   \n" +
				"to test   ";
			ZString expectedResultForFollowOn = "this formatter with";
			FormatterForBillTypeLayoutTestClassForShipment formatter = new FormatterForBillTypeLayoutTestClassForShipment();
			formatter.TestMarksAndNumbersWidth = 10;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 9;
			AssertEquals("Result includes marks and numbers", expectedResultForMarksAndNumbers, formatter.MarksAndNumbers);
			AssertEquals("Result includes marks and numbers", expectedResultForFollowOn, formatter.FollowOnMarksAndNumbers);

			expectedResultForMarksAndNumbers =
				"          \n" +
				"          \n" +
				"          \n" +
				"          \n" +
				"This is   \n" +
				"the marks \n" +
				"and       \n" +
				"numbers   \n" +
				"to test   \n" +
				"this      \n" +
				"formatter \n" +
				"with      ";
			expectedResultForFollowOn = ZString.Empty;
			formatter = new FormatterForBillTypeLayoutTestClassForShipment();
			formatter.TestMarksAndNumbersWidth = 10;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 12;
			AssertEquals("Result includes marks and numbers", expectedResultForMarksAndNumbers, formatter.MarksAndNumbers);
			AssertEquals("Result includes marks and numbers", expectedResultForFollowOn, formatter.FollowOnMarksAndNumbers);
		}

		public void TestDescriptionColumn()
		{
			ZString expectedResultForDescription1 =
				 "2 X 20FR CONTAINER(S)         \n" +
				 "1 X 40OT CONTAINER(S)         \n" +
				 "STC 52 Pallet(s)              \n" +
				 "and 48 Pallet(s) LCL Cargo    \n" +
				 "This is the goods description ";
			ZString expectedResultForDescription2 =
				 "1 X 40OT CONTAINER(S)         \n" +
				 "2 X 20FR CONTAINER(S)         \n" +
				 "STC 52 Pallet(s)              \n" +
				 "and 48 Pallet(s) LCL Cargo    \n" +
				 "This is the goods description ";
			ZString expectedFollowOn = "to test this formatter with";

			FormatterForBillTypeLayoutTestClassForShipment formatter = new FormatterForBillTypeLayoutTestClassForShipment();
			formatter.TestDescriptionWidth = 30;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 5;
			ZString result = formatter.GoodsDescription;
			Assert("Result includes description", result == expectedResultForDescription1 || result == expectedResultForDescription2);
			AssertEquals("Result includes description", expectedFollowOn, formatter.FollowOnGoodsDescription);

			expectedResultForDescription1 =
				"2 X 20FR CONTAINER(S)         \n" +
				"1 X 40OT CONTAINER(S)         \n" +
				"STC 52 Pallet(s)              \n" +
				"and 48 Pallet(s) LCL Cargo    \n" +
				"This is the goods description \n" +
				"to test this formatter with   ";
			expectedResultForDescription2 =
				"1 X 40OT CONTAINER(S)         \n" +
				"2 X 20FR CONTAINER(S)         \n" +
				"STC 52 Pallet(s)              \n" +
				"and 48 Pallet(s) LCL Cargo    \n" +
				"This is the goods description \n" +
				"to test this formatter with   ";
			expectedFollowOn = ZString.Empty;

			formatter = new FormatterForBillTypeLayoutTestClassForShipment();
			formatter.TestDescriptionWidth = 30;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 6;
			result = formatter.GoodsDescription;
			Assert("Result includes description", result == expectedResultForDescription1 || result == expectedResultForDescription2);
			AssertEquals("Result includes description", expectedFollowOn, formatter.FollowOnGoodsDescription);
		}

		public void TestWeight()
		{
			ZString expectedResultForWeight =
				"0.123 KG\n" +
				"(123.000\n" +
				"G)      ";
			FormatterForBillTypeLayoutTestClassForShipment formatter = new FormatterForBillTypeLayoutTestClassForShipment();
			formatter.TestWeightWidth = 8;
			AssertEquals("Result for weight format", expectedResultForWeight, formatter.Weight);
		}

		public void TestVolume()
		{
			ZString expectedResultForVolume =
				"3.483 M3\n" +
				"(123.000\n" +
				"CF)     ";
			FormatterForBillTypeLayoutTestClassForShipment formatter = new FormatterForBillTypeLayoutTestClassForShipment();
			formatter.TestVolumeWidth = 8;
			AssertEquals("Result for volume format", expectedResultForVolume, formatter.Volume);
		}

		public void TestContainerLayout()
		{
			ZString containerNumber =
				"1               \n" +
				"2               \n" +
				"3               ";
			ZString sealNumber =
				"seal 1                  \n" +
				"-                       \n" +
				"seal 3                  ";
			ZString type =
				"20FR        \n" +
				"20FR        \n" +
				"40OT        ";
			ZString weight =
				"-              \n" +
				"23             \n" +
				"45             ";
			ZString volume =
				"4                   \n" +
				"-                   \n" +
				"-                   ";
			ZString packages =
				"50            \n" +
				"1             \n" +
				"1             ";
			ZString deliveryMode =
				"CY/CY       \n" +
				"CY/CY       \n" +
				"-           ";

			ZString followOnContainerNumber = "4               ";
			ZString followOnSealNumber = "seal 4                  ";
			ZString followOnType = "40OT        ";
			ZString followOnWeight = "-              ";
			ZString followOnVolume = "34                  ";
			ZString followOnPackages = "1             ";
			ZString followOnDeliveryMode = "CY/CY       ";

			FormatterForBillTypeLayoutTestClassForShipment formatter = new FormatterForBillTypeLayoutTestClassForShipment();
			formatter.TestContainerRowHeight = 3;

			AssertEquals("Result for container number format", containerNumber, formatter.ContainerNumberColumn);
			AssertEquals("Result for container number follow on format", followOnContainerNumber, formatter.FollowOnContainerNumberColumn);

			AssertEquals("Result for seal number format", sealNumber, formatter.ContainerSealNumColumn);
			AssertEquals("Result for seal number follow on format", followOnSealNumber, formatter.FollowOnContainerSealNumColumn);

			AssertEquals("Result for type format", type, formatter.ContainerTypeColumn);
			AssertEquals("Result for type follow on format", followOnType, formatter.FollowOnContainerTypeColumn);

			AssertEquals("Result for weight format", weight, formatter.ContainerWeightColumn);
			AssertEquals("Result for weight follow on format", followOnWeight, formatter.FollowOnContainerWeightColumn);

			AssertEquals("Result for volume format", volume, formatter.ContainerVolumeColumn);
			AssertEquals("Result for volume follow on format", followOnVolume, formatter.FollowOnContainerVolumeColumn);

			AssertEquals("Result for pacakges format", packages, formatter.ContainerPackagesColumn);
			AssertEquals("Result for packages follow on format", followOnPackages, formatter.FollowOnContainerPackagesColumn);

			AssertEquals("Result for delivery mode format", deliveryMode, formatter.ContainerModeColumn);
			AssertEquals("Result for delivery follow on format", followOnDeliveryMode, formatter.FollowOnContainerModeColumn);
		}

		public void TestCombinationOfAllFields1()
		{
			ZString expectedResult =
				"           2 X 20FR CONTAINER(S)          0.123 KG 3.483 M3\n" +
				"           1 X 40OT CONTAINER(S)          (123.000 (123.000\n" +
				"           STC 52 Pallet(s)               G)       CF)     \n" +
				"           and 48 Pallet(s) LCL Cargo                      \n" +
				"This is    This is the goods description                   \n" +
				"the marks  to test this formatter with                     \n" +
				"and                                                        \n" +
				"numbers                                                    \n" +
				"to test                                                    \n" +
				"CONTAINER        SEAL                     TYPE         WEIGHT(KG)      VOLUME(M3)           PACKAGE        MODE        \n" +
				"1                seal 1                   20FR         -               4                    50             CY/CY       \n" +
				"2                -                        20FR         23              -                    1              CY/CY       \n" +
				"3                seal 3                   40OT         45              -                    1              -           ";

			FormatterForBillTypeLayoutTestClassForShipment formatter = new FormatterForBillTypeLayoutTestClassForShipment();
			formatter.TestContainerRowHeight = 3;
			formatter.TestDescriptionWidth = 30;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 9;
			formatter.TestMarksAndNumbersWidth = 10;
			formatter.TestWeightWidth = 8;
			formatter.TestVolumeWidth = 8;
			AssertEquals("Fileds combined as one", expectedResult, formatter.GoodDetails);
		}

		public void TestCombinationOfAllFields2()
		{
			ZString expectedResult =
				"           2 X 20FR CONTAINER(S)          0.123 KG 3.483 M3\n" +
				"           1 X 40OT CONTAINER(S)          (123.000 (123.000\n" +
				"           STC 52 Pallet(s)               G)       CF)     \n" +
				"           and 48 Pallet(s) LCL Cargo                      \n" +
				"This is    This is the goods description                   \n" +
				"the marks  to test this formatter with                     \n" +
				"and                                                        \n" +
				"numbers                                                    \n" +
				"to test                                                    \n" +
				"this                                                       \n" +
				"formatter                                                  \n" +
				"with                                                       \n" +
				"CONTAINER        SEAL                     TYPE         WEIGHT(KG)      VOLUME(M3)           PACKAGE        MODE        \n" +
				"1                seal 1                   20FR         -               4                    50             CY/CY       \n" +
				"2                -                        20FR         23              -                    1              CY/CY       \n" +
				"3                seal 3                   40OT         45              -                    1              -           \n" +
				"4                seal 4                   40OT         -               34                   1              CY/CY       ";

			FormatterForBillTypeLayoutTestClassForShipment formatter = new FormatterForBillTypeLayoutTestClassForShipment();
			formatter.TestContainerRowHeight = 3;
			formatter.TestDescriptionWidth = 30;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 15;
			formatter.TestMarksAndNumbersWidth = 10;
			formatter.TestWeightWidth = 8;
			formatter.TestVolumeWidth = 8;
			AssertEquals("Fileds combined as one", expectedResult, formatter.GoodDetails);
		}
	}
}
