using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationDataObjectReader : DataTransfer.Universal.JobDeclarationDataObjectReader
	{
		internal JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null)
			: base(declarationDataObject, logger, factory, shipment)
		{
		}

		protected override DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}
		static int[] AllowablePartialTariffLengths
		{
			get { return allowablePartialTariffLengths ?? (allowablePartialTariffLengths = new int[] { 4, 6, 7, 9, 10, 13 }); }
		}
		[ThreadSafe]
		static int[] allowablePartialTariffLengths;

		protected override void AddFetchHintsRelatedToCommerialInvoiceLineTariff(BaseJobDeclaration declaration, List<ZString> harmonisedCodes)
		{
			base.AddFetchHintsRelatedToCommerialInvoiceLineTariff(declaration, harmonisedCodes);

			var classifications = harmonisedCodes.Select(c => c.Split(' ')[0].Replace(".", "")).Distinct();
			var fetchHintFactory = factory.BOFactory;
			fetchHintFactory.AddFetchHint(CMRStatisticalClassificationPeriodSnapshotSchema.Instance, new ZQuery(CMRStatisticalClassificationPeriodSnapshotSchema.SC_TariffClassificationNumber, classifications));
			fetchHintFactory.AddFetchHint(CMRTariffRatePeriodSnapshotSchema.Instance, new ZQuery(CMRTariffRatePeriodSnapshotSchema.TT_TariffClassificationNumber, classifications));

			if (declaration.IsExport)
			{
				if (AUCAHECCWrapper.EnableCWRefForAHECC)
				{
					foreach (var tariffCode in harmonisedCodes)
					{
						fetchHintFactory.AddFetchHint(AUCClassWrapper.GetTariffFetchHint(fetchHintFactory, tariffCode));
					}
				}
				else
				{
					fetchHintFactory.AddFetchHint(AUCAHECCSchema.Instance, new ZQuery(AUCAHECCSchema.UA_AHECC, harmonisedCodes));
				}
			}
			else if (AUCClassWrapper.UseCustomsReferenceData)
			{
				foreach (var tariffCode in harmonisedCodes)
				{
					fetchHintFactory.AddFetchHint(AUCClassWrapper.GetTariffFetchHint(fetchHintFactory, tariffCode));
				}
			}
			else
			{
				var allowableHarmonisedCodes = new List<ZString>();
				foreach (var harmonisedCode in harmonisedCodes)
				{
					if (harmonisedCode.Length == 2)
					{
						allowableHarmonisedCodes.Add(harmonisedCode);
						fetchHintFactory.AddFetchHint(AUCClassSchema.UJ_Code, harmonisedCode);
					}
					else if (harmonisedCode.Length >= 4)
					{
						foreach (int i in AllowablePartialTariffLengths)
						{
							if (i <= harmonisedCode.Length)
							{
								allowableHarmonisedCodes.Add(harmonisedCode);
								fetchHintFactory.AddFetchHint(AUCClassSchema.UJ_Code, harmonisedCode.SubstringSafe(0, i));
							}
						}
					}
				}

				var tariffs = factory.Load<AUCClass>(new ZQuery(AUCClassSchema.UJ_Code, allowableHarmonisedCodes));
				fetchHintFactory.AddFetchHint(AUCClassSchema.Instance, new ZQuery(AUCClassSchema.UJ_UJ, tariffs.Select(t => t.UJ_UJ).Distinct()));
			}
		}

		protected override List<ZString> GetAddressTypesHandleSeparately()
		{
			var result = base.GetAddressTypesHandleSeparately();
			result.Add(nameof(MasterFiles.Integration.DocAddressType.AQISProcessingEstablishment));
			return result;
		}
	}
}
