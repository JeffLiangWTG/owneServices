using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business
{
	public class JiProcedureHelper
	{
		public JiProcedureHelper(JobComInvoiceLine jobComInvoiceLine)
		{
			invoiceLine = Argument.NotNull(jobComInvoiceLine, nameof(jobComInvoiceLine));
		}

		protected readonly JobComInvoiceLine invoiceLine;

		public virtual void HandleSettingOfCPC(ZPropertyInfo valueThatHasChanged)
		{
			var itemDefaults = GetItemDefaults(SourceTypesList.Codes.CustomsProcedureCodeBox37, valueThatHasChanged.Value.ToString());
			foreach (var cpcDefaults in itemDefaults)
			{
				switch (cpcDefaults.TargetType)
				{
					case TargetTypesList.Codes.SupervisingOfficeBox44:
						var orgAddress = invoiceLine?.Factory.Load<OrgAddress>(cpcDefaults.TargetOrgAddress);
						if (orgAddress != null)
						{
							invoiceLine.JI_OA_SupervisingOffice = cpcDefaults.TargetOrgAddress;
						}
						break;
					case TargetTypesList.Codes.SupportingDocumentBox44:
						var sd = invoiceLine?.SupportingDocuments.AddNew();
						if (sd != null)
						{
							sd.CSI_Code = cpcDefaults.TargetCode.Left(sd.CSI_CodeInfo.MaxLength);
						}
						break;
					case TargetTypesList.Codes.AdditionalInformationStatementBox44:
						var ai = invoiceLine?.AdditionalInfos.AddNew();
						if (ai != null)
						{
							ai.CSI_Code = cpcDefaults.TargetCode.Left(ai.CSI_CodeInfo.MaxLength);
						}
						break;
					default:
						if (cpcDefaults.TargetType == TargetTypesList.Codes.ClientEoriForDucrTickbox && !(invoiceLine?.Declaration?.DucrGenerationOptionsReadOnly ?? true))
						{
							invoiceLine.Declaration.UseClientEoriForDucr = true;
						}
						break;
				}
			}

			BringInGuaranteeDetailsForCertainCPCs(valueThatHasChanged);
		}

		void BringInGuaranteeDetailsForCertainCPCs(ZPropertyInfo valueThatHasChanged)
		{
			// Implementation of CIP(16)37 - https://www.gov.uk/government/publications/customs-information-paper-37-2016-completion-notes-for-import-entries-for-inward-processing/customs-information-paper-37-2016-completion-notes-for-import-entries-for-inward-processing
			var grntrCode = "GRNTR";
			var specialCpcs = new[] { "5100001", "5100003", "5154001", "5171001", "5171004" };
			var newCpc = valueThatHasChanged.Value.ToString();
			if (specialCpcs.Contains(newCpc))
			{
				var dec = invoiceLine.Declaration;
				if (!dec?.JE_PaymentMethod.IsEmpty ?? false)
				{
					var danA00 = dec.JE_DefermentAccountNumber;
					var danVat = dec.ZG_VATDeferNumber;
					AdditionalInfo grntrStatementA00 = null;
					AdditionalInfo grntrStatementB00 = null;
					var grntrStatements = invoiceLine.AdditionalInfos.OfType<AdditionalInfo>().Where(ai => ai.CSI_Code == grntrCode).ToArray();

					if ((from AdditionalInfo g in grntrStatements let d = g.CSI_Description where !d.IsEmpty && d != danA00 && d != danVat select g).Any())
					{
						// GRNTR AI already exists but for a different number; do nothing
						return;
					}

					if (!grntrStatements.Any())
					{
						grntrStatementA00 = invoiceLine.AdditionalInfos.AddNew();
						grntrStatementA00.CSI_Code = grntrCode;
					}
					else if (grntrStatements.Length == 1)
					{
						grntrStatementA00 = grntrStatements.First();
					}
					else if (grntrStatements.Length == 2)
					{
						grntrStatementA00 = grntrStatements.First();
						grntrStatementB00 = grntrStatements.Last();
					}

					if (grntrStatementA00 != null)
					{
						grntrStatementA00.CSI_Description = danA00;
					}
					if (!dec.ZG_VATDeferType.IsEmpty && dec.ZG_VATDeferNumber != dec.JE_DefermentAccountNumber)
					{
						if (grntrStatementB00 == null)
						{
							grntrStatementB00 = invoiceLine.AdditionalInfos.AddNew();
							grntrStatementB00.CSI_Code = grntrCode;
						}
						grntrStatementB00.CSI_Description = danVat;
					}

					FindOrCreateTax(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, EU.Business.TaxRateCustomsDutyListImport.Codes.DutyTheGoodsAreLiableToDutyAtTheFullRateThisIncludesGoodsBeingEnteredForATariffQuotaReliefToWhichNoneOfTheCodesBelowApply);
					FindOrCreateTax(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, "");
				}
			}
		}

		public virtual void EnsureBox47TaxLineForVatRateWhenSettingTaxType(string vateRateCode) { }

		void FindOrCreateTax(string taxType, string taxRate)
		{
			var tax = invoiceLine.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault(t => t.Data.G4_Type == taxType) ?? invoiceLine.Taxes.AddNew();

			tax.Data.G4_RateDuty = taxRate;
			tax.Data.G4_Type = taxType;
			tax.Data.G4_MethodOfPayment = SecurityFromDefermentAccountMcdDeferment;
		}

		public virtual void HandleSettingOfPREF(ZPropertyInfo valueThatHasChanged)
		{
			var itemDefaults = GetItemDefaults(SourceTypesList.Codes.Preference, valueThatHasChanged.Value.ToString());
			foreach (var prefDefaults in itemDefaults)
			{
				switch (prefDefaults.TargetType)
				{
					case TargetTypesList.Codes.SupportingDocumentBox44:
						var sd = invoiceLine?.SupportingDocuments.AddNew();
						if (sd != null)
						{
							sd.CSI_Code = prefDefaults.TargetCode.Left(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema.CodeMaxLength);
						}
						break;
				}
			}
		}

		IEnumerable<ItemDefaulterSetting> GetItemDefaults(string sourceType, string valueThatHasChanged)
		{
			var itemDefaults = GBCustomsDataRegistry.Instance.ItemDefaults.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			return from ItemDefaulterSetting ids in itemDefaults where ids.SourceType == sourceType && new Regex(ids.SourceValue).IsMatch(valueThatHasChanged) select ids;
		}

		const string SecurityFromDefermentAccountMcdDeferment = "Q";
	}
}
