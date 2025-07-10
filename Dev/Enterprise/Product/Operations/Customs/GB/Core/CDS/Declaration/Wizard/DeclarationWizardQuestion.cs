using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class DeclarationWizardQuestion
	{
		public ZString QuestionText { get; set; }
		public List<DeclarationWizardOption> Options { get; set; }
		public ZInt QuestionNumber { get; set; }
	}
}
