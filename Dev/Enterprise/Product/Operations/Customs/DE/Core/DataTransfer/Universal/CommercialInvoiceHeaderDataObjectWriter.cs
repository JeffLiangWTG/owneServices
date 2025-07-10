using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DE.DataTransfer.Universal;

public class CommercialInvoiceHeaderDataObjectWriter : EU.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
{
	public CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager,
		UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null,
		CusEntryHeader relatedEntry = null)
		: base(manager, helper, landedCostDataWriter, relatedEntry)
	{
	}

	protected override ZBool IsPopulateBondedWarehouseDetails(BaseJobComInvoiceLine invoiceLineBO)
	{
		return invoiceLineBO.HasOutOfWarehouseProcedure || invoiceLineBO.HasIntoWarehouseProcedure || invoiceLineBO.HasOutOfInwardProcessingProcedure || invoiceLineBO.HasIntoInwardProcessingProcedure || ((invoiceLineBO.Declaration as Business.Declaration.JobDeclaration)?.IsWarehouseAdjustment ?? false);
	}

	protected override State GetStateOfOrigin(BaseJobComInvoiceLine invoiceLineBO)
	{
		return new State() { Code = invoiceLineBO.JI_StateOrRegionOfOrigin };
	}

	protected override List<CustomsSupportingInformation> GetInvoiceLineCustomsSupportingInformationCollectionCore(BaseJobComInvoiceLine invoiceLineBO)
	{
		var jobComInvoiceLine = (Business.Declaration.JobComInvoiceLine)invoiceLineBO;
		var supportingInformation = base.GetInvoiceLineCustomsSupportingInformationCollectionCore(invoiceLineBO);
		SetAuthorizationNumber(supportingInformation, jobComInvoiceLine);
		SetUsualProcessingFlag(supportingInformation, jobComInvoiceLine);

		return supportingInformation;
	}

	static void SetUsualProcessingFlag(List<CustomsSupportingInformation> supportingInformation, Business.Declaration.JobComInvoiceLine jobComInvoiceLine)
	{
		if (supportingInformation != null)
		{
			var previousProcedures = supportingInformation.Where(GetCustomsProcedurePredicate).ToArray();
			for (var i = 0; i < jobComInvoiceLine.PreviousProcedures.Count; i++)
			{
				var previousProcedureSource = jobComInvoiceLine.PreviousProcedures[i];
				var previousProceduresDestination = previousProcedures[i];
				var addInfoCollection = previousProceduresDestination.AddInfoCollection ??= new List<AddInfo>();
				addInfoCollection.Add(new AddInfo { Key = Constants.AddInfoKeys.PreviousProcedure.UsualProcessingFlag, Value = previousProcedureSource.UsualProcessingFlag ? YesNoList.Codes.Yes : YesNoList.Codes.No });
			}
		}
	}

	static void SetAuthorizationNumber(List<CustomsSupportingInformation> supportingInformation, Business.Declaration.JobComInvoiceLine jobComInvoiceLine)
	{
		if (supportingInformation != null & !jobComInvoiceLine.PreviousProcedureMaster.AuthorizationNumber.IsEmpty)
		{
			var previousProcedureMaster = supportingInformation.FirstOrDefault(GetCustomsProcedurePredicate);
			if (previousProcedureMaster != null)
			{
				var referenceNumberCollection = previousProcedureMaster.ReferenceNumberCollection ??= new List<Reference>();
				referenceNumberCollection.Add(new Reference { Type = new EntryType { Code = Constants.ReferenceNumbers.AuthorizationNumberCode, Description = Constants.ReferenceNumbers.AuthorizationNumberDescription }, ReferenceNumber = jobComInvoiceLine.PreviousProcedureMaster.AuthorizationNumber });
			}
		}
	}

	protected override List<AddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
	{
		var jobComInvoiceLine = (Business.Declaration.JobComInvoiceLine)invoiceLineBO;
		var addInfoCollection = base.GetInvoiceLineAddInfoCollection(invoiceLineBO);
		addInfoCollection.Add(new AddInfo { Key = Constants.AddInfoKeys.InvoiceLine.IsMainPack, Value = jobComInvoiceLine.JI_IsMainPack ? YesNoList.Codes.Yes : YesNoList.Codes.No });
		return addInfoCollection;
	}

	static bool  GetCustomsProcedurePredicate(CustomsSupportingInformation supportingInformation) =>
		supportingInformation.Category.Code.GetValueOrDefault() == Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument && new[] { PreviousProcedureList.Codes._ATAV, PreviousProcedureList.Codes._ATZL }.Contains((string)supportingInformation.Procedure.Code.GetValueOrDefault());
}
