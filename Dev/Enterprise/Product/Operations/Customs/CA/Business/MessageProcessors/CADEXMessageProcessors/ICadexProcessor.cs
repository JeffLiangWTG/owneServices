using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Cadex.RecordParsers
{
	public interface ICadexProcessor : IFactoryProvider
	{
		void ShowError(string message);
		void ShowRecordError(string record, string message);
		void AddDeleteFetchHint(string tableName, ZQuery filter);
	}
}
