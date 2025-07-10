using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>
	{
		public CNInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, BaseJobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader)
		{
		}

		protected override void ImportCountrySpecificRelatedData(BaseJobComInvoiceHeader invoiceBO, Dictionary<string, ValueSetter> delaySetters)
		{
			base.ImportCountrySpecificRelatedData(invoiceBO, delaySetters);
			FillContractNumbers(invoiceBO);
		}

		void FillContractNumbers(BaseJobComInvoiceHeader invoiceBO)
		{
			var contractNums = dataObject.CustomsReferenceCollection?.Where(x => x.Type.Code.GetValueOrDefault() == JobComInvoiceHeaderContract.Constants.CTR);
			if (invoiceBO is JobComInvoiceHeader invoiceHeaderCN && contractNums != null && contractNums.Any())
			{
				invoiceHeaderCN.ContractNumbers.DeleteAll();
				foreach (var contractNum in contractNums)
				{
					var newNum = invoiceHeaderCN.ContractNumbers.AddNew();
					newNum.J2_ReferenceNumber = new ZString(contractNum.Reference);
				}
			}
		}

		protected override void FillCountrySpecificDetails(CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, BaseJobComInvoiceLine parentInvoiceLine, bool invoiceLineIsInDatabase)
		{
			base.FillCountrySpecificDetails(invoiceLineData, invoiceLineRow, delaySetters, parentInvoiceLine, invoiceLineIsInDatabase);

			if (invoiceLineData.AddInfoCollection != null)
			{
				var ingredientAddInfo = invoiceLineData.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.CIQIngredient, logger);
				if (ingredientAddInfo.HasValue && !ingredientAddInfo.Value.IsEmpty)
				{
					var invoiceLinePk = invoiceLineRow.GetValue(JobComInvoiceLineSchema.PK);
					var noteRow = GetColumnIndexer(helper.LoadOrCreateStmNoteForReaderUpdate(invoiceLinePk, JobComInvoiceLineSchema.Constants.TableName, invoiceLineIsInDatabase, PredefinedNoteTypes.Instance.CustomsQuarantineIngredient.Description));
					SetValue(noteRow, StmNoteSchema.ST_NoteText, ingredientAddInfo.Value);
				}
			}
		}

		protected override IEnumerable<ZString> GetColumnNamesForSuspendSetting(CommercialInvoiceLine invoiceLineData)
		{
			foreach (var columnName in base.GetColumnNamesForSuspendSetting(invoiceLineData))
			{
				yield return columnName;
			}
			yield return AutoCNJobComInvoiceLine.Schema.JI_CIQTariff;
			yield return CusSupportingDocument.Schema.CSI_Code;
		}
	}
}
