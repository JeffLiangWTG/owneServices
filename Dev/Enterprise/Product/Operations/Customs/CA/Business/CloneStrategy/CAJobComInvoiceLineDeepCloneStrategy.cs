using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CAJobComInvoiceLineDeepCloneStrategy : JobComInvoiceLineDeepCloneStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public CAJobComInvoiceLineDeepCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, CloneType cloneType, BaseJobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
		{
		}

		public override BusinessObject Clone()
		{
			var result = (JobComInvoiceLine)base.Clone();

			var declaration = (JobDeclaration)invoiceLineToClone.Declaration;

			if (declaration != null && InvoiceLineToClone.DutiesAndTaxes != null)
			{
				CloneDutyAndTax(result);
			}

			if (declaration != null && declaration.CA_OGDCFIA)
			{
				CloneCFIARegistrationNumbers(result);
			}

			if (declaration != null && declaration.CA_OGDIC)
			{
				CloneSITTCertificationNumbers(result);
			}

			if (declaration != null && declaration.IsIID)
			{
				CAPGADetailsCopyHelper.CopyPGADetails(result, InvoiceLineToClone);
			}
			return result;
		}

		void CloneCFIARegistrationNumbers(JobComInvoiceLine clonedInvoice)
		{
			foreach (var cfiaRegistrationNumber in InvoiceLineToClone.CFIARegistrationNumbers)
			{
				var clonedCFIARegistrationNumber = (CFIARegistrationNumber)cfiaRegistrationNumber.Clone();
				using (clonedCFIARegistrationNumber.GetValidationSuspender())
				using (clonedCFIARegistrationNumber.SuspendSettingHasChanges())
				{
					clonedCFIARegistrationNumber.Parent = clonedInvoice;
					clonedInvoice.CFIARegistrationNumbers.Add(clonedCFIARegistrationNumber);
				}
			}
		}

		void CloneSITTCertificationNumbers(JobComInvoiceLine clonedInvoice)
		{
			foreach (var sittCertificationNumber in InvoiceLineToClone.SITTCertificationNumbers)
			{
				var clonedSITTCerficationNumber = (SITTCertificationNumber)sittCertificationNumber.Clone();
				using (clonedSITTCerficationNumber.GetValidationSuspender())
				using (clonedSITTCerficationNumber.SuspendSettingHasChanges())
				{
					clonedSITTCerficationNumber.Parent = clonedInvoice;
					clonedInvoice.SITTCertificationNumbers.Add(clonedSITTCerficationNumber);
				}
			}
		}

		void CloneDutyAndTax(JobComInvoiceLine clonedInvoice)
		{
			foreach (DutyAndTax dutyandtax in InvoiceLineToClone.DutiesAndTaxes)
			{
				DutyAndTax clonedDutyAndTax = (DutyAndTax)dutyandtax.Clone();
				using (clonedDutyAndTax.GetValidationSuspender())
				using (clonedDutyAndTax.SuspendSettingHasChanges())
				{
					clonedDutyAndTax.Parent = clonedInvoice;
					clonedInvoice.DutiesAndTaxes.Add(clonedDutyAndTax);
				}
			}
		}

		protected JobComInvoiceLine InvoiceLineToClone
		{
			get { return (JobComInvoiceLine)bizObjToClone; }
		}
	}
}
