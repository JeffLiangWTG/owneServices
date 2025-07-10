using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// A temporary class for use while transitioning from CMRStatisticalClassificationPeriodSnapshot to RefCusTariff
	/// </summary>
	public class ClassificationPeriodSnapshotWrapper
	{
		internal ClassificationPeriodSnapshotWrapper(CMRStatisticalClassificationPeriodSnapshot classificationBO)
		{
			this.classificationBO = classificationBO;
		}

		internal ClassificationPeriodSnapshotWrapper(TariffView tariffBO)
		{
			this.tariffBO = tariffBO;
		}

		readonly CMRStatisticalClassificationPeriodSnapshot classificationBO;
		readonly TariffView tariffBO;

		public static ClassificationPeriodSnapshotWrapper Load(BusinessObjectFactory factory, ZString tariffNumber, ZString statCode, ZDateTime effectiveDutyDate)
		{
			return factory.GetCachedValue($"ClassificationPeriodSnapshotWrapper|{tariffNumber}{statCode}{effectiveDutyDate}", () =>
			{
				ClassificationPeriodSnapshotWrapper wrapper = null;
				if (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.Value)
				{
					var unformattedNumber = new ZString(tariffNumber + statCode);
					var formattedNumber = unformattedNumber.KeepNumericCharacters();
					var dataGroupCode = AUCustomsDataRegistry.Instance.UseCMRTariffTestData.Value ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
					var tariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(dataGroupCode, TariffTypes.Import, formattedNumber, effectiveDutyDate);
					if (tariff != null)
					{
						wrapper = new ClassificationPeriodSnapshotWrapper(tariff);
					}
				}
				else
				{
					var classification = CMRStatisticalClassificationPeriodSnapshot.Load(factory, tariffNumber, statCode, effectiveDutyDate);
					if (classification != null)
					{
						wrapper = new ClassificationPeriodSnapshotWrapper(classification);
					}
				}
				return wrapper;
			});
		}

		public ZString QuantityUnit
		{
			get
			{
				var quantityUnit = ZString.Empty;
				if (tariffBO != null)
				{
					quantityUnit = tariffBO.ZZ1_ZZ8_UQ1;
				}
				else if (classificationBO != null)
				{
					quantityUnit = classificationBO.SC_QuantityUnit;
				}
				return quantityUnit;
			}
		}

		public ZString SecondQuantityUnit
		{
			get
			{
				var secondQuantityUnit = ZString.Empty;
				if (tariffBO != null)
				{
					secondQuantityUnit = tariffBO.ZZ1_ZZ8_UQ2;
				}
				else if (classificationBO != null)
				{
					secondQuantityUnit = classificationBO.SC_SecondQuantityUnit;
				}
				return secondQuantityUnit;
			}
		}
	}
}
