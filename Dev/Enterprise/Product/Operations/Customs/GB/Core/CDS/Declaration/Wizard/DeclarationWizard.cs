using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS
{
	public class DeclarationWizard
	{
		public DeclarationWizardItem[] WizardItems;
		public DeclarationWizardFilter[] Filter = new DeclarationWizardFilter[6];
		public bool IsMultiInstructionDeclaration;
		readonly BusinessObjectFactory factory;

		public DeclarationWizard(BusinessObjectFactory factory)
		{
			this.factory = factory;
			WizardItems = new DeclarationWizardHelper().ReadCsvString();
			QuestionNumber = 1;
		}

		public DeclarationWizardItem[] FilteredWizardItems()
		{
			var query = WizardItems.AsQueryable();
			foreach (var filterItem in Filter)
			{
				if (filterItem != null)
				{
					switch (filterItem.QuestionNumber)
					{
						case 1:
							{
								query = query.Where(x => x.Q1 == filterItem.Filter);
								break;
							}
						case 2:
							{
								query = query.Where(x => x.Q2DeclarationType == filterItem.Filter);
								break;
							}
						case 3:
							{
								query = query.Where(x => x.Q3 == filterItem.Filter);
								break;
							}
						case 6:
							{
								query = query.Where(x => x.AdditionalDeclarationTypes().Any(y => y.Equals(filterItem.Filter, System.StringComparison.OrdinalIgnoreCase)));
								break;
							}
					}
				}
			}

			return query.ToArray();
		}

		internal void PopulateDeclaration(JobDeclaration declaration)
		{
			var wizardItem = FilteredWizardItems().FirstOrDefault();

			if (wizardItem != null)
			{
				var createOrUpdateInstructionOption = Questions[4].Options.FirstOrDefault(x => x.Selected)?.Code ?? string.Empty;
				var instruction = declaration.CusEntryInstruction;
				if (ZString.Equals(createOrUpdateInstructionOption, "CREATE") && declaration.CustomsEntryHeaders.Any(entryHeader => !entryHeader.IsDeleted))
				{
					declaration.JE_EntryStyle = Questions[0].Options.FirstOrDefault(x => x.Selected)?.Code ?? string.Empty;
					var newInstruction = declaration.CustomsEntryInstructions.AddNew();
					newInstruction.CEI_Style = wizardItem.ProcedureCategory;
					newInstruction.CEI_SubStyle = Questions[5].Options.FirstOrDefault(x => x.Selected)?.Code ?? string.Empty;
					instruction = newInstruction;
				}
				else
				{
					declaration.JE_MessageType = wizardItem.IsImport ? MessageTypeList.Codes.Import : MessageTypeList.Codes.Export;
					declaration.JE_DeclarationType = wizardItem.ProcedureCategory;
					declaration.JE_EntryStyle = Questions[0].Options.FirstOrDefault(x => x.Selected)?.Code ?? string.Empty;
					declaration.JE_EntrySubStyle = Questions[5].Options.FirstOrDefault(x => x.Selected)?.Code ?? string.Empty;
				}

				var procedureCode = wizardItem.RequestedProcedure.PadRight(7, '0');

				if (declaration.InvoiceLines.Any())
				{
					var invoiceLine = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault();
					invoiceLine.JI_Procedure = procedureCode;
					invoiceLine.JI_CEI = instruction.PK;
				}
				else
				{
					var invoice = declaration.Invoices.FirstOrDefault() ?? declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_Procedure = procedureCode;
					invoiceLine.JI_CEI = instruction.PK;
				}

				Globals.Message.Show(GetSummaryText(declaration, instruction, wizardItem), "Declaration Created", ZMessageBoxButtons.OK, ZMessageBoxIcon.Information);
			}
		}

		public ZString GetSummaryText(JobDeclaration declaration, CusEntryInstruction instruction, DeclarationWizardItem wizardItem)
		{
			var entrySubStyleList = new EntrySubStyleCodeList(factory);
			entrySubStyleList.Load();
			ZString summaryText = FormattableString.Invariant($@"Your declaration is:

	Type: {instruction.CEI_Style} ({GetDeclarationTypeDescription(wizardItem)})
	Style: {declaration.JE_EntryStyle} ({GetEntryStyleDescription(wizardItem, declaration.JE_EntryStyle)})
	Substyle: {instruction.CEI_SubStyle} ({entrySubStyleList.GetDescriptionFromCode(instruction.CEI_SubStyle)})
	Procedure: {wizardItem.RequestedProcedure} ({wizardItem.ProcedureDefinition})");

			return summaryText;
		}

		ZString GetDeclarationTypeDescription(DeclarationWizardItem wizardItem)
		{
			var list = wizardItem.IsImport ? (CodeDescriptionPairList)new ImportDeclarationTypeList() : new ExportDeclarationTypeList();
			return list.GetDescriptionFromCode(wizardItem.ProcedureCategory);
		}

		ZString GetEntryStyleDescription(DeclarationWizardItem wizardItem, ZString entryStyle)
		{
			var list = wizardItem.IsImport ? (CodeDescriptionPairList)new EntryStyleListImport() : new EntryStyleListExport();
			return list.GetDescriptionFromCode(entryStyle);
		}

		public bool IsFirstOrLastQuestion()
		{
			return IsFirstQuestion() || IsLastQuestion();
		}

		public bool IsFirstQuestion() => QuestionNumber == 1;

		public bool IsLastQuestion() => QuestionNumber == Questions.Length;

		public int QuestionNumber { get; set; }

		public void NextQuestion(bool skipping = false)
		{
			var option = CurrentQuestion.Options.FirstOrDefault(x => x.Selected);
			if (option != null && !skipping)
			{
				Filter[QuestionNumber - 1] = new DeclarationWizardFilter { QuestionNumber = QuestionNumber, Filter = option.Code };
			}
			QuestionNumber++;

			if (!GetOptions().Any())
			{
				if (!IsLastQuestion())
				{
					NextQuestion(true);
				}
			}
		}

		public void PreviousQuestion()
		{
			ResetOptionsSelection();

			QuestionNumber--;
			Filter[QuestionNumber - 1] = null;

			if (!GetOptions().Any())
			{
				if (!IsFirstQuestion())
				{
					PreviousQuestion();
				}
			}
		}

		void ResetOptionsSelection()
		{
			var options = GetOptions();
			options.ForEach(x => x.Selected = false);
		}

		public void FinishWizard()
		{
			var option = CurrentQuestion.Options.FirstOrDefault(x => x.Selected);
			if (option != null)
			{
				Filter[QuestionNumber - 1] = new DeclarationWizardFilter { QuestionNumber = QuestionNumber, Filter = option.Code };
			}
		}

		public DeclarationWizardQuestion CurrentQuestion => Questions.FirstOrDefault(x => x.QuestionNumber == QuestionNumber);

		public ZString QuestionText => (ZString)CurrentQuestion?.QuestionText;

		public List<DeclarationWizardOption> GetOptions()
		{
			var options = new List<DeclarationWizardOption>();

			switch (QuestionNumber)
			{
				case 1:
					{
						options = Questions[0].Options.Where(x => FilteredWizardItems().Select(y => y.Q1).Distinct().Any(z => z == x.Code)).ToList();
						break;
					}
				case 2:
					{
						options = Questions[1].Options.Where(x => FilteredWizardItems().Select(y => y.Q2DeclarationType).Distinct().Any(z => z == x.Code)).ToList();
						break;
					}
				case 3:
					{
						options = Questions[2].Options.Where(x => FilteredWizardItems().Select(y => y.Q3).Distinct().Any(z => z == x.Code)).ToList();
						break;
					}
				case 4:
					{
						options = Questions[3].Options.ToList();
						break;
					}
				case 5:
					{
						var candidateOptions = Questions[4].Options;
						if (IsMultiInstructionDeclaration)
						{
							candidateOptions = candidateOptions.Where(o => o.Code == "CREATE").ToList();
						}
						options = candidateOptions;
						break;
					}
				case 6:
					{
						options = GetOptionsForFinalQuestion();
						Questions[5].Options = options;
						break;
					}
			}

			SetDefaultFilterIfNotAlreadySelected(options);

			return options;
		}

		List<DeclarationWizardOption> GetOptionsForFinalQuestion()
		{
			List<DeclarationWizardOption> options;
			var goodsArrivedOrNotArrivedUserAnswer = Questions[3].Options.FirstOrDefault(x => x.Selected).Code;
			var query = new ZQuery(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub);
			query.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
			query.AddToFilter(RefCusCodeListSchema.ZZD_Description, SQLComparisonOperator.Contains, goodsArrivedOrNotArrivedUserAnswer);
			var codeQuery = new ZQuery(RefCusCodeListSchema.ZZD_Code, FilteredWizardItems().SelectMany(x => x.AdditionalDeclarationTypes()).Distinct().ToList());
			query.AddToFilter(codeQuery);
			var codes = factory.Load<RefCusCodeList>(query);
			options = codes.Select(x => new DeclarationWizardOption
			{
				Code = x.ZZD_Code,
				Description = x.ZZD_Description.Replace("(Goods arrived)", string.Empty).Replace("(Goods not arrived)", string.Empty).Trim()
			}).ToList();

			return options;
		}

		void SetDefaultFilterIfNotAlreadySelected(List<DeclarationWizardOption> options)
		{
			if (options.Any() && !options.Any(x => x.Selected))
			{
				options.FirstOrDefault().Selected = true;
			}
		}

		public DeclarationWizardQuestion[] Questions = new DeclarationWizardQuestion[]
		{
			new DeclarationWizardQuestion
			{
				QuestionNumber = 1,
				QuestionText = "Where are your goods coming from/to?", // a third country or from/to a special territory of the EU?",
				Options = new DeclarationWizardOption[]
				{
					new DeclarationWizardOption { Code = "IM", Description = "Import from a Third country", Selected = true },
					new DeclarationWizardOption { Code = "CO", Description = "Import/Export to/from a Special territory", Selected = false },
					new DeclarationWizardOption { Code = "EX", Description = "Export to a Third country", Selected = false }
				}.ToList()
			},
			new DeclarationWizardQuestion
			{
				QuestionNumber = 2,
				QuestionText = "What kind of declaration are you doing?",
				Options = new DeclarationWizardOption[]
				{
					new DeclarationWizardOption { Code = "FC", Description = "Free circulation", Selected = false },
					new DeclarationWizardOption { Code = "IP", Description = "Inward Processing", Selected = false },
					new DeclarationWizardOption { Code = "TA", Description = "Temporary Admission", Selected = false },
					new DeclarationWizardOption { Code = "CW", Description = "Customs Warehousing", Selected = false },
					new DeclarationWizardOption { Code = "PE", Description = "Permanent Export", Selected = false },
					new DeclarationWizardOption { Code = "IPPEE", Description = "Inward Processing (IP) with Prior Export Equivalence (PEE)", Selected = false },
					new DeclarationWizardOption { Code = "OP1", Description = "Temporary Export under Outward Processing (OP)", Selected = false },
					new DeclarationWizardOption { Code = "OP2", Description = "Temporary Export under Outward Processing (OP) if not covered by Requested Procedure Code 21", Selected = false },
					new DeclarationWizardOption { Code = "RGR", Description = "Returned Goods Relief (RGR)", Selected = false },
					new DeclarationWizardOption { Code = "Re-Exp", Description = "Re-export of non-Union goods following a Special Procedure", Selected = false }
				}.ToList()
			},
			new DeclarationWizardQuestion
			{
				QuestionNumber = 3,
				QuestionText = "What options for this declaration type?",
				Options = new DeclarationWizardOption[]
				{
					new DeclarationWizardOption { Code = "None", Description = "No special option", Selected = true },
					new DeclarationWizardOption { Code = "OD", Description = "Onward Dispatch", Selected = false },
					new DeclarationWizardOption { Code = "EXC", Description = "Excise warehousing", Selected = false },
					new DeclarationWizardOption { Code = "IP", Description = "Inward Processing", Selected = false },
					new DeclarationWizardOption { Code = "IPEU", Description = "Inward Processing in EU", Selected = false },
					new DeclarationWizardOption { Code = "TA", Description = "Temporary Admission", Selected = false },
					new DeclarationWizardOption { Code = "CW", Description = "Customs Warehousing", Selected = false },
					new DeclarationWizardOption { Code = "OSR", Description = "Onward Supply Relief", Selected = false },
					new DeclarationWizardOption { Code = "End Use", Description = "End Use Relief", Selected = false },
					new DeclarationWizardOption { Code = "Re-Imp", Description = "Re-Importation", Selected = false }
				}.ToList()
			},
			new DeclarationWizardQuestion
			{
				QuestionNumber = 4,
				QuestionText = "Have the goods arrived or not?",
				Options = new DeclarationWizardOption[]
				{
					new DeclarationWizardOption { Code = "(Goods arrived)", Description = "Arrived", Selected = true },
					new DeclarationWizardOption { Code = "(Goods not arrived)", Description = "Not arrived", Selected = false }
				}.ToList()
			},
			new DeclarationWizardQuestion
			{
				QuestionNumber = 5,
				QuestionText = "Create an additional entry instruction for the result?",
				Options = new DeclarationWizardOption[]
				{
					new DeclarationWizardOption { Code = "CREATE", Description = "Create additional", Selected = true },
					new DeclarationWizardOption { Code = "UPDATE", Description = "Re-use existing", Selected = false }
				}.ToList()
			},
			new DeclarationWizardQuestion
			{
				QuestionNumber = 6,
				QuestionText = "Is your declaration full, supplementary or some other variety?"
			}
		};
	}
}
