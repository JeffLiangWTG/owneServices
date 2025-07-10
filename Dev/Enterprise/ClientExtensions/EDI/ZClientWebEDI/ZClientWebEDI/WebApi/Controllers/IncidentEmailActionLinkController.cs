using System;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/incident")]
	public class IncidentEmailActionLinkController : ControllerWithEnvironment
	{
		public IncidentEmailActionLinkController()
		{
			this.lazyAccessControl = new Lazy<ITokenizedAccessControl>(() => new TokenizedAccessControl());
		}

		readonly Lazy<ITokenizedAccessControl> lazyAccessControl;
		ITokenizedAccessControl AccessControl => lazyAccessControl.Value;

		[HttpGet]
		[Route("resolve")]
		public IHttpActionResult Resolve(string token)
		{
			if (!string.IsNullOrWhiteSpace(token))
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory = new BusinessObjectFactory();
					var tokenQuery = new ZDBOnlyQuery(typeof(StmAccessToken));
					tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_Token, token);
					var accessToken = factory.LoadTop1<StmAccessToken>(tokenQuery);

					if (accessToken == null)
					{
						return Redirect(Global.IncidentActionLinkInvalidLinkPage);
					}

					var consumed = AccessControl.TryConsume(token, AccessTokenTypes.IncidentEmailActionLink, out var tokenInfo);
					if (consumed && tokenInfo.ParentTableCode == IncidentMainSchema.Constants.Prefix && tokenInfo.ParentId != Guid.Empty)
					{
						if (tokenInfo.Scope == SupportIncidentEmailBodyGeneralControls.ConfirmResolvedButtonID)
						{
							var query = new ZDBOnlyQuery(typeof(IncidentRequest));
							query.AddToFilter(IncidentRequestSchema.INC_IsCustomerResolved, false);
							var subQuery = new ZDBOnlySubQuery(typeof(IncidentMainBase), IncidentMainSchema.IM_INC_Request, IncidentRequestSchema.PK);
							subQuery.AddToFilter(IncidentMainSchema.PK, tokenInfo.ParentId);
							query.AddSubQuery(subQuery, JoinCondition.And);

							var request = factory.LoadTop1<IncidentRequest>(query);
							if (request != null)
							{
								//same as Glow button "Confirm Resolved"
								request.INC_IsCustomerResolved = true;
								request.INC_Status = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
								request.Logs.AddNew(AutoEvents.MiscellaneousEvent, SupportRequestProcessor.DeemedResolvedLogReference + " - Email");
								factory.Save();
								return Redirect(Global.IncidentActionLinkConfirmResolvedPage);
							}
						}
					}
				}
			}

			return Redirect(Global.IncidentActionLinkInvalidLinkPage);
		}

		[HttpGet]
		[Route("confirm-resolve")]
		public IHttpActionResult ConfirmResolve(string token)
		{
			if (!string.IsNullOrWhiteSpace(token))
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory = new BusinessObjectFactory();
					var tokenQuery = new ZDBOnlyQuery(typeof(StmAccessToken));
					tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_Token, token);
					var accessToken = factory.LoadTop1<StmAccessToken>(tokenQuery);

					if (accessToken == null)
					{
						return Redirect(Global.IncidentActionLinkInvalidLinkPage);
					}

					return Redirect(Global.IncidentActionLinkConfirmResolvedPage + $"?token={token}");
				}
			}

			return Redirect(Global.IncidentActionLinkInvalidLinkPage);
		}
	}
}

