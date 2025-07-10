using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Argument = CargoWise.Common.Argument;
using MessageBuilderExtensions = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	sealed class CusReconEntryLineSnapshotBuilder : SnapshotBuilder<DEMonthlyClosingEntryLineSnapshot>
	{
		public CusReconEntryLineSnapshotBuilder(IMonthlyClosingEntryLineSnapshot provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
		}
		readonly IMonthlyClosingEntryLineSnapshot provider;

		public override DEMonthlyClosingEntryLineSnapshot GenerateMessage()
		{
			var entryLineSnapshot = new DEMonthlyClosingEntryLineSnapshot();
			entryLineSnapshot.CessionManagementFlag = provider.CessionManagementFlag;
			PopulateNetMassMeasure(entryLineSnapshot);
			entryLineSnapshot.TobaccoRevenueStampNumber = provider.TobaccoRevenueStampNumber;
			entryLineSnapshot.OriginCountry = provider.OriginCountry;
			entryLineSnapshot.PreferentialCountry = provider.PreferentialCountry;
			entryLineSnapshot.DepartureCountry = provider.DepartureCountry;
			entryLineSnapshot.SupplementaryInformation = provider.SupplementaryInformation;
			entryLineSnapshot.CommodityCode = provider.CommodityCode;
			entryLineSnapshot.AdditionalProcedure = PopulateAdditionalProcedures();
			entryLineSnapshot.SupplementaryCodes = PopulateSupplementaryCodes();
			entryLineSnapshot.ForeignTradeStatistics = PopulateForeignTradeStatistics();
			entryLineSnapshot.ForeignTradeFlag = provider.ForeignTradeFlag;
			entryLineSnapshot.InwardMovementAmount = MessageBuilderExtensions.CreateCommonAmount<Amount>(provider.InwardMovementAmount);
			entryLineSnapshot.Assessment = PopulateAssessment();
			entryLineSnapshot.ExciseDuty = PopulateExciseDuties();
			entryLineSnapshot.PreferentialTreatment = PopulatePreferentialTreatment();
			entryLineSnapshot.Document = PopulateDocuments();
			entryLineSnapshot.BorderTransportMeans = PopulateBorderTransportMeans();
			entryLineSnapshot.LastUpdateTimeUtc = ZDateTime.UtcNow.ToDateTime();
			entryLineSnapshot.LastUpdateTimeUtcSpecified = true;
			return entryLineSnapshot;
		}

		void PopulateNetMassMeasure(DEMonthlyClosingEntryLineSnapshot entryLineSnapshot)
		{
			var netMassMeasure = provider.NetMassMeasure;
			entryLineSnapshot.NetMassMeasure = provider.GetNetMassMeasure();
			entryLineSnapshot.NetMassMeasureSpecified = !netMassMeasure.IsZero();
		}

		DEMonthlyClosingEntryLineSnapshotAdditionalProcedure[] PopulateAdditionalProcedures()
		{
			return provider.AdditionalProcedure.Select(x => new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure() { Code = x }).ToArray();
		}

		DEMonthlyClosingEntryLineSnapshotSupplementaryCodes[] PopulateSupplementaryCodes()
		{
			return provider.SupplementaryCodes.Select(x => new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes() { Code = x }).ToArray();
		}

		DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics PopulateForeignTradeStatistics()
		{
			var grossMassMeasure = provider.ForeignTradeStatisticsGrossMassMeasure;
			return new DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics()
			{
				InlandTransportMode = provider.ForeignTradeStatisticsInlandTransportMode,
				GrossMassMeasure = provider.GetForeignTradeStatisticsGrossMassMeasure(),
				GrossMassMeasureSpecified = !grossMassMeasure.IsZero(),
			};
		}

		DEMonthlyClosingEntryLineSnapshotAssessment PopulateAssessment()
		{
			var customsValue = provider.AssessmentCustomsValue;
			return new DEMonthlyClosingEntryLineSnapshotAssessment()
			{
				CustomsValue = provider.GetAssessmentCustomsValue(),
				CustomsValueSpecified = !customsValue.IsZero(),
				Amount = provider.AssessmentAmount.Select(MessageBuilderExtensions.CreateCommonAmount<Amount>).ToArray(),
				SpecificRate = provider.AssessmentSpecificRate.Select(x => PopulateAssessmentSpecificRate(x)).ToArray(),
				ContentInformation = provider.AssessmentContentInformation.Select(c => PopulateAssessmentContentInformation(c)).ToArray()
			};

			DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate PopulateAssessmentSpecificRate(IImportSpecificRate rate)
			{
				return new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate
				{
					Type = rate.Type,
					Value = rate.GetImportSpecificRateValue()
				};
			}

			DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation PopulateAssessmentContentInformation(IContentInformation contentInformation)
			{
				return new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation
				{
					Type = contentInformation.ContentType,
					DegreePercentage = contentInformation.GetContentInformationDegreePercentage()
				};
			}
		}

		DEMonthlyClosingEntryLineSnapshotExciseDuty[] PopulateExciseDuties()
		{
			return provider.ExciseDuty.Select(d => PopulateExciseDuty(d)).ToArray();

			DEMonthlyClosingEntryLineSnapshotExciseDuty PopulateExciseDuty(IExciseDuty exciseDuty)
			{
				var degreePercentage = exciseDuty.DegreePercentage;
				var value = exciseDuty.Value;
				return new DEMonthlyClosingEntryLineSnapshotExciseDuty
				{
					Code = exciseDuty.Code,
					DegreePercentage = exciseDuty.GetExciseDutyDegreePercentage(),
					DegreePercentageSpecified = !degreePercentage.IsZero(),
					Value = exciseDuty.GetExciseDutyValue(),
					ValueSpecified = !value.IsZero(),
					Amount = MessageBuilderExtensions.CreateCommonAmount<Amount>(exciseDuty.Amount)
				};
			}
		}

		DEMonthlyClosingEntryLineSnapshotPreferentialTreatment PopulatePreferentialTreatment()
		{
			var preferentialTreatment = provider.PreferentialTreatment;
			return preferentialTreatment != null ? new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment
			{
				RequestedPreferentialTreatment = preferentialTreatment.RequestedPreferentialTreatment,
				Declaration = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclaration
				{
					Contingent = preferentialTreatment.ContingentNumber.Select(cn => PopulateContingentNumber(cn)).ToArray(),
					PreferentialTreatmentQuantity = PopulatePreferentialTreatmentQuantity(preferentialTreatment.Quantity),
				}
			} : null;

			DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent PopulateContingentNumber(string contingentNumber)
			{
				return new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent
				{
					ContingentNumber = contingentNumber
				};
			}

			DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationPreferentialTreatmentQuantity PopulatePreferentialTreatmentQuantity(IAmount amount)
			{
				return amount != null ? MessageBuilderExtensions.CreateCommonAmount<DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationPreferentialTreatmentQuantity>(amount) : null;
			}
		}

		DEMonthlyClosingEntryLineSnapshotDocument[] PopulateDocuments()
		{
			return provider.Documents.Select(d => PopulateDocument(d)).ToArray();

			DEMonthlyClosingEntryLineSnapshotDocument PopulateDocument(IImportLineDocument document)
			{
				var issuingDate = document.IssuingDate;
				return new DEMonthlyClosingEntryLineSnapshotDocument
				{
					Division = document.Division.MapCodeToEnumWithItemPrefix<DEMonthlyClosingEntryLineSnapshotDocumentDivision>().EnumValue,
					Type = document.DocumentType,
					ReferenceNumber = document.ReferenceNumber,
					IssuingDate = issuingDate.GetValueOrDefault(),
					IssuingDateSpecified = issuingDate.HasValue,
					AtHandFlag = document.AtHandFlag,
					WriteOff = MessageBuilderExtensions.CreateCommonAmount<Amount>(document.WriteOff)
				};
			}
		}

		DEMonthlyClosingEntryLineSnapshotBorderTransportMeans PopulateBorderTransportMeans()
		{
			return new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans
			{
				Mode = provider.BorderTransportMeansMode,
				Type = provider.BorderTransportMeansType,
				Information = provider.BorderTransportMeansInformation,
				Nationality = provider.BorderTransportMeansNationality
			};
		}
	}
}
