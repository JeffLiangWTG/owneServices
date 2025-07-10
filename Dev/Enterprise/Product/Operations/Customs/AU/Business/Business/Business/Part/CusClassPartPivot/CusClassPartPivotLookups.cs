using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(CusClassPartPivot parent) : base(parent)
		{
		}

		public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

		public override CodeDescriptionPairList ClassificationTypes
		{
			get
			{
				return Factory.GetCachedValue("AUCusClassPartPivot_ClassificationType", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ClassificationTypeList.Codes.HTI, ClassificationTypeList.Descriptions.HTI);
					result.AddPair(ClassificationTypeList.Codes.HTE, ClassificationTypeList.Descriptions.HTE);
					return result;
				}
				);
			}
		}

		public override IBaseClassificationCollection<BaseCusClassification> ClassificationList
		{
			get
			{
				string type = Parent.CI_ChildType;
				var result = Factory.GetCachedValue(FormattableString.Invariant($"AUCusClassPartPivot_Calssifications_{type}"), () =>
				{
					if (type == ClassificationTypeList.Codes.HTE)
					{
						return new ExportClassificationCollection(Factory);
					}
					else if (type == ClassificationTypeList.Codes.HTI)
					{
						return new ImportClassificationCollection(Factory);
					}
					else
					{
						return new BaseClassificationCollection<Classification>(Factory);
					}
				});
				return result;
			}
		}

		public override BusinessObjectCollection Tariffs
		{
			get { return (BusinessObjectCollection)ClassificationList; }
		}
	}
}
