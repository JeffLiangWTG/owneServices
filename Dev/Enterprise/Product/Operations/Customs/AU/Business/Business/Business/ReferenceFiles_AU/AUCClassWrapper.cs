using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// A temporary class for use while transitioning from AUCClass to TariffView;
	/// Governed by UseCustomsReferenceData registry value.
	/// </summary>
	public class AUCClassWrapper : ITariffView
	{
		internal AUCClassWrapper(AUCClass aucClass)
		{
			this.aucClass = aucClass;
		}

		internal AUCClassWrapper(TariffView tariffView)
		{
			this.tariffView = tariffView;
		}

		readonly AUCClass aucClass;
		readonly TariffView tariffView;

		public static bool UseCustomsReferenceData => AUCustomsDataRegistry.Instance.UseCustomsReferenceData.Value;

		public static bool UseCMRTariffTestData => AUCustomsDataRegistry.Instance.UseCMRTariffTestData.Value;

		public static AUCClassWrapper Load(BusinessObjectFactory factory, ZString tariffCode, ZDateTime valuationDate)
		{
			AUCClassWrapper result = null;

			if (UseCustomsReferenceData)
			{
				var formattedCode = new AUImportTariffUniversalFormatter().Format(tariffCode).Trim();
				if (!formattedCode.IsEmpty)
				{
					var dataGroupCode = UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
					var tariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(dataGroupCode, TariffTypes.Import, formattedCode, valuationDate);
					if (tariff != null)
					{
						result = new AUCClassWrapper(tariff);
					}
				}
			}
			else
			{
				var formattedCode = new AUImportTariffFormatter().Format(tariffCode);
				if (!formattedCode.IsEmpty)
				{
					var tariff = AUCClass.GetClassForCompleteCode(factory, formattedCode);
					if (tariff != null)
					{
						result = new AUCClassWrapper(tariff);
					}
				}
			}

			return result;
		}

		public static AUCClassWrapper LoadPartialCode(BusinessObjectFactory factory, ZString tariffCode, ZDateTime valuationDate)
		{
			AUCClassWrapper result = null;

			if (UseCustomsReferenceData)
			{
				result = Load(factory, tariffCode, valuationDate);
			}
			else
			{
				var formattedCode = new AUImportTariffFormatter().Format(tariffCode);
				if (!formattedCode.IsEmpty)
				{
					var tariff = AUCClass.GetClassForPartialCode(factory, formattedCode);
					if (tariff != null)
					{
						result = new AUCClassWrapper(tariff);
					}
				}
			}

			return result;
		}

		#region Wrapped Properties

		public ZString ZZ1_TariffCode => tariffView?.ZZ1_TariffCode ?? aucClass?.UJ_Code ?? ZString.Empty;

		public ZString ZZ1_TariffCodeForDisplay
		{
			get
			{
				var result = ZString.Empty;

				if (tariffView != null)
				{
					result = new AUImportTariffUniversalFormatter().FormatDotted(tariffView.ZZ1_TariffCode);
				}
				else if (aucClass != null)
				{
					result = aucClass.UJ_Code;
				}

				return result;
			}
		}

		public ZString ZZ1_Description
		{
			get
			{
				var result = ZString.Empty;

				if (tariffView != null)
				{
					result = tariffView.ZZ1_Description.ToUpper();
				}
				else if (aucClass != null)
				{
					result = aucClass.ImportDescription;
				}

				return result;
			}
		}

		public ZString ZZ1_ZZ8_UQ1 => tariffView?.ZZ1_ZZ8_UQ1 ?? aucClass?.UJ_UQ1 ?? ZString.Empty;
		public ZString ZZ1_ZZ8_UQ2 => tariffView?.ZZ1_ZZ8_UQ2 ?? aucClass?.UJ_UQ2 ?? ZString.Empty;

		#endregion

		public bool HasChildren => aucClass?.HasChildren ?? false; // TariffView does not have children.

		public static IFetchHint GetTariffFetchHint(BusinessObjectFactory factory, ZString tariffCode)
		{
			tariffCode = tariffCode.KeepNumericCharacters();
			var filter = TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.Australia, Customs.Universal.Constants.TariffTypes.Import, tariffCode, ZDateTime.Today);
			return new ZQueryFetchHint(TariffViewSchema.Instance, filter);
		}
	}
}
