using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

#if DEBUG
using NUnit.Framework;
#endif

namespace Enterprise.DocumentEngine
{
	class TextMacroProcessor : ITextMacroProcessor
	{
		public IReadOnlyCollection<IReportError> ReportErrors { get; private set; } = new List<IReportError>();

		public string Replace(string text, object[] businessObjects, bool throwError = false, object stmMenuItem = null, bool? useJs = null, bool shouldEscapeAllSpecialCharacters = false)
		{
			//TODO: Change businessObjects parameter type to IBusiness[].
			return ReplaceCore(text, (IBusiness[])businessObjects, throwError, GetMenuItem(stmMenuItem), useJs, shouldEscapeAllSpecialCharacters);

			StmMenuItem GetMenuItem(object menuItem)
			{
				if (menuItem == null)
				{
					return null;
				}
				else if (menuItem is StmMenuItem miBizo)
				{
					return miBizo;
				}
				else if (menuItem is IStmMenuItem miInterface)
				{
					return (businessObjects.First() as IFactoryProvider).Factory.Load<StmMenuItem>(miInterface.PK);
				}
				else
				{
					throw new ArgumentException($"{nameof(stmMenuItem)} should be null or '{typeof(IStmMenuItem).FullName}' or '{typeof(StmMenuItem).FullName}'");
				}
			}
		}

		string ReplaceCore(string text, IBusiness[] businessObjects, bool throwError, StmMenuItem stmMenuItem, bool? useJs, bool shouldEscapeAllSpecialCharacters)
		{
			var result = text;
			var bizos = businessObjects.Cast<BusinessObject>().ToArray();
			var dataProviders = BODocDataProvider.GetArray(bizos);
			var dataProviderList = new DataProviderList(dataProviders);
			var userControlProviderList = new UserControlProviderList();
			var useJsEvaluator = useJs ?? RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value;
			var factory = bizos.FirstOrDefault()?.Factory;
			Report = new Report(null, null, dataProviderList, "", userControlProviderList, DocumentDirection.ANY, false, true, useJsEvaluator, factory)
			{
				MenuItem = stmMenuItem,
			};

			try
			{
				if (RegexProvider.OutermostMacroRegex.IsMatch(text))
				{
					Report.RegisterDocumentAndReportRelatedMacroProviders();

					var evaluator = shouldEscapeAllSpecialCharacters
						? new MatchEvaluator(match =>
						{
							var macroResult = Report.ReplaceSingleMacroWithSkippingEscapeAngleBracket(match);
							macroResult = macroResult.EscapeForJScript();
							return macroResult;
						})
						: new MatchEvaluator(Report.ReplaceSingleMacroNotInTemplateBody);

					result = RegexProvider.OutermostMacroRegex.Replace(text, evaluator);

					Report.MacroTranslator.ResetUsedProviders();
				}

				result = shouldEscapeAllSpecialCharacters ? result.UnEscapeForJScript() : result.UnEscapeAngleBrackets();
			}
			catch (DataProviderException ex)
			{
				var message = Res.GetString("01D7AD42-1CC8-45F9-B27D-2864F1656900", "Cannot evaluate {0}. {1}", text, ex.Message);
				Report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Error, ex));
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				if (throwError)
				{
					throw new DocumentEngineException(text);
				}
				var cause = ex.GetBaseException() ?? ex;
				var message = Res.GetString("ac77f687-d2db-464d-a827-63b42ed1685e",
@"There was a problem trying to evaluate a macro template. The macro text is as follows:

{0}

The error was:

{1}", text, cause.Message);

				Report.ErrorManager.Add(new ReportProcessingError(cause.Message, ReportProcessingErrorSeverity.Error, ex));

				var validTransactionCount =
#if DEBUG
					TransactionedTestCase.InTransactionedTestCase ? 1 : // In TransactionedTestCases transaction level is expected to 1, otherwise 0 is the limit.
#endif
						0;
				if (Globals.CanShowDialogs && Db.Connection.AppTransactionCount == validTransactionCount)
				{
					UserNotification.Show(message);
				}
				else
				{
					SendNotificationEmail(ex, message, businessObjects);
				}
			}
			finally
			{
				ReportErrors = Report.GetReportErrors().ToList();
				Report.Dispose();
			}

			return result;
		}

#if DEBUG
		public StmMenuItem LastReportMenuItem_ForTest => Report.MenuItem;
#endif

		Report Report { get; set; }

		internal IUserNotification UserNotification
		{
			get { return userNotification ?? Globals.Message; }
			set { userNotification = value; }
		}
		IUserNotification userNotification;

		void SendNotificationEmail(Exception ex, string message, object[] businessObjects)
		{
			var caption = Res.GetString("ce012e35-bec9-4e47-a19b-6fc93a268656", "There was a problem trying to evaluate a text template");
			var serviceTaskCode = System.Environment.GetEnvironmentVariable("ServiceTaskCode") ?? "N/A"; // Environment variable string cant be translated, and the N/A is also ok

			var body = new StringBuilder(message)
				.AppendLine()
				.AppendLine()
				.AppendLine()
				.AppendLine(Res.GetString("4694DF26-5E25-472E-8E5A-0A0FE69C1055", "Service task code: {0}", serviceTaskCode))
				.AppendLine(Res.GetString("9B21BD9F-DBD3-4F8D-9703-92A3E30C70C2", "Relevant Business Objects:"));

			AddHumanReadableListOfObjects(body, businessObjects);

			body.AppendLine()
				.AppendLine(Res.GetString("3FEEF689-3957-40A4-8F8B-E48E9AF30B47", "Error details:"))
				.AppendLine(ex.ToString());

			var email = new EmailDef();
			email.Subject = caption;
			email.Body = body.ToString();

			var currentUser = Env.CurrentUser;
			if (currentUser != null && currentUser.IsActive && !string.IsNullOrEmpty(currentUser.EmailAddress))
			{
				email.AddRecipientForSystemCommunication(currentUser.EmailAddress, RecipientDef.RecipientTypes.TO);
				Env.OutgoingMailManager.CreateAndSave(email);
			}
			else
			{
				Env.OutgoingMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
			}
		}

		void AddHumanReadableListOfObjects(StringBuilder result, object[] objects)
		{
			if (objects.Length == 0)
			{
				result.AppendLine(Res.GetString("44259E90-D39C-4E36-AF6D-C08CDC1762EE", "None"));
			}

			foreach (var o in objects)
			{
				var readableName = (o as BusinessObject)?.HumanReadableName;
				if (string.IsNullOrEmpty(readableName))
				{
					readableName = o.ToString();
				}

				result.AppendFormat("\t- {0}\r\n", readableName); // Formatting text, doesnt get translated
			}
		}

		public IEnumerable<ITextMacroExpression> ParseMacro(string macro, IEnumerable<Type> businessObjectTypes)
		{
			return new TextMacroParser().ParseMacro(macro, businessObjectTypes);
		}
		public IEnumerable<ITextMacroExpression> ParseMacro(string macro, IEnumerable<IBusiness> businessObjects)
		{
			return new TextMacroParser().ParseMacro(macro, businessObjects);
		}
	}
}
