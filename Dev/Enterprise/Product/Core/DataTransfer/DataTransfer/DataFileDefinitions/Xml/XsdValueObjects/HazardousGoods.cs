using System;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoHazardousGoods")]
	public class HazardousGoods : Xsd.AutoHazardousGoods
	{
		/// <summary>
		/// This method is legacy and used where XSDs contain both a collection of DGs, 
		/// and a single DG (for backwards compatibility)
		/// </summary>
		/// <param name="uNDGs"></param>
		public void ExportSingleItemFromUNDGDataItems(UNDGDataItem[] uNDGs)
		{
			IsSpecified = false;
			foreach (UNDGDataItem dgItem in uNDGs)
			{
				if (dgItem.Substance != null)
				{
					IsSpecified = true;

					UNDGCode = dgItem.Substance.DG_Code;
					Standard = dgItem.Substance.DG_Standard;
					FlashPoint = dgItem.DI_DGFlashPoint.ToString();
					IMOClass = dgItem.Substance.DG_Class;
					ProperShippingName = dgItem.Substance.DG_PSN;
					MarinePollutant = dgItem.DI_MPMarinePollutant.IsEmpty ? dgItem.Substance.DG_MP : dgItem.DI_MPMarinePollutant;
					TechnicalName = dgItem.DI_TechnicalName;
					PackingGroup = dgItem.Substance.DG_PG;
					Weight.IsSpecified = true;
					Volume.IsSpecified = true;
					PackedInLimitedQuantity = dgItem.DI_IsLimitedQuantity;

					break;
				}
			}
		}

		public bool ImportSingleItemToUNDGDataItems(Func<UNDGDataItemCollection> uNDGsFunc)
		{
			if (!UNDGCode.IsEmpty)
			{
				var uNDGs = uNDGsFunc();
				var dgItem = uNDGs.TryGetOrCreate(UNDGCode, Standard);
				if (dgItem == null)
				{
					return false;
				}

				decimal flashPoint;
				if (decimal.TryParse(FlashPoint, out flashPoint))
				{
					dgItem.DI_DGFlashPoint = flashPoint;
				}

				if (!dgItem.DI_TechnicalName_ReadOnly && TechnicalNameSpecified)
				{
					dgItem.DI_TechnicalName = TechnicalName.Left(dgItem.DI_TechnicalNameInfo.MaxLength);
				}

				if (!dgItem.DI_MPMarinePollutant_ReadOnly && MarinePollutantSpecified)
				{
					dgItem.DI_MPMarinePollutant = ((MarinePollutant == UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code) ? ZString.Empty : MarinePollutant).Left(dgItem.DI_MPMarinePollutantInfo.MaxLength);
				}
			}
			return !UNDGCode.IsEmpty;
		}
	}
}
