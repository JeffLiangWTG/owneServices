using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CodeAndDescriptionWrapper : DocumentWrappers.GenericWrappers.CodeAndDescriptionWrapper
	{
		protected CodeAndDescriptionWrapper(ZString code, ICodeDescriptionPairList list, BusinessObjectFactory factory) : base(code, list, factory)
		{
		}

		protected CodeAndDescriptionWrapper(ZString code, ZString description, BusinessObjectFactory factory) : base(code, description, factory)
		{
		}

		public static CodeAndDescriptionWrapper New(ZString code, ICodeDescriptionPairList list, BusinessObjectFactory factory)
		{
			return new CodeAndDescriptionWrapper(code, list, factory);
		}

		public static CodeAndDescriptionWrapper New(ZString code, ZString description, BusinessObjectFactory factory)
		{
			return new CodeAndDescriptionWrapper(code, description, factory);
		}

		public static CodeAndDescriptionWrapper New(BusinessObjectFactory factory, ZString code, ZString codeType, ZDateTime date)
		{
			return new CodeAndDescriptionWrapper(code, CNRefCusCodeListLoader.GetCNCodeByType(factory, code, codeType, date)?.ZZD_Description ?? ZString.Empty, factory);
		}

		public static CodeAndDescriptionWrapper New(BusinessObjectFactory factory, ZBool value)
		{
			return new CodeAndDescriptionWrapper(value ? ConfirmationTypeList.Codes.Yes : ConfirmationTypeList.Codes.No, factory.GetCachedValue<ConfirmationTypeList>(), factory);
		}

		public ZString CodeInParenthesesAndDesc => Code.IsEmpty ? ZString.Empty : ZString.Format("({0}){1}", Code, Description);
	}
}
