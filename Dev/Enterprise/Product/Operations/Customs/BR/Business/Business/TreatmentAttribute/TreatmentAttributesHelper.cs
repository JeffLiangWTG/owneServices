using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.TTCE;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public static class TreatmentAttributesHelper
	{
		public static CodeDescriptionPairList GetDuimpLegalBaseListFromMessage(this JobDeclaration declaration, ZString tariffCode, ZString countryOfOrigin, bool optionalOnly = false)
		{
			var ttce = LoadTreatmentAttributes(declaration, tariffCode, countryOfOrigin);
			if (ttce == null)
			{
				return null;
			}

			return declaration?.Factory.GetCachedValue($"GetDuimpLegalBaseListFromMessage_{declaration.PK}_{tariffCode}_{countryOfOrigin}_{optionalOnly}", () =>
			{
				var list = new CodeDescriptionPairList();
				ttce.tratamentosTributarios?.Where(treatment => !optionalOnly || (treatment.fundamentoLegal?.tipo.Equals(Constants.LegalBasisType.Optional, StringComparison.OrdinalIgnoreCase) ?? false))
					.ForEach(treatment => list.AddPairIfNotExist(treatment.fundamentoLegal.codigo, treatment.fundamentoLegal.nome));

				LoadMandatoryTreatmentAttributes(declaration, tariffCode, countryOfOrigin)?.fundamentosOpcionaisDisponiveis?
					.ForEach(treatment => list.AddPairIfNotExist(treatment.fundamentoLegal.codigo, treatment.fundamentoLegal.nome));

				list.Sort();
				return list;
			});
		}

		public static TariffProfile[] GetTariffProfilesFromMessage(this JobDeclaration declaration, ZString tariffCode, ZString countryOfOrigin)
		{
			var ttce = LoadTreatmentAttributes(declaration, tariffCode, countryOfOrigin);
			if (ttce == null)
			{
				return null;
			}

			return declaration?.Factory.GetCachedValue($"GetTariffProfilesFromMessage_{declaration.PK}_{tariffCode}_{countryOfOrigin}", () =>
			{
				var optionalLegalBasis = LoadMandatoryTreatmentAttributes(declaration, tariffCode, countryOfOrigin)?.fundamentosOpcionaisDisponiveis;
				return GetTariffProfilesFromTreatments(ttce.tratamentosTributarios).Concat(GetTariffProfilesFromTreatments(optionalLegalBasis)).ToArray();
			});
		}

		public static TariffProfile[] GetTariffProfilesFromTreatments(IEnumerable<TratamentoTributarioDTO> treatments)
		{
			return treatments?.SelectMany(treatment => treatment.mercadorias?.SelectMany(x => x.atributos).GroupBy(x => x.codigo)
				.Select(attributes => new TariffProfile()
				{
					PK = ZGuid.NewZGuid(),
					QuestionCode = attributes.Key,
					Regime = treatment.regime?.codigo,
					LegalCode = treatment.fundamentoLegal?.codigo,
					TaxType = GetTaxType(treatment.tributo?.codigo),
					IsMandatory = treatment.fundamentoLegal?.tipo.Equals(Constants.LegalBasisType.Normal, StringComparison.OrdinalIgnoreCase) ?? false,
				}).WhereNotNull()).ToArray() ?? [];
		}

		static TariffProfile[] GetTariffProfilesFromTreatments(IEnumerable<FundamentoLegalOpcionalDisponivelDTO> optionalLegalBasis)
		{
			return optionalLegalBasis?.Select(legalBase => new TariffProfile()
			{
				PK = ZGuid.NewZGuid(),
				QuestionCode = ZString.Empty,
				Regime = legalBase.regime?.codigo,
				LegalCode = legalBase.fundamentoLegal?.codigo,
				TaxType = GetTaxType(legalBase.tributo?.codigo),
				IsMandatory = false
			}).WhereNotNull().ToArray() ?? [];
		}

		internal static ZString GetTaxType(string taxCode)
		{
			return taxCode switch
			{
				"1" => Constants.RateTypes.ImportDuty,
				"2" => Constants.RateTypes.IPI,
				"3" => Constants.RateTypes.Antidumping,
				"6" => Constants.RateTypes.PIS,
				"7" => Constants.RateTypes.Cofins,
				_ => ZString.Empty,
			};
		}

		public static TariffProfileQuestion[] GetTariffProfileQuestionsFromMessage(this JobDeclaration declaration, ZString tariffCode, ZString countryOfOrigin)
		{
			var ttce = LoadTreatmentAttributes(declaration, tariffCode, countryOfOrigin);
			if (ttce == null)
			{
				return null;
			}

			return declaration?.Factory.GetCachedValue($"GetTariffProfileQuestionsFromMessage_{declaration.PK}_{tariffCode}_{countryOfOrigin}", () =>
			{
				return ttce.tratamentosTributarios?.SelectMany(treatment => treatment.mercadorias?.SelectMany(x => x.atributos)).WhereNotNull().GroupBy(x => x.codigo)
					.Select(attributes =>
					{
						var codeType = attributes.First().tipoCodigo.ToLower();
						var isList = codeType == Constants.AttributeAnswerDataType.DynamicDomain;
						var question = new TariffProfileQuestion()
						{
							PK = ZGuid.NewZGuid(),
							Code = attributes.Key,
							Name = attributes.First().descricaoCodigo,
							AnswerDataType = isList ? Universal.Constants.ProfileQuestion.AnswerDataTypes.List : Universal.Constants.ProfileQuestion.AnswerDataTypes.Number,
							AnswerDecimalPlaces = codeType == Constants.AttributeAnswerDataType.DecimalNumber ? (ZShort)5 : ZShort.Zero,
							IsAnswerMandatory = true,
							AnswerList = isList ? attributes.GroupBy(x => x.valor).Select(x => new TariffProfileQuestionAnswer()
							{
								Value = x.Key,
								Description = x.First().descricaoValor,
							}).ToArray() : null,
						};

						return question;
					}).ToArray();
			});
		}

		static RespostaObterTratamentosTributariosImportacaoDTO LoadTreatmentAttributes(JobDeclaration declaration, ZString tariffCode, ZString countryOfOrigin)
		{
			return declaration?.Factory.GetCachedValue($"LoadTreatmentAttributes_{declaration.PK}_{tariffCode}_{countryOfOrigin}", () =>
			{
				return declaration.LoadMostRecentTreatmentAttributesMessage(tariffCode, countryOfOrigin) is EDIMessage message
						? DeserializeTreatmentAttributes(message) : null;
			});
		}

		static RespostaObterTratamentosTributariosImportacaoDTO LoadMandatoryTreatmentAttributes(JobDeclaration declaration, ZString tariffCode, ZString countryOfOrigin)
		{
			return declaration?.Factory.GetCachedValue($"LoadMandatoryTreatmentAttributes_{declaration.PK}_{tariffCode}_{countryOfOrigin}", () =>
			{
				return declaration.LoadMostRecentTreatmentAttributesMessage(tariffCode, countryOfOrigin, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment) is EDIMessage message
						? DeserializeTreatmentAttributes(message) : null;
			});
		}

		static RespostaObterTratamentosTributariosImportacaoDTO DeserializeTreatmentAttributes(EDIMessage message)
		{
			return BRMessageHelper.DeserializeObject<RespostaObterTratamentosTributariosImportacaoDTO>(message.EM_MessageText, throwExceptionIfOccurs: false);
		}

		internal static EDIMessage LoadMostRecentTreatmentAttributesMessage(this JobDeclaration declaration, ZString tariffCode, ZString countryOfOrigin, params ZString[] subTypes)
		{
			if (declaration == null || !declaration.IsInDatabase || tariffCode.IsEmpty || countryOfOrigin.IsEmpty)
			{
				return null;
			}

			return LoadTreatmentAttributesMessages(declaration).Where(x => subTypes?.Length == 0 || subTypes.Contains(x.EM_MessageSubType))
						.Where(x => x.EM_ApplicationReference.StartsWith($"{tariffCode}|{countryOfOrigin}|"))
						.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
		}

		static EDIMessage[] LoadTreatmentAttributesMessages(JobDeclaration declaration)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms)
								.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.RTT)
								.AddToFilter(EDIMessageSchema.EM_MessageSubType, new[] { EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes })
								.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, declaration.PK)
								.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);

			return declaration.Factory.Load<EDIMessage>(query);
		}
	}
}
