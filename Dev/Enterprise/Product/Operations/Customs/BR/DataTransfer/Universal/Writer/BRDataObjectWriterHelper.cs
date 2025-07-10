using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BRDataObjectWriterHelper : UniversalDataObjectWriterHelper
	{
		public BRDataObjectWriterHelper(BusinessObjectFactory factory) : base(factory, Core.Constants.CountryCodes.Brazil)
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

			var invoiceLine = bizObj as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				result.AddRange(GetAdditionalCustomsReferenceDataForInvoiceLine<JobComInvLineRefs>(invoiceLine.LPCOJobComInvLineRefsCollection, LpcoType, x => x.JG_ReferenceNumber));
			}
			return result;
		}

		IEnumerable<CustomsReference> GetAdditionalCustomsReferenceDataForInvoiceLine<T>(IBusinessObjectCollection collection, CodeDescriptionPair type, Func<T, ZString> getReference) where T : BusinessObject
		{
			return collection.Cast<T>().Where(x => !getReference(x).IsEmpty).Select(x => new CustomsReference { Type = type, Reference = getReference(x) });
		}

		CodeDescriptionPair LpcoType => new CodeDescriptionPair() { Code = Constants.JobComInvLineRefsType.Codes.LPCO, Description = Constants.JobComInvLineRefsType.Descriptions.LPCO };

		protected override IAdditionalAddInfoGroupCollectionDataObjectWriter GetAdditionalAddInfoGroupCollectionSupportForCore(BusinessObject bizObj, IDataWritingManager writeManager)
		{
			IAdditionalAddInfoGroupCollectionDataObjectWriter writer = null;
			if (bizObj is JobComInvoiceLine invoiceLine)
			{
				writer = new BRAdditionalAddInfoGroupCollectionDataObjectWriterForInvoiceLine(invoiceLine);
			}
			return writer;
		}
	}
}
