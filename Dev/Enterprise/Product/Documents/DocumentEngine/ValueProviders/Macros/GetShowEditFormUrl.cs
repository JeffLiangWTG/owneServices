using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GetShowEditFormUrl : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetShowEditFormUrl({controllerid},{businessobjectpk})>",
				ResString.GetMultilingualString("cec2246f-aa79-4de8-9f9c-cb032295285a",
				@"Returns a clickable link (URL) that will allow the consumer of the resulting document to click through to the form for the Business Object indicated by the Controller ID and Business Object PK specified. 
The link will only work on machines connected to your local network that are able to log into {0} and the user will have to have security privileges to the form indicated by the Controller ID. 
If you want a link external contacts can use, try using the {1} macro instead.",
Core.Constants.ProductName, "GetTrackingUrl"),
				new List<(string example, object expectedResult)> {
					("<GetShowEditFormUrl(JobConsol, <PK>)>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.JobConsol, new Guid("6E449683-C509-11CF-AAFA-00AA00B6015C"))),
					((NoResString)"<GetShowEditFormUrl(Organisation, <Creditor.PK>)>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, new Guid("bf51f612-4d56-48fb-ab7a-79e764f1819d"))) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Guid pk = Guid.Empty;
			string controllerIDAsString = Regex.Match(macro).Groups[1].ToString().Trim();
			var controllerFactory = ObjectFactory.Get<IControllerFactory>();
			ControllerID controllerID = controllerFactory.GetRegisteredIdentifierByName(controllerIDAsString);
			try
			{
				if (controllerID == null)
				{
					throw new ArgumentException("No such ControllerID '" + controllerIDAsString + "'");
				}

				var controller = controllerFactory.Create(controllerID);
				try
				{
					_ = controller.TypeOfTopLevelBusinessObject;
					_ = controller.ModuleID;
				}
				catch (ModuleGuiNotSupportedException)
				{
					ReportMacroError(report, Res.GetString("889B3C39-471D-476E-9B19-8838F681C54B", "The Controller ID '{0}' cannot be used with this macro.", controllerIDAsString), ReportErrorManagement.ReportProcessingErrorSeverity.WarningWithoutErrorReport);
					return controllerID?.Name;
				}

				ZString pkAsString = Regex.Match(macro).Groups[2].ToString();
				try
				{
					pk = (pkAsString == "") ? Guid.Empty : new Guid(pkAsString);
				}
				catch (FormatException ex)
				{
					throw new ArgumentException("Invalid PK '" + pkAsString + "'; " + ex.Message, ex);
				}
			}
			catch (ArgumentException ex)
			{
				ReportMacroError(report, ex.Message);
			}
			return pk == ZGuid.Empty ? string.Empty : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerID, pk);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)GetShowEditFormUrl(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)(.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
