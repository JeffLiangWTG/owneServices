using System;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAgreementEDocRequestHandler : DataRequestHandler<UserAgreementEDocRequestHelper>
	{
		public override string FileName => EDoc.FileName;

		public override ZBlob GetBinaryData()
		{
			var edoc = EDoc;
			if (edoc == null)
			{
				return ZBlob.Empty;
			}

			return edoc.ImageData;
		}

		protected IeDoc EDoc
		{
			get
			{
				if (eDoc == null)
				{
					var userAgreement = (BusinessObjects.Length > 0) ? BusinessObjects[0] as IDocManagerSupport : null;
					if (userAgreement != null)
					{
						var token = QueryString.Get(UserAgreementEDocRequestHelper.TokenKey);

						var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();
						var isTokenValid = !string.IsNullOrEmpty(token);
						if (isTokenValid && tokenControl.TryPeek(token, AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo))
						{
							var tokenScope = UserAgreementQueryToken.FromJson(tokenInfo.Scope);
							var uniqueKeyString = QueryString.Get(UserAgreementEDocRequestHelper.UniqueKey);

							if (tokenScope.AgreementType == ((EdiUserAgreement)userAgreement).ERA_Type && ZGuid.TryParse(uniqueKeyString, out var uniqueKey))
							{
								eDoc = userAgreement.DocManagerInfo.AllEDocs.GetFromUniqueKey(uniqueKey.ToGuid());
							}
						}
					}
				}

				return eDoc;
			}
		}

		IeDoc eDoc;

		protected override BusinessObject[] GetNewBusinessObjects()
		{
			return (PKs.Length > 0)
				? [Factory.Load<EdiUserAgreement>(PKs[0])]
				: Array.Empty<BusinessObject>();
		}
	}
}

