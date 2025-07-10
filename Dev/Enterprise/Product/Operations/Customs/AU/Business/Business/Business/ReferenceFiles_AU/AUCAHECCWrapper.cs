using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using TariffTypes = Enterprise.Customs.Universal.Constants.TariffTypes;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// A temporary class for use while transitioning from AUCAHECC to TariffView;
	/// Governed by UseCustomsReferenceData registry value.
	/// </summary>
	public class AUCAHECCWrapper : ITariffView
	{
		internal AUCAHECCWrapper(AUCAHECC ahecc)
		{
			this.ahecc = ahecc;
		}

		internal AUCAHECCWrapper(TariffView tariffView)
		{
			this.tariffView = tariffView;
		}

		readonly AUCAHECC ahecc;
		readonly TariffView tariffView;

		public static bool EnableCWRefForAHECC => AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.Value;

		public static bool UseCMRTariffTestData => AUCustomsDataRegistry.Instance.UseCMRTariffTestData.Value;

		public static AUCAHECCWrapper Load(BusinessObjectFactory factory, ZString tariffCode, ZDateTime valuationDate)
		{
			AUCAHECCWrapper result = null;

			if (EnableCWRefForAHECC)
			{
				var formattedCode = new AUExportTariffUniversalFormatter().Format(tariffCode).Trim();
				if (!formattedCode.IsEmpty)
				{
					var dataGroupCode = UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
					var exportTariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(dataGroupCode, TariffTypes.Export, formattedCode, valuationDate);
					if (exportTariff != null)
					{
						result = new AUCAHECCWrapper(exportTariff);
					}
				}
			}
			else
			{
				var formattedCode = new AUExportTariffFormatter().Format(tariffCode);
				if (!formattedCode.IsEmpty)
				{
					var exportTariff = factory.LoadFromNaturalKey<AUCAHECC>(AUCAHECCSchema.UA_AHECC, formattedCode);
					if (exportTariff != null)
					{
						result = new AUCAHECCWrapper(exportTariff);
					}
				}
			}

			return result;
		}

		public static IFetchHint GetTariffFetchHint(BusinessObjectFactory factory, ZString tariffCode)
		{
			tariffCode = tariffCode.KeepNumericCharacters();
			var filter = TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.Australia, TariffTypes.Export, tariffCode, ZDateTime.Today);
			return new ZQueryFetchHint(TariffViewSchema.Instance, filter);
		}

		#region Wrapped TariffView Properties

		public ZString ZZ1_TariffCode => tariffView?.ZZ1_TariffCode ?? ahecc?.UA_AHECC ?? ZString.Empty;

		public ZString ZZ1_TariffCodeForDisplay
		{
			get
			{
				var result = ZString.Empty;

				if (tariffView != null)
				{
					result = new AUExportTariffUniversalFormatter().FormatDotted(tariffView.ZZ1_TariffCode);
				}
				else if (ahecc != null)
				{
					result = ahecc.UA_AHECC;
				}

				return result;
			}
		}

		public ZString ZZ1_ZZ8_UQ1 => tariffView?.ZZ1_ZZ8_UQ1 ?? ahecc?.UA_UQ ?? ZString.Empty;
		public ZString ZZ1_ZZ8_UQ2 => tariffView?.ZZ1_ZZ8_UQ2 ?? ZString.Empty;

		public ZString ZZ1_Description
		{
			get
			{
				var result = ZString.Empty;

				if (tariffView != null)
				{
					result = tariffView.ZZ1_Description;
				}
				else if (ahecc != null)
				{
					result = ahecc.UA_LongDescription;
					if (result.IsEmpty)
					{
						result = ahecc.UA_ShortDescription;
					}
				}

				return result;
			}
		}

		#endregion

		public bool HasChildren => ahecc?.HasChildren ?? false; // TariffView does not have children
	}
}
