using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class EXSRepresentativeWrapper : EXSContactPersonWithIdWrapper, IEXSRepresentative
	{
		public static EXSRepresentativeWrapper New(JobDeclaration jobDeclaration, OrgAddress orgAddress)
		{
			return orgAddress?.Header != null ? new EXSRepresentativeWrapper(jobDeclaration, orgAddress) : null;
		}

		EXSRepresentativeWrapper(JobDeclaration jobDeclaration, OrgAddress orgAddress) : base(orgAddress)
		{
			jDeclaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
			orgA = orgAddress;
		}
		readonly JobDeclaration jDeclaration;
		readonly OrgAddress orgA;

		public ZString DirectRepresentation
		{
			get
			{
				var result = ZString.Empty;
				if (jDeclaration.JE_DeclarantType == ESRepresentationTypeList.Codes._2Direct || jDeclaration.JE_DeclarantType == ESRepresentationTypeList.Codes._3Indirect)
				{
					result = jDeclaration.JE_DeclarantType;
				}
				return result;
			}
		}

		protected override ZString EmailAddressCore => GetEmailFromAddress(orgA, jDeclaration);

		protected override ZString PhoneCore => GetPhoneFromAddress(orgA);
	}
}
