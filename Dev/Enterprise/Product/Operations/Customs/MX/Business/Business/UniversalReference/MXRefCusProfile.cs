using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.MX.Business
{
	public static class MXRefCusProfile
	{
		public static RefCusProfileQuestion FindByQuestionCode(this IEnumerable<RefCusProfileQuestion> profileQuestions, ZString code)
		{
			return profileQuestions.FirstOrDefault(x => x.XQ2_Code == code);
		}

		public static RefCusProfileQuestion[] GetProfileQuestions(BusinessObjectFactory factory, ZString profileType, ZDateTime effectiveDate)
		{
			return !profileType.IsEmpty ? factory.GetCachedValue($"MX_GetProfileQuestions_{profileType}_{effectiveDate}", GetProfileQuestions) : [];

			RefCusProfileQuestion[] GetProfileQuestions()
			{
				var questions = new RefCusProfileQuestion.Loader(factory).Load(profileType, Core.Constants.CountryCodes.Mexico, effectiveDate);
				questions.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
				return questions;
			}
		}

		public static RefCusProfileType[] GetProfileTypes(BusinessObjectFactory factory, ZString tariffType)
		{
			return !tariffType.IsEmpty ? factory.GetCachedValue($"MX_GetProfileTypes_{tariffType}", GetProfileTypes) : [];

			RefCusProfileType[] GetProfileTypes()
			{
				return new RefCusProfileType.Loader(factory).Load(tariffType, Core.Constants.CountryCodes.Mexico);
			}
		}
	}
}
