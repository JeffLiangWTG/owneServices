using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class SummarySection : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SummarySection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public SummaryLine Header
		{
			get { return header ?? (header = new SummaryLine(Factory)); }
		}
		SummaryLine header;

		public SummaryLineCollection Lines
		{
			get { return lines ?? (lines = new SummaryLineCollection(Factory)); }
		}
		SummaryLineCollection lines;

		public int NumberOfColumnsUsed
		{
			get => numberOfColumnsUsed ?? new[] { Header.Column1, Header.Column2, Header.Column3, Header.Column4, Header.Column5, Header.Column6, Header.Column7, Header.Column8, Header.Column9, Header.Column10 }.TakeWhile(x => !x.IsEmpty).Count();

			set { numberOfColumnsUsed = value; }
		}

		int? numberOfColumnsUsed;
	}
}

