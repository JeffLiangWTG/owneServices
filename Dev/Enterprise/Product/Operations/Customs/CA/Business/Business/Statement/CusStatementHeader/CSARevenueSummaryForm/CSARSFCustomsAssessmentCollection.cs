using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFCustomsAssessmentCollection : NonPersistentBusinessObjectCollection<CSARSFAssessment>
	{
		public CSARSFCustomsAssessmentCollection(CusStatementHeader cusStatementHeader) : base(cusStatementHeader.Factory)
		{
			this.cusStatementHeader = cusStatementHeader;
			Load();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var cusStatementLine = cusStatementHeader.StatementLines.AddNew();
			cusStatementLine.B3_EntryType = CSARSFAssessmentTypes.Codes.CustomsAssessment;
			return new CSARSFAssessment(cusStatementLine);
		}

		readonly CusStatementHeader cusStatementHeader;

		public override void Load()
		{
			using (SuspendSettingHasChanges())
			{
				RemoveAll();
				var assessmentLines = cusStatementHeader.StatementLines.Cast<CusStatementLine>().Where(x => x.B3_EntryType == CSARSFAssessmentTypes.Codes.CustomsAssessment);
				foreach (var line in assessmentLines)
				{
					Add(new CSARSFAssessment(line));
				}
			}
		}
	}
}
