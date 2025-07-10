using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceLineCompleteCollection : Customs.Business.InvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public new JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)base[index]; }
		}

		public new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}

		#region New

		public void Clone(BusinessObjectCollection collectionToClone)
		{
			Hashtable guidConversionTable = new Hashtable();
			RemoveAndDeleteAll();
			foreach (JobComInvoiceLine line in collectionToClone)
			{
				JobComInvoiceLine clonedLine = (JobComInvoiceLine)line.Clone();
				if (declaration != null)
				{
					declaration.InvoiceLines.Add(clonedLine);
				}
				guidConversionTable.Add(line.PK.ToString(), clonedLine.PK.ToString());
			}

			//Since our cloned lines have diverent PKs to our originals we need to map
			//the ZA_RelatedLinePK_Hidden to the correct cloned line's PK
			foreach (JobComInvoiceLine line in this)
			{
				object newGuidString = guidConversionTable[line.AddInfo.ZA_RelatedLinePK_Hidden];
				if (newGuidString != null)
				{
					line.AddInfo.ZA_RelatedLinePK_Hidden = newGuidString.ToString();
				}
			}
		}

		#endregion

		#region Amber reason
		public bool HasAnAmberReason => Factory.GetValue(ref hasAnAmberReasonCached, new GetValueDelegate<bool>(GetHasAnAmberReason));

		CachedProperty<bool> hasAnAmberReasonCached;

		bool GetHasAnAmberReason()
		{
			foreach (JobComInvoiceLine invoiceLine in this)
			{
				if (!invoiceLine.AddInfo.ZA_AMB.IsEmpty)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Drawbacks
		public bool HasADrawbackAmberReason => Factory.GetValue(ref hasADrawbackAmberReasonCached, new GetValueDelegate<bool>(GetHasADrawbackAmberReason));

		CachedProperty<bool> hasADrawbackAmberReasonCached;

		bool GetHasADrawbackAmberReason()
		{
			foreach (JobComInvoiceLine invoiceLine in this)
			{
				if (!invoiceLine.AddInfo.ZA_DARC_Hidden.IsEmpty)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Overriden

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)child;
			using (invoiceLine.GetValidationSuspender())
			using (invoiceLine.AddInfo.SuspendSettingHasChanges())
			{
				if (declaration.IsExport && !declaration.IsQuarantine)
				{
					invoiceLine.JI_Drawback = true;
				}

				if (declaration.IsDrawback)
				{
					JobComInvoiceHeader invoiceLinesHeader = invoiceLine.InvoiceHeader;
					invoiceLine.AddInfo.ZA_PST = "GEN";
					invoiceLine.AddInfo.ZA_RNO = "001";
					if (invoiceLinesHeader == null)
					{
						invoiceLine.AddInfo.ZA_DAM_Hidden = declaration.DrawbackHeaderAssesmentMethod;
						invoiceLine.AddInfo.ZA_EDN_Hidden = declaration.AddInfo.ZA_EDN_Hidden;
					}
					else
					{
						invoiceLine.AddInfo.ZA_DAM_Hidden = invoiceLinesHeader.AddInfo.ZA_DAM_Hidden;
						invoiceLine.AddInfo.ZA_EDN_Hidden = invoiceLinesHeader.AddInfo.ZA_EDN_Hidden;
					}
				}
			}
		}

		protected override void SetDefaultForFirstInvoiceLine(BaseJobComInvoiceLine firstInvoiceLine)
		{
			base.SetDefaultForFirstInvoiceLine(firstInvoiceLine);

			if (declaration.ActiveGroupHeader[0].JobComInvoiceHeaders.Count > 0)
			{
				JobComInvoiceHeader currentHeader = declaration.ActiveGroupHeader[0].JobComInvoiceHeaders[0];
				JobComInvoiceLine line = (JobComInvoiceLine)firstInvoiceLine;
				if (currentHeader != null && currentHeader.Supplier != null && currentHeader.Supplier.UNLOCO != null)
				{
					if (line.JI_CountryOfOrigin == Core.Constants.CountryCodes.Australia && currentHeader.Supplier.UNLOCO.CountryStates != null)
					{
						line.JI_AUState = currentHeader.Supplier.UNLOCO.CountryStates.RW_Code;
					}
				}
				if (declaration != null && currentHeader != null)
				{
					CommodityCodeDefaulter.AttemptDefaultFromImporter(declaration.Importer, line);
					CommodityCodeDefaulter.AttemptDefaultFromSupplier(currentHeader.Supplier, line);
				}
			}
		}

		protected override void SetDefaultFromPreviousLine(BaseJobComInvoiceLine previousLine, BaseJobComInvoiceLine currentInvoiceLine)
		{
			base.SetDefaultFromPreviousLine(previousLine, currentInvoiceLine);

			JobComInvoiceLine aUPreviousLine = (JobComInvoiceLine)previousLine;
			JobComInvoiceLine aUCurrenctLine = (JobComInvoiceLine)currentInvoiceLine;

			CopyOriginAndStateForExport(aUPreviousLine, aUCurrenctLine);
			aUCurrenctLine.JI_RH_NKCommodity_Code = aUPreviousLine.JI_RH_NKCommodity_Code;
		}

		protected void CopyOriginAndStateForExport(JobComInvoiceLine previousLine, JobComInvoiceLine currentLine)
		{
			if (declaration != null && declaration.IsExport && previousLine != null)
			{
				if (!previousLine.JI_CountryOfOrigin.IsEmpty)
				{
					currentLine.JI_CountryOfOrigin = previousLine.JI_CountryOfOrigin;
					if (currentLine.JI_CountryOfOrigin == Core.Constants.CountryCodes.Australia)
					{
						currentLine.JI_AUState = previousLine.JI_AUState;
					}
				}
			}
		}

		CommodityCodeDefaulter CommodityCodeDefaulter
		{
			get
			{
				if (fCommodityCodeDefaulter == null)
				{
					fCommodityCodeDefaulter = new CommodityCodeDefaulter(declaration);
				}
				return fCommodityCodeDefaulter;
			}
		}
		CommodityCodeDefaulter fCommodityCodeDefaulter;

		#endregion
	}
}
