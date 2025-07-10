using System.Linq;
using CargoWise.Integration;
using Enterprise.Customs.Common.Shared;
using Enterprise.Edifact.D96A.Elements;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ImportDeclarationEDIReleaseEntryStatusListTest : TestCase
	{
		public void TestGetEntryStatusByProcessingIndicatorCoded()
		{
			AssertEquals(EDIReleaseImportEntryStatusList.Codes.MessageContentAccepted,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.MessageContentAccepted.ToString()));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.MessageContentRejected,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.MessageContentRejectedWithComment));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.GoodsReleased,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.GoodsReleased));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.GoodsRequiredForExamination,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.GoodsRequiredForExamination));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.AllDocumentsOrAsSpecifiedToBeProduced));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.GoodsDetained,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.GoodsDetained));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.GoodsMayMove,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.GoodsMayMoveUnderCustomsTransfer));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.Cancelled,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival, MessageSubTypeCodes.Codes.Cancellation));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.Cancelled,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival, EDIReleaseImportEntryStatusList.Codes.Cancelled));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.Transit,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.Transit));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.Error,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.ErrorMessage));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.Import));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.AwaitingCustomsProcessing,
						 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.TransactionAwaitingProcessing));

			AssertEquals(EDIReleaseImportEntryStatusList.Codes.AcceptedWithWarning,
			 EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.DeclarationAcceptedWithWarning));

			AssertEquals(string.Empty, EDIReleaseImportEntryStatusList.GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.GetFromString("AAA")));
		}

		public void TestIsError()
		{
			var errors = new[]
							{
								EDIReleaseImportEntryStatusList.Codes.Error,
								EDIReleaseImportEntryStatusList.Codes.SyntaxError,
								EDIReleaseImportEntryStatusList.Codes.MessageContentRejected
							};

			foreach (var status in errors)
			{
				Assert(string.Format("Is {0} error?", status), EDIReleaseImportEntryStatusList.IsError(status));
			}

			foreach (var codeDescription in new EDIReleaseImportEntryStatusList().Cast<ICodeDescription>().Where(x => !errors.Contains(x.Code)))
			{
				Assert(string.Format("{0} is not error", codeDescription), !EDIReleaseImportEntryStatusList.IsError(codeDescription.Code));
			}
		}
	}
}
