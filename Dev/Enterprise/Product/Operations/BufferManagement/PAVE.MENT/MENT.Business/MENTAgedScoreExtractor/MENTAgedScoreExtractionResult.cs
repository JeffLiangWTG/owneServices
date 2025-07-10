using System.Collections.ObjectModel;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreExtractionResult
	{
		public MENTAgedScoreExtractionResult(Collection<MENTDataRow> resultSet, string name)
		{
			this.resultSet = resultSet;
			this.name = name;
		}

		public Collection<MENTDataRow> Results
		{
			get { return resultSet; }
		}

		readonly Collection<MENTDataRow> resultSet;

		public string Name
		{
			get { return name; }
		}

		readonly string name;
	}
}
