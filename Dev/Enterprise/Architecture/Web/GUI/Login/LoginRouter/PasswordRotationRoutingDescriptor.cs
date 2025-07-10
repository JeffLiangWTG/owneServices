using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public class PasswordRotationRoutingDescriptor : ILoginRoutingDescriptor
	{
		public PasswordRotationRoutingDescriptor(OrgContact contact)
		{
			this.contact = contact;
			RoutingUrlStringLazy = new Lazy<Uri>(() => GetRoutingUrl());
		}

		protected readonly OrgContact contact;

		public bool IsRoutingRequired
		{
			get
			{
				if (WebDataRegistry.Instance.WebPasswordRotationDays.Value > 0 && contact.HasPassword)
				{
					var logParent = contact.Person.HasPassword ? (IStmALogParent)contact.Person : contact;
					var query = new ZQuery(StmALogSchema.SL_Parent, logParent.LogsParentPK);
					query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WebAccessPasswordChanged.Code);
					query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, string.Empty);
					query.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + OrderByClause.Descending;
					var eventLog = contact.Factory.LoadTop1<StmALog>(query);

					var lastPasswordChangeDate = eventLog?.SL_PostedTimeUtc ?? WebDataRegistry.Instance.WebPasswordRotationEffectiveDate.Value;

					if (lastPasswordChangeDate.IsValid)
					{
						return lastPasswordChangeDate.Date.AddDays(WebDataRegistry.Instance.WebPasswordRotationDays.Value) < ZDateTime.UtcToday;
					}
				}

				return false;
			}
		}

		public Uri RoutingUrl => RoutingUrlStringLazy.Value;

		Uri GetRoutingUrl()
		{
			var token = PasswordInstructionUrlStrategy.GeneratePasswordInstructionToken(contact, PasswordInstructionType.Reset, null, TimeSpan.FromMinutes(10));
			var resetPasswordPage = ((IPasswordInstructionEmailSource)contact).ShouldSendMasterPassword ? TrackingConstants.RelativePath.ResetMasterPasswordPage : TrackingConstants.RelativePath.ResetPasswordPage;
			var uriAsString = FormattableString.Invariant($"~/{resetPasswordPage}?{TrackingConstants.QueryStringKeys.ResetPasswordKey}={token}&{RefKey}={RefValue}"); // uri query string
			return new Uri(uriAsString, UriKind.Relative);
		}

		readonly Lazy<Uri> RoutingUrlStringLazy;

		public void RoutingAction()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "uri query string")]
		public const string RefKey = "ref";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "uri query string")]
		public const string RefValue = "exp";
	}
}
