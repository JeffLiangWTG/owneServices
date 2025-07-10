using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusAttributeFilterValidation : Customs.Business.CusAttributeFilterValidation
	{
		public CusAttributeFilterValidation(CusAttributeFilter parent)
			: base(parent)
		{
		}

		protected new CusAttributeFilter Parent
		{
			get { return (CusAttributeFilter)base.Parent; }
		}

		protected new CusClassPartPivot Pivot
		{
			get { return (CusClassPartPivot)base.Pivot; }
		}

		protected override void CheckBG_AttributeValue1()
		{
			base.CheckBG_AttributeValue1();
			CheckDuplicateAttribute();
		}

		void CheckDuplicateAttribute()
		{
			CusClassPartPivot pivot = Pivot;
			if (pivot != null && pivot.IsImportClassification)
			{
				CusAttributeFilterCollection collection = null;
				ZString attributeType = ZString.Empty;
				if (Parent.IsAttribute2)
				{
					collection = pivot.Attributes2;
					attributeType = AttributeName.Attribute2;
				}
				else if (Parent.IsAttribute3)
				{
					collection = pivot.Attributes3;
					attributeType = AttributeName.Attribute3;
				}
				else if (Parent.IsAttribute1)
				{
					collection = pivot.Attributes1;
					attributeType = AttributeName.Attribute1;
				}
				if (collection != null && collection.HasSameValue1(Parent))
				{
					Parent.BG_AttributeValue1Info.AddError(AttributeValueShouldBeUniqueFor(attributeType));
				}
			}
		}

		public static string AttributeValueShouldBeUniqueFor(string attributeType)
		{
			return Res.GetString("75fa112a-3f1b-4022-aa7e-691fc74acdf0", "Attribute value should be unique for {0}.", attributeType);
		}
	}
}
