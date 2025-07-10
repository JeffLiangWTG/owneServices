using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SGHeader : IImport5SGHeader
	{
		public string ApplicationNumber { get; set; }
		public int SequenceNo { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string UnipassDeclarantID { get; set; }
		public Import5SGEntry[] Declarations { get; set; }

		ZString IImport5SGHeader.ApplicationNumber => ApplicationNumber;
		ZInt IImport5SGHeader.SequenceNo => SequenceNo;
		ZString IImport5SGHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImport5SGHeader.UnipassDeclarantID => UnipassDeclarantID;
		public IEnumerable<IImport5SGEntry> Entries => Declarations;
	}
}
