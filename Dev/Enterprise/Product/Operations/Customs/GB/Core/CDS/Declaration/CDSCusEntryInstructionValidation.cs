using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSCusEntryInstructionValidation : CusEntryInstructionValidation
	{
		protected new CusEntryInstruction Parent => base.Parent as CusEntryInstruction;

		public CDSCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCurrentLocationFor523();
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			CheckUnusedInstruction(Parent.CEI_StyleInfo);
			CheckTransportNationality();
			Check_CEI_Style_H8_ImporterHas_TrustedTraderNumber(Parent.CEI_StyleInfo);
		}

		public void ValidateCurrentLocationFor523()
		{
			ValidateCalculatedProperty(Parent.CurrentLocationFor523Info);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ZPropertyInfo validation method call via reflection. Origin call is ValidateCurrentLocationFor523")]
		void CheckCurrentLocationFor523()
		{
			var locationOfGoods = Parent.CurrentLocationFor523;
			if (locationOfGoods.Length > 0 && locationOfGoods.Length < 5)
			{
				Parent.CurrentLocationFor523Info.AddMessageError(locationOfGoodsValidationWarning);
			}
			var r = new Regex("\\s{3,}");
			if (r.IsMatch(locationOfGoods))
			{
				Parent.CurrentLocationFor523Info.AddMessageError(locationOfGoodsWhitespaceValidationWarning);
			}
		}

		const string locationOfGoodsValidationWarning = "Location of goods for 5/23 is required.For warehouses, select an organisation in the warehouse field(s) below; for other locations such as ports and transit sheds, supply data on the main declaration screen.";
		const string locationOfGoodsWhitespaceValidationWarning = "Location of goods for 5/23 appears incorrect";

		void CheckUnusedInstruction(ZPropertyInfo targetInfo)
		{
			var parent = Parent;
			var jobDeclaration = parent?.JobDeclaration;

			if (jobDeclaration != null)
			{
				var entryInstructions = jobDeclaration.InvoiceLines.OfType<JobComInvoiceLine>().Select(x => x.EntryInstruction?.PK);

				if (entryInstructions != null && !entryInstructions.Contains(parent.PK))
				{
					targetInfo.AddMessageError("This Entry Instruction has not been used on an invoice line");
				}
			}
		}

		void Check_CEI_Style_H8_ImporterHas_TrustedTraderNumber(ZPropertyInfo cEI_StyleInfo)
		{
			var parent = Parent;
			if (parent.CEI_Style.Equals(ImportDeclarationTypeList.Codes.ReducedDataSetDeclaration))
			{
				var jobDeclaration = parent?.JobDeclaration;

				if (jobDeclaration != null)
				{
					var orgHeader = parent.Factory.Load<OrgHeader>(jobDeclaration.JE_OH_Importer);
					if (orgHeader != null)
					{
						var orgCusCode = orgHeader.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType.Equals(OrgCusCode.EuropeanUnionSharedCodeTypes.TrustedTrader));
						if (orgCusCode == null)
						{
							cEI_StyleInfo.AddMessageError("H8 declaration requires that the importer is a Trusted Trader, indicated by the presence of a TTD configuration record.");
						}
					}
				}
			}
		}

		void CheckTransportNationality()
		{
			Parent?.JobDeclaration?.Validation?.ValidateJE_RN_NKTransportNationality();
		}
	}
}
