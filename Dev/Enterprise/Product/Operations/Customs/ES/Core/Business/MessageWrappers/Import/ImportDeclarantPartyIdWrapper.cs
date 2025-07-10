using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ImportDeclarantPartyIdWrapper : PartyNameWrapper, IImportDeclarantPartyIdProvider
	{
		public static ImportDeclarantPartyIdWrapper New(JobDeclaration jobDeclaration)
		{
			var orgHeader = jobDeclaration?.Declarant?.Header;
			return orgHeader == null ? null : new ImportDeclarantPartyIdWrapper(jobDeclaration, orgHeader);
		}

		ImportDeclarantPartyIdWrapper(JobDeclaration jobDeclaration, OrgHeader orgH)
			: base(orgH)
		{
			declaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
		}

		readonly JobDeclaration declaration;

		public ZString Type
		{
			get
			{
				var declarantType = declaration.JE_DeclarantType;

				switch (declarantType)
				{
					case EU.Business.RepresentationTypeList.Codes._1Self:
						declarantType = ESRepresentationTypeList.Codes._1Auto;
						break;
					case EU.Business.RepresentationTypeList.Codes._2Direct:
						declarantType = ESRepresentationTypeList.Codes._2Direct;
						break;
					case EU.Business.RepresentationTypeList.Codes._3Indirect:
						declarantType = ESRepresentationTypeList.Codes._3Indirect;
						break;
				}

				return declarantType;
			}
		}

		public ZString EmailAddress => declaration.DeclEmailAddr;

		public ZBool IsAuthorized => declaration.ZG_AuthPerDeclaration;
	}
}
