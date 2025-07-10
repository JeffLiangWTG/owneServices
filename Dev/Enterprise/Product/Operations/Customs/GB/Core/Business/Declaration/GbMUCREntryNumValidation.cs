using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GbMUCREntryNumValidation : CusEntryNumValidation, Integration.Customs.GB.CCSUK.IGbCcsukMUCREntryNumValidation
	{
		public GbMUCREntryNumValidation(CusEntryNumber parent)
			: base(parent)
		{
		}

		void CheckMucrIsValidUkFormat(ZString mucr, ZPropertyInfo prop)
		{
			if (Declaration != null)
			{
				var allowedFormatsAndApplicabilityMatchDelegate = new Dictionary<string, DeclarationMatchesDelegate>();
				string inventorySystemFormat = @"^(GB/.{3,4}-)?[A-Z0-9\-\(\)]{5,25}$";          // 3- or 4-char inventory ID, dash, then 5-25 chars for ref. e.g. GB/CUK1-FOOBAR or just FOOBAR
				string airFormatExportsOnly = @"^A:[a-zA-Z0-9]{3}\d{8}(\d{8})?$"; // A: then 3-alphanumeric mawp, 8-digip mawn and option 8-digit hawb.  e.g. A:0001111111122222222
				string turnFormatExportsOnly = @"^GB/\d{9}(\d{3})?-[A-Z0-9\-\(\)]{1,}$";  //  GB/ then 9-or 12-digit vat/turn, dash, reference of at least 1 char (whole thing must be 35 chars, but BusinessObject takes care of that for us)
				string gemsFormat = @"^[A-Z ]{4}(\d{3}|\w{3})(\d{8}|\w{8})([A-Z0-9]{8}|[A-Z0-9]{8}\d{2}|[ ]{8}\d{2})?$";
				string cnsCourier = @"^[A-Z0-9]{3}\d[A-Z]{3}[A-Za-z0-9]{1,16}$";

				allowedFormatsAndApplicabilityMatchDelegate.Add(inventorySystemFormat, delegate
				{ return Declaration.ZG_Gateway != GatewayList.Codes.NES; });  // i.e this is moving via an inventory system
				allowedFormatsAndApplicabilityMatchDelegate.Add(airFormatExportsOnly, delegate
				{ return Declaration.IsExport && Declaration.IsAir; });
				allowedFormatsAndApplicabilityMatchDelegate.Add(turnFormatExportsOnly, delegate
				{ return Declaration.IsExport; });
				allowedFormatsAndApplicabilityMatchDelegate.Add(cnsCourier, delegate
				{ return Declaration.ZG_Gateway == GatewayList.Codes.CNS_CUSDECOnly; });
				allowedFormatsAndApplicabilityMatchDelegate.Add(gemsFormat, delegate
				{ return true; });

				bool matchesSomething = false;
				foreach (string onePattern in allowedFormatsAndApplicabilityMatchDelegate.Keys)
				{
					var formatIsRelevantForThisDeclaration = allowedFormatsAndApplicabilityMatchDelegate[onePattern](Declaration);
					var regex = new Regex(onePattern);
					if (formatIsRelevantForThisDeclaration && regex.IsMatch(mucr))
					{
						matchesSomething = true;
						break;
					}
				}

				if (Declaration.ZG_IsTrainingDeclaration && !mucr.IsEmpty)
				{
					var addiontalInfo = Declaration.IsImport ? "If the entry is accepted with a MUCR it may expend the inventory record before you have completed an Operational-mode entry."
																: "The Training DUCR may be included in the Consolidation MUCR, incorrectly inflating the Operational DUCR count of the MUCR.";
					prop.AddMessageError("This declaration is in Training mode.\r\n" + addiontalInfo + "\r\nIt is recommended to remove the MUCR while in Training mode and then restore it for Operational mode.");
				}

				if (!matchesSomething)
				{
					prop.AddMessageError("MUCR should match one of the Customs-defined formats");
				}
			}
		}

		protected override void CheckCE_EntryNum()
		{
			base.CheckCE_EntryNum();
			if (Parent.CE_EntryType == CusEntryNumberTypes.EU.MasterUCR)
			{
				if (!Parent.CE_EntryNum.IsEmpty)
				{
					CheckMucrNotAlreadyUsed();
					CountrySpecificValidation(Parent.CE_EntryNum, Parent.CE_EntryNumInfo);
				}
			}

			var declaration = Declaration;
			if (declaration != null
				&& !declaration.JE_MasterUCR.IsEmpty
				&& declaration.IsImport
				&& declaration.ZG_Gateway == GatewayList.Codes.CDS)
			{
				Parent.CE_EntryNumInfo.AddMessageError("Inventory-linked imports cannot be sent directly to CDS, and must instead go via a CSP.  This is a policy restriction from HMRC. Either select another profile via a CSP or remove the MUCR.");
			}
		}

		protected virtual void CountrySpecificValidation(ZString mucrNumber, ZPropertyInfo zPropertyInfo)
		{
			if (Parent.CE_EntryType == CusEntryNumberTypes.EU.MasterUCR)
			{
				CheckMucrIsValidUkFormat(mucrNumber, zPropertyInfo);
			}
		}

		void CheckMucrNotAlreadyUsed()
		{
			// Check whether this MUCR is unique
			ZQuery q = new ZQuery();
			q.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.EU.MasterUCR);
			ZQuery expireBeyondToday = new ZQuery(CusEntryNumSchema.CE_ExpiryDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now);
			ZQuery expire = new ZQuery(CusEntryNumSchema.CE_ExpiryDate, ZDateTime.Empty);
			expire.AddToFilter(expireBeyondToday, JoinCondition.Or);
			q.AddToFilter(expire);
			q.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Parent.CE_RN_NKCountryCode);
			q.AddToFilter(CusEntryNumSchema.CE_EntryNum, Parent.CE_EntryNum);
			q.AddToFilter(CusEntryNumSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			q.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobDeclaration.Schema.TableName);
			CusEntryNumber[] existingNums = Parent.Factory.Load<CusEntryNumber>(q);
			if (existingNums != null && existingNums.Length > 0)
			{
				Parent.CE_EntryNumInfo.AddWarning(string.Format("Master UCR '{0}' has already been used within your company's country/region on another declaration. Please check that the re-use of this is correct, otherwise inventory linking may not work correctly.", Parent.CE_EntryNum));
			}
		}

		JobDeclaration declaration;
		protected JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					if (Parent.CE_ParentTable == JobDeclarationSchema.Constants.TableName)
					{
						var result = Parent.Factory.Load<JobDeclaration>(Parent.CE_ParentID);
						declaration = result;
					}
					else if (Parent.CE_ParentTable == CusEntryHeaderSchema.Constants.TableName)
					{
						var result = Parent.Factory.Load<CusEntryHeader>(Parent.CE_ParentID);
						if (result != null)
						{
							declaration = result.Declaration;
						}
					}
				}
				return declaration;
			}
		}

		delegate bool DeclarationMatchesDelegate(EU.Business.Declaration.JobDeclaration dec);
	}
}
