using System;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.Client.EDI;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.WiseTechAcademy;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/WiseTechAcademy")]
	public class WiseTechAcademyController : ControllerWithEnvironment
	{
		public WiseTechAcademyController()
		{
			this.lazyAccessControl = new Lazy<ITokenizedAccessControl>(() => new TokenizedAccessControl());
		}

		readonly Lazy<ITokenizedAccessControl> lazyAccessControl;
		ITokenizedAccessControl AccessControl => lazyAccessControl.Value;

		[HttpPost]
		[Route("token")]
		public IHttpActionResult VerifyToken(WiseTechAcademyAccessTokenData accessTokenData)
		{
			if (EDIDataRegistry.Instance.EnableAcademyJWTAuthentication.Value)
			{
				return BadRequest();
			}

			using (Db.DisposableActionForDbConnection())
			{
				var token = accessTokenData.Token;
				var contactPk = accessTokenData.ContactPk;
				var consumed = AccessControl.TryConsume(token, WiseTechAcademyAccessTokenData.TokenType, out AccessTokenInfo tokenInfo);

				if (!consumed || tokenInfo.ParentId != contactPk)
				{
					return NotFound();
				}
				else
				{
					return Ok();
				}
			}
		}
	}
}
