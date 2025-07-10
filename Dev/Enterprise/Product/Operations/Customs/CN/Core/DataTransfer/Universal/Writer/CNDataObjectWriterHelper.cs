using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNDataObjectWriterHelper : UniversalDataObjectWriterHelper
	{
		public CNDataObjectWriterHelper(BusinessObjectFactory factory) : base(factory, Core.Constants.CountryCodes.China)
		{
		}

		protected override IEnumerable<CustomsReference> GetAdditionalCustomsReferenceDataForCore(BusinessObject bizObj, IDataWritingManager writeManager, string dataContext)
		{
			var result = new List<CustomsReference>();
			var baseResult = base.GetAdditionalCustomsReferenceDataForCore(bizObj, writeManager, dataContext);
			if (baseResult != null)
			{
				result.AddRange(baseResult);
			}

			if (bizObj is JobComInvoiceHeader invoiceHeader)
			{
				var contractsNums = invoiceHeader.ContractNumbers.Cast<JobComInvoiceHeaderRefs>().Select(@ref => @ref.J2_ReferenceNumber);
				foreach (var contractNum in contractsNums)
				{
					var cusRef = new CustomsReference
					{
						Type = new CodeDescriptionPair
						{
							Code = JobComInvoiceHeaderContract.Constants.CTR,
							Description = Res.GetString("1c5d28fe-c1f9-40ed-9c70-ca9b09642c4a", "Contract")
						},
						Reference = contractNum
					};
					result.Add(cusRef);
				}
			}

			return result;
		}
	}
}
