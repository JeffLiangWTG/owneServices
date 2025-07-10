using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CreditOnHold : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CreditOnHold(BusinessObject)>",
				ResString.GetMultilingualString("45da9637-14b1-4ff4-a923-98b8582216e5", @"Check if a business object is in credit on hold status. If no parameter is passed in it will do the checking on the primary data provider of the document."),
				new List<(string example, object expectedResult)> {
					("<CreditOnHold()>", "Y"),
					((NoResString)"<CreditOnHold(Consignee)>", "N") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string macroMatch = Regex.Match(macro).Groups[1].Value;
			object destBizO = ValueProviderHelper.GetBusinessObjectFromDataProvider(macroMatch, report);

			if (destBizO == null)
			{
				ReportMacroError(report, Res.GetString("C992EE01-B3AD-4B4E-A093-60655ABB9C50", "Couldn't find business object for checking credit on hold."));
				return null;
			}
			else
			{
				if (destBizO is OrgHeader)
				{
					OrgHeader orgHeader = destBizO as OrgHeader;
					if (orgHeader.CreditChecker != null)
					{
						return (orgHeader.CreditChecker.IsCreditOnHold() || orgHeader.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit() > 0)
							? "Y" : "N";
					}
				}
				else if (destBizO is ICreditControlledDocumentDelivery && destBizO is BusinessObject)
				{
					ZGuid menuItemPK = report.MenuItem == null
						? ZGuid.Empty
						: report.MenuItem.PK;
					return ObjectFactory.Get<IDocumentDeliveryCreditControlManager>().GetDocumentDeliveryStatusForCreditManagement((BusinessObject)destBizO, Res.GetString("7D0B84AF-218A-4028-AF62-191EA0FB00F3", "document"), menuItemPK)
						== ZString.Empty ? "N" : "Y";
				}
				else
				{
					ReportMacroError(report, Res.GetString("39F790A1-CC96-433D-A503-7B170DF22EF1", "Business Object is not applicable for checking credit on hold."));
					return null;
				}
			}

			return null;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)CreditOnHold(?:[\s]*)\((?:[\s]*)(\s*\S*\s*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
