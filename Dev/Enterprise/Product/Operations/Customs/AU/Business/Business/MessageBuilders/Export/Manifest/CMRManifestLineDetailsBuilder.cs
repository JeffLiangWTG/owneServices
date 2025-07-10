using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRManifestLineDetailsBuilder
	{
		public CMRManifestLineDetailsBuilder(bool isMainManifest)
		{
			this.isMainManifest = isMainManifest;
		}

		public void PopulateSegment(SegmentGroup7 group7, IManifestHeaderWrapper header, IManifestLineWrapper line)
		{
			MessageUtilities.PopulateCNI(group7.CNI[0], line.LineNumber.ToString(), line.LineActionCode);

			if (line.LineActionCode != LineAction.Delete)
			{
				int cNTNumber = -1;
				if (line.PackCount != 0)
				{
					MessageUtilities.PopulateCNT(group7.CNT[++cNTNumber], ControlTotalTypeCodeQualifierList.TotalNumberOfPackages, line.PackCount.ToString());
				}

				if (!header.IsAir && line.ContainerCount != 0)
				{
					MessageUtilities.PopulateCNT(group7.CNT[++cNTNumber], ControlTotalTypeCodeQualifierList.NumberOfContainersToBeLoaded, line.ContainerCount.ToString());
				}

				int group8Number = -1;
				ZString cCAN = line.CCAN;
				ZString cAN = line.CAN;
				ZString exemptionCode = line.ExemptionCode;
				ZString houseBillNumber = line.HouseBillNumber;
				if (!cAN.IsEmpty)
				{
					MessageUtilities.PopulateRFF(group7.Group8[++group8Number].RFF[0], ReferenceFunctionCodeQualifierList.TransactionReferenceNumber, cAN, null);
				}
				else if (!cCAN.IsEmpty)
				{
					MessageUtilities.PopulateRFF(group7.Group8[++group8Number].RFF[0], ReferenceFunctionCodeQualifierList.TaxExemptionLicenceNumber, "EXCC", null);
					MessageUtilities.PopulateRFF(group7.Group8[++group8Number].RFF[0], ReferenceFunctionCodeQualifierList.ManualProcessingAuthorityNumber, cCAN, null);
				}
				else if (!exemptionCode.IsEmpty)
				{
					MessageUtilities.PopulateRFF(group7.Group8[++group8Number].RFF[0], ReferenceFunctionCodeQualifierList.TaxExemptionLicenceNumber, exemptionCode, null);
				}

				if (!houseBillNumber.IsEmpty)
				{
					MessageUtilities.PopulateRFF(group7.Group8[++group8Number].RFF[0], ReferenceFunctionCodeQualifierList.HouseWaybillNumber, houseBillNumber, null);
				}

				if (isMainManifest && header.IsAir && !line.AirWaybillNumber.IsEmpty)
				{
					MessageUtilities.PopulateRFF(group7.Group8[++group8Number].RFF[0], ReferenceFunctionCodeQualifierList.AirWaybillNumber, line.AirWaybillNumber, null);
				}

				if (ShouldIncludeAdditionalLineDetails(line))
				{
					if (!line.CountryOfDestination.IsEmpty)
					{
						MessageUtilities.PopulateLOC(group7.Group8[group8Number].LOC[0], LocationFunctionCodeQualifierList.CountryOfDestinationOfGoods, line.CountryOfDestination, CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization);
					}

					if (!line.GoodsOwner.IsEmpty || !line.GoodsOwnerPartyID.IsEmpty)
					{
						string ownerCode = line.GoodsOwnerPartyID;
						if (!line.GoodsOwner.IsEmpty || string.IsNullOrEmpty(ownerCode))
						{
							ownerCode = null;
						}

						MessageUtilities.PopulateNAD(group7.Group8[group8Number].Group11[0].NAD[0], PartyFunctionCodeQualifierList.GoodsOwner, ownerCode, ownerCode == null ? null : CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, line.GoodsOwner, null);
					}
				}

				foreach (SegmentGroup8 group8 in group7.Group8)
				{
					MessageUtilities.PopulateGID(group8.Group14[0].GID[0], "1");
				}

				if (ShouldIncludeAdditionalLineDetails(line) && !line.GoodsDescription.IsEmpty)
				{
					MessageUtilities.PopulateFTX(group7.Group8[group8Number].Group14[0].FTX[0], TextSubjectCodeQualifierList.GoodsDescription, line.GoodsDescription);
				}
			}
		}

		#region Implementation

		protected internal bool ShouldIncludeAdditionalLineDetails(IManifestLineWrapper line)
		{
			return (!isMainManifest && (line.ExemptionCode == CMRExportExemptionCodes.EXLV.Code || line.ExemptionCode == CMRExportExemptionCodes.EXPE.Code));
		}

		protected bool isMainManifest;

		#endregion
	}
}
