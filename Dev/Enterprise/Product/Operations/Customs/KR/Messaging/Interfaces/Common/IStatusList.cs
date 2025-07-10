using CargoWise.Integration;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IStatusList : ICodeDescriptionPairList
	{
		bool ShouldUsersBeWarnedPriorToPrintingDocument(string code);

		string GetDocumentPrintingWarningMessage();
	}
}
