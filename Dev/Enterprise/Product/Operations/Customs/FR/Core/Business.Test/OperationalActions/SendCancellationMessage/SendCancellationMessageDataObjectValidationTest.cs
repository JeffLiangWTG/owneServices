using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	public class SendCancellationMessageDataObjectValidationTest : TestCaseWithFactory
	{
		public void TestAreTargetsValid_DeclarationsDeltaIEAllowed()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.Targets = new BusinessObject[] { declaration1, declaration2 };
			var areTargetsValid = cancellationMessage.Validation.AreTargetsValid();

			AssertEquals("Delta I/E declarations should be allowed", true, areTargetsValid);
		}

		public void TestAreTargetsValid_HeadersDeltaIEAllowed()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.Targets = new BusinessObject[] { entryHeader1_1, entryHeader1_2, entryHeader2_1 };
			var areTargetsValid = cancellationMessage.Validation.AreTargetsValid();

			AssertEquals("Delta I/E headers should be allowed", true, areTargetsValid);
		}

		public void TestAreTargetsValid_DeclarationNonDeltaIEDisallowed()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interface;
			declaration1.JE_DeclarationReference = "D0001";
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.Targets = new BusinessObject[] { declaration1, declaration2 };
			var areTargetsValid = cancellationMessage.Validation.AreTargetsValid();
			AssertEquals("Not all declarations are of Delta I/E, should be disallowed", false, areTargetsValid);
			AssertEquals("Not all declarations are of Delta I/E, should be disallowed", true, cancellationMessage.HasErrors);
			var errorNotifications = cancellationMessage.GetErrors();

			AssertEquals("One declaration had improper application code", 1, errorNotifications.Count());
			AssertHasRowError("One declaration had improper application code", cancellationMessage, "Declaration D0001 is not Delta I/E.");
		}

		public void TestAreTargetsValid_HeadersNonDeltaIEDisallowed()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interface;
			declaration1.JE_DeclarationReference = "D0001";
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			entryHeader1_1.CH_BGMReference = "EH0001";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			entryHeader1_2.CH_BGMReference = "EH0002";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.Targets = new BusinessObject[] { entryHeader1_1, entryHeader1_2, entryHeader2_1 };
			var areTargetsValid = cancellationMessage.Validation.AreTargetsValid();
			AssertEquals("Not all declarations are of Delta I/E, should be disallowed", false, areTargetsValid);
			AssertEquals("Not all declarations are of Delta I/E, should be disallowed", true, cancellationMessage.HasErrors);
			var errorNotifications = cancellationMessage.GetErrors();

			AssertEquals("Two headers belonging to a declaration having improper application code", 2, errorNotifications.Count());

			AssertHasRowError("Two headers belonging to a declaration having improper application code", cancellationMessage, "Declaration D0001 of the entry EH0001 is not Delta I/E.");
			AssertHasRowError("Two headers belonging to a declaration having improper application code", cancellationMessage, "Declaration D0001 of the entry EH0002 is not Delta I/E.");
		}

		public void TestAreTargetsValid_LrnsExistValidated()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration1.JE_DeclarationReference = "D0001";
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			entryHeader1_1.CH_BGMReference = "EH0001";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.Targets = new BusinessObject[] { entryHeader1_1, entryHeader1_2, entryHeader2_1 };
			var areTargetsValid = cancellationMessage.Validation.AreTargetsValid();
			AssertEquals("One of headers misses CorrelationID, validator should deny", false, areTargetsValid);
			AssertEquals("One of headers misses CorrelationID, error messages should exist", true, cancellationMessage.HasErrors);
			var errorNotifications = cancellationMessage.GetErrors();

			AssertEquals("One of headers misses CorrelationID, number of error messages should be 1", 1, errorNotifications.Count());

			AssertHasRowError("An error about missing Reference Number should exist", cancellationMessage, "Field Reference Number of the entry EH0001 has error 'Please enter a Reference Number.'.");
		}

		public void TestAreTargetsValid_CrnsExistValidated()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration1.JE_DeclarationReference = "D0001";
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CH_BGMReference = "EH0001";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.Targets = new BusinessObject[] { entryHeader1_1, entryHeader1_2, entryHeader2_1 };
			var areTargetsValid = cancellationMessage.Validation.AreTargetsValid();
			AssertEquals("One of headers misses CorrelationID, validator should deny", false, areTargetsValid);
			AssertEquals("One of headers misses CorrelationID, error messages should exist", true, cancellationMessage.HasErrors);
			var errorNotifications = cancellationMessage.GetErrors();

			AssertEquals("One of headers misses CorrelationID, number of error messages should be 1", 1, errorNotifications.Count());

			AssertHasRowError("An error about missing CRN should exist", cancellationMessage, "Field Customs Registration Number of the entry EH0001 has error 'Please enter a Customs Registration Number.'.");
		}

		public void TestAreTargetsValid_MrnsExistValidated()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration1.JE_DeclarationReference = "D0001";
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.CRN = "1";
			entryHeader1_1.CH_BGMReference = "EH0001";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.Targets = new BusinessObject[] { entryHeader1_1, entryHeader1_2, entryHeader2_1 };
			var areTargetsValid = cancellationMessage.Validation.AreTargetsValid();
			AssertEquals("One of headers misses CorrelationID, validator should deny", false, areTargetsValid);
			AssertEquals("One of headers misses CorrelationID, error messages should exist", true, cancellationMessage.HasErrors);
			var errorNotifications = cancellationMessage.GetErrors();

			AssertEquals("One of headers misses CorrelationID, number of error messages should be 1", 1, errorNotifications.Count());

			AssertHasRowError("An error about missing MRN should exist", cancellationMessage, "Field MRN # of the entry EH0001 has error 'Please enter a MRN #.'.");
		}

		public void TestAreTargetsValid_MaxAmountValidated()
		{
			var maxCount = 3;
			var cancellationMessage = new Moq.Mock<SendCancellationMessageDataObject>() { CallBase = true };
			cancellationMessage.Setup(s => s.MaxCount()).Returns(maxCount);
			cancellationMessage.Protected().Setup<SendCancellationMessageDataObjectValidation>("GetNewValidation").Returns(new SendCancellationMessageDataObjectValidation(cancellationMessage.Object));

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var headers = new List<BusinessObject>();
			for (var i = 1; i <= maxCount; i++)
			{
				headers.Add(AddHeader(i));
			}

			cancellationMessage.Object.Targets = headers.ToArray();

			var areTargetsValid = cancellationMessage.Object.Validation.AreTargetsValid();
			AssertEquals("All requirements are met, validator should pass", true, areTargetsValid);
			AssertNoRowErrors(cancellationMessage.Object);

			headers.Add(AddHeader(maxCount + 1));
			cancellationMessage.Object.Targets = headers.ToArray();
			areTargetsValid = cancellationMessage.Object.Validation.AreTargetsValid();
			AssertEquals("The number of entries exceeds maximum allowed, validator should deny", false, areTargetsValid);
			AssertHasRowError("An error about exceeding maximum allowed number of items should exist", cancellationMessage.Object, "The number of targets exceeds maximum allowed of 3 items.");

			CusEntryHeader AddHeader(int i)
			{
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CorrelationID = i.ToString();
				entryHeader.CRN = i.ToString();
				entryHeader.MRN = i.ToString();
				return entryHeader;
			}
		}

		public void TestAreTargetsValid_NoEntriesDisallowed()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration1.JE_DeclarationReference = "D0001";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration2.JE_DeclarationReference = "D0002";
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "1";
			entryHeader2_1.MRN = "1";
			entryHeader2_1.CRN = "1";
			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.Targets = new BusinessObject[] { declaration1, declaration2 };
			var areTargetsValid = cancellationMessage.Validation.AreTargetsValid();
			var errorNotifications = cancellationMessage.GetErrors();

			AssertEquals("One of declarations doesn't have entries, validator should deny", false, areTargetsValid);
			AssertEquals("One of declarations doesn't have entries, number of error messages should be 1", 1, errorNotifications.Count());
			AssertHasRowError("Declaration with no entries should be disallowed", cancellationMessage, "Declaration D0001 doesn't have entries.");
		}

		public void TestCheckInvalidationMotivation()
		{
			var cancellationMessage = new SendCancellationMessageDataObject();
			cancellationMessage.InvalidationMotivation = ZString.Empty;
			AssertHasErrorContaining(cancellationMessage.InvalidationMotivationInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
