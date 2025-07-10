using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class ForeignOperatorCustomsStatusTypeList
	{
		public static ZString MapToCWCode(ZString code)
		{
			switch (code.ToUpper())
			{
				case "ATIVADO":
					return ForeignOperatorCustomsStatusTypeList.Codes.Active;
				case "DESATIVADO":
					return ForeignOperatorCustomsStatusTypeList.Codes.Inactive;
				default:
					return string.Empty;
			}
		}
	}
}
