using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Business
{
	public class IdentifierLookups : CusSupportingInfoLookups
	{
		public IdentifierLookups(Identifier parent) : base(parent)
		{
		}

		protected new Identifier Parent
		{
			get { return (Identifier)base.Parent; }
		}

		public override ICollection CodeList
		{
			get
			{
				return Factory.GetCachedValue("MX_IdentifierLookups_CodeList", () =>
				{
					var refCusProfileTypes = MXRefCusProfile.GetProfileTypes(Factory, Constants.Profile.TariffTypes.LineLevelIdentifiers);
					var result = new CodeDescriptionPairList();
					foreach (var profileType in refCusProfileTypes)
					{
						result.AddPairIfNotExist(profileType.XXX_ProfileType, profileType.XXX_Description);
					}
					result.Sort();
					return result;
				});
			}
		}

		public CodeDescriptionPairList Complement1List => GetPossibleValues(Parent.Complement1Question);

		public CodeDescriptionPairList Complement2List => GetPossibleValues(Parent.Complement2Question);

		public CodeDescriptionPairList Complement3List => GetPossibleValues(Parent.Complement3Question);

		CodeDescriptionPairList GetPossibleValues(RefCusProfileQuestion question)
		{
			if (!Parent.CSI_Code.IsEmpty && question.XQ2_AnswerDataType == Universal.Constants.ProfileQuestion.AnswerDataTypes.List)
			{
				return Factory.GetCachedValue($"MX_ComplementList_{question.PK}", () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (var answer in question.Answers)
					{
						result.AddPairIfNotExist(answer.XQ4_Value, answer.XQ4_Description);
					}
					result.Sort();
					return result;
				});
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}
	}
}
