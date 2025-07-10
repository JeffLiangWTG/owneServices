using System;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business
{
	public sealed class ImportCusAuthorizationUsageValidation : CusAuthorizationUsageValidation
	{
		public ImportCusAuthorizationUsageValidation(CusAuthorizationUsage parent, BusinessObject cusAuthorizationUsageParent) : base(parent)
		{
			this.cusAuthorizationUsageParent = Argument.NotNull(cusAuthorizationUsageParent, nameof(cusAuthorizationUsageParent));
		}
		readonly BusinessObject cusAuthorizationUsageParent;

		public static class MessageError
		{
			public const string BR0339 = "[BR0339] ";

			public static string BR1031 => Res.GetString("e6ebf9c8-3a37-4410-bf08-b1730af7d34e", "[BR1031] If Requested Procedure is '44' or '51' then Holder of the Authorization must be the same as Importer.");
			public static string BR1130 => Res.GetString("2E62E737-C6FB-4881-A60D-14D119501006", "[BR1130] When Requested Procedure is '44', Owner must be the same as Importer.");
			public static string BR1131 => Res.GetString("797CF4E7-2EF9-423B-B029-E221F9968061", "[BR1131] If Requested Procedure is '71', then Holder of the Authorization must be the same as Importer.");
		}

		protected override void CheckAGC_OH_Owner()
		{
			base.CheckAGC_OH_Owner();
			CheckOwnerEqualsToImporter();

			if (cusAuthorizationUsageParent is CusEntryInstruction instruction && instruction.IsI1)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AGC_OH_OwnerInfo, messagePrefix: MessageError.BR0339);
			}
		}

		void CheckOwnerEqualsToImporter()
		{
			if (cusAuthorizationUsageParent is JobComInvoiceLine invoiceLine && Parent.AGC_OH_Owner.IsValid && invoiceLine.Declaration is JobDeclaration declaration)
			{
				var ownerRules = invoiceLine.JI_Calc_RequestedProcedure.ToString() switch
				{
					ProcedureCodes.ProcedureCode._44 => [MessageError.BR1031, MessageError.BR1130],
					ProcedureCodes.ProcedureCode._51 => [MessageError.BR1031],
					ProcedureCodes.ProcedureCode._71 => [MessageError.BR1131],
					_ => Array.Empty<string>(),
				};

				if (ownerRules.Length > 0)
				{
					var targetInfo = Parent.AGC_OH_OwnerInfo;
					var importerPK = declaration.ImporterDocumentaryAddress.OrganisationPK;
					if (Parent.AGC_OH_Owner != importerPK)
					{
						ownerRules.ForEach(targetInfo.AddMessageError);
					}
				}
			}
		}

		protected override void CheckAGC_Number()
		{
			base.CheckAGC_Number();

			var parent = Parent;
			ValidateRuleBR2039(parent.AGC_Code, parent.AGC_Number, parent.AGC_NumberInfo);
		}

		void ValidateRuleBR2039(ZString code, ZString number, ZPropertyInfo info)
		{
			if (code.EqualsIgnoringCase(AuthorizationUsageType.Codes.REX))
			{
				var regex = new Regex("^[A-Z]{2}" + AuthorizationUsageType.Codes.REX + "[A-Z0-9]{1,30}$");
				var match = regex.Match(number);
				if (!match.Success)
				{
					info.AddMessageError(Res.GetString("C361B65F-6594-4410-99BE-F7D1A7E80C3E", "[BR2039] If [12 12 002 000] Authorization type is 'REX', then [12 12 001 000] Reference Number must conform to the following Format: (A2)(REX)(AN1)(AN..29) where a) A2 represents 2 digit country code (capital letters), b) REX is the constant text, c) after the 'REX' constant there must be minimum 1 to maximum 30 characters. These characters can only be numeric 0..9 and/or alphas A..Z in capital letters."));
				}
			}
		}
	}
}
