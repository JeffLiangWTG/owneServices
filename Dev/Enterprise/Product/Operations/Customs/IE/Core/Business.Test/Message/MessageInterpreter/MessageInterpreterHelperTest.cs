using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AIS;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class MessageInterpreterHelperTest : TestCaseWithFactory
	{
		public void TestGetCodeAndDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType("CL560", "Business Rejection Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL560", "ERR", "Test 560 Rejection Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType("CL180", "Error Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL180", "12", "Error Code Item12", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertEquals("Get correct code and description from CL180", "12 - Error Code Item12", MessageInterpreterHelper.GetCodeAndDescription(Factory, "12", "CL180", ZDateTime.Now));
			AssertEquals("Get correct code and description from CL560", "ERR - Test 560 Rejection Type", MessageInterpreterHelper.GetCodeAndDescription(Factory, "ERR", "CL560", ZDateTime.Now));
		}

		public void TestUnevenZipExtension()
		{
			var list1 = new Collection<string> { "a", "b", "c" };
			var list2 = new Collection<string> { "d", "e" };
			var list3 = new Collection<string> { };

			var mergedList1 = list1.UnevenZipExtension(list2, (l1, l2) => l1 + l2);
			var mergedList2 = list1.UnevenZipExtension(list3, (l1, l3) => l1 + l3);
			var mergedList3 = list3.UnevenZipExtension(list2, (l3, l2) => l3 + l2);

			AssertContainsExactElementsInExactOrder(new Collection<string> { "ad", "be", "c" }, mergedList1);
			AssertContainsExactElementsInExactOrder(new Collection<string> { "a", "b", "c" }, mergedList2);
			AssertContainsExactElementsInExactOrder(new Collection<string> { "d", "e" }, mergedList3);
		}

		public void TestGetDescriptionFromCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType("CL180", "Error Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL180", "12", "Error Code Item12", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("When Code = 12", "Error Code Item12", MessageInterpreterHelper.GetDescriptionFromCode(Factory, "12", "CL180", ZDateTime.Now));
				AssertEquals("When Code = 13, Invalid Code", null, MessageInterpreterHelper.GetDescriptionFromCode(Factory, "13", "CL180", ZDateTime.Now));
			});
		}

		public void TestGetMessageDetailsOfMFunctionalError01Provider()
		{
			var provider = new MFunctionalError01Provider(new MFunctionalErrorType01
			{
				SequenceNumber = "1",
				ErrorPointer = "ErrorPointer001",
				ErrorCode = "13",
				ErrorReason = "ER1",
				Remarks = "Functional Error Remarks 1",
				OriginalAttributeValue = "Original Attribute Value 1",
			});
			AssertContainsExactElementsInExactOrder(new (string, string)[]
			{
				("Functional Error", "1"),
				("Error Pointer", "ErrorPointer001"),
				("Error Code", "13"),
				("Error Code Description", "Error Code Description"),
				("Error Reason", "ER1"),
				("Remarks", "Functional Error Remarks 1"),
				("Original Attribute Value", "Original Attribute Value 1"),
			}, provider.GetMessageDetails("Error Code Description"));
		}

		public void TestGetGetFunctionalErrorDetails()
		{
			var provider = new FunctionalErrorTypeProvider(new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX.FunctionalErrorType
			{
				ErrorPointer = "ErrorPointer001",
				ErrorType = "13",
				ErrorReason = "ER1",
				ErrorMessage = "Functional Error Message 1",
				OriginalAttributeValue = "Original Attribute Value 1",
			});

			AssertContainsExactElementsInExactOrder(new (string, string)[]
			{
				("Functional Error", ""),
				("Error Reason", "ER1"),
				("Error Type", "13"),
				("Error Type Description", "Error Code Description"),
				("Error Message", "Functional Error Message 1"),
				("Original Attribute Value", "Original Attribute Value 1"),
				("Error Pointer", "ErrorPointer001"),
			}, provider.GetFunctionalErrorDetails("Error Code Description"));
		}

		public void TestGetMessageDetailsOfItemControlResultsProvider()
		{
			var provider = new ItemControlResultsProvider(new MItemControlResultsType01
			{
				SequenceNumber = "1",
				DeclarationGoodsItemNumber = "10001",
				ControlResultCode = "A4",
				ResultsOfControl = new Collection<MControlResultType02>
				{
					new MControlResultType02
					{
						SequenceNumber = "1", RiskAreaCode = "100000", ControlType = "Red", ControlDate = new DateTime(2023, 08, 09, 14, 30, 45), Remarks = "Control Results Remarks 011",
						ControlDetails = new Collection<MControlDetailsType>
						{
							new MControlDetailsType { SequenceNumber = "1", TypeOfDiscrepancies = "TD", AttributePointer = "Attribute Pointer 111", CorrectedValue = "Corrected Value 111", Remarks = "Control Details Remarks 111" },
							new MControlDetailsType { SequenceNumber = "2", TypeOfDiscrepancies = "TE", AttributePointer = "Attribute Pointer 112", CorrectedValue = "Corrected Value 112", Remarks = "Control Details Remarks 112" },
						}
					},
					new MControlResultType02
					{
						SequenceNumber = "2", RiskAreaCode = "100000", ControlType = "Red", ControlDate = new DateTime(2023, 08, 10, 14, 30, 45), Remarks = "Control Results Remarks 012",
						ControlDetails = new Collection<MControlDetailsType>
						{
							new MControlDetailsType { SequenceNumber = "1", TypeOfDiscrepancies = "TF", AttributePointer = "Attribute Pointer 121", CorrectedValue = "Corrected Value 111", Remarks = "Control Details Remarks 121" },
							new MControlDetailsType { SequenceNumber = "2", TypeOfDiscrepancies = "TG", AttributePointer = "Attribute Pointer 122", CorrectedValue = "Corrected Value 112", Remarks = "Control Details Remarks 122" },
						}
					},
				}
			});
			AssertContainsExactElementsInExactOrder(new (string, string)[]
			{
				("Control Results", "1"),
				("-Declaration Goods Item Number", "10001"),
				("-Control Result Code", "A4"),
				("-Results of Control", "1"),
				("--Risk Area Code", "100000"),
				("--Risk Area Code Description", "RiskAreaCode 100000 Description"),
				("--Control Type", "Red"),
				("--Control Type Description", "ControlType Red Description"),
				("--Control Date", "09-Aug-23"),
				("--Remarks", "Control Results Remarks 011"),
				("--Control Details", "1"),
				("---Type of Discrepancies", "TD"),
				("---Attribute Pointer", "Attribute Pointer 111"),
				("---Corrected Value", "Corrected Value 111"),
				("---Remarks", "Control Details Remarks 111"),
				("--Control Details", "2"),
				("---Type of Discrepancies", "TE"),
				("---Attribute Pointer", "Attribute Pointer 112"),
				("---Corrected Value", "Corrected Value 112"),
				("---Remarks", "Control Details Remarks 112"),
				("-Results of Control", "2"),
				("--Risk Area Code", "100000"),
				("--Risk Area Code Description", "RiskAreaCode 100000 Description"),
				("--Control Type", "Red"),
				("--Control Type Description", "ControlType Red Description"),
				("--Control Date", "10-Aug-23"),
				("--Remarks", "Control Results Remarks 012"),
				("--Control Details", "1"),
				("---Type of Discrepancies", "TF"),
				("---Attribute Pointer", "Attribute Pointer 121"),
				("---Corrected Value", "Corrected Value 111"),
				("---Remarks", "Control Details Remarks 121"),
				("--Control Details", "2"),
				("---Type of Discrepancies", "TG"),
				("---Attribute Pointer", "Attribute Pointer 122"),
				("---Corrected Value", "Corrected Value 112"),
				("---Remarks", "Control Details Remarks 122"),
			}, provider.GetMessageDetails((riskAreaCode) => $"RiskAreaCode {riskAreaCode} Description", (controlType) => $"ControlType {controlType} Description"));
		}
	}
}
