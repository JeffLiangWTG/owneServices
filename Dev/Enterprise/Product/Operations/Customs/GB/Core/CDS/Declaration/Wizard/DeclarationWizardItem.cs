using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.CDS
{
	public class DeclarationWizardItem
	{
		public ZString RequestedProcedure { get; set; }
		public ZString ProcedureDefinition { get; set; }
		public ZString Q2DeclarationType { get; set; }
		public ZString Q3 { get; set; }
		public ZString DeclarationType { get; set; }
		public ZString Q1 { get; set; }
		public ZString AdditionalDeclarationType { get; set; }
		public ZString ProcedureCategory { get; set; }
		public string[] AdditionalDeclarationTypes() => ((string)AdditionalDeclarationType).Split(new string[] { ",", "or" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
		public bool IsImport => new ImportDeclarationTypeList().ContainsCode(ProcedureCategory);
	}
}
