using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business
{
	class LeadSheetDocumentWrapper : NonPersistentBusinessObject, IObsoleteValidation, IDocumentWrapper, ISourceIdentifierProvider
	{
		public LeadSheetDocumentWrapper(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public ZString ImporterName
		{
			get
			{
				if (declaration.ImporterOfRecordAddress.HasRealAddress)
				{
					return declaration.ImporterOfRecordAddress.E2_CompanyNameTruncated;
				}
				else
				{
					var importer = declaration.Importer;
					return importer != null ? importer.OH_FullNameTruncated : ZString.Empty;
				}
			}
		}

		public ZString TransactionNumber
		{
			get { return declaration.TransactionNumber.ToString(); }
		}

		public ZString ImporterNumber
		{
			get
			{
				var importer = declaration.ImporterOfRecordAddress.HasRealOrganisation ? declaration.ImporterOfRecordAddress.Organisation : declaration.Importer;
				return importer != null ? importer.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Canada,
					OrgCusCode.CACodeTypes.BusinessNumberForImportExport, OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial) : ZString.Empty;
			}
		}

		public ZString NoOfInvoicePages
		{
			get
			{
				var invoiceLines = declaration.InvoiceLines;
				return invoiceLines.Count > 0 ? invoiceLines.Max(x => ((JobComInvoiceLine)x).CA_PageNumber).ToString() : string.Empty;
			}
		}

		public ZString CargoControlNumber
		{
			get
			{
				var result = ZString.Empty;
				var ccn = (CusEntryNumber)declaration.AdditionalReferenceNumbers.FirstOrDefault(x => ((CusEntryNumber)x).CE_EntryType == CusCodeDataTypeList.Codes.CCN);
				if (ccn != null)
				{
					result = ccn.CE_EntryNum;
				}
				else
				{
					var cargoControlNumber = declaration.CargoControlNumbers.FirstOrDefault();
					if (cargoControlNumber != null)
					{
						result = cargoControlNumber.CY_CargoControlNumber;
					}
				}

				return result;
			}
		}

		public ZString LocationOfGoods
		{
			get { return declaration.JE_LocationOfGoods; }
		}

		public ZString ArrivingPer
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(declaration.CA_CarrierName);
				result.AppendIfNotEmpty(declaration.JE_DateOfFirstArrival.IsValid ? declaration.JE_DateOfFirstArrival.ToString("d/MMM/yyyy h:mm tt") : string.Empty);
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString ContainerNumber
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (CusContainer container in declaration.CusContainers)
				{
					result.AppendIfNotEmpty(container.CO_ContainerNumber);
				}
				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public ZString Weight
		{
			get { return !declaration.JE_TotalWeight.IsEmpty ? (declaration.JE_TotalWeight + " " + declaration.JE_TotalWeightUnit) : string.Empty; }
		}

		public ZString BLNumber
		{
			get
			{
				var result = new ZStringBuilder();
				var masterBills = declaration.Bills.Cast<Bill>().Where(x => x.CU_BillType == BillTypeList.Codes.MasterBill && !x.CU_BillNum.IsEmpty);
				var houseBills = declaration.Bills.Cast<Bill>().Where(x => x.CU_BillType == BillTypeList.Codes.HouseBill && !x.CU_BillNum.IsEmpty);
				foreach (var masterBill in masterBills)
				{
					result.Append(masterBill.CU_BillNum);
					var concatenatedHouseBills = new ZStringBuilder();
					foreach (var houseBill in houseBills.Where(x => x.CU_CU_ParentBill == masterBill.PK))
					{
						concatenatedHouseBills.Append(houseBill.CU_BillNum);
					}
					result.Append(concatenatedHouseBills.ToStringWithDelimiterBetweenAppends(", "));
				}
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString Reference
		{
			get { return declaration.JE_OwnerRef; }
		}

		public ZString Miscellaneous
		{
			get
			{
				var shipment = declaration.Shipment;
				StmNote leadSheetComments = null;
				if (shipment != null)
				{
					leadSheetComments = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.LeadSheetComments.Description).FirstOrDefault();
				}
				else
				{
					leadSheetComments = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.LeadSheetComments.Description).FirstOrDefault();
				}
				return leadSheetComments != null ? leadSheetComments.ST_NoteDataAsText : ZString.Empty;
			}
		}

		public ZString Port
		{
			get { return declaration.JE_CustomsOffice.TrimStart('0'); }
		}

		public ZString PARS
		{
			get
			{
				return declaration.CA_ServiceOption == ServiceOptions.Codes.PARS || declaration.CA_ServiceOption == ServiceOptions.Codes.PARSOGD ?
					"PARS" : string.Empty;
			}
		}

		ZGuid ISourceIdentifierProvider.SourceIdentifier => this.declaration.PK;
	}
}
