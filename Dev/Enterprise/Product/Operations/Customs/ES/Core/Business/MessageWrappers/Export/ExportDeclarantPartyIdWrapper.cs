using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExportDeclarantPartyIdWrapper : PartyNameWrapper, IExportDeclarantPartyIdProvider
	{
		public static ExportDeclarantPartyIdWrapper New(OrgHeader orgHeader, ZString declarantType, ZString declarantEmail, ZBool authPerDeclaration)
		{
			return orgHeader != null ? new ExportDeclarantPartyIdWrapper(orgHeader, declarantType, declarantEmail, authPerDeclaration) : null;
		}

		ExportDeclarantPartyIdWrapper(OrgHeader orgH, ZString declarantType, ZString declarantEmail, ZBool authPerDeclaration) : base(orgH)
		{
			this.declarantType = declarantType;
			EmailAddress = declarantEmail;
			NameCode = authPerDeclaration ? (ZString)"O" : ZString.Empty;
		}

		readonly ZString declarantType;

		public ZString PartyQualifier
		{
			get
			{
				var partyQualifier = declarantType;

				switch (partyQualifier)
				{
					case EU.Business.RepresentationTypeList.Codes._1Self:
						partyQualifier = ESRepresentationTypeList.Codes._1Auto;
						break;
					case EU.Business.RepresentationTypeList.Codes._2Direct:
						partyQualifier = ESRepresentationTypeList.Codes._2Direct;
						break;
					case EU.Business.RepresentationTypeList.Codes._3Indirect:
						partyQualifier = ESRepresentationTypeList.Codes._3Indirect;
						break;
				}

				return partyQualifier;
			}
		}

		public ZString EmailAddress { get; }

		public ZString NameCode { get; }
	}
}
