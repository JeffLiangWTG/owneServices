using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	static class CusReconEntryLineSnapshotMerger
	{
		public static DEMonthlyClosingEntryLineSnapshot DoMerge(DEMonthlyClosingEntryLineSnapshot lodged, DEMonthlyClosingEntryLineSnapshot current)
		{
			var result = lodged;
			MergeAdditionalProcedure();
			MergeAssessment();
			MergeBorderTransportMeans();
			MergeCessionManagementFlag();
			MergeCommodityCode();
			MergeDepartureCountry();
			MergeDocument();
			MergeExciseDuty();
			MergeForeignTradeFlag();
			MergeForeignTradeStatistics();
			MergeInwardMovementAmount();
			MergeNetMassMeasure();
			MergeOriginCountry();
			MergePreferentialCountry();
			MergePreferentialTreatment();
			MergePreferentialTreatmentDeclaration();
			MergeSupplementaryCodes();
			MergeSupplementaryInformation();
			MergeTobaccoRevenueStampNumber();
			UpdateLastUpdateTimeUtc();
			return result;

			void MergeAdditionalProcedure()
			{
				if (current.AdditionalProcedure != null)
				{
					result.AdditionalProcedure = current.AdditionalProcedure;
				}
			}

			void MergeAssessment()
			{
				if (current.Assessment != null)
				{
					if (result.Assessment == null)
					{
						result.Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment();
					}

					if (current.Assessment.Amount != null)
					{
						result.Assessment.Amount = current.Assessment.Amount;
					}

					if (current.Assessment.ContentInformation != null)
					{
						result.Assessment.ContentInformation = current.Assessment.ContentInformation;
					}

					if (current.Assessment.CustomsValueSpecified)
					{
						result.Assessment.CustomsValue = current.Assessment.CustomsValue;
						result.Assessment.CustomsValueSpecified = true;
					}

					if (current.Assessment.SpecificRate != null)
					{
						result.Assessment.SpecificRate = current.Assessment.SpecificRate;
					}
				}
			}

			void MergeBorderTransportMeans()
			{
				if (current.BorderTransportMeans != null)
				{
					if (result.BorderTransportMeans == null)
					{
						result.BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans();
					}

					if (current.BorderTransportMeans.Information != null)
					{
						result.BorderTransportMeans.Information = current.BorderTransportMeans.Information;
					}

					if (current.BorderTransportMeans.Mode != null)
					{
						result.BorderTransportMeans.Mode = current.BorderTransportMeans.Mode;
					}

					if (current.BorderTransportMeans.Nationality != null)
					{
						result.BorderTransportMeans.Nationality = current.BorderTransportMeans.Nationality;
					}

					if (current.BorderTransportMeans.Type != null)
					{
						result.BorderTransportMeans.Type = current.BorderTransportMeans.Type;
					}
				}
			}

			void MergeCessionManagementFlag()
			{
				if (current.CessionManagementFlag != null)
				{
					result.CessionManagementFlag = current.CessionManagementFlag;
				}
			}

			void MergeCommodityCode()
			{
				if (current.CommodityCode != null)
				{
					result.CommodityCode = current.CommodityCode;
				}
			}

			void MergeDepartureCountry()
			{
				if (current.DepartureCountry != null)
				{
					result.DepartureCountry = current.DepartureCountry;
				}
			}

			void MergeDocument()
			{
				if (current.Document != null)
				{
					result.Document = current.Document;
				}
			}

			void MergeExciseDuty()
			{
				if (current.ExciseDuty != null)
				{
					result.ExciseDuty = current.ExciseDuty;
				}
			}

			void MergeForeignTradeFlag()
			{
				if (current.ForeignTradeFlag != null)
				{
					result.ForeignTradeFlag = current.ForeignTradeFlag;
				}
			}

			void MergeForeignTradeStatistics()
			{
				if (current.ForeignTradeStatistics != null)
				{
					if (result.ForeignTradeStatistics == null)
					{
						result.ForeignTradeStatistics = new DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics();
					}

					if (current.ForeignTradeStatistics.GrossMassMeasureSpecified)
					{
						result.ForeignTradeStatistics.GrossMassMeasure = current.ForeignTradeStatistics.GrossMassMeasure;
						result.ForeignTradeStatistics.GrossMassMeasureSpecified = true;
					}

					if (current.ForeignTradeStatistics.InlandTransportMode != null)
					{
						result.ForeignTradeStatistics.InlandTransportMode = current.ForeignTradeStatistics.InlandTransportMode;
					}
				}
			}

			void MergeInwardMovementAmount()
			{
				if (current.InwardMovementAmount != null)
				{
					result.InwardMovementAmount = current.InwardMovementAmount;
				}
			}

			void MergeNetMassMeasure()
			{
				if (current.NetMassMeasureSpecified)
				{
					result.NetMassMeasure = current.NetMassMeasure;
					result.NetMassMeasureSpecified = true;
				}
			}

			void MergeOriginCountry()
			{
				if (current.OriginCountry != null)
				{
					result.OriginCountry = current.OriginCountry;
				}
			}

			void MergePreferentialCountry()
			{
				if (current.PreferentialCountry != null)
				{
					result.PreferentialCountry = current.PreferentialCountry;
				}
			}

			void MergePreferentialTreatment()
			{
				if (current.PreferentialTreatment != null)
				{
					if (result.PreferentialTreatment == null)
					{
						result.PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment();
					}

					if (current.PreferentialTreatment.RequestedPreferentialTreatment != null)
					{
						result.PreferentialTreatment.RequestedPreferentialTreatment = current.PreferentialTreatment.RequestedPreferentialTreatment;
					}
				}
			}

			void MergePreferentialTreatmentDeclaration()
			{
				if (current.PreferentialTreatment?.Declaration != null)
				{
					if (result.PreferentialTreatment.Declaration == null)
					{
						result.PreferentialTreatment.Declaration = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclaration();
					}

					if (current.PreferentialTreatment.Declaration.Contingent != null)
					{
						result.PreferentialTreatment.Declaration.Contingent = current.PreferentialTreatment.Declaration.Contingent;
					}

					if (current.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity != null)
					{
						result.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity = current.PreferentialTreatment.Declaration.PreferentialTreatmentQuantity;
					}
				}
			}

			void MergeSupplementaryCodes()
			{
				if (current.SupplementaryCodes != null)
				{
					result.SupplementaryCodes = current.SupplementaryCodes;
				}
			}

			void MergeSupplementaryInformation()
			{
				if (current.SupplementaryInformation != null)
				{
					result.SupplementaryInformation = current.SupplementaryInformation;
				}
			}

			void MergeTobaccoRevenueStampNumber()
			{
				if (current.TobaccoRevenueStampNumber != null)
				{
					result.TobaccoRevenueStampNumber = current.TobaccoRevenueStampNumber;
				}
			}

			void UpdateLastUpdateTimeUtc()
			{
				result.LastUpdateTimeUtc = ZDateTime.UtcNow.ToDateTime();
				result.LastUpdateTimeUtcSpecified = true;
			}
		}
	}
}
