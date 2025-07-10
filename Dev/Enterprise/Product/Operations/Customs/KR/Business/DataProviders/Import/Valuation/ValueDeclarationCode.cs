using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ValueDeclarationCode : IValueDeclarationCode
	{
		public string Code { get; set; }
		public string CodeOtherDescription { get; set; }

		ZString IValueDeclarationCode.Code => Code;
		ZString IValueDeclarationCode.CodeOtherDescription => CodeOtherDescription;
	}
}
