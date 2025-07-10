using System;
using System.Drawing;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class AirWaybillRegistry
	{
		public AirWaybillRegistry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}

		#region AWBCategory

		public bool AllowAutoCalculationOfTax
		{
			get { return (bool)RawRegistry.AllowAutoCalculationOfTax.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AllowAutoCalculationOfTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string ShipperAddressDefaultsTo
		{
			get { return (string)RawRegistry.ShipperAddressDefaultsTo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ShipperAddressDefaultsTo.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public string ConsigneeAddressDefaultsTo
		{
			get { return (string)RawRegistry.ConsigneeAddressDefaultsTo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.ConsigneeAddressDefaultsTo.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		#endregion

		#region HAWBCategory

		public string HAWBDimensionsDefault
		{
			get { return (string)RawRegistry.HAWBDimensionsDefault.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.HAWBDimensionsDefault.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		#region HAWBDotMatrixCategory

		public string HAWBPaperType
		{
			get { return (string)RawRegistry.HAWBPaperType.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.HAWBPaperType.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		#endregion

		#region HAWBLaser Category

		public AWBDocumentTitle GetHAWBDocumentTitles()
		{
			var xmlBytes = (byte[])RawRegistry.HAWBDocumentTitles.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			return new AWBDocumentTitle(xmlBytes);
		}

		public AWBDocumentTitle GetMAWBDocumentTitles()
		{
			var xmlBytes = (byte[])RawRegistry.MAWBDocumentTitles.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			return new AWBDocumentTitle(xmlBytes);
		}

		public Image HAWBLogo
		{
			get { return (Image)RawRegistry.HAWBLogo.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.HAWBLogo.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		public string PrintAsAgreedOnFirstSetHAWB
		{
			get { return (string)RawRegistry.PrintAsAgreedOnFirstSetHAWB.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.PrintAsAgreedOnFirstSetHAWB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PrintAsAgreedOnSecondSetHAWB
		{
			get { return (string)RawRegistry.PrintAsAgreedOnSecondSetHAWB.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.PrintAsAgreedOnSecondSetHAWB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PrintAsAgreedOnFirstSetMAWB
		{
			get { return (string)RawRegistry.PrintAsAgreedOnFirstSetMAWB.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.PrintAsAgreedOnFirstSetMAWB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PrintAsAgreedOnSecondSetMAWB
		{
			get { return (string)RawRegistry.PrintAsAgreedOnSecondSetMAWB.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.PrintAsAgreedOnSecondSetMAWB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool ShowChargeCodeForOtherChargesInHAWBScreen
		{
			get { return (bool)RawRegistry.ShowChargeCodeForOtherChargesInHAWBScreen.Value; }
			set { RawRegistry.ShowChargeCodeForOtherChargesInHAWBScreen.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool AllowShortGoodsDescriptionOverrideforFHL
		{
			get { return (bool)RawRegistry.AllowShortGoodsDescriptionOverrideforFHL.Value; }
			set { RawRegistry.AllowShortGoodsDescriptionOverrideforFHL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public string AirWaybillHAWBWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.AirWaybillHAWBWeightAndVolumeDisplay); }
		}

		public string HAWBDefaultShipperText
		{
			get { return (string)RawRegistry.HAWBDefaultShipperText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }

#if DEBUG
			set { RawRegistry.HAWBDefaultShipperText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string HAWBDefaultCarrierText
		{
			get { return (string)RawRegistry.HAWBDefaultCarrierText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }

#if DEBUG
			set { RawRegistry.HAWBDefaultCarrierText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region MAWBCategory

		public string MAWBDimensionsDefault
		{
			get { return (string)RawRegistry.MAWBDimensionsDefault.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MAWBDimensionsDefault.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		#region MAWBDotMatrixCategory

		public string MAWBPaperType
		{
			get { return (string)RawRegistry.MAWBPaperType.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
#if DEBUG
			set { RawRegistry.MAWBPaperType.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		#endregion

		#region MAWBIssuingCarrierAgentCategory

		public string IssuingCarrierAgentName
		{
			get { return (string)RawRegistry.IssuingCarrierAgentName.GetValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.IssuingCarrierAgentName.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public string IssuingCarrierAgentCity
		{
			get { return (string)RawRegistry.IssuingCarrierAgentCity.GetValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.IssuingCarrierAgentCity.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public string IssuingCarrierAgentIATACode
		{
			get { return (string)RawRegistry.IssuingCarrierAgentIATACode.GetValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.IssuingCarrierAgentIATACode.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		public string IssuingCarrierAgentAccountNumber
		{
			get { return (string)RawRegistry.IssuingCarrierAgentAccountNumber.GetValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.IssuingCarrierAgentAccountNumber.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, value); }
#endif
		}

		#endregion

		public bool AllowAsAgreed
		{
			get { return (bool)RawRegistry.AllowAsAgreed.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AllowAsAgreed.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool SelectFHLByDefault
		{
			get { return (bool)RawRegistry.SelectFHLByDefault.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty); }
#if DEBUG
			set { RawRegistry.SelectFHLByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string AirWaybillMAWBWeightAndVolumeDisplay
		{
			get { return GetWeightAndVolumeDisplayType(RawRegistry.AirWaybillMAWBWeightAndVolumeDisplay); }
		}
		public string MAWBDefaultShipperText
		{
			get { return (string)RawRegistry.MAWBDefaultShipperText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }

#if DEBUG
			set { RawRegistry.MAWBDefaultShipperText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string MAWBDefaultCarrierText
		{
			get { return (string)RawRegistry.MAWBDefaultCarrierText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }

#if DEBUG
			set { RawRegistry.MAWBDefaultCarrierText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		string GetWeightAndVolumeDisplayType(IRegistryItem item)
		{
			string value = (string)item.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			if (string.IsNullOrEmpty(value))
			{
				value = item.DefaultValue.ToString();
			}
			return value;
		}

		readonly RawDataRegistry RawRegistry;
	}
}
