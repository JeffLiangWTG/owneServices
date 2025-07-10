using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper
{
	public class DeclarationCreator
	{
		public static DeclarationCreator GetDeclarationCreator(JobDeclaration declaration, IEntryHeaderFilter filter)
		{
			return ObjectFactory.GetCountrySpecificOrDefault<DeclarationCreator>(declaration.CountryCode, declaration, filter);
		}

		public DeclarationCreator(JobDeclaration declaration, IEntryHeaderFilter filter)
		{
			this.declaration = declaration;
			this.filter = filter;
		}

		IDisposable EnableContainersCloningEvenNotLinked(JobDeclaration jobDeclaration)
		{
			jobDeclaration.ShouldCloneContainersEvenNotLinked = true;
			return new DisposableAction(() => jobDeclaration.ShouldCloneContainersEvenNotLinked = false);
		}

		public List<BaseJobDeclaration> RelatedDeclaration() => RelatedDeclarationImp().ToList();

		IEnumerable<BaseJobDeclaration> RelatedDeclarationImp()
		{
			using (EnableContainersCloningEvenNotLinked(declaration))
			{
				foreach (var header in filter.EntryHeaders)
				{
					var oldInstructionPk = header.CH_CEI_Instruction;
					var oldInstructionIndex = declaration.CustomsEntryInstructions.IndexOf(x => x.PK == oldInstructionPk);

					var newDeclaration = declaration.GetNewRelatedDeclaration(declaration.Factory, EUCommonConstants.DeclarationRelationshipType.SupplementaryDeclaration);
					var newInstructionPk = newDeclaration.CustomsEntryInstructions[oldInstructionIndex].PK;

					var instructionsToDelete = newDeclaration.CustomsEntryInstructions.Where(x => x.PK != newInstructionPk).ToList();
					foreach (var inst in instructionsToDelete)
					{
						newDeclaration.CustomsEntryInstructions.RemoveAndDelete(inst);
					}

					foreach (var invoice in newDeclaration.Invoices)
					{
						var linesToDelete = invoice.InvoiceLines.Where(x => x.JI_CEI.IsEmpty).ToList();
						foreach (var line in linesToDelete)
						{
							invoice.InvoiceLines.RemoveAndDelete(line);
						}
					}

					var invoicesToDelete = newDeclaration.Invoices.Where(x => x.InvoiceLines.Count == 0).ToList();
					foreach (var invoice in invoicesToDelete)
					{
						invoice.Delete();
					}

					var mrn = header.MovementReferenceNumber;
					foreach (var invoiceLine in newDeclaration.InvoiceLines.Cast<BaseJobComInvoiceLine>())
					{
						UpdateInvoiceLine(invoiceLine, mrn);
					}
					UpdateEntryInstruction(newDeclaration.CustomsEntryInstructions[0], mrn);
					UpdateDeclaration(newDeclaration, header);
					UpdateRelatedDeclaration(newDeclaration, declaration);
					UpdateRelatedDeclarationFromEntryHeader(newDeclaration, header);
					yield return newDeclaration;
				}
			}
		}

		public List<BaseJobDeclaration> NewInstruction()
		{
			foreach (var header in filter.EntryHeaders)
			{
				var newInstruction = (CusEntryInstruction)header.EntryInstruction.Clone();
				declaration.CustomsEntryInstructions.Add(newInstruction);
				var mrn = header.MovementReferenceNumber;
				foreach (var invoice in declaration.Invoices)
				{
					var linesToCopy = invoice.InvoiceLines.Where(x => x.JI_CEI == header.EntryInstruction.PK).ToList();
					foreach (var line in linesToCopy)
					{
						var newLine = line.Clone();
						invoice.InvoiceLines.Add(newLine);
						newLine.JI_CEI = newInstruction.PK;
						newLine.JI_MatchingKey = ZString.Empty;
						newLine.JI_LineNo = (ZShort)(invoice.InvoiceLineLineNumberGenerator.SequenceStartingNumber + invoice.InvoiceLines.Count - 1);
						UpdateInvoiceLine(newLine, mrn);
					}
				}

				UpdateEntryInstruction(newInstruction, mrn);
				UpdateDeclaration(declaration, header);
			}

			declaration.DoMerge();
			return new List<BaseJobDeclaration> { declaration };
		}

		public List<BaseJobDeclaration> ReuseInstruction()
		{
			foreach (var header in filter.EntryHeaders)
			{
				var originalInvoiceLines = header.InvoiceLines.ToList();
				header.CH_EntryStatus = ZString.Empty;
				header.MergedLines.RemoveAndDeleteAll();

				var messages = header.Messages.Cast<EDIMessage>().ToList();
				header.Messages.RemoveAll();
				declaration.Messages.AddRange(messages);

				var cusEntryNumber = CusEntryNumber.Load(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode);
				var originalMRN = cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (!originalMRN.IsEmpty)
				{
					cusEntryNumber.Delete();
				}

				if (!header.EntryNumber.IsEmpty)
				{
					header.EntryNumber = ZString.Empty;
				}

				foreach (var invoiceLine in originalInvoiceLines)
				{
					UpdateInvoiceLine(invoiceLine, originalMRN);
				}

				UpdateEntryInstruction(header.EntryInstruction, originalMRN);
				UpdateDeclaration(declaration, header);
			}
			return new List<BaseJobDeclaration> { declaration };
		}

		protected virtual ZString GetProcedure(Customs.Business.CusEntryInstruction entryInstruction) => entryInstruction.CEI_Procedure;

		protected virtual void UpdateEntryInstruction(Customs.Business.CusEntryInstruction entryInstruction, ZString originalMRN)
		{
			entryInstruction.CEI_Style = GetDefaultEntryStyleFromProcedure(entryInstruction);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
		}

		protected virtual ZString GetDefaultEntryStyleFromProcedure(Customs.Business.CusEntryInstruction entryInstruction)
		{
			return (string)GetProcedure(entryInstruction) switch
			{
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._53 => (ZString)EUCommonConstants.ImportDeclarationTypeList.H3,
				Core.Constants.Customs.Universal.RefCusProcedure.Codes._51 => (ZString)EUCommonConstants.ImportDeclarationTypeList.H4,
				_ => (ZString)EUCommonConstants.ImportDeclarationTypeList.H1,
			};
		}

		protected virtual void UpdateInvoiceLine(BaseJobComInvoiceLine invoiceLine, ZString originalMRN)
		{
		}

		void UpdateDeclaration(BaseJobDeclaration declaration, CusEntryHeader header)
		{
			const string declarationAcceptedCode = "ACC";
			var cesEvents = header.Logs.Find((log) => log.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && log.SL_Reference == declarationAcceptedCode);
			var latestCES = cesEvents.OrderByDescending(x => x.SL_EventTimeUtc).FirstOrDefault();
			if (latestCES != null)
			{
				declaration.JE_EntryAuthorisationDate = latestCES.SL_EventTime;
			}
		}

		protected virtual void UpdateRelatedDeclaration(BaseJobDeclaration newDeclaration, BaseJobDeclaration declaration)
		{
			newDeclaration.JE_DateOfFirstArrival = declaration.JE_DateOfFirstArrival;

			var isLinkedPackages = newDeclaration.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PackagesForInvoiceLinesForBindingOnly).Cast<BaseCusLinkPackage>().Where(y => y.IsLinked);
			foreach (var package in newDeclaration.Packages)
			{
				package.CW_PackQty = isLinkedPackages.Where(x => x.Package.PK == package.PK).Sum(y => y.PackQty);
			}
			newDeclaration.Packages.Where(x => x.CW_PackQty == 0).DeleteAll();
		}

		protected virtual void UpdateRelatedDeclarationFromEntryHeader(BaseJobDeclaration newDeclaration, Customs.Business.CusEntryHeader header)
		{
			newDeclaration.JE_TotalNoOfPacks = header.PackagesCount;
		}

		readonly JobDeclaration declaration;
		readonly IEntryHeaderFilter filter;
	}
}
