using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business
{
	public class AttributeCusCodeDataLookups : Customs.Business.CusCodeDataLookups
	{
		public AttributeCusCodeDataLookups(AttributeCusCodeData parent)
			: base(parent)
		{
		}

		protected new AttributeCusCodeData Parent
		{
			get { return (AttributeCusCodeData)base.Parent; }
		}

		public CodeDescriptionPairList PossibleValues
		{
			get
			{
				if (Parent.TariffProfileQuestion is TariffProfileQuestion question)
				{
					if (question.AnswerDataType == Universal.Constants.ProfileQuestion.AnswerDataTypes.List
						|| question.AnswerDataType == Universal.Constants.ProfileQuestion.AnswerDataTypes.String)
					{
						return Factory.GetCachedValue($"AttributeCusCodeDataLookups_PossibleValues_{question.PK}", () =>
						{
							var result = new CodeDescriptionPairList();
							foreach (var item in question.AnswerList)
							{
								result.AddPairIfNotExist(item.Value, item.Description);
							}
							result.Sort();
							return result;
						});
					}
					else if (question.AnswerDataType == Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean)
					{
						return Factory.GetCachedValue($"AttributeCusCodeDataLookups_PossibleValues_Boolean", () =>
						{
							var result = new CodeDescriptionPairList();
							result.AddPair(Profile.AnswerValues.No, Res.GetString("B7F2C1A0-C91D-4E52-8D15-20175366B7E3", "Não"));
							result.AddPair(Profile.AnswerValues.Yes, Res.GetString("8018EBCB-FC3D-4570-954F-3C5ED1E7DCB8", "Sim"));
							return result;
						});
					}
				}
				return new CodeDescriptionPairList();
			}
		}
	}
}

