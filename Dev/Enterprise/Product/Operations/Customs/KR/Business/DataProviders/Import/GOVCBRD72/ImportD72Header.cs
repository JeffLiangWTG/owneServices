using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportD72Header : IImportD72Header
	{
		public string ImportDeclarationNumber { get; set; }
		public int SequenceNo { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public Organisation Declarant { get; set; }
		public string DeclarantType { get; set; }
		public DateTime BeforeReExportScheduledDate { get; set; }
		public DateTime AfterReExportScheduledDate { get; set; }
		public string ReasonDescription { get; set; }
		public ImportD72Line[] Lines { get; set; }

		ZString IImportD72Header.ImportDeclarationNumber => ImportDeclarationNumber;
		ZInt IImportD72Header.SequenceNo => SequenceNo;
		ZString IImportD72Header.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImportD72Header.DeclarationCustomsDivision => DeclarationCustomsDivision;
		IOrganization IImportD72Header.Declarant => Declarant;
		ZString IImportD72Header.DeclarantType => DeclarantType;
		ZDate IImportD72Header.BeforeReExportScheduledDate => (ZDate)BeforeReExportScheduledDate;
		ZDate IImportD72Header.AfterReExportScheduledDate => (ZDate)AfterReExportScheduledDate;
		ZString IImportD72Header.ReasonDescription => ReasonDescription;
		IEnumerable<IImportD72Line> IImportD72Header.Lines => Lines;
	}
}
