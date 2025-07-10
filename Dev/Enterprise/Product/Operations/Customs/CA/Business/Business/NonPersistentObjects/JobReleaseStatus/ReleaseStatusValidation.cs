using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using CusEntryNumHelperForCargoControlNumber = Enterprise.Customs.Business.CusEntryNumHelperForCargoControlNumber;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ReleaseStatusValidation : AutoReleaseStatusValidation
	{
		public ReleaseStatusValidation(AutoReleaseStatus parent)
			: base(parent) { }

		#region Implementation

		public new ReleaseStatus Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReleaseStatus)base.Parent; }
		}

		#endregion

		#region CheckRL_Bill

		protected override void CheckRL_Bill()
		{
			base.CheckRL_Bill();
			var bills = Parent.Bills as BillCollection;
			if (bills != null && Parent.RL_Bill.IsEmpty && bills.Count > 1 && bills.DefaultBill == null)
			{
				var declaration = bills.Master;
				if (declaration != null && declaration.IsIID)
				{
					Parent.RL_BillInfo.AddMessageError(NoAssociatedBill);
				}
			}
		}

		#endregion

		internal static string NoAssociatedBill
		{
			get { return Res.GetString("80e8de9d-e09c-41d7-86bf-36f816ab3dc2", "This CCN is not associated with a bill and there are multiple bills specified."); }
		}

		protected override void CheckRL_CargoControlNumber()
		{
			var bills = Parent.Bills as BillCollection;
			if (bills != null && Parent.IsPersistent)
			{
				var declaration = bills.Master;
				ValidateCCNNumber(Parent.RL_CargoControlNumberInfo, Parent.RL_CargoControlNumber, declaration);
				if (declaration != null && declaration.IsIID && declaration.Invoices.Any())
				{
					if (declaration.CargoControlNumbers.Count > 1 && !declaration.Invoices.Cast<JobComInvoiceHeader>().Any(line => line.CargoControlNumbersList.Any(x => x.J2_ReferenceNumber.EqualsIgnoringCase(Parent.RL_CargoControlNumber))))
					{
						Parent.RL_CargoControlNumberInfo.AddMessageError(NoAssociatedInvoiceLine);
					}
				}
				if (Parent.RL_CargoControlNumber.HasCharactersNotSupportedByCAMessaging())
				{
					Parent.RL_CargoControlNumberInfo.AddWarning(Res.GetString("062b9848-73e6-4144-a2b3-f44f2103cdb1", "The Cargo Control Number has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs."));
				}
			}
		}

		internal static void ValidateCCNNumber(ZPropertyInfo info, ZString ccNumber, JobDeclaration declaration, bool mandatory = true)
		{
			if (ccNumber.Length < 5)
			{
				info.AddMessageError(InvalidCCNCode);
			}

			if (!ccNumber.IsEmpty && ccNumber.HasInvalidSymbolsForCCN())
			{
				info.AddMessageError(Res.GetString("CF8D229A-449D-48D5-AF2A-16FAA334962B", "Cargo Control Number should only contain letters, numbers, dash(-) and spaces."));
			}

			ValidateDuplicatedCCNNumber(info, ccNumber, declaration, mandatory);
		}

		internal static void ValidateDuplicatedCCNNumber(ZPropertyInfo info, ZString ccNumber, JobDeclaration declaration, bool mandatory)
		{
			if (declaration != null && !ccNumber.IsEmpty)
			{
				if (declaration.CargoControlNumbers.Count(x => !x.CY_CargoControlNumber.IsEmpty && x.CY_CargoControlNumber.Replace(" ", "").EqualsIgnoringCase(ccNumber.Replace(" ", ""))) > 1)
				{
					info.AddMessageError(Res.GetString("B73527A8-E154-459C-8D49-F52FDFA6C36A", "Duplicated CCN numbers on this declaration (CCN Number:'{0}')", ccNumber));
				}

				var otherDuplicateDec = declaration.Factory.LoadTop1<JobDeclaration>(GetDuplicateCCNQuery(ccNumber, declaration));
				if (otherDuplicateDec != null)
				{
					var messageText = AlreadyContainsCCN(otherDuplicateDec.JE_DeclarationReference, otherDuplicateDec.Company.GC_Name, (otherDuplicateDec.Branch != null ? otherDuplicateDec.Branch.GB_BranchName : ZString.Empty));
					info.AddMessageError(messageText);
				}
			}
			if (mandatory)
			{
				MandatoryValidation.CheckEntered(info);
			}
		}

		internal static string NoAssociatedInvoiceLine
		{
			get { return Res.GetString("a807d390-a34b-4171-ab85-b1a4a76d2e82", "This CCN is not associated with an invoice."); }
		}

		internal static string InvalidCCNCode
		{
			get { return Res.GetString("E4727D2D-1684-4EC1-AC74-A042379C834D", "A Cargo Control Number must start with a valid 4 digit carrier code and be followed by a unique number."); }
		}

		public static string AlreadyContainsCCN(string reference, string companyName, string branchName)
		{
			return Res.GetString("E472DA57-F452-4E49-865F-5378A4559FB6", "Another Declaration already contains the same CCN as this declaration (Declaration: '{0}', Company: '{1}', Branch: '{2}').", reference, companyName, branchName);
		}

		static ZQuery GetDuplicateCCNQuery(ZString ccNumber, JobDeclaration declaration)
		{
			var result = JobDeclarationFilter.ForCountry(false, declaration.CountryCode, declaration.Factory);
			result.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration.PK);
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.NotEqual, JobMessageTypeList.Codes.Export);
			var jobDeclaration = new ZDBOnlyQuery(typeof(JobDeclaration));

			jobDeclaration.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobDeclarationSchema.Constants.TableName, SQLComparisonOperator.Equal, ccNumber), JoinCondition.Or);
			jobDeclaration.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator.Equal, ccNumber), JoinCondition.Or);
			result.AddToFilter(jobDeclaration);

			var timeLimit = CACustomsDataRegistry.Instance.CCNReuseTimeSetting.Value;
			if (timeLimit > 0)
			{
				var systemCreateDateQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				systemCreateDateQuery.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo,
					new ZDateTime(ZDateTime.Now.AddYears(-timeLimit).Year, 1, 1));
				result.AddToFilter(systemCreateDateQuery);
			}

			result.IsNoResultQuery = false;
			result.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc + " DESC";
			return result;
		}
	}
}
