using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.PBN.Messaging;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class LPBPBNMessageInterpreter : InboundMessageInterpreter<LPBPBNProvider>
	{
		public LPBPBNMessageInterpreter(PBNInboundEDIMessage message, LPBPBNProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("6866a4cf-979b-494f-9599-44712a80867b", "Lookup PBN");

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (Res.GetString("E330EF13-0EEC-4D44-B51E-1AC48E86DD27", "PBN ID"), provider.PbnID.ToString());
			yield return (Res.GetString("FE30A713-967F-4233-A8FA-3CF3948E377F", "Status"), provider.Status.ToString());
			yield return (Res.GetString("B2C3D4E5-F6A7-8901-2345-6789ABCDEF01", "Issue"), provider.Issue.ToString());
			yield return (Res.GetString("C3D4E5F6-A7B8-9012-3456-789ABCDEF012", "Direction"), provider.Direction.ToString());
			yield return (Res.GetString("D4E5F6A7-B8C9-0123-4567-89ABCDEF0123", "Empty Vehicle"), provider.EmptyVehicle.ToString());

			if (provider.ContactDetails != null)
			{
				yield return (Res.GetString("E5F6A7B8-C9D0-1234-5678-9ABCDEF01234", "Email"), provider.ContactDetails.Email.ToString());
				yield return (Res.GetString("F6A7B8C9-D0E1-2345-6789-ABCDEF012345", "Mobile Number 1"), provider.ContactDetails.MobileNum1.ToString());
				yield return (Res.GetString("A7B8C9D0-E1F2-3456-789A-BCDEF0123456", "Mobile Number 2"), provider.ContactDetails.MobileNum2.ToString());
			}
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			if (provider.Declarations != null && provider.Declarations.Count > 0)
			{
				bool isSummaryAdded = false;
				foreach (var declaration in provider.Declarations)
				{
					var summary = "";
					if (!isSummaryAdded)
					{
						summary = Res.GetString("Declarations", "Declarations");
						isSummaryAdded = true;
					}
					yield return (summary,
						new (string, string)[]
						{
					(Res.GetString("B8C9D0E1-F2A3-4567-89AB-CDEF01234567", "Declaration ID"), declaration.DeclarationId.ToString()),
					(Res.GetString("C9D0E1F2-A3B4-5678-9ABC-DEF012345678", "Declaration Type"), declaration.DeclarationType.ToString())
						});
				}
			}
		}
	}
}
