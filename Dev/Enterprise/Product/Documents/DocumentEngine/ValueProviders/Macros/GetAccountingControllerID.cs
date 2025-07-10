using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GetAccountingControllerID : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetAccountingControllerID({transactiontype},{ledger})>",
				ResString.GetMultilingualString("7525cbf2-2533-44ac-bf17-00b75bf264d1", @"Returns the Controller ID when provided a transaction type and ledger.Returns empty string if transaction type and ledger combination is not valid."),
				new List<(string example, object expectedResult)> { ((NoResString)"<GetAccountingControllerID(INV, AP)>", ControllerIDs.APInvoice.Name) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string transactionType = Regex.Match(macro).Groups[1].ToString().Trim();
			string ledger = Regex.Match(macro).Groups[2].ToString().Trim();

			var accountingControllerIdDecider = ObjectFactory.Get<IAccountingControllerIdDecider>();
			var controllerId = accountingControllerIdDecider.GetControllerID(transactionType, ledger, null);
			if (controllerId == null)
			{
				return string.Empty;
			}

			return controllerId.Name;
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)GetAccountingControllerID(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)([^\s]+)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
