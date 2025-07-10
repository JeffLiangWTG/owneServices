using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class UniversalDataObjectWriterHelper : Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper
	{
		public UniversalDataObjectWriterHelper(JobDeclaration declarationBO)
			: base(declarationBO.Factory, Core.Constants.CountryCodes.Canada)
		{
			this.declaration = Argument.NotNull(declarationBO, "JobDeclaration");
		}
		readonly JobDeclaration declaration;

		protected override IEnumerable<CustomsReference> GetAdditionalCustomsReferenceDataForCore(BusinessObject bizObj, IDataWritingManager writeManager, string dataContext)
		{
			var cusRefDataCollection = new List<CustomsReference>();
			var cargoControlNumbers = Enumerable.Empty<ZString>();

			var dec = bizObj as JobDeclaration;
			if (dec != null)
			{
				cargoControlNumbers = dec.CargoControlNumbers.Cast<CargoControlNumber>().Select(x => x.CA_CCNInfoNumber);
			}
			else
			{
				var invoiceHeader = bizObj as JobComInvoiceHeader;
				if (invoiceHeader != null)
				{
					cargoControlNumbers = invoiceHeader.CargoControlNumbersList.Cast<JobComInvoiceHeaderCCNs>().Select(x => x.J2_ReferenceNumber);
				}
			}

			foreach (var oneCCN in cargoControlNumbers)
			{
				var data = new CustomsReference
				{
					Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN },
					Reference = oneCCN
				};
				cusRefDataCollection.Add(data);
			}

			return cusRefDataCollection.Count > 0 ? cusRefDataCollection : null;
		}

		protected override IAdditionalAddInfoGroupCollectionDataObjectWriter GetAdditionalAddInfoGroupCollectionSupportForCore(BusinessObject bizObj, IDataWritingManager writeManager)
		{
			if (bizObj is CargoControlNumber cargoControlNumber)
			{
				return new AdditionalAddInfoGroupCollectionDataObjectWriterForCargoControlNumber(cargoControlNumber, declaration);
			}
			else if (bizObj is ILPCOCollectionParent lpcoCollectionParent)
			{
				return new AdditionalAddInfoGroupCollectionDataObjectWriterForILPCOCollectionParent(lpcoCollectionParent, writeManager);
			}
			else
			{
				return base.GetAdditionalAddInfoGroupCollectionSupportForCore(bizObj, writeManager);
			}
		}

		protected override void UpdateOrganizationAddressCollectionCore(IOrganizationAddressCollectionParent parent, CusAddInfo cusAddInfo, IDataWritingManager writeManager)
		{
			switch (cusAddInfo.B7_Type)
			{
				case CusAddInfoTypeAttribute.Codes.CADFOPGAHeader:
					AddInfoGroupOrganizationAddressCollectionUpdator.UpdateForDFO(parent, cusAddInfo as DFOPGAHeader, writeManager);
					break;
				case CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader:
					AddInfoGroupOrganizationAddressCollectionUpdator.UpdateForECCC(parent, cusAddInfo as ECCCPGAHeader, writeManager);
					break;
			}
		}
	}
}
