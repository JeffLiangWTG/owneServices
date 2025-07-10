using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5SHMessageData
	{
		ZString ApplicationNumber { get; }
		ZDate ApprovalDate { get; }
		ZString ResultType { get; }
		ZString DismissalReason { get; }
		ZString CustomsManagerName { get; }
		ZString CustomsOffice { get; }
		IEnumerable<IEntryDetail> EntryDetails { get; }
	}

	public interface IEntryDetail
	{
		ZString ImportDeclarationNumber { get; }
		ZDate EntryReleaseDate { get; }
	}

	class GOVCBR5SHMessageData : IGOVCBR5SHMessageData
	{
		public ZString ApplicationNumber { get; set; }
		public ZDate ApprovalDate { get; set; }
		public ZString ResultType { get; set; }
		public ZString DismissalReason { get; set; }
		public ZString CustomsManagerName { get; set; }
		public ZString CustomsOffice { get; set; }
		public IEnumerable<IEntryDetail> EntryDetails { get; set; }
	}

	class EntryDetail : IEntryDetail
	{
		public ZString ImportDeclarationNumber { get; set; }
		public ZDate EntryReleaseDate { get; set; }
	}
}
