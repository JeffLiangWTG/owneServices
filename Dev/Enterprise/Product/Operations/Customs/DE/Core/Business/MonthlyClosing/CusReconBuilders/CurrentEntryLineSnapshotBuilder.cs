using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using Argument = CargoWise.Common.Argument;
using MessageBuilderExtensions = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	sealed class CurrentEntryLineSnapshotBuilder : SnapshotBuilder<DEMonthlyClosingEntryLineSnapshot>
	{
		public CurrentEntryLineSnapshotBuilder(IMonthlyClosingEntryLineSnapshot provider, DEMonthlyClosingEntryLineSnapshot basicSnapshot)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			this.basicSnapshot = Argument.NotNull(basicSnapshot, nameof(basicSnapshot));
		}
		readonly IMonthlyClosingEntryLineSnapshot provider;
		readonly DEMonthlyClosingEntryLineSnapshot basicSnapshot;

		public static ImmutableHashSet<string> CessionManagementFlagList
		{
			get { return cessionManagementFlagList ?? (cessionManagementFlagList = new[] { "01", "02", "03" }.ToImmutableHashSet()); }
		}
		[ThreadStatic]
		static ImmutableHashSet<string> cessionManagementFlagList;

		public override DEMonthlyClosingEntryLineSnapshot GenerateMessage()
		{
			var newCurrentEntryLineSnapshotObject = new DEMonthlyClosingEntryLineSnapshot();
			UpdateDeMonthlyClosingEntryLineSnapshot(newCurrentEntryLineSnapshotObject);
			UpdateAdditionalProceduce(newCurrentEntryLineSnapshotObject);
			UpdateSupplementaryCodes(newCurrentEntryLineSnapshotObject);
			UpdateForeignTradeStatics(newCurrentEntryLineSnapshotObject);
			UpdateInwardMovementAmount(newCurrentEntryLineSnapshotObject);
			UpdateAssessment(newCurrentEntryLineSnapshotObject);
			UpdateExciseDuty(newCurrentEntryLineSnapshotObject);
			UpdatePreferentialTreatment(newCurrentEntryLineSnapshotObject);
			UpdateDocument(newCurrentEntryLineSnapshotObject);
			UpdateBorderTransportMeans(newCurrentEntryLineSnapshotObject);
			return newCurrentEntryLineSnapshotObject;
		}

		void UpdateDeMonthlyClosingEntryLineSnapshot(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			snapshot.CessionManagementFlag = ReturnFirstWhenNotEqual(provider.CessionManagementFlag, basicSnapshot.CessionManagementFlag);

			if (string.IsNullOrEmpty(provider.CessionManagementFlag) && CessionManagementFlagList.Contains(basicSnapshot.CessionManagementFlag))
			{
				snapshot.CommodityCode = provider.CommodityCode;
			}
			else
			{
				var netMassMeasureIsZero = provider.NetMassMeasure.IsZero();
				var netMassMeasure = provider.GetNetMassMeasure();

				if (!netMassMeasureIsZero && netMassMeasure != basicSnapshot.NetMassMeasure)
				{
					snapshot.NetMassMeasure = netMassMeasure;
					snapshot.NetMassMeasureSpecified = true;
				}
			}
			snapshot.OriginCountry = ReturnFirstWhenNotEqual(provider.OriginCountry, basicSnapshot.OriginCountry);
			snapshot.PreferentialCountry = ReturnFirstWhenNotEqual(provider.PreferentialCountry, basicSnapshot.PreferentialCountry);
			snapshot.DepartureCountry = ReturnFirstWhenNotEqual(provider.DepartureCountry, basicSnapshot.DepartureCountry);
			snapshot.SupplementaryInformation = ReturnFirstWhenNotEqual(provider.SupplementaryInformation, basicSnapshot.SupplementaryInformation);
			snapshot.ForeignTradeFlag = ReturnFirstWhenNotEqual(provider.ForeignTradeFlag, basicSnapshot.ForeignTradeFlag);

			if (provider.CommodityCode != basicSnapshot.CommodityCode)
			{
				snapshot.CommodityCode = provider.CommodityCode;
				snapshot.CessionManagementFlag = provider.CessionManagementFlag;
			}
		}

		void UpdateAdditionalProceduce(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var result = provider.AdditionalProcedure.Select(p => new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure() { Code = p }).ToArray();

			var additionalProcedureIsSame =
				basicSnapshot.AdditionalProcedure?.EqualIgnoringOrder(result, new DEMonthlyClosingEntryLineSnapshotAdditionalProcedureComparer()) ?? result.Length == 0;

			if (provider.CommodityCode != basicSnapshot.CommodityCode || !additionalProcedureIsSame)
			{
				snapshot.CommodityCode = provider.CommodityCode;
				snapshot.AdditionalProcedure = result;
			}
		}

		void UpdateSupplementaryCodes(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var result = provider.SupplementaryCodes.Select(x => new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes() { Code = x }).ToArray();

			var supplementaryCodesIsSame =
				basicSnapshot.SupplementaryCodes?.EqualIgnoringOrder(result, new DEMonthlyClosingEntryLineSnapshotSupplementaryCodesComparer()) ?? result.Length == 0;

			if (provider.CommodityCode != basicSnapshot.CommodityCode || !supplementaryCodesIsSame)
			{
				snapshot.CommodityCode = provider.CommodityCode;
				snapshot.SupplementaryCodes = result;
			}
		}

		void UpdateForeignTradeStatics(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var foreignTradeStatisticsGrossMassMeasure = provider.GetForeignTradeStatisticsGrossMassMeasure();

			var transportModeDifferent = provider.ForeignTradeStatisticsInlandTransportMode != basicSnapshot.ForeignTradeStatistics?.InlandTransportMode;
			var grossMassMeasureDifferent = !provider.ForeignTradeStatisticsGrossMassMeasure.IsZero()
				 && foreignTradeStatisticsGrossMassMeasure != basicSnapshot.ForeignTradeStatistics?.GrossMassMeasure;

			if (transportModeDifferent || grossMassMeasureDifferent)
			{
				if (snapshot.ForeignTradeStatistics == null)
				{
					snapshot.ForeignTradeStatistics = new DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics();
				}
				if (transportModeDifferent)
				{
					snapshot.ForeignTradeStatistics.InlandTransportMode = provider.ForeignTradeStatisticsInlandTransportMode;
				}
				if (grossMassMeasureDifferent)
				{
					snapshot.ForeignTradeStatistics.GrossMassMeasure = foreignTradeStatisticsGrossMassMeasure;
					snapshot.ForeignTradeStatistics.GrossMassMeasureSpecified = true;
				}
			}
		}

		void UpdateInwardMovementAmount(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			if (provider.InwardMovementAmount == null)
			{
				return;
			}

			var result = MessageBuilderExtensions.CreateCommonAmount<Amount>(provider.InwardMovementAmount);

			var quantityDifferent = basicSnapshot.InwardMovementAmount == null || basicSnapshot.InwardMovementAmount.Quantity != result.Quantity;
			var measurementUnitDifferent = basicSnapshot.InwardMovementAmount?.MeasurementUnit != result.MeasurementUnit;
			var qualifierDifferent = basicSnapshot.InwardMovementAmount?.Qualifier != result.Qualifier;

			if (quantityDifferent || measurementUnitDifferent || qualifierDifferent)
			{
				snapshot.InwardMovementAmount = result;
			}
		}

		void UpdateAssessment(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			UpdateCustomsValue(snapshot);
			UpdateAmount(snapshot);
			UpdateSpecificRate(snapshot);
			UpdateContentInformation(snapshot);
		}

		void UpdateCustomsValue(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var assessmentCustomsValueIsEmpty = provider.AssessmentCustomsValue.IsZero();
			var assessmentCustomsValue = provider.GetAssessmentCustomsValue();

			if (basicSnapshot.Assessment == null
				|| (!assessmentCustomsValueIsEmpty && basicSnapshot.Assessment.CustomsValue != assessmentCustomsValue))
			{
				MakeSureAssessmentIsNotNull(snapshot);
				snapshot.Assessment.CustomsValue = assessmentCustomsValue;
				snapshot.Assessment.CustomsValueSpecified = true;
			}
		}

		void UpdateAmount(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var result = provider.AssessmentAmount.Select(x => MessageBuilderExtensions.CreateCommonAmount<Amount>(x)).ToArray();

			var amountIsSame = basicSnapshot.Assessment?.Amount?.EqualIgnoringOrder(result, new AmountComparer()) ?? result.Length == 0;

			if (!amountIsSame)
			{
				MakeSureAssessmentIsNotNull(snapshot);
				snapshot.Assessment.Amount = result;
			}
		}

		void UpdateSpecificRate(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var result = provider.AssessmentSpecificRate.Select(x => new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate
			{
				Type = x.Type,
				Value = x.GetImportSpecificRateValue()
			}).ToArray();

			var specificRateIsSame =
				basicSnapshot.Assessment?.SpecificRate?.EqualIgnoringOrder(result, new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparer()) ?? result.Length == 0;

			if (!specificRateIsSame)
			{
				MakeSureAssessmentIsNotNull(snapshot);
				snapshot.Assessment.SpecificRate = result;
			}
		}

		void UpdateContentInformation(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var result = provider.AssessmentContentInformation.Select(x => new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation
			{
				DegreePercentage = x.GetContentInformationDegreePercentage(),
				Type = x.ContentType
			}).ToArray();

			var contentInformationIsSame =
				basicSnapshot.Assessment?.ContentInformation?.EqualIgnoringOrder(result, new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparer()) ?? result.Length == 0;

			if (!contentInformationIsSame)
			{
				MakeSureAssessmentIsNotNull(snapshot);
				snapshot.Assessment.ContentInformation = result;
			}
		}

		void UpdateExciseDuty(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var result = provider.ExciseDuty.Select(x => new DEMonthlyClosingEntryLineSnapshotExciseDuty()
			{
				Amount = MessageBuilderExtensions.CreateCommonAmount<Amount>(x.Amount),
				Code = x.Code,
				DegreePercentage = x.GetExciseDutyDegreePercentage(),
				DegreePercentageSpecified = !x.DegreePercentage.IsZero(),
				Value = x.GetExciseDutyValue(),
				ValueSpecified = !x.Value.IsZero(),
			}).ToArray();

			var exciseDutyIsSame = basicSnapshot.ExciseDuty?.EqualIgnoringOrder(result, new DEMonthlyClosingEntryLineSnapshotExciseDutyComparer()) ?? result.Length == 0;

			if (!exciseDutyIsSame)
			{
				snapshot.ExciseDuty = result;
			}
		}

		void UpdatePreferentialTreatment(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			UpdateRequestedPreferentialTreatment(snapshot);
			if (!UpdateDeclarationContingent(snapshot))
			{
				UpdateDeclarationPreferentialTreatmentQuantityQuantity(snapshot);
			}
		}

		void UpdateRequestedPreferentialTreatment(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var providerRequestedPreferentialTreatment = provider.PreferentialTreatment?.RequestedPreferentialTreatment;
			if (providerRequestedPreferentialTreatment != null && providerRequestedPreferentialTreatment != basicSnapshot.PreferentialTreatment?.RequestedPreferentialTreatment)
			{
				if (snapshot.PreferentialTreatment == null)
				{
					snapshot.PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment();
				}
				snapshot.PreferentialTreatment.RequestedPreferentialTreatment = provider.PreferentialTreatment.RequestedPreferentialTreatment;
			}
		}

		bool UpdateDeclarationContingent(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var result = provider.PreferentialTreatment?.ContingentNumber.Select(x => new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent()
			{
				ContingentNumber = x
			}).ToArray() ?? Array.Empty<DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent>();

			var contingentIsSame = basicSnapshot.PreferentialTreatment?.Declaration?.Contingent?.EqualIgnoringOrder(result, new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingentComparer()) ?? result.Length == 0;

			if (!contingentIsSame)
			{
				if (snapshot.PreferentialTreatment == null)
				{
					snapshot.PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment();
				}
				if (snapshot.PreferentialTreatment.Declaration == null)
				{
					snapshot.PreferentialTreatment.Declaration = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclaration();
				}
				snapshot.PreferentialTreatment.Declaration.Contingent = result;
				UpdateDeclarationPreferentialTreatmentQuantityQuantity(snapshot, true);
				return true;
			}
			return false;
		}

		void UpdateDeclarationPreferentialTreatmentQuantityQuantity(DEMonthlyClosingEntryLineSnapshot snapshot, bool isSkipCheck = false)
		{
			var result = MessageBuilderExtensions.CreateCommonAmount<DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationPreferentialTreatmentQuantity>(provider.PreferentialTreatment?.Quantity);
			if (result == null)
			{
				return;
			}

			if (isSkipCheck
				|| basicSnapshot.PreferentialTreatment?.Declaration?.PreferentialTreatmentQuantity == null
				|| result.Quantity != basicSnapshot.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Quantity
				|| result.MeasurementUnit != basicSnapshot.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.MeasurementUnit
				|| result.Qualifier != basicSnapshot.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity.Qualifier)
			{
				if (snapshot.PreferentialTreatment == null)
				{
					snapshot.PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment();
				}
				if (snapshot.PreferentialTreatment.Declaration == null)
				{
					snapshot.PreferentialTreatment.Declaration = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclaration();
				}
				snapshot.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity = result;
			}
		}

		void UpdateDocument(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var result = provider.Documents.Select(x => new DEMonthlyClosingEntryLineSnapshotDocument()
			{
				Division = x.Division.MapCodeToEnumWithItemPrefix<DEMonthlyClosingEntryLineSnapshotDocumentDivision>().EnumValue,
				Type = x.DocumentType,
				ReferenceNumber = x.ReferenceNumber,
				IssuingDate = x.GetImportLineDocumentIssuingDate().GetValueOrDefault(),
				IssuingDateSpecified = x.IssuingDate.HasValue,
				AtHandFlag = x.AtHandFlag,
				WriteOff = MessageBuilderExtensions.CreateCommonAmount<Amount>(x.WriteOff)
			}).ToArray();

			var documentIsSame = basicSnapshot.Document?.EqualIgnoringOrder(result, new DEMonthlyClosingEntryLineSnapshotDocumentComparer()) ?? result.Length == 0;

			if (!documentIsSame)
			{
				snapshot.Document = result;
			}
		}

		void UpdateBorderTransportMeans(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			var modeDifferent = basicSnapshot.BorderTransportMeans?.Mode != provider.BorderTransportMeansMode;
			var typeDifferent = basicSnapshot.BorderTransportMeans?.Type != provider.BorderTransportMeansType;
			var nationalityDifferent = basicSnapshot.BorderTransportMeans?.Nationality != provider.BorderTransportMeansNationality;
			var informationDifferent = basicSnapshot.BorderTransportMeans?.Information != provider.BorderTransportMeansInformation;

			if (modeDifferent || typeDifferent || nationalityDifferent || informationDifferent)
			{
				if (snapshot.BorderTransportMeans == null)
				{
					snapshot.BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans();
				}
				if (modeDifferent)
				{
					snapshot.BorderTransportMeans.Mode = provider.BorderTransportMeansMode;
				}
				if (typeDifferent)
				{
					snapshot.BorderTransportMeans.Type = provider.BorderTransportMeansType;
				}
				if (nationalityDifferent)
				{
					snapshot.BorderTransportMeans.Nationality = provider.BorderTransportMeansNationality;
				}
				if (informationDifferent)
				{
					snapshot.BorderTransportMeans.Information = provider.BorderTransportMeansInformation;
				}
			}
		}

		static void MakeSureAssessmentIsNotNull(DEMonthlyClosingEntryLineSnapshot snapshot)
		{
			if (snapshot.Assessment == null)
			{
				snapshot.Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment();
			}
		}

		static string ReturnFirstWhenNotEqual(string first, string second)
		{
			return first == second ? null : first;
		}
	}
}
