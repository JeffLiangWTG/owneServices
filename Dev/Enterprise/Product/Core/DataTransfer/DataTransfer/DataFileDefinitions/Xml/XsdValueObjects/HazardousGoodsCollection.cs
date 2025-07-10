using System;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class HazardousGoodsCollection : Xsd.AutoHazardousGoodsCollection
	{
		public void ExportFromUNDGDataItems(UNDGDataItem[] dGItems, string errorContext, IValueObjectExportContext exportContext)
		{
			foreach (UNDGDataItem dg in dGItems)
			{
				if (dg.Substance != null)
				{
					Xsd.HazardousGoods xsdDG = AddNew();
					xsdDG.UNDGCode = dg.Substance.DG_Code;
					xsdDG.Standard = dg.Substance.DG_Standard;
					xsdDG.IMOClass = dg.Substance.DG_Class;
					xsdDG.ProperShippingName = dg.Substance.DG_PSN;
					xsdDG.FlashPoint = dg.DI_DGFlashPoint.ToString();
					xsdDG.Contact = new ContactValueObjectHelper(errorContext).ToContactReference(dg.DGContact, exportContext);
					xsdDG.MarinePollutant = dg.DI_MPMarinePollutant.IsEmpty ? dg.Substance.DG_MP : dg.DI_MPMarinePollutant;
					xsdDG.PackingGroup = dg.Substance.DG_PG;
					xsdDG.TechnicalName = dg.DI_TechnicalName;

					xsdDG.Weight.Value = dg.DI_DGWeight;
					xsdDG.Weight.DimensionType = dg.DI_UnitOfWeight;

					xsdDG.Volume.Value = dg.DI_DGVolume;
					xsdDG.Volume.DimensionType = dg.DI_UnitOfVolume;

					if (dg.DI_IsLimitedQuantity)
					{
						xsdDG.PackedInLimitedQuantity = true;
						xsdDG.PackedInLimitedQuantitySpecified = true;
					}
				}
			}
		}

		public bool ImportToUNDGDataItems(Func<UNDGDataItemCollection> dGsFunc, string errorContext, IValueObjectImportContext importContext)
		{
			if (this.Count > 0)
			{
				var dGs = dGsFunc();
				foreach (Xsd.HazardousGoods xsdHaz in this)
				{
					var dgItem = dGs.TryGetOrCreate(xsdHaz.UNDGCode, xsdHaz.Standard);
					if (dgItem != null)
					{
						decimal flashPoint;
						if (decimal.TryParse(xsdHaz.FlashPoint, out flashPoint))
						{
							dgItem.DI_DGFlashPoint = flashPoint;
						}

						var contact = new ContactValueObjectHelper(errorContext).FromContactReference(xsdHaz.Contact, importContext);
						if (contact != null)
						{
							dgItem.DI_OC_DGContact = contact.PK;
						}

						if (xsdHaz.Weight.IsSpecified)
						{
							dgItem.DI_DGWeight = xsdHaz.Weight.Value;
							importContext.SetPropertyInfoValue(dgItem.DI_UnitOfWeightInfo, xsdHaz.Weight.DimensionType);
						}

						if (xsdHaz.Volume.IsSpecified)
						{
							dgItem.DI_DGVolume = xsdHaz.Volume.Value;
							importContext.SetPropertyInfoValue(dgItem.DI_UnitOfVolumeInfo, xsdHaz.Volume.DimensionType);
						}

						if (xsdHaz.PackedInLimitedQuantitySpecified)
						{
							dgItem.DI_IsLimitedQuantity = xsdHaz.PackedInLimitedQuantity;
						}

						if (!dgItem.DI_TechnicalName_ReadOnly && xsdHaz.TechnicalNameSpecified)
						{
							dgItem.DI_TechnicalName = xsdHaz.TechnicalName.Left(dgItem.DI_TechnicalNameInfo.MaxLength);
						}

						if (!dgItem.DI_MPMarinePollutant_ReadOnly && xsdHaz.MarinePollutantSpecified)
						{
							dgItem.DI_MPMarinePollutant = ((xsdHaz.MarinePollutant == UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code) ? ZString.Empty : xsdHaz.MarinePollutant).Left(dgItem.DI_MPMarinePollutantInfo.MaxLength);
						}
					}
				}
			}
			return this.Count > 0;
		}
	}
}
