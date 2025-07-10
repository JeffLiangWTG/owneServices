using System;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class ResponseBase
	{
		public bool Succeeded { get; set; }
		public string ErrorMessage { get; set; }
	}
}
