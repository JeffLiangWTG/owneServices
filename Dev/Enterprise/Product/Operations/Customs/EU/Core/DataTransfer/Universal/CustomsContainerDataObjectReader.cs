using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsContainerDataObjectReader : CustomsContainerDataObjectReader<JobDeclaration, CusContainer>
	{
		public CustomsContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger,UniversalDataObjectReaderHelper helper, JobDeclaration declaration, ILandedCostDataReader landedCostDataReader = null)
			: base(containerDataObject, logger, helper, declaration, landedCostDataReader)
		{
		}

		protected override void PopulateCountrySpecificData(CusContainer container, Container dataObject)
		{
			if (declaration.AdditionalSealsRequired && (dataObject.AdditionalSealNumberCollection != null || dataObject.ThirdSeal.HasValue))
			{
				using (((ICusSealSequenceNumberGeneratorProvider)container).SequenceNumberGenerator.GetLineNumberSuspender())
				{
					var additionalSeals = container.AdditionalSeals;
					var existingSeals = additionalSeals.Cast<CusSeal>().GroupBy(x => x.BK_SealNumber.ToUpperInvariant()).ToDictionary(x => x.Key, y => new Queue<CusSeal>(y));
					AddAdditionalSealIfNeeded(additionalSeals, existingSeals, dataObject.ThirdSeal.GetValueOrDefault());
					if (dataObject.AdditionalSealNumberCollection is List<UniversalDataBuss.DataObjects.Universal.Customs.SealNumber> sealNumberList)
					{
						foreach (var seal in sealNumberList)
						{
							AddAdditionalSealIfNeeded(additionalSeals, existingSeals, seal.Number.GetValueOrDefault());
						}
					}
					existingSeals.Values.SelectMany(x => x).DeleteAll();
					var sequenceNumber = ZShort.Zero;
					additionalSeals.OrderBy(x => x.BK_SealNumber).ForEach(x =>
					{
						x.BK_SequenceNumber = ++sequenceNumber;
					});
				}
			}
		}

		void AddAdditionalSealIfNeeded(CusSealCollection additionalSeals, Dictionary<ZString, Queue<CusSeal>> existingSeals, ZString sealNumber)
		{
			if (!sealNumber.IsEmpty)
			{
				CusSeal seal;
				if (existingSeals.TryGetValue(sealNumber, out var queue))
				{
					_ = queue.Dequeue();
					if (queue.Count == 0)
					{
						existingSeals.Remove(sealNumber);
					}
				}
				else
				{
					seal = additionalSeals.AddNew();
					var sealRow = GetColumnIndexer(seal);
					SetValue(sealRow, CusSealSchema.BK_SealNumber, sealNumber);
				}
			}
		}
	}
}
