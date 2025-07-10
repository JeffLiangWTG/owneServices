using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class AttributeCusCodeDataCollection : CusCodeDataCollection<AttributeCusCodeData>
	{
		public AttributeCusCodeDataCollection(JobComInvoiceLine parent, string type)
			: base(parent, type)
		{
		}

		public AttributeCusCodeDataCollection(CusClassPartPivot parent)
			: base(parent, CusCodeDataTypeList.Codes.Attribute)
		{
		}

		public AttributeCusCodeDataCollection(CusGoodsCatalog parent)
			: base(parent, CusCodeDataTypeList.Codes.Attribute)
		{
		}

		public override void Load()
		{
			Factory.AddFetchHint(CusCodeDataSchema.Instance, new ZQuery(CusCodeDataSchema.CY_ParentID, Master.PK).AddToFilter(CusCodeDataSchema.CY_Type, CY_Type));
			base.Load();
		}

		public AttributeCusCodeData AddNewOrUpdateExistingAttribute(ZString code, ZString data)
		{
			var attribute = GetFirstElementHaving(code) ?? AddNew(code);
			if (attribute.CY_Data != data)
			{
				attribute.CY_Data = data;
			}
			return attribute;
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter()
				.AddToFilter(CusCodeDataSchema.CY_Order, ZShort.Zero);
		}

		internal void Rebuild()
		{
			this.Cast<AttributeCusCodeData>().ForEach(x => x.TariffProfileQuestion = null);

			if (Master is IAttributeCusCodeDataParent parent)
			{
				var effectiveDate = parent.EffectiveAssessmentDate;
				TariffProfileQuestion[] questions = null;
				TariffProfile[] profiles = null;
				if (CY_Type == CusCodeDataTypeList.Codes.Attribute)
				{
					if (Master is CusGoodsCatalog catalog)
					{
						questions = catalog.UniversalTariff?.GetTariffNCMQuestions(catalog.CGC_Type, effectiveDate, Constants.ProfileQuestion.AttributeValues.Product);
					}
					else if (Master is CusClassPartPivot pivot)
					{
						questions = pivot.UniversalTariff?.GetTariffNCMCharacteristics(effectiveDate, pivot.IsExportClassification, pivot.IsImportClassification);
					}
					else if (Master is JobComInvoiceLine invoiceLine)
					{
						if (invoiceLine.IsImportOnly)
						{
							questions = invoiceLine.UniversalTariff?.GetTariffNCMQuestions(BRJobMessageTypeList.Codes.Import, effectiveDate, Constants.ProfileQuestion.AttributeValues.Duimp);
						}
						else if (invoiceLine.IsExport)
						{
							questions = invoiceLine.UniversalTariff?.GetTariffNCMCharacteristics(effectiveDate, isExport: invoiceLine.IsExport, isImport: false);
						}
						else if (invoiceLine.IsLPCO)
						{
							questions = invoiceLine.UniversalTariff?.GetTariffLPCCharacteristics(effectiveDate);
						}
					}
				}
				else if (CY_Type == CusCodeDataTypeList.Codes.TaxRegimeAttribute && Master is JobComInvoiceLine invoiceLine && invoiceLine.IsImportOnly)
				{
					profiles = invoiceLine.DuimpTaxRegimes.Cast<DuimpTaxRegime>().SelectMany(s => s.Profiles).ToArray();

					var questionCodes = profiles.Select(x => x.QuestionCode).ToHashSet();
					questions = invoiceLine.Declaration.GetTariffProfileQuestionsFromMessage(invoiceLine.JI_Tariff, invoiceLine.JI_CountryOfOrigin)?.Where(x => questionCodes.Contains(x.Code)).ToArray();
					questions ??= BRRefCusProfile.GetProfileQuestions(profiles.Select(x => x.RefCusProfile).WhereNotNull().ToArray(), effectiveDate);
				}
				if (questions != null)
				{
					IEnumerable<RefCusProfileQuestionPathway> pathways = null;
					var refCusProfileQuestion = questions.Select(x => x.RefCusProfileQuestion).WhereNotNull().ToArray();
					if (refCusProfileQuestion.Length > 0)
					{
						pathways = new RefCusProfileQuestionPathway.Loader(Factory).Load(questions.Select(s => s.PK).ToArray(), effectiveDate, recursive: true);
						questions = questions.Concat(Factory.Load<RefCusProfileQuestion>(new ZQuery(RefCusProfileQuestionSchema.PK, pathways.Select(s => s.XQP_XQ2_QuestionChild))).Select(TariffProfileQuestion.New)).ToArray();
					}
					questions.OrderBy(x => x.Code).ForEach(x => AddNewOrUpdate(x.Code, x, profiles?.Where(w => w.QuestionCode == x.Code), pathways));
				}
			}
			RemoveNullProfileQuestions();
		}

		AttributeCusCodeData AddNewOrUpdate(ZString code, TariffProfileQuestion question, IEnumerable<TariffProfile> profiles = null, IEnumerable<RefCusProfileQuestionPathway> pathways = null)
		{
			var attribute = GetFirstElementHaving(code) ?? AddNew();

			using (attribute.GetValidationSuspender())
			using (attribute.SuspendSettingHasChanges())
			{
				attribute.CY_Code = code;
				attribute.TariffProfileQuestion = question;
				attribute.QuestionPathway = pathways?.FirstOrDefault(f => f.XQP_XQ2_QuestionChild == question.PK);
				attribute.SetTaxTypeAndLegalBase(profiles);
			}

			return attribute;
		}

		void RemoveNullProfileQuestions()
		{
			foreach (var attribute in this.Cast<AttributeCusCodeData>().Where(x => x.TariffProfileQuestion == null).ToArray())
			{
				RemoveAndDelete(attribute);
			}
		}

		public IEnumerable<AttributeCusCodeData> GetEffectiveAttributes(Func<AttributeCusCodeData, bool> predicate = null)
		{
			var listEffectiveAtt = this.Where(x => !x.CY_Data.IsEmpty && !x.IsEffectiveInFuture);
			if (predicate != null)
			{
				listEffectiveAtt = listEffectiveAtt.Where(predicate);
			}
			return listEffectiveAtt;
		}

		public void CopyDataFrom(AttributeCusCodeDataCollection collectionToClone)
		{
			foreach (AttributeCusCodeData att in collectionToClone.ToArray())
			{
				var attribute = GetFirstElementHaving(att.CY_Code);
				if (attribute != null)
				{
					using (attribute.GetValidationSuspender())
					using (attribute.SuspendSettingHasChanges())
					{
						attribute.CY_Data = att.CY_Data;
					}
				}
			}
		}
	}
}
