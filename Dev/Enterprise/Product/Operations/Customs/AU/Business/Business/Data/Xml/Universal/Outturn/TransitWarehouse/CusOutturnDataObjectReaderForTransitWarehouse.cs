using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnDataObjectReaderForTransitWarehouse : ShipmentDataObjectReader<CusOutturn>
	{
		public CusOutturnDataObjectReaderForTransitWarehouse(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusUnderbond underbond, Customs.Business.CusHAWB cusHAWB) : base(dataObject, logger, factory)
		{
			this.underbond = Argument.NotNull(underbond, nameof(underbond));
			this.cusHAWB = cusHAWB;
		}

		readonly CusUnderbond underbond;
		readonly Customs.Business.CusHAWB cusHAWB;

		public override DataContextType DataContextType => DataContextType.Outturn;

		protected override IMatchingBusinessEntityFinder<CusOutturn> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override CusOutturn GetNewBusinessObject()
		{
			var outturn = underbond.Outturns.AddNew();
			outturn.C5_HouseBill = houseBill.Value;
			outturn.C5_MasterBill = masterBill.Value;
			if (cusHAWB != null)
			{
				outturn.C5_ParentTableCode = CusHAWBSchema.Constants.Prefix;
				outturn.C5_ParentID = cusHAWB.PK;
			}

			return outturn;
		}

		protected override CusOutturn GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var validReferences = Shipment?.AdditionalReferenceCollection?.Where(a => a.ReferenceNumber.HasValue && a.Type != null && a.Type.Code.HasValue);

			masterBill = validReferences.FirstOrDefault(a => a.Type.Code.Value == (ZString)AdditionalReferenceTypes.Codes.MasterBill)?.ReferenceNumber;
			houseBill = Shipment?.WayBillNumber;

			if (!string.IsNullOrEmpty(houseBill) && !string.IsNullOrEmpty(masterBill))
			{
				var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
				query.AddToFilter(CusOutturnSchema.C5_HouseBill, this.houseBill);
				query.AddToFilter(CusOutturnSchema.C5_MasterBill, this.masterBill);

				var matchingOutturns = factory.BOFactory.Load<CusOutturn>(query);
				switch (matchingOutturns.Length)
				{
					case 1:
						logger.Log(Integration.LogType.Information,
							Res.GetString("3C8A91E8-521B-4DD5-BAA0-DBA7B36DB6A6",
								"Matching Cargo Line found for Master Bill: {0} and House Bill: {1}.",
								masterBill,
								houseBill));
						return matchingOutturns[0];
					case 0:
						logger.Log(Integration.LogType.Information, Res.GetString("08C4A95D-AFF3-4BE5-AE82-FE582A6DD1F6",
							"No matching Cargo Lines found for Master Bill: {0} and House Bill: {1}.",
							masterBill,
							houseBill));
						return null;
					default:
						throw new DataObjectReadFailureException(Res.GetString("08312906-BE89-4F8C-8DB0-C7BDC17D0188",
							"Multiple matching Cargo Lines found for Master Bill: {0} and House Bill: {1}.",
							masterBill,
							houseBill));
				}
			}
			else
			{
				throw new DataObjectReadFailureException(Res.GetString("97F21A4B-D5E4-4ACB-B42D-F66E34CCD04A",
					"UXML does not have a House Bill or Master Bill number to match to."));
			}
		}

		protected override void PopulateBusinessObject(CusOutturn targetBO)
		{
			var packLines = Shipment.PackingLineCollection;
			if (packLines != null && packLines.Count > 0)
			{
				var packLinesWithOutturn = packLines.Where(p => p.OutturnQty.HasValue && p.PackQty.HasValue).ToArray();

				try
				{
					targetBO.C5_OuterPacks = targetBO.C5_OuterPacks.IsEmpty ?
						(ZInt)packLinesWithOutturn.Where(p => p.AddInfoCollection != null && p.AddInfoCollection.Any(info => info.Key.ToString() == AddInfoKeyTypes.Types.IsManifestedPackage && info.Value.ToString() == true.ToString())).Sum(p => p.PackQty.Value)
						: targetBO.C5_OuterPacks;
				}
				catch (InvalidCastException)
				{
					logger.Log(Integration.LogType.Warning, Res.GetString("1ab07ba3-334c-4deb-ab78-5c8e2e8f398c", "Cannot update outturn's package quantity correctly because package quantity in UXML exceeds the supported max value."));
				}

				targetBO.C5_PackagesOutturned = packLinesWithOutturn.Sum(p => p.OutturnQty.Value);
				targetBO.C5_PackagesUnits = targetBO.C5_OuterPackUnits;
				targetBO.C5_VolumeOutturned = packLinesWithOutturn.Where(p => p.OutturnedVolume.HasValue).Sum(p => p.OutturnedVolume.Value);
				targetBO.C5_VolumeOutturnedUQ = SeaCargoUtilities.ConvertVolumeUnitToCMRVolumeUnit(packLinesWithOutturn.Where(p => p.VolumeUnit != null).FirstOrDefault()?.VolumeUnit.GetNullableCodeAsUpperCase() ?? ZString.Empty);

				targetBO.C5_WeightOutturned = packLinesWithOutturn.Where(p => p.OutturnedWeight.HasValue).Sum(p => p.OutturnedWeight.Value);
				targetBO.C5_WeightOutturnedUQ = packLinesWithOutturn.Where(p => p.OutturnedWeight.HasValue).FirstOrDefault()?.WeightUnit?.Code ?? ZString.Empty;
				targetBO.C5_PillageIndicator = packLinesWithOutturn.Any(p => p.OutturnPillagedQty.HasValue && p.OutturnPillagedQty.Value > 0);
				targetBO.C5_DamageIndicator = packLinesWithOutturn.Any(p => p.OutturnDamagedQty.HasValue && p.OutturnDamagedQty.Value > 0);
				targetBO.C5_PackagesUnits = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(packLinesWithOutturn.Where(p => p.PackType != null).FirstOrDefault()?.PackType?.GetNullableCodeAsUpperCase() ?? ZString.Empty);

				ZString goodsDescription = String.Join(", ", packLinesWithOutturn.Select(p => p.GoodsDescription).Distinct());
				targetBO.C5_GoodsDescription = goodsDescription.Truncate(CusOutturnSchema.C5_GoodsDescription.MaxLength);

				ZDateTime unpackDate = Shipment.RelatedShipmentCollection?.FirstOrDefault()?.DateCollection?.FirstOrDefault()?.Value ?? ZDateTime.Empty;
				if (!unpackDate.IsEmpty)
				{
					targetBO.C5_CargoUnpackDate = unpackDate;
					if (targetBO.C5_CargoReceiptDate.IsEmpty)
					{
						targetBO.C5_CargoReceiptDate = unpackDate;
					}
				}
			}
			else
			{
				throw new DataObjectReadFailureException(Res.GetString("4059eb4f-f8cd-4d03-a190-9d2b854a1137", "Can not update Cargo line because UXML received doesn't contain any packline information."));
			}
		}

		ZString? houseBill;
		ZString? masterBill;
		Shipment Shipment => (Shipment)DataObject;
	}
}
