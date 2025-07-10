using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public interface IGOVCBRR99MessageData
	{
		ZString DeclarationType { get; }
		ZString ApplicationNumber { get; }
		ZInt AmendSequence { get; }
		ZString DeclarationSubType { get; }
		ZString CustomsOffice { get; }
		ZString CustomsPersonID { get; }
		ZString CustomsPersonName { get; }
		ZDate DeclarationDate { get; }
		ZDateTime AcceptDateTime { get; }
		ZDateTime NoticeDateTime { get; }
		ZString NoticeNumber { get; }
		IEnumerable<IContent> Content { get; }
		ZString EntryLineNo5FN { get; }
	}

	public interface IContent
	{
		ZString ContentType { get; }
		ZString ContentDescription { get; }
	}

	class GOVCBRR99MessageData : IGOVCBRR99MessageData
	{
		public ZString DeclarationType { get; set; }
		public ZString ApplicationNumber { get; set; }
		public ZInt AmendSequence { get; set; }
		public ZString DeclarationSubType { get; set; }
		public ZString CustomsOffice { get; set; }
		public ZString CustomsPersonID { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZDate DeclarationDate { get; set; }
		public ZDateTime AcceptDateTime { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString NoticeNumber { get; set; }
		public IEnumerable<IContent> Content { get; set; }
		public ZString EntryLineNo5FN { get; set; }
	}

	class Content : IContent
	{
		public ZString ContentType { get; set; }
		public ZString ContentDescription { get; set; }
	}
}
