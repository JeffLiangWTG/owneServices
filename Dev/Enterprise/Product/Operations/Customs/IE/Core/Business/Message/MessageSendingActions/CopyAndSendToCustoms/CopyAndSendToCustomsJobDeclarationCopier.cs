using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CloneType = Enterprise.Customs.Business.CloneType;

namespace Enterprise.Customs.IE.Business
{
	public static class CopyAndSendToCustomsJobDeclarationCopier
	{
		public static IEnumerable<JobDeclaration> CreateCopy(ZInt numberOfCopies, ZInt invoiceLineNumberOfCopies, JobDeclaration originalDeclaration)
		{
			for (int i = 0; i < numberOfCopies; i++)
			{
				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var deepCloneStrategy = new JobDeclarationDeepCloneStrategy(originalDeclaration, CloneType.DeepTemplateCopy, factory);
				var copiedDeclaration = (JobDeclaration)deepCloneStrategy.Clone();

				CopyAndUpdateDeclarationValues(copiedDeclaration, originalDeclaration, GetIdentifier(i, numberOfCopies));

				DuplicateInvoiceLines(copiedDeclaration, invoiceLineNumberOfCopies);

				var wasSaved = false;
				try
				{
					factory.Save();
					wasSaved = true;
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
				if (wasSaved)
				{
					yield return copiedDeclaration;
				}
			}
		}

		static void CopyAndUpdateDeclarationValues(JobDeclaration copiedDeclaration, JobDeclaration originalDeclaration, string identifier)
		{
			copiedDeclaration.JE_DeclarationReference = identifier;
			copiedDeclaration.JE_MasterBill = "MB" + identifier;
			copiedDeclaration.JE_HouseBill = (originalDeclaration.JE_HouseBill.IsEmpty) ? null : "HB" + identifier;
			copiedDeclaration.JE_OwnerRef = "OR" + identifier;

			copiedDeclaration.JE_VesselName = originalDeclaration.JE_VesselName;
			copiedDeclaration.JE_VoyageFlightNo = originalDeclaration.JE_VoyageFlightNo;
			copiedDeclaration.JE_LloydsIMO = originalDeclaration.JE_LloydsIMO;

			var invoices = copiedDeclaration.Invoices;
			var invCount = invoices.Count;
			var invoiceNumberPadding = GetNumberOfDigits(invCount);
			for (int i = 0; i < invCount; i++)
			{
				var invoice = invoices[i];
				invoice.JZ_InvoiceNumber = "INV" + identifier + ((i + 1).ToString().PadLeft(invoiceNumberPadding, '0'));
			}
		}

		static void DuplicateInvoiceLines(JobDeclaration copiedDeclaration, ZInt invoiceLineNumberOfCopies)
		{
			if (invoiceLineNumberOfCopies > 1)
			{
				var baseInvoiceHeader = copiedDeclaration.Invoices.Cast<JobComInvoiceHeader>().OrderBy(x => x.JZ_SystemCreateTimeUtc).ThenBy(x => x.JZ_InvoiceDisplaySequence).FirstOrDefault();
				var baseInvoiceLine = baseInvoiceHeader?.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.JI_LineNo).FirstOrDefault();
				var baseEntryInstruction = copiedDeclaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault();
				var instructionPK = baseEntryInstruction.PK;
				baseInvoiceLine.JI_MatchingKey = "";
				if (baseInvoiceLine != null)
				{
					for (int l = 1; l < invoiceLineNumberOfCopies; l++)
					{
						var copiedInvoiceLine = (JobComInvoiceLine)new EU.Business.Declaration.JobComInvoiceLineDeepCloneStrategy(baseInvoiceLine, CloneType.DeepTemplateCopy, baseInvoiceHeader, null).Clone();
						copiedInvoiceLine.JI_CEI = instructionPK;
					}
				}
			}
		}

		static string GetIdentifier(int i, int numberOfCopies)
		{
			var padding = GetNumberOfDigits(numberOfCopies);
			return GlbStaff.CurrentUser.GS_Code + ZDateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture) + ((i + 1).ToString().PadLeft(padding, '0'));
		}

		static int GetNumberOfDigits(int number)
		{
			return (int)Math.Floor(Math.Log10(number) + 1);
		}
	}
}
