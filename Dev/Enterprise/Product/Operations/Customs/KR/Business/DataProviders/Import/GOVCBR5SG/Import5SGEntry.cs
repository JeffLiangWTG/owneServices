using System;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SGEntry : IImport5SGEntry
	{
		public string ImportDeclarationNumber { get; set; }
		public DateTime ExtensionDate { get; set; }
		public string ApplicationReason { get; set; }

		ZString IImport5SGEntry.ImportDeclarationNumber => ImportDeclarationNumber;
		ZDate IImport5SGEntry.ExtensionDate => new ZDate(ExtensionDate);
		ZString IImport5SGEntry.ApplicationReason => ApplicationReason;
	}
}
