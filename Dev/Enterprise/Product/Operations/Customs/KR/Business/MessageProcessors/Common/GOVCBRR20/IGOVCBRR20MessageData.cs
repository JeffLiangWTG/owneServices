using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public interface IGOVCBRR20MessageData
	{
		ZString DeclarationType { get; }
		ZDateTime NoticeDateTime { get; }
		ZString ApplicationNumber { get; }
		ZInt AmendSequence { get; }
		ZDateTime AcceptDateTime { get; }
		ZString CustomsOfficeAndDivision { get; }
		IEnumerable<IError> Error { get; }
		ZString EntryLineNo5FN { get; }
	}
	public interface IError
	{
		ZString ErrorDescription { get; }
		IEnumerable<ZString> ApplicationKey { get; }
	}
	class GOVCBRR20MessageData : IGOVCBRR20MessageData
	{
		public ZString DeclarationType { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ApplicationNumber { get; set; }
		public ZInt AmendSequence { get; set; }
		public ZDateTime AcceptDateTime { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
		public IEnumerable<IError> Error { get; set; }

		public ZString EntryLineNo5FN { get; set; }
	}
	class Error : IError
	{
		public ZString ErrorDescription { get; set; }
		public IEnumerable<ZString> ApplicationKey { get; set; }
	}
}
