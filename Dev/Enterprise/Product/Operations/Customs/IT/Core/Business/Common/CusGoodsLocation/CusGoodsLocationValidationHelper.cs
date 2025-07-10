namespace Enterprise.Customs.IT.Business;

public static class CusGoodsLocationValidationHelper
{
	public static void ValidateCGL_Qualifier(EU.Business.CusGoodsLocation cusGoodsLocation)
	{
		if (cusGoodsLocation.CGL_Qualifier.IsEmpty && !cusGoodsLocation.CGL_Type.IsEmpty)
		{
			cusGoodsLocation.CGL_QualifierInfo.AddMessageError(GetQualifierAndTypeAreRequiredMessage());
		}
	}

	public static void ValidateCGL_Type(EU.Business.CusGoodsLocation cusGoodsLocation)
	{
		if (cusGoodsLocation.CGL_Type.IsEmpty && !cusGoodsLocation.CGL_Qualifier.IsEmpty)
		{
			cusGoodsLocation.CGL_TypeInfo.AddMessageError(GetQualifierAndTypeAreRequiredMessage());
		}
	}

	#region Implementation

	static string GetQualifierAndTypeAreRequiredMessage()
	{
		return Res.GetString("52c78ef0-888f-4870-976c-98eda7bf0768", "Qualifier and Type field must be both filled or both empty");
	}

	#endregion
}
